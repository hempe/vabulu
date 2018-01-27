using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Vabulu.Models;

namespace Vabulu.Services.I18n {
    public class TranslationService {

        private static Dictionary<string, object> loaded = new Dictionary<string, object>();
        public async Task<object> GetTranslationsAsync(string lang) {
            return await this.LoadAsync(lang);
        }

        public async Task<string> TranslateAsync(string lang, string key) {
            var t = await this.InternalTranslateAsync(lang, key);
            if (string.IsNullOrWhiteSpace(t))
                return $"###{key}###";
            return t;
        }

        private async Task<string> InternalTranslateAsync(string lang, string key) {
            if (string.IsNullOrWhiteSpace(lang))
                lang = "en";
            var trans = await this.LoadAsync(lang) ?? await this.LoadAsync("en");
            if (TryGetValue(trans, key.Split('.'), out var t1))
                return t1;
            if (TryGetValue(await this.LoadAsync("en"), key.Split('.'), out var t2))
                return t2;
            return null;
        }

        private bool TryGetValue(object trx, string[] key, out string value) {
            try {
                if (key.Length == 0) {
                    value = null;
                    return false;
                }
                if (key.Length == 1) {
                    dynamic dyn = trx;
                    value = dyn[key[0]];
                    return true;
                } else {
                    dynamic dyn = trx;
                    return TryGetValue(dyn[key[0]], key.Skip(1).ToArray(), out value);
                }

            } catch {
                value = null;
                return false;
            }
        }

        private async Task<Object> LoadAsync(string lang) {
            try {
                if (loaded.ContainsKey(lang))
                    return loaded[lang];
            } catch { }

            var stream = this.GetType().Assembly.GetManifestResourceStream($"Vabulu.Services.I18n.{lang}.json");
            if (stream == null) {
                if (lang != "en")
                    return await LoadAsync("en");
                else
                    throw new NotSupportedException($"Language {lang} is not supported and no fallback was found");
            }

            using(var reader = new StreamReader(stream, Encoding.UTF8)) {
                var content = await reader.ReadToEndAsync();
                var dict = JsonConvert.DeserializeObject(content);
                try {
                    loaded[lang] = dict;
                } catch {
                    loaded[lang] = dict;
                }
                return dict;
            }
        }
    }
}