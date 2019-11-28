using Common;
using YobiWi.Development;
using Microsoft.AspNetCore.Mvc;
using YobiWi.Development.Models;

namespace Controllers
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly YobiWiContext context;
        public UsersManager usersManager;
        public UsersController(YobiWiContext context)
        {
            this.context = context;
            this.usersManager = new UsersManager(context);
        }
        /// <summary>
        /// Registration user with user_email and user_password.
        /// </summary>
        /// <param name="user">User data for registration.</param>
        [HttpPost]
        [ActionName("Registration")]
        public ActionResult<dynamic> Registration(Users user)
        {
            string message = string.Empty;
            if (usersManager.RegistrationUser(user, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "User account was successfully registered. " +
                    "See your email to activate account by link." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RegistrationEmail")]
        public ActionResult<dynamic> RegistrationEmail(Users user)
        {
            string message = null;
            if (usersManager.RegistrationEmail(user.user_email, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Send confirm email to user." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("Login")]
        public ActionResult<dynamic> Login(Users user)
        {
            string message = null;
            UserCache userCache = usersManager.Login(user, ref message);
            if (userCache != null)
            {
                return new 
                { 
                    success = true, 
                    data = new 
                    { 
                        user = new 
                        {
                            user_token = userCache.userToken,
                            user_email = userCache.userEmail,
                            created_at = userCache.createdAt,
                            last_login_at = userCache.lastLoginAt
                        }
                    } 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("LogOut")]
        public ActionResult<dynamic> LogOut(Users user)
        {
            string message = null;
            if (usersManager.LogOut(user.user_token, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Log out is successfully." 
                }; 
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("RecoveryPassword")]
        public ActionResult<dynamic> RecoveryPassword(Users user)
        {
            string message = null;
            if (usersManager.RecoveryPassword(user.user_email, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Recovery password. Send message with code to email=" 
                    + user.user_email + "." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("CheckRecoveryCode")]
        public ActionResult<dynamic> CheckRecoveryCode(Users user)
        {
            string message = null;
            string recoveryToken = usersManager.CheckRecoveryCode(user.user_email, 
            user.recovery_code, ref message);
            if (recoveryToken != null)
            {
                return new 
                { 
                    success = true, 
                    data = new 
                    { 
                        recovery_token = recoveryToken 
                    }
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        public ActionResult<dynamic> ChangePassword(Users user)
        {
            string message = null;
            if (usersManager.ChangePassword(user.recovery_token,
            user.user_password, user.user_confirm_password, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Change user password." 
                };
            }   
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Activate")]
        public ActionResult<dynamic> Activate([FromQuery] string hash)
        {
            string message = null;
            if (usersManager.Activate(hash, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "User account is successfully active." 
                };
            }
            return Return500Error(message);
        }
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete(Users user)
        { 
            string message = null;
            if (usersManager.Delete(user.user_token, ref message))
            {
                return new 
                { 
                    success = true, 
                    message = "Account was successfully deleted." 
                };
            }
            return Return500Error(message);
        }
        public dynamic Return500Error(string message)
        {
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            Log.Warn(message, HttpContext?.Connection.RemoteIpAddress.ToString() ?? "");
            return new 
            { 
                success = false, 
                message = message 
            };
        }
    }
}