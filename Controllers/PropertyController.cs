using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Services;

namespace Vabulu.Controllers
{
    [Route("api/property")]
    [Authorize(Roles = "user, edit, admin")]
    public class PropertyController : BaseController
    {
        private readonly ImageService imageService;
        public PropertyController(UserManager<User> userManager, TableStore tableStore, ImageService imageService) : base(userManager, tableStore)
        {
            this.imageService = imageService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(Property[]), 200)]
        public async Task<IActionResult> GetAll()
        {
            var values = await this.TableStore.GetAllAsync<Tables.Property>();
            return this.Ok(values.Select(x => (Property)x));
        }

        [HttpGet("{propertyId}")]
        [ProducesResponseType(typeof(Property), 200)]
        public async Task<IActionResult> Get([FromRoute] string propertyId)
        {
            Property value = await this.TableStore.GetAsync(new Tables.Property { Id = propertyId });
            return this.Ok(value ?? new Property { Id = propertyId });
        }

        [HttpGet("{propertyId}/images")]
        [ProducesResponseType(typeof(ImageUrls[]), 200)]
        public async Task<IActionResult> GetImages([FromRoute] string propertyId)
        {
            return this.Ok(await this.imageService.GetImagesAsync(propertyId));
        }
    }
}