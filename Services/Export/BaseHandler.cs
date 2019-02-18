using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vabulu.Models;

namespace Vabulu.Services.Export
{
    public class ExportData
    {
        public string Language { get; set; }
    }

    public class BaseHandler
    {
        internal TableStore TableStore { get; private set; }
        internal I18n.TranslationService I18n { get; private set; }

        public BaseHandler(TableStore TableStore, I18n.TranslationService i18n)
        {
            this.TableStore = TableStore;
            this.I18n = i18n;
        }

        public async Task<Stream> GetExportAsync(string userId)
        {
            var data = await this.GetJsonAsync(userId);
            return new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
        }

        public async Task<ExportData> GetJsonAsync(string userId)
        {
            await Task.CompletedTask;
            return new ExportData
            {
                Language = "en"
            };
        }
    }
}