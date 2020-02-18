using System;
using Common;
using Microsoft.AspNetCore;
using YobiWi.Development.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Common
{
    public class Program
    {
        public static bool requestView = false;    
        public static IConfigurationRoot serverConfig;  
        public static void Main(string[] args)
        {
            if (args != null)
            {                
                if (args.Length >= 1)
                {
                    if (args[0] == "-c")
                    {
                        DeleteDatabase();
                        return;
                    }
                    if (args[0] == "-v")
                        requestView = true;
                }
            }
            Initialization();
            CreateWebHostBuilder(args).Build().Run();
        }
        
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) 
        => WebHost.CreateDefaultBuilder(args)
        .UseUrls(Config.GetHostsUrl(), Config.GetHostsHttpsUrl())
        .UseStartup<Startup>();
        
        public static void Initialization()
        {
            using (YobiWiContext context = new YobiWiContext())
            {
                context.Database.EnsureCreated();
            }
            Log.Info("Start server program.");
            Config.Initialization();
            MailF.Init();
        }
        public static void DeleteDatabase()
        {
            using (YobiWiContext context = new YobiWiContext())
            {
                context.Database.EnsureDeleted();
            }
            Console.WriteLine("Database '" + Config.databaseConfig["Database"].ToString()
            + "' was deleted.");
        }
        public static IConfigurationRoot serverConfiguration()
        {
            if (serverConfig == null) {
                serverConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("server.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"server.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", 
                    optional: true, reloadOnChange: true)
                .Build();
            }
            return serverConfig;
        }
    }
}