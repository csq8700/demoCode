public class WindowsAuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult(); // 未认证返回 401
        }
        base.OnActionExecuting(context);
    }
}

public class UserGroupFilter : ActionFilterAttribute
{
    private readonly string _requiredGroup;
    
    public UserGroupFilter(string requiredGroup)
    {
        _requiredGroup = requiredGroup;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // 检查用户是否属于指定的 AD 组
        if (!user.IsInRole(_requiredGroup))
        {
            context.Result = new ForbidResult(); // 无权限返回 403
        }

        base.OnActionExecuting(context);
    }
}

public class RedirectToHomeFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var session = context.HttpContext.Items["IsHomeVisited"] as bool?;

        // 如果用户没有访问主页，直接跳转错误页面
        if (session != true)
        {
            context.Result = new RedirectToActionResult("Unauthorized", "Error", null);
        }

        base.OnActionExecuting(context);
    }
}
public class TimeoutFilter : ActionFilterAttribute
{
    private static readonly Dictionary<string, DateTime> UserLastRequestTime = new();

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User.Identity.Name;
        if (string.IsNullOrEmpty(user))
        {
            return;
        }

        lock (UserLastRequestTime)
        {
            if (UserLastRequestTime.TryGetValue(user, out var lastRequest))
            {
                if ((DateTime.UtcNow - lastRequest).TotalMinutes > 15)
                {
                    UserLastRequestTime.Remove(user);
                    context.Result = new RedirectToActionResult("Timeout", "Error", null);
                    return;
                }
            }
            UserLastRequestTime[user] = DateTime.UtcNow;
        }

        base.OnActionExecuting(context);
    }
}

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<WindowsAuthFilter>();    // Windows 认证
    options.Filters.Add<TimeoutFilter>();        // 超时控制
});

[ServiceFilter(typeof(WindowsAuthFilter))]
[ServiceFilter(typeof(UserGroupFilter), Arguments = new object[] { "AdminGroup" })]
[ServiceFilter(typeof(RedirectToHomeFilter))]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

public class AdminController : Controller
{
    [ServiceFilter(typeof(UserGroupFilter), Arguments = new object[] { "AdminGroup" })]
    public IActionResult ManageUsers()
    {
        return View();
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public List<User> GetUsers()
    {
        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT Id, Name, Email FROM Users", connection);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2)
                });
            }
        }

        return users;
    }

    public void AddUser(User user)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO Users (Name, Email) VALUES (@Name, @Email)", connection);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.ExecuteNonQuery();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly DatabaseService _databaseService;

    public HomeController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public IActionResult Index()
    {
        // 获取所有用户
        var users = _databaseService.GetUsers();
        return View(users);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(User user)
    {
        if (ModelState.IsValid)
        {
            // 添加新用户
            _databaseService.AddUser(user);
            return RedirectToAction("Index");
        }
        return View(user);
    }
}


using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文（使用 EF Core）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加全局过滤器
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalActionFilter>();
});

// 添加 Session 服务
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 配置 HTTP 请求管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 启用 Session 中间件
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
