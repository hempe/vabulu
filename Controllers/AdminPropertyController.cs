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

namespace Vabulu.Controllers {

    [Route("api/admin/property")]
    [Authorize(Roles = "edit, admin")]
    public class AdminPropertyController : BaseController {
        private readonly ImageService imageService;
        public AdminPropertyController(UserManager<User> userManager, TableStore tableStore, ImageService imageService) : base(userManager, tableStore) {
            this.imageService = imageService;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(Property[]), 200)]
        public async Task<IActionResult> GetAll() {
            var values = await this.TableStore.GetAllAsync<Tables.Property>();
            return this.Ok(values.Select(x =>(Property) x));
        }

        [HttpGet("{propertyId}")]
        [ProducesResponseType(typeof(Property), 200)]
        public async Task<IActionResult> Get([FromRoute] string propertyId) {
            Property value = await this.TableStore.GetAsync(new Tables.Property { Id = propertyId });
            return this.Ok(value ?? new Property { Id = propertyId });
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(Property), 200)]
        public async Task<IActionResult> Post([FromBody] Property data) {
            if (data == null)
                return this.BadRequest("Failed to save data.");

            if (string.IsNullOrWhiteSpace(data.Id))
                data.Id = Guid.NewGuid().ToString();

            var result = await this.TableStore.AddOrUpdateAsync<Tables.Property>(data);
            if (result.Success())
                return this.Ok(data);
            return this.BadRequest("Failed to save data.");
        }

        [HttpDelete("{propertyId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string propertyId) {
            var images = await this.imageService.GetImagesAsync(propertyId);
            foreach (var img in images) {
                try {
                    await this.imageService.DeleteImageAsync(propertyId, img.Url);
                } catch { }
                try {
                    await this.imageService.DeleteImageAsync(propertyId, img.ThumbnailUrl);
                } catch { }
            }

            await this.TableStore.DeleteAsync(new Tables.Property { Id = propertyId });
            return this.Ok();
        }

        [HttpGet("{propertyId}/images")]
        [ProducesResponseType(typeof(ImageUrls[]), 200)]
        public async Task<IActionResult> GetImages([FromRoute] string propertyId) {
            return this.Ok(await this.imageService.GetImagesAsync(propertyId));
        }

        [HttpDelete("{propertyId}/images/{imageId}")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> DeleteImage([FromRoute] string propertyId, [FromRoute] string imageId) {
            await this.imageService.DeleteImageAsync(propertyId, imageId);
            return this.Ok();
        }

        [HttpPost("{propertyId}/images/upload")]
        [HttpPut("{propertyId}/images/upload")]
        public async Task<IActionResult> UploadImages([FromRoute] string propertyId) {
            var boundary = GetBoundary(Request.ContentType);

            if (boundary == null) {
                if (await this.imageService.UploadFileToStorageAsync(Request.Body, propertyId, Guid.NewGuid().ToString())) {
                    return this.Ok();
                }
                return this.BadRequest();
            }

            var reader = new MultipartReader(boundary, Request.Body, 80 * 1024);
            MultipartSection section;

            using(Stream stream = new MemoryStream()) {
                while ((section = await reader.ReadNextSectionAsync()) != null) {
                    var contentDispo = section.GetContentDispositionHeader();

                    if (contentDispo.IsFileDisposition()) {
                        var fileSection = section.AsFileSection();
                        var bufferSize = 32 * 1024;
                        byte[] buffer = new byte[bufferSize];

                        if (stream.Position != 0)
                            return BadRequest("Only one file is accepted per request.");

                        await fileSection.FileStream.CopyToAsync(stream);
                    } else if (contentDispo.IsFormDisposition()) {
                        return BadRequest("Only one file is accepted per request.");
                    } else {
                        return BadRequest("Malformatted message body.");
                    }
                }

                if (stream == null)
                    return BadRequest("No file submitted.");

                stream.Seek(0, SeekOrigin.Begin);
                if (await this.imageService.UploadFileToStorageAsync(stream, propertyId, Guid.NewGuid().ToString())) {
                    return this.Ok();
                }
                return this.BadRequest();
            }
        }

        private static string GetBoundary(string contentType) {
            if (contentType == null)
                return null;

            var elements = contentType.Split(' ');
            var element = elements.FirstOrDefault(entry => entry.StartsWith("boundary="));
            if (element == null)
                return null;

            var boundary = element.Substring("boundary=".Length);

            var segment = HeaderUtilities.RemoveQuotes(boundary);
            boundary = segment.HasValue ? segment.Value : string.Empty;
            return boundary;
        }
    }
}