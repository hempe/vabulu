using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Services;

namespace Vabulu.Services.Export
{
    public class HtmlHandler : IDisposable
    {
        private readonly BaseHandler baseHandler;
        private readonly TemplateService templateService;
        public HtmlHandler(BaseHandler baseHandler, TemplateService templateService)
        {
            this.baseHandler = baseHandler;
            this.templateService = templateService;
        }

        public void Dispose()
        {

        }

        public async Task<Stream> GetExportAsync(string userId)
        {
            var data = await this.baseHandler.GetJsonAsync(userId);
            var content = await this.templateService.LoadTemplateAsync("Templates/Export.html.template");
            var result = this.templateService.Render(content, data, data?.Language);
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

    }
}