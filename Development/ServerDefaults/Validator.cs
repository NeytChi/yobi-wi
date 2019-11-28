using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public static class Validator
    {
		private const int minLength = 6;
        private const int maxLength = 20;
        private static  EmailAddressAttribute emailChecker = new EmailAddressAttribute();
		public static Random random = new Random();
        private static string AlphaviteNumbers = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static string passwordCrypt = "abc123";

        public static bool ValidateEmail(string email)
        {
            bool bar = false;
            if (!string.IsNullOrEmpty(email))
            {
                bar = emailChecker.IsValid(email);
            }
            Log.Info("Validating email->'" + email ?? "" + "' success ->" + bar + ".");
            return bar;
        } 
		public static bool ValidatePassword(string password, ref string answer) 
		{
			if (!string.IsNullOrEmpty(password)) 
            {
                if (RequiredLength(password, ref answer))
                {
                    return HasValues(password, ref answer);
                }
            }
            else
            {
                answer = "Password is empty.";
            }
            Log.Info(answer);
            return false;
        }
        public static bool HasValues(string password, ref string answer)
        {
            if (HasLetter(password, ref answer))
            {
                if (HasDigit(password, ref answer))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool RequiredLength(string password, ref string answer)
        {
            if (password.Length >= minLength)
            {
                if (password.Length <= maxLength)
                {
                    return true;
                }
            } 
            answer = "Password must be more than " + minLength 
            + " characters and less that " + maxLength + ".";
            return false;
        }
        public static bool HasLetter(string password, ref string answer)
        {
            if (password != null)
            {
                foreach (char c in password)
                {
                    if (char.IsLetter(c))
                    { 
                        return true;
                    }
                }
            }
            answer = "Current password doesn't has letter.";
            return false;
        }
        public static bool HasDigit(string password, ref string answer)
        {
            if (password != null)
            {
                foreach (char c in password)
                {
                    if (char.IsDigit(c))
                    { 
                        return true;
                    }
                }
            }            
            answer = "Current password doesn't has decimal digit.";
            return false;
        }
        public static string GenerateHash(int hashLength)
        {
            string hash = "";
            for (int i = 0; i < hashLength; i++)
            {
                hash += AlphaviteNumbers[random.Next(AlphaviteNumbers.Length)];
            }
            return hash;
        }
        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                Log.Error("Input value is null, function HashPassword()");
                return "";
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                return false;
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(ref buffer3,ref buffer4);
        }
        private static bool ByteArraysEqual(ref byte[] b1,ref byte[] b2)
        {
            if (b1 == b2)
            {
                return true;
            }
            if (b1 == null || b2 == null)
            { 
                return false; 
            }
            if (b1.Length != b2.Length)
            {
                return false;
            }
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) 
                {
                    return false;
                }
            }
            return true;
        }
        public static string Encrypt(ref string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passwordCrypt,  new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(ref string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passwordCrypt, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
