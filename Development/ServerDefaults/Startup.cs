﻿using Common;
using Microsoft.AspNetCore.Mvc;
using YobiWi.Development.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Common
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }        
        public IConfiguration Configuration { get; }
        public readonly string AllowSpecificOrigins = "AllowSpecificOrigins";


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(AllowSpecificOrigins,
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                    //.AllowCredentials();
                });
            });
            services.AddDbContext<YobiWiContext>((serviceProvider, options)
            => { options.UseMySql(Config.GetDatabaseConfigConnection()); }, ServiceLifetime.Transient);
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory(AllowSpecificOrigins));
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            if (Program.requestView)
            {
                app.Use(next 
                => context 
                => 
                { 
                    context.Request.EnableRewind(); 
                    return next(context); 
                });
                app.UseMiddleware<NRequestWatchingSteamMiddleware.RequestWatchingSteamMiddleware>();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors(AllowSpecificOrigins);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}