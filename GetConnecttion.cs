namespace WebApplication1.Services
{
    public static class GetConnecttion
    {
        public static string GetConnectionString()
        {
            String connectionString = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build()
                .GetConnectionString("DefaultConnection");
            return connectionString;
        }
    }
}