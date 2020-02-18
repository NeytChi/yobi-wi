using Common;
using System;
using System.Linq;
using YobiWi.Development.Models;

namespace YobiWi.Development
{
    public class UsersManager
    {
        private YobiWiContext context;
        public UsersManager(YobiWiContext context)
        {
            this.context = context;
        }
        public UsersManager()
        {
            this.context = new YobiWiContext();
        }
        public bool RegistrationUser(Users userData, ref string message)
        {
            if (userData != null)
            {
                if (CheckUserData(userData, ref message))
                {
                    UserCache user = new UserCache();
                    user.userEmail = userData.user_email;
                    user.userPassword = userData.user_password;
                    if (ValidateUser(user, ref message))
                    {
                        UserCache oldUser = context.Users.Where(u => u.userEmail == user.userEmail).FirstOrDefault();
                        if (oldUser == null)
                        {
                            RegistrateUser(user);
                            SendConfirmEmail(user.userEmail, user.userHash);
                            return true;
                        }
                        else
                        {
                            return RestoreUser(user,ref message);
                        }
                    }
                }
            }
            return false;
        }
        public bool RestoreUser(UserCache user, ref string message)
        {
            if (user != null)
            {
                if (user.deleted == true)
                {
                    user.deleted = false;
                    user.userToken = Validator.GenerateHash(40); 
                    context.Users.Update(user);
                    context.SaveChanges();
                    Log.Info("User account was restored.", user.userId);
                    return true;
                }
                else 
                {
                    message = "An account with email ->'" + user.userEmail + "' already exists.";
                    Log.Warn(message); 
                }
            }
            return false;
        }
        /// <summary> 
        /// Save current user to database with required fields.
        /// </summary>
        public void RegistrateUser(UserCache user)
        {
            if (user != null)
            {
                user.userPassword = Validator.HashPassword(user.userPassword);
                user.userHash = Validator.GenerateHash(100);
                user.createdAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                user.userToken = Validator.GenerateHash(40);
                user.activate = true;
                context.Users.Add(user);
                context.SaveChanges();
                Log.Info("Registrate new user.", user.userId);
            }
            else
            {
                Log.Warn("Server can't registrate user, because user is null.", user.userId);
            }
        }
        public void SendConfirmEmail(string userEmail, string userHash)
        {
            if (userEmail != null)
            {
                if (userHash != null)
                {
                    MailF.SendEmail(userEmail, "Confirm account", 
                    "Confirm account: <a href=http://devinstasoft.yobidev.com/mailLink/" 
                    + userHash + ">Confirm url!</a>");
                }
            }
        }
        public void SendRecoveryEmail(string userEmail, int recoveryCode)
        {
            if (userEmail != null)
            {
                MailF.SendEmail(userEmail, "Recovery password", 
                "Recovery code=" + recoveryCode);
            }
        }
        public bool ValidateUser(UserCache user, ref string message)
        {
            if (Validator.ValidateEmail(user.userEmail))
            {
                if (Validator.ValidatePassword(user.userPassword, ref message))
                {
                    return true;
                }
                else
                {
                    Log.Warn("email -> '" + user.userEmail + "'; " + message);
                } 
            }
            else 
            { 
                message = "Wrong email ->" + user.userEmail + ".";
                Log.Warn(message);
            }
            return false;
        }
        public UserCache GetByHash(string userHash, ref string message)
        {
            if (userHash != null)
            {
                UserCache user = context.Users.Where(
                u => u.userHash == userHash
                && u.activate == false
                && u.deleted == false).FirstOrDefault();
                if (user != null)
                {
                    return user;
                }
            }
            message = "Server can't define user by hash."; 
            return null;
        }
        public UserCache GetNonDeleted(string userEmail, ref string message)
        {
            if (userEmail != null)
            {
                UserCache user = context.Users.Where(
                u => u.userEmail == userEmail
                && u.deleted == false).FirstOrDefault();
                if (user != null)
                {
                    return user;
                }
            }
            message = "Server can't define user by email."; 
            return null;
        }
        public UserCache GetByToken(string userToken, ref string message)
        {
            if (userToken != null)
            {
                UserCache user = context.Users.Where(
                u => u.userToken == userToken
                && u.deleted == false).FirstOrDefault();;
                return user;
            }
            else
            {
                message = "Server can't define user by promotion token."; 
            }
            return null;
        }
        public UserCache GetByRecoveryToken(string recoveryToken, ref string message)
        {
            if (recoveryToken != null)
            {
                UserCache user = context.Users.Where(
                u => u.recoveryToken == recoveryToken
                && u.deleted  == false).FirstOrDefault();;
                return user;
            }
            else
            {
                message = "Server can't define user by promotion token."; 
            }
            return null;
        }
        
        public UserCache GetActivateNonDeleted(string userEmail, ref string message)
        {
            if (userEmail != null)
            {
                UserCache user = context.Users.Where(
                u => u.userEmail == userEmail
                && u.deleted == false
                && u.activate == true).FirstOrDefault();;
                return user;
            }
            else
            {
                message = "Server can't define user by email."; 
            }
            return null;
        }
        public bool CheckUserData(Users user, ref string message)
        {
            if (!string.IsNullOrEmpty(user.user_email))
            {
                if (!string.IsNullOrEmpty(user.user_password))
                {
                    return true;    
                }
                else
                {
                    message = "User's password is empty or null.";
                }       
            }
            else 
            { 
                message = "User's email is empty or null.";
            }
            return false;
        }
        public UserCache Login(Users user, ref string message)
        {
            if (CheckUserData(user, ref message))
            {
                UserCache userCache = GetActivateNonDeleted(user.user_email, ref message);
                if (userCache != null)
                {
                    if (Validator.VerifyHashedPassword(userCache.userPassword, user.user_password))
                    {
                        userCache.lastLoginAt = DateTimeOffset.Now.ToUnixTimeSeconds();
                        context.Users.Update(userCache);
                        context.SaveChanges();
                        Log.Info("User login.", userCache.userId);
                        return userCache;        
                    }
                    else 
                    { 
                        message = "Wrong password."; 
                    }
                }
                else
                {
                    message = "Server can't define user by email."; 
                }
            }
            return null;
        }
        public bool LogOut(string userToken, ref string message)
        {
            UserCache user = GetByToken(userToken, ref message);
            if (user != null)
            {
                user.userToken = Validator.GenerateHash(40);
                context.Users.Update(user);
                context.SaveChanges();
                Log.Info("User log out.", user.userId);
                return true;
            }
            return false;
        }
        public bool RecoveryPassword(string userEmail,ref string message)
        {
            if (!string.IsNullOrEmpty(userEmail))
            {
                UserCache user = GetActivateNonDeleted(userEmail, ref message);
                if (user != null)
                {
                    user.recoveryCode = Validator.random.Next(100000, 999999);
                    context.Users.Update(user);
                    context.SaveChanges();
                    SendRecoveryEmail(user.userEmail, (int)user.recoveryCode);
                    Log.Info("Recovery password.", user.userId);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Confirm that current user get recovery code and ready to change his password.
        /// </summary>
        /// <return>Recovery token - for access to change password.</return>
        public string CheckRecoveryCode(string userEmail, int recoveryCode, ref string message)
        {
            if (userEmail != null)
            {
                UserCache user = GetActivateNonDeleted(userEmail, ref message);
                if (user != null)
                {
                    if (user.recoveryCode == recoveryCode)
                    {
                        user.recoveryToken = Validator.GenerateHash(40);
                        user.recoveryCode = -1;
                        context.Users.Update(user);
                        context.SaveChanges();
                        Log.Info("Check user's recovery code.", user.userId);
                        return user.recoveryToken;
                    }
                    else
                    {
                        message = "Wrong recovery code."; 
                    }
                }
            }
            return null;
        }
        public bool ChangePassword(string recoveryToken, string userPassword, string userConfirmPassword, ref string message)
        {
            if (recoveryToken != null)
            {
                UserCache user = GetByRecoveryToken(recoveryToken, ref message);
                if (user != null)
                {
                    if (userPassword.Equals(userConfirmPassword))
                    {
                        if (Validator.ValidatePassword(userPassword, ref message))
                        {
                            user.userPassword = Validator.HashPassword(userPassword);
                            user.recoveryToken  = "";
                            context.Users.Update(user);
                            context.SaveChanges();
                            Log.Info("Change user password.", user.userId);
                            return true;
                        }
                    }
                    else 
                    { 
                        message = "Password are not match to each other."; 
                    }            
                }
            }
            return false;
        }
        public bool RegistrationEmail(string userEmail, ref string message)
        {
            if (userEmail != null)
            {
                UserCache user = GetNonDeleted(userEmail, ref message);
                if (user != null)
                {
                    SendConfirmEmail(user.userEmail, user.userHash);
                    Log.Info("Send registration email to user.", user.userId);
                    return true;
                }
            }
            return false;
        }
        public bool Activate(string hash, ref string message)
        {
            if (hash != null)
            {
                UserCache user = GetByHash(hash, ref message);
                if (user != null)
                {
                    user.activate = true;
                    context.Users.Update(user);
                    context.SaveChanges();                
                    Log.Info("Active user account.", user.userId);
                    return true;
                }
            }
            return false;
        }
        public bool Delete(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken))
            {
                UserCache user = GetByToken(userToken, ref message);
                if (user != null)
                {
                    user.deleted = true;
                    user.userToken = null;
                    context.Users.Update(user);
                    context.SaveChanges();
                    Log.Info("Account was successfully deleted.", user.userId); 
                    return true;
                }
            }
            return false;
        }
    }
}