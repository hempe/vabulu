using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Vabulu.Services
{
    public class TemplateService
    {
        I18n.TranslationService I18n;
        private string language;
        private object data;
        public TemplateService(I18n.TranslationService translationService)
        {
            I18n = translationService;
        }

        public string Render(string template, object data, string language = null)
        {
            this.language = language;
            this.data = data;

            var parsed = Template.Parse(template);
            var context = new TemplateContext()
            {
                TryGetMember = this.TryGetMember,
                TryGetVariable = this.TryGetVariable,
                MemberFilter = x => false,
                MemberRenamer = x => ToCamelCase(x.Name),
            };

            return parsed.Render(context);
        }

        public async Task<string> LoadTemplateAsync(string path)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }

            var resource = $"Vabulu.{string.Join(".", path.Split("/"))}";
            var stream = this.GetType().Assembly.GetManifestResourceStream(resource);
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }

        private bool TryGetMember(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = GetValue(target, member);
            return true;
        }

        private bool TryGetVariable(TemplateContext context, SourceSpan span, ScriptVariable variable, out object value)
        {
            try
            {
                switch (variable.Name)
                {
                    case "default":
                        value = new Default();
                        return true;
                    case "translate":
                        value = new Translate(this.I18n, this.language);
                        return true;
                    case "numberWithSeperator":
                        value = new NumberWithSeperator();
                        return true;
                    default:
                        value = GetValue(this.data, variable.Name);
                        return true;
                }
            }
            catch
            {
                value = null;
                return false;
            }
        }

        private static object GetValue(object obj, string memeber)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();
            var prop = type.GetProperties().Where(p => string.Equals(p.Name, memeber, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (prop != null)
                return prop.GetValue(obj);

            var field = type.GetFields().Where(p => string.Equals(p.Name, memeber, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (field != null)
                return field.GetValue(obj);
            return null;
        }

        private class Default : IScriptCustomFunction
        {
            object IScriptCustomFunction.Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var name = arguments[0].ToString();
                try
                {
                    var type = Type.GetType($"{name}", false, true) ??
                        Type.GetType($"System.{name}", false, true) ??
                        Type.GetType($"Vabulu.{name}", false, true) ??
                        AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(t => t.GetTypes())
                        .FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.InvariantCultureIgnoreCase));

                    if (type == null)
                        return null;
                    if (type.IsValueType)
                        return Activator.CreateInstance(type);

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        private class NumberWithSeperator : IScriptCustomFunction
        {
            object IScriptCustomFunction.Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var seperator = "'";
                var str = arguments[0].ToString();
                if (decimal.TryParse(str, out var dec))
                {
                    return dec.ToString("#,##0.00").Replace(",", seperator);
                }
                return string.Empty;
            }
        }

        private class Translate : IScriptCustomFunction
        {
            private readonly I18n.TranslationService i18n;
            private readonly string lang;

            public Translate(I18n.TranslationService i18n, string lang)
            {
                this.i18n = i18n;
                this.lang = lang;
            }

            object IScriptCustomFunction.Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
            {
                var key = arguments[0].ToString();
                return this.i18n.TranslateAsync(this.lang, key).GetAwaiter().GetResult();
            }
        }
        private static string ToCamelCase(string str) => (str == null || str.Length < 2) ? str : Char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}