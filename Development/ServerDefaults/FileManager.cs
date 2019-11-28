using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Common
{
    /// <summary>
    /// This class save files by specific method.
    /// <summary>
    public class FileManager
    {
        public string domenName = Config.Domen;
        public string currentDirectory = Config.currentDirectory;
        public DateTime currentTime = DateTime.Now;
        public string dailyFolder = "/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "/";
        private FileManager()
        {

        }
        private static FileManager instance;
        public static FileManager GetInstance()
        {
            if (instance == null)
            {
                instance = new FileManager();
            }
            return instance;
        }
        /// <summary>
        /// Save file by specific path.
        /// <summary>
        /// <param>Relative path without first and last '/'</param>
        /// <return>Saved file path.</return>
        public string SaveFile(IFormFile file, string RelativePath)
        {
            string fileName = Validator.GenerateHash(10);
            ChangeDailyPath();
            string fileRelativePath = "/" + RelativePath + dailyFolder;
            CheckDirectory(fileRelativePath);            
            if (SaveTo(file, fileRelativePath, fileName))
            {
                return fileRelativePath + fileName;
            }
            return null;
        }
        public bool SaveTo(IFormFile file, string relativePath, string fileName)
        {
            if (!File.Exists(currentDirectory + relativePath + fileName))
            {
                using (var stream = new FileStream(currentDirectory 
                + relativePath + fileName, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                Log.Info("Create file; fileName->" + fileName + ".");
                return true;
            }
            else
            {
                Log.Error("Server can't save file with same file names.");
                return false;
            }
        }
        public bool SaveTo(IFormFile file, string fullPathWithName)
        {
            if (!File.Exists(fullPathWithName))
            {
                using (var stream = new FileStream(fullPathWithName, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                Log.Info("Create file.");
                return true;
            }
            else
            {
                Log.Error("Server can't save file with same file names.");
                return false;
            }
        }
        /// <summary>
        /// Change daily path to save files in daily new folder. That need to save file without override another file.
        /// <summary>
        private void ChangeDailyPath()
        {
            if (currentTime.Day != DateTime.Now.Day)
            {
                currentTime = DateTime.Now;
                dailyFolder = "/" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "/";
            }
        }
        public void CheckDirectory(string fileRelativePath)
        {
            if (!Directory.Exists(currentDirectory + fileRelativePath))
            {
                Directory.CreateDirectory(currentDirectory + fileRelativePath);
            }
        }
        public void DeleteFile(string relativePath)
        {
            if (relativePath != null)
            {
                if (File.Exists(currentDirectory + relativePath))
                {
                    File.Delete(currentDirectory + relativePath);
                    Log.Info("File was deleted. Relative path ->/" + relativePath + ".");
                }
            }
        }
    }
}

