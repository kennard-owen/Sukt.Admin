﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using SuktCore.AutoMapper;
using Sukt.Core.Domain.Models;
using Sukt.Core.IdentityServerFourStore;
using SuktCore.Shared.AppOption;
using SuktCore.Shared.Events;
using SuktCore.Shared.Extensions;
using SuktCore.Shared.Modules;
using SuktCore.Shared.SuktDependencyAppModule;
using System;
using System.Linq;
using System.Security.Principal;

namespace Sukt.Core.AuthenticationCenter.Startups
{
    [SuktDependsOn(
        typeof(IdentityModule),
        typeof(DependencyAppModule),
        typeof(EventBusAppModuleBase),
        typeof(AuthenticationCenterEntityFrameworkCoreModule),
        typeof(SuktAutoMapperModuleBase),
        typeof(IdentityServer4Module),
        typeof(MigrationModuleBase)
    )]
    public class SuktAppWebModule : SuktAppModule
    {
        private string _corePolicyName = string.Empty;
        public override void ApplicationInitialization(ApplicationContext context)
        {
            var app = context.GetApplicationBuilder();
            if (!_corePolicyName.IsNullOrEmpty())
            {
                app.UseCors(_corePolicyName); //添加跨域中间件
            }
            app.UseStaticFiles();
            app.UseRouting();
            if (!_corePolicyName.IsNullOrEmpty())
            {
                app.UseCors(_corePolicyName); //添加跨域中间件
            }
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public override void ConfigureServices(ConfigureServicesContext context)
        {
            var service = context.Services;
#if DEBUG
            service.AddRazorPages().AddRazorRuntimeCompilation();//判断是否是开发环境
#else
service.AddMvc();
#endif
            context.Services.AddTransient<IPrincipal>(provider =>
            {
                IHttpContextAccessor accessor = provider.GetService<IHttpContextAccessor>();
                return accessor?.HttpContext?.User;
            });
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath; //获取项目路径
            context.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(basePath));
            var configuration = service.GetConfiguration();
            service.Configure<AppOptionSettings>(configuration.GetSection("SuktCore"));
            var settings = service.GetAppSettings();
            if (!settings.Cors.PolicyName.IsNullOrEmpty() && !settings.Cors.Url.IsNullOrEmpty()) //添加跨域
            {
                _corePolicyName = settings.Cors.PolicyName;
                service.AddCors(c =>
                {
                    c.AddPolicy(settings.Cors.PolicyName, policy =>
                    {
                        policy.WithOrigins(settings.Cors.Url
                          .Split(",", StringSplitOptions.RemoveEmptyEntries).ToArray())
                        //policy.WithOrigins("http://localhost:5001")//支持多个域名端口，注意端口号后不要带/斜杆：比如localhost:8000/，是错的
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();//允许cookie;
                    });
                });
            }
        }
    }
}
