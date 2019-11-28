using System;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Common
{
    public static class Config
    {
        public static JObject serverConfig;
        public static JObject databaseConfig;
        public static string configurationName = "conf.json";
        public static string dbconfName = "dbconf.json";
        public static string IP = "127.0.0.1";
        public static string Domen = "(none)";
        public static int Port = 8023;
        public static string currentDirectory = Directory.GetCurrentDirectory();       // Return of the path occurs without the last '/' (pointer to the directory)
        public static bool initiated = false;
        public static bool logFiles = false;

        public static void Initialization()
        {
            initiated = true;
            FileInfo confExist = new FileInfo(currentDirectory + "/" + configurationName);
            FileInfo dbconfExist = new FileInfo(currentDirectory + "/" + dbconfName);
            if (confExist.Exists && dbconfExist.Exists)
            {
                string confInfo = ReadConfigJsonData(configurationName);
                string dbconfInfo = ReadConfigJsonData(dbconfName);
                serverConfig = JObject.Parse(confInfo);
                databaseConfig = JObject.Parse(dbconfInfo);
                if (serverConfig != null && databaseConfig != null)
                {
                    Port = GetServerConfigValue("port", JTokenType.Integer);
                    IP = GetServerConfigValue("ip", JTokenType.String);
                    Domen = GetServerConfigValue("domen", JTokenType.String);
                    logFiles = GetServerConfigValue("log_files", JTokenType.Boolean);
                }
                else 
                {
                    Console.WriteLine("Start with default config setting.");
                }
            }
            else
            {
                Console.WriteLine("Start with default config setting.");
            }
        }
        private static string ReadConfigJsonData(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (var fstream = File.OpenRead(fileName))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    string textFromFile = System.Text.Encoding.Default.GetString(array);
                    fstream.Close();
                    return textFromFile;
                }
            }
            else
            {
                Console.WriteLine("Can not read file=" + fileName + " , function Config.ReadConfigJsonData()");
                return string.Empty;
            }
        }
        public static string GetHostsUrl()
        {
            string urlConnection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (serverConfig != null)
            {
                if (serverConfig.ContainsKey("ip")
                && serverConfig.ContainsKey("port"))
                {
                    
                    urlConnection = "http://" + serverConfig["ip"].ToString() + ":" + 
                    serverConfig["port"].ToString() + "/"; 
                }
                else 
                { 
                    Console.WriteLine("Can't create url connetion string, one of values doesn't exist."); 
                }
            }
            else 
            { 
                Console.WriteLine("Server can't define conf.json; Can't get url connetion string."); 
            }
            return urlConnection;
        }
        public static string GetHostsHttpsUrl()
        {
            string urlConnection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (serverConfig != null)
            {
                if (serverConfig.ContainsKey("ip")
                && serverConfig.ContainsKey("port"))
                {
                    
                    urlConnection = "https://" + serverConfig["ip"].ToString() + ":" + 
                    (serverConfig["port"].ToObject<int>() + 1) + "/"; 
                }
                else 
                { 
                    Console.WriteLine("Can't create url connetion string, one of values doesn't exist."); 
                }
            }
            else 
            { 
                Console.WriteLine("Server can't define conf.json; Can't get url connetion string."); 
            }
            return urlConnection;
        }
        public static string GetDatabaseConfigConnection()
        {
            string mysqlConnection = null;
            if (!initiated)
            {
                Initialization();
            }
            if (databaseConfig != null)
            {
                if (databaseConfig.ContainsKey("Server")
                && databaseConfig.ContainsKey("Database")
                && databaseConfig.ContainsKey("User")
                && databaseConfig.ContainsKey("Password"))
                {
                    mysqlConnection = "Server=" + databaseConfig["Server"].ToString() + ";" +
                    "Database=" + databaseConfig["Database"].ToString() + ";" + 
                    "User=" + databaseConfig["User"].ToString() + ";" + 
                    "Pwd=" + databaseConfig["Password"].ToString() + ";Charset=utf8;";
                }
                else 
                { 
                    Console.WriteLine("Can't create mysql connetion string, one of values doesn't exist."); 
                }
            }
            else 
            { 
                Console.WriteLine("Server can't define dbconf.json; Can't get mysql connetion string."); 
            }
            return mysqlConnection;
        }
        public static dynamic GetServerConfigValue(string configurationName, JTokenType typeValue)
        {
            if (!initiated)
            {
                Initialization();
            }
            if (serverConfig != null)
            {
                if (serverConfig.ContainsKey(configurationName))
                {
                    switch (typeValue)
                    {
                        case JTokenType.Integer:
                            if (serverConfig[configurationName].Type == JTokenType.Integer) 
                            { 
                                return serverConfig[configurationName].ToObject<int>(); 
                            }
                            else 
                            { 
                                return -1; 
                            }
                        case JTokenType.String:
                            if (serverConfig[configurationName].Type == JTokenType.String) 
                            { 
                                return serverConfig[configurationName].ToObject<string>(); 
                            }
                            else 
                            { 
                                return ""; 
                            }
                        case JTokenType.Boolean:
                            if (serverConfig[configurationName].Type == JTokenType.Boolean) 
                            { 
                                return serverConfig[configurationName].ToObject<bool>(); 
                            }
                            else 
                            { 
                                return false; 
                            }
                        default:
                            Console.WriteLine("Can't get configuration value, type of value isn't define.");
                            return null;
                    }
                }
                else 
                { 
                    Console.WriteLine("Can't get configuration value, json doesn't have value=" + configurationName + "."); 
                }
            }
            else 
            { 
                Console.WriteLine("Can't get configuration value, json is null."); 
            }
            switch (typeValue)
            {
                case JTokenType.Integer: 
                    return -1;
                case JTokenType.String: 
                    return null;
                default: 
                    return null;
            }
        }
    }
}