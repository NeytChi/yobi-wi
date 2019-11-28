using Common;
using System.Linq;
using YobiWi.Development;
using Microsoft.AspNetCore.Mvc;
using YobiWi.Development.Models;
using Microsoft.AspNetCore.Http;

namespace Controllers
{
    /// <summary>
    /// User functional for general movement. This class will be generate functional for user ability.
    /// </summary>
    [Route("v1.0/[controller]/[action]/")]
    [ApiController]
    public class BuildController : ControllerBase
    {
        private readonly YobiWiContext context;
        public UploaderBuilds uploader;
        public BuildController(YobiWiContext context)
        {
            this.context = context;
            this.uploader = new UploaderBuilds(context, Config.Domen, Config.Domen, Config.Domen);
        }
        [HttpPost]
        [ActionName("Upload")]
        public ActionResult<dynamic> UploadBuild(IFormFile build, [FromQuery] string userToken)
        {
            string message = null;
            if (uploader.UploadBuild(build, userToken, ref message))
            {
                return new { success = true };
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("All")]
        public ActionResult<dynamic> All([FromQuery] string userToken, [FromQuery] int from, [FromQuery] int count)
        {
            return (from build in context.Builds
            join user in context.Users on build.userId equals user.userId
            where user.userToken == userToken 
            && user.activate == true
            && user.deleted == false
            select build).Skip(from * count)
            .Take(from).ToList();
        }
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete([FromQuery] string hash)
        {
            string message = "";
            Build build = context.Builds.Where(b => b.buildHash == hash).FirstOrDefault();
            if (build != null)
            {
                Files file = context.Files.Where(f => f.fileId == build.fileId).First(); 
                //LoaderFile.DeleteFile(file);
                //uploader.DeleteDirectory(app.app_hash);
                return new { success = true };
            }
            else
            {
                message = "Server can't define build.";
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("Current")]
        public ActionResult<dynamic> Current([FromQuery] string hash)
        {
            return context.Builds.Where(b => b.buildHash == hash).FirstOrDefault();
        }
        public dynamic Return500Error(string message)
        {
            Log.Warn(message, HttpContext.Connection.LocalIpAddress.ToString());
            if (Response != null)
            {
                Response.StatusCode = 500;
            }
            return new { success = false, message = message, };
        }
    }
}