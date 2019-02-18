using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Vabulu.Services.I18n;

namespace Vabulu.Controllers
{
    [Route("api/i18n")]
    public class TranslateController : Controller
    {
        public TranslateController() { }

        [HttpGet("{lang}")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Get([FromServices] TranslationService translationService, [FromRoute] string lang)
        {
            var translations = await translationService.GetTranslationsAsync(lang) ?? new object();
            return this.Ok(translations);
        }

        [HttpGet("{lang}/{key}")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> Translate([FromServices] TranslationService translationService, [FromRoute] string lang, [FromRoute] string key)
        {
            return this.Ok(await translationService.TranslateAsync(lang, key));
        }
    }
}