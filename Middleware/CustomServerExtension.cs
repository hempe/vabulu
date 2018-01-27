using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace Vabulu.Middleware {

    /// <summary>
    /// Application builder extensions.
    /// </summary>
    public static class CustomServerExtension {
        private static readonly string[] UncachedFiles = new [] {
            "/config.js",
            "/config.json",
            "/index.html",
            "/loader.js",
            "/silent-signin-callback.html"
        };

        /// <summary>
        /// Use custom static files hosting.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <returns>The updated application builder</returns>
        public static IApplicationBuilder UseCustomStaticFiles(this IApplicationBuilder app, string rootDirectory) {
            var options = new StaticFileOptions() {
                FileProvider = new PhysicalFileProvider(rootDirectory),
                    ServeUnknownFileTypes = true,
                    OnPrepareResponse = (context) => {
                        var path = context.Context.Request.Path.Value.ToLower();
                        if (UncachedFiles.Contains(path)) {
                            var headers = context.Context.Response.Headers;
                            headers.Append("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1
                            headers.Append("Pragma", "no-cache"); // HTTP 1.0
                            headers.Append("Expires", "0"); // Proxies
                        }
                    }
            };

            app.UseStaticFiles(options);

            // If the StaticFiles middleware finds the file, it will unwind the pipeline to the beginning,
            // and this part of it is never called. However, if we DO reach this point, it means the file
            // did not exist and we can check whether we have to do HTML5 shenanigans.
            app.MapWhen(context =>
                !context.Request.Path.StartsWithSegments("/api") &&
                !context.Request.Path.StartsWithSegments("/.auth") &&
                !context.Request.Path.StartsWithSegments("/swagger") &&
                !Path.HasExtension(context.Request.Path), innerApp => {
                    innerApp.Use((ctx, next) => {
                        ctx.Request.Path = "/index.html";
                        ctx.Response.StatusCode = 200;

                        return next();
                    });

                    innerApp.UseStaticFiles(options);
                });

            return app;
        }
    }
}