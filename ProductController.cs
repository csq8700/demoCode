using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    public class ProductController : Controller
    {
        
        // 显示表单页面
        [HttpGet]
        public ActionResult Create()
        {
            String str = GetConnecttion.GetConnectionString();
            ViewData["ConnectionString"] = str;

            String DPAEncrypt = DPAPIEncryption.Encrypt(str);
            ViewData["DPAEncrypt"] = DPAEncrypt;
            String DPADecrypt = DPAPIEncryption.Decrypt(DPAEncrypt);
            ViewData["DPADecrypt"] = DPADecrypt;

            string AESEncrypt = AESEncryption.Encrypt(str);
            ViewData["AESEncrypt"] = AESEncrypt;
            string AESDecrypt = AESEncryption.Decrypt(AESEncrypt);
            ViewData["AESDecrypt"] = AESDecrypt;

            return View();
        }

        // 处理表单提交
        [HttpGet] // 使用HttpGet提交
        public ActionResult Confirm(Product product)
        {
            if (!int.TryParse(product.Price.ToString(), out int result))
            {
                ModelState.AddModelError("Price", " 请输入有效的数值");
            }
            // 手动验证模型
            if (!ModelState.IsValid)
            {
                // 如果验证失败，返回表单页面并显示错误信息
                return View("Create", product);
            }

            // 验证成功，跳转到确认页面
            return View(product);
        }
    }
}
