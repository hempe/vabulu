using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Vabulu {
    public class Program {
        public static void Main(string[] args) {

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new Middleware.FuzzyPropertyNameMatchingConverter() }
            };

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseContentRoot(Environment.GetEnvironmentVariable("ASPNETCORE_CONTENTROOTPATH") ?? Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .Build();
    }
}