using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Services;

namespace Vabulu.Services.Export {
    public class XlsHandler : IDisposable {

        private readonly ExcelPackage package;
        private readonly BaseHandler baseHandler;
        private string lang;
        public XlsHandler(BaseHandler baseHandler) {
            this.baseHandler = baseHandler;

            this.package = new ExcelPackage();
        }

        public void Dispose() {
            this.package.Dispose();
        }

        private string Trx(string key) {
            return this.baseHandler.I18n.TranslateAsync(this.lang, key).GetAwaiter().GetResult();
        }

        public async Task<Stream> GetExportAsync(string userId) {
            var export = await this.baseHandler.GetJsonAsync(userId);
            this.lang = export?.Language;

            return new MemoryStream(package.GetAsByteArray());
        }
    }
}