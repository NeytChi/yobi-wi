using System.Collections.Generic;

namespace YobiWi.Development.Models
{
    public partial class UserCache
    {
        public UserCache()
        {
        }
        public int userId { get; set; }
        public string userToken { get; set; }
        public string userEmail { get; set; }
        public long createdAt { get; set; }
        public long lastLoginAt { get; set; }
        public string userHash { get; set; }
        public bool activate { get; set; }
        public int? recoveryCode { get; set; }
        public string recoveryToken { get; set; }
        public string userPassword { get; set; }
        public bool deleted { get; set; }
    }
    public class Users
    {
        public string user_email { get; set; }
        public string user_password { get; set; }
        public string user_token { get; set; }
        public int recovery_code { get; set; }
        public string recovery_token { get; set; }
	    public string user_confirm_password { get; set; }
    }
}
