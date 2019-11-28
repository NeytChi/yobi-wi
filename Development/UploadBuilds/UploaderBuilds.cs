using System;
using Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Iteedee.ApkReader;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using YobiWi.Development.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YobiWi.Development
{
    public class UploaderBuilds
    {
        public YobiWiContext context;
        public Random random = new Random();
        public FileManager fileManager = FileManager.GetInstance();
        public string pathArchives = Config.GetServerConfigValue("path_archives", JTokenType.String);
        public string fullPathPlist = "";
        public string relativePathPlist = "/Plist/";
        public string fullPathUpload = "";
        public string relativePathUpload = "/files/Upload/";
        public string keyValueSet = "(?<=<string>)(.*)(?=</string>)";
        public string domen = "";
        public string sslDomen = "";
        //string ipaPlistPath = "itms-services://?action=download-manifest&url=https://" + Ssl_Domen + "/Plist/" + ipa.app_hash + ".plist";
        
        public UploaderBuilds(YobiWiContext context, string domen, string sslDomen, string pathPlist)
        {
            this.domen = domen;
            this.context = context;
            this.sslDomen = sslDomen;
            this.fullPathPlist = pathPlist;
            this.fullPathUpload = Config.currentDirectory + relativePathUpload;
            this.fullPathPlist = fullPathPlist + relativePathPlist;
            Directory.CreateDirectory(fullPathUpload);
            Directory.CreateDirectory(fullPathPlist);
        }
        public bool UploadBuild(IFormFile build, string userToken, ref string message)
        {
            if (build != null && !string.IsNullOrEmpty(userToken))
            {
                if (build.ContentType.Contains("application/vnd.android.package-archive"))
                {
                    UploadAPK(build, userToken, ref message);
                }
                else if (build.ContentType.Contains("application/octet-stream"))
                {
                    UploadIpa(build, userToken, ref message);
                }
                else
                {
                    message = "Server can't define build type.";
                }
            }
            return false;
        }
        public Build UploadIpa(IFormFile build, string userToken, ref string message)
        {
            if (build != null)
            {
                string hash = Validator.GenerateHash(6);
                string pathDirectory = CreateBuildDirectory(build, hash, ref message);
                if (pathDirectory != null)
                {
                    string plistPath = SearchPathToFile("Info.plist", pathDirectory);
                    if (!string.IsNullOrEmpty(plistPath))
                    {
                        Build ipa = GetIpaBuildWithInto(plistPath, pathDirectory);
                        if (ipa != null)
                        { 
                            ipa.userId = GetUserId(userToken);
                            ipa.buildHash = hash;
                            ipa.archiveName = build.FileName;
                            ipa.urlManifest = pathDirectory + "/" + build.FileName;
                            ipa.installLink = plistPath.Substring(pathArchives.Length);
                            ipa.createdAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                            SaveBuild(ipa);
                            CreateInstallPlist(ipa);
                            Log.Info("Uploaded IPA build, Hash ->" + hash + ".");
                        }
                    }
                    else
                    {
                        message = "Can't search plist file.";
                        Log.Error(message);
                    }
                }
            }
            return null;
        }
        public string CreateBuildDirectory(IFormFile build, string hash, ref string message)
        {
            if (build != null && !string.IsNullOrEmpty(hash))
            {
                DirectoryInfo directory = Directory.CreateDirectory(pathArchives + hash);
                if (fileManager.SaveTo(build, pathArchives + hash + "/" + build.FileName))
                {
                    try
                    {   
                        ZipFile.ExtractToDirectory(pathArchives + hash + "/" + build.FileName, pathArchives + hash + "/" );
                        return pathArchives + hash + "/" ;
                    }
                    catch (Exception ex)
                    {
                        message = "Server can't unzip archive.";
                        Log.Info(message + " Exception message -> "  + ex.Message);
                    }
                }
            }
            return null;
        }
        public Build GetIpaBuildWithInto(string plistPath, string pathDirectoryIPA)
        {
            if (!string.IsNullOrEmpty(plistPath) && !string.IsNullOrEmpty(pathDirectoryIPA))
            {
                Dictionary<string, string> xml = GetXML(plistPath);
                Build ipa = new Build();
                if (xml.ContainsKey("CFBundleName"))
                {
                    ipa.buildName = xml["CFBundleName"];
                }
                if (xml.ContainsKey("CFBundleIconFiles"))
                {
                    string iconPath = SearchPathRelativeFileName(xml["CFBundleIconFiles"], pathDirectoryIPA);
                    if (iconPath != null)
                    {
                        ipa.urlIcon = iconPath.Substring(Config.currentDirectory.Length);
                    }
                }
                if (xml.ContainsKey("CFBundleShortVersionString"))
                {
                    ipa.version = xml["CFBundleShortVersionString"];
                }
                if (xml.ContainsKey("CFBundleVersion"))
                {
                    ipa.numberBuild = xml["CFBundleVersion"];
                }
                if (xml.ContainsKey("CFBundleIdentifier"))
                {
                    ipa.bundleIdentifier = xml["CFBundleIdentifier"];
                }
                Log.Info("Get IPA set info.");
                return ipa;
            }
            return null;
        }
        public void CreateInstallPlist(Build ipa)
        {
            XElement xAssets = new XElement("key", "assets");
            XElement xFirstDict = new XElement("dict", 
            new XElement("key", "kind"),
            new XElement("string", "software-package"),
            new XElement("key", "url"),
            new XElement("string", ipa.urlManifest));
            XElement xFirstArray = new XElement("array", xFirstDict);
            XElement xMetadata = new XElement("key", "metadata");
            XElement xSecondDict = new XElement("dict", 
            new XElement("key", "bundle-identifier"),
            new XElement("string", ipa.bundleIdentifier),
            new XElement("key", "bundle-version"),
            new XElement("string", "4.0"),
            new XElement("key", "kind"),
            new XElement("string", "software"),
            new XElement("key", "title"),
            new XElement("string", ipa.buildName));
            XElement xThirdDict = new XElement("dict", xAssets, xFirstArray, xMetadata, xSecondDict);
            XElement xItems = new XElement("key", "items");
            XElement xSecondArray = new XElement("array", xThirdDict);
            XElement xFourDict = new XElement("dict", xItems, xSecondArray);
            XElement xPlist = new XElement("plist", new XAttribute("version", "1.0"), xFourDict);
            XDocument xDocument = new XDocument(xPlist);
            xDocument.AddFirst(new XDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN", 
            "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null));
            xDocument.Save(pathArchives + "Info.plist");
            Log.Info("Create installation plist.");
        }
        // public string SetManifestLinkToPlist(string plistInfo, string pathIpaArchive)
        // {
        //     int searchStart = plistInfo.IndexOf("CFBundleIdentifier", StringComparison.Ordinal) - "<key>".Length;
        //     if (searchStart != -1)
        //     {
        //         pathIpaArchive = "https://" + Domen + "/YobiApp" + pathIpaArch.Substring(CurrentDirectory.Length);
        //         string insertValue = "<key>url</key>" + "<string>" + pathIpaArchive + "</string>";
        //         string value = plistInfo.Insert(searchStart, insertValue);
        //         return value;
        //     }
        //     return null;
        // }
        public string SetKeyValueXML(string xml, string key, string value)
        {
            if (!string.IsNullOrEmpty(xml) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                int start = xml.IndexOf(@"<key>" + key + @"</key>", StringComparison.Ordinal);
                if (start != -1) 
                {
                    Regex regex = new Regex(keyValueSet, RegexOptions.Multiline);
                    Match match = regex.Match(xml, start);
                    if (match.Success)
                    {
                        xml = xml.Remove(match.Index, match.Length);
                        xml = xml.Insert(match.Index, value);
                        return xml;
                    }
                    else
                    {
                        return xml;
                    } 
                }
            }
            return "";
        }
        public Dictionary<string, string> GetXML(string pathPList)
        {
            if (!string.IsNullOrEmpty(pathPList))
            {
                XDocument docs = XDocument.Load(pathPList);
                var elements = docs.Descendants("dict");
                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                keyValues = docs.Descendants("dict")
                .SelectMany(d => d.Elements("key")
                .Zip(d.Elements()
                .Where(e => e.Name != "key"), (k, v) => new { Key = k, Value = v }))
                .ToDictionary(i => i.Key.Value, i => i.Value.Value);
                return keyValues;
            }
            return null;
        }
        public string SearchPathToFile(string nameFile, string startSearchFolder)
        {
            string findPathFile = "";
            string pathCurrent = startSearchFolder;
            string[] files = Directory.GetFiles(pathCurrent);
            foreach (string file in files)
            {
                if (file == pathCurrent + "/" + nameFile) 
                { 
                    return file; 
                }
            }
            string[] folders = Directory.GetDirectories(pathCurrent);
            foreach (string folder in folders)
            {
                FileAttributes attr = File.GetAttributes(folder);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    findPathFile = SearchPathToFile(nameFile, folder);
                }
            }
            return findPathFile;
        }
        public string SearchPathRelativeFileName(string nameFile, string startSearchFolder)
        {
            string[] paths = Directory.GetFiles(startSearchFolder, nameFile + "*", SearchOption.AllDirectories);
            if (paths != null)
            {
                if (paths.Length > 0)
                {
                    return paths[0];
                }
            }
            return null;
        }
        public Build UploadAPK(IFormFile build, string userToken, ref string message)
        {
            if (build != null)
            {
                string hash = Validator.GenerateHash(6);
                string pathDirectory = CreateBuildDirectory(build, hash, ref message);
                if (pathDirectory != null)
                {
                    Build apk = new Build
                    {
                        userId = GetUserId(userToken),
                        buildHash = hash,
                        archiveName = build.FileName,
                        urlManifest = "/" + hash + "/" + build.FileName,
                        installLink = "/" + hash + "/" + build.FileName,
                        createdAt = DateTimeOffset.Now.ToUnixTimeSeconds() 
                    };
                    GetAPKBuildWithInfo(pathDirectory, ref apk);
                    SaveBuild(apk);
                    return apk;
                }
                Log.Info("Uploaded APK file with hash ->" + hash + ".");
            }
            return null;
        }
        public void GetAPKBuildWithInfo(string pathDirectoryAPK, ref Build apk)
        {
            if (!string.IsNullOrEmpty(pathDirectoryAPK) && apk != null)
            {
                ApkInfo info = GetAndroidInfo(pathDirectoryAPK);
                apk.buildName = info.packageName;
                apk.urlIcon = apk.buildHash + info.iconFileName[0];
                apk.version = info.versionName;
                apk.numberBuild = info.targetSdkVersion;
                apk.bundleIdentifier = info.label;
            }
        }
        private ApkInfo GetAndroidInfo(string pathDirectoryAPK)
        {
            if (!string.IsNullOrEmpty(pathDirectoryAPK))
            {
                string manifestPath = SearchPathToFile("AndroidManifest.xml", pathDirectoryAPK);
                if (!string.IsNullOrEmpty(manifestPath))
                {
                    string resourcesPath = SearchPathToFile("resources.arsc", pathDirectoryAPK);
                    if (!string.IsNullOrEmpty(resourcesPath))
                    {
                        byte[] manifestData = File.ReadAllBytes(manifestPath);
                        byte[] resourcesData = File.ReadAllBytes(resourcesPath);
                        if (manifestData != null || resourcesData != null) 
                        {
                            ApkReader apkReader = new ApkReader();
                            ApkInfo info = apkReader.extractInfo(manifestData, resourcesData);
                            Log.Info("Get android info from archive.");
                            return info; 
                        }
                    }
                    else
                    {
                        Log.Error("Can't search resources.arsc file.");
                    }
                }
                else
                {
                    Log.Error("Can't search AndroidManifest.xml file.");
                }
            }
            return null;
        }
        public void SaveBuild(Build build)
        {
            if (build != null)
            {
                context.Builds.Add(build);
                context.SaveChanges();
            }
        }
        public string ReadFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string textFromFile = "";
                using (FileStream fstream = File.OpenRead(path))
                {
                    byte[] array = new byte[fstream.Length];
                    fstream.Read(array, 0, array.Length);
                    textFromFile = Encoding.Default.GetString(array);
                    fstream.Close();
                }
                return textFromFile;
            }
            return null;
        }
        public bool writeFile(string path, string text)
        {
            using (FileStream fstream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                byte[] array = Encoding.ASCII.GetBytes(text);
                fstream.Write(array, 0, array.Length);
                fstream.Close();
                return true;
            }
        }
        public int GetUserId(string userToken)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                return context.Users.Where(u => u.userToken == userToken)
                .Select(u => u.userId).FirstOrDefault();
            }
            return 0;
        }
    }
}