// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Script.WebHost.Middleware;

namespace Microsoft.Azure.WebJobs.Script.WebHost
{
    public static class WebJobsApplicationBuilderExtension
    {
        public static IApplicationBuilder UseWebJobsScriptHost(this IApplicationBuilder builder, IApplicationLifetime applicationLifetime)
        {
            return UseWebJobsScriptHost(builder, applicationLifetime, null);
        }

        public static IApplicationBuilder UseWebJobsScriptHost(this IApplicationBuilder builder, IApplicationLifetime applicationLifetime, Action<WebJobsRouteBuilder> routes)
        {
            WebScriptHostManager hostManager = builder.ApplicationServices.GetService(typeof(WebScriptHostManager)) as WebScriptHostManager;

            builder.UseHttpBindingRouting(applicationLifetime, routes);

            // Only run this if it's not an admin requests.
            // All admin routes need to expect and handle the ScriptHost not running.
            builder.UseWhen(context => !context.Request.Path.StartsWithSegments("/admin"), config =>
            {
                config.UseMiddleware<ScriptHostCheckMiddleware>();
            });

            // Register /admin/vfs, and /admin/zip to the VirtualFileSystem middleware.
            builder.UseWhen(VirtualFileSystemMiddleware.IsVirtualFileSystemRequest, config => config.UseMiddleware<VirtualFileSystemMiddleware>());

            builder.UseMvc(r =>
            {
                r.MapRoute(name: "Home",
                    template: string.Empty,
                    defaults: new { controller = "Home", action = "Get" });
            });

            return builder;
        }
    }
}