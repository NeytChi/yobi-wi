using Common;
using System.Linq;
using YobiWi.Development;
using Microsoft.AspNetCore.Mvc;
using YobiWi.Development.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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
            this.uploader = new UploaderBuilds(context);
        }
        [HttpPost]
        [ActionName("Upload")]
        public ActionResult<dynamic> UploadBuild(IFormFile build, [FromQuery] string userToken)
        {
            string message = null;
            Build buildUploaded = uploader.UploadBuild(build, userToken, ref message);
            if (buildUploaded != null)
            {
                return new 
                { 
                    success = true,
                    data = ResponseBuild(buildUploaded)
                };
            }
            return Return500Error(message);
        }
        [HttpGet]
        [ActionName("All")]
        public ActionResult<dynamic> All([FromQuery] string userToken, 
        [FromQuery] int from, [FromQuery] int count)
        {
            List<Build> builds = (from build in context.Builds
            join user in context.Users on build.userId equals user.userId
            where user.userToken == userToken 
            && user.activate == true
            && user.deleted == false
            && build.buildDeleted == false
            select build).Skip(from * count)
            .Take(count).ToList();
            Log.Info("Select all builds.");
            return new 
            {
                success = true,
                data = ResponseBuilds(builds)
            };
        }
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult<dynamic> Delete([FromQuery] string hash)
        {
            string message = "";
            Build buildToDelete = context.Builds.Where(build
            => build.buildHash == hash
            && build.buildDeleted == false).FirstOrDefault();
            if (buildToDelete != null)
            {
                buildToDelete.buildDeleted = true;
                context.Builds.Update(buildToDelete);
                context.SaveChanges();
                Log.Info("Delete build, buildId ->" + buildToDelete.buildId + ".");
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
            Build currentBuild = context.Builds.Where(build 
            => build.buildHash == hash
            && build.buildDeleted == false).FirstOrDefault();
            if (currentBuild != null)
            {
                Log.Info("Select build by hash ->" + hash + ".");
                return new 
                {
                    success = true,
                    data = ResponseBuild(currentBuild)
                };
            }
            else
            {
                return Return500Error("Server can't define build.");
            }
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
        public dynamic ResponseBuilds(List<Build> builds)
        {
            if (builds != null)
            {
                List<dynamic> data = new List<dynamic>();
                foreach(Build build in builds)
                {
                    data.Add(ResponseBuild(build));
                }
                return data;
            }
            return null;
        }
        public dynamic ResponseBuild(Build build)
        {
            if (build != null)
            {
                return new 
                {
                    build_id = build.buildId,
                    build_name = build.buildName,
                    archive_name = build.archiveName,
                    build_hash = build.buildHash,
                    url_archive = Config.Domen + build.urlArchive,
                    url_install = Config.Domen + build.urlInstall,
                    url_icon = Config.Domen + build.urlIcon,
                    version = build.version,
                    build_number = build.buildName,
                    bundle_identifier = build.bundleIdentifier,
                    create_at = build.createdAt
                };
            }
            return null;
        }
    }
}