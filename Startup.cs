using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Vabulu.Middleware;
using Vabulu.Models;

namespace Vabulu {
    public class Startup {
        public Startup(IHostingEnvironment env) {

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new Middleware.FuzzyPropertyNameMatchingConverter() }
            };

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional : false, reloadOnChange : true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional : true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddIdentity<User, IdentityRole>
                (options => {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequiredUniqueChars = 2;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    options.User.RequireUniqueEmail = true;
                })
                .AddDefaultTokenProviders();

            services.AddNodeServices();

            services.AddCustomStores(
                Configuration.GetConnectionString("Store"),
                Configuration.GetValue<string>("Store:TablePrefix"),
                Configuration.GetValue<string>("Store:ImageContainer")
            );

            services.AddTransient<Services.TemplateService>();

            services.AddTransient<Services.MailService>()
                .Configure<Services.MailServiceOptions>(Configuration.GetSection("MailService"));

            services.AddTransient<Services.WarmUp>()
                .Configure<Services.WarmUpOptions>(Configuration.GetSection("WarmUpService"));

            services.AddTransient<Services.Export.BaseHandler>()
                .AddTransient<Services.Export.HtmlHandler>()
                .AddTransient<Services.Export.XlsHandler>();

            services.AddTransient<Services.I18n.TranslationService>();

            services.AddTransient<Services.ImageService>();

            services.AddAuthentication()
                .AddGoogle(option => {
                    option.ClientId = Configuration["Authentication:Google:ClientId"];
                    option.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                    option.CallbackPath = "/.auth/signin/google/callback";
                    option.AccessType = "offline";
                    option.SaveTokens = true;
                })
                .AddMicrosoftAccount(option => {
                    option.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    option.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                    option.CallbackPath = "/.auth/signin/microsoft/callback";
                    option.SaveTokens = true;
                });

            services.ConfigureApplicationCookie(options => {
                options.LoginPath = "/.auth/error/401";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.AccessDeniedPath = "/.auth/error/403";
                options.LogoutPath = "/.auth/signout";
                options.ReturnUrlParameter = "";
                options.SlidingExpiration = true;
                options.Cookie.Name = "auth";
                options.Cookie.Expiration = TimeSpan.FromMinutes(15);
            });

            services
                .AddMvc()
                .AddJsonOptions(options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.Converters.Add(new Middleware.FuzzyPropertyNameMatchingConverter());
                });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new Info { Title = "Vabulu", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Services.WarmUp warmUp) {
            if (Configuration.GetValue<bool>("StaticFileHost:Enabled")) {
                var rootDirectory = Path.Combine(env.ContentRootPath, Configuration.GetValue<string>("StaticFileHost:RootDirectory"));
                app.UseCustomStaticFiles(rootDirectory);
            }

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vabulu");
            });

            app.UseAuthentication()
                .UseMvc();

            warmUp.Load();
        }
    }
}