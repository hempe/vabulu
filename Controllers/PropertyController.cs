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

    public class ImageUrls {
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }

    }

    [Route("api/property")]
    [Authorize]
    public class PropertyController : BaseController {
        private readonly StoreOption storageOptions;
        public PropertyController(UserManager<User> userManager, TableStore tableStore, IOptions<StoreOption> storageOptions) : base(userManager, tableStore) {
            this.storageOptions = storageOptions.Value;
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(Property[]), 200)]
        public async Task<IActionResult> Get() {
            var values = await this.TableStore.GetAllAsync<Tables.Property>(new Args { });
            return this.Ok(values.Select(x =>(Property) x));
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(CalendarEvent), 200)]
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
            await this.TableStore.DeleteAsync(new Tables.Property { Id = propertyId });
            return this.Ok();
        }

        [HttpGet("{propertyId}/images")]
        [ProducesResponseType(typeof(ImageUrls[]), 200)]
        public async Task<IActionResult> GetImages([FromRoute] string propertyId) {
            var folderBlob = this.GetBlobReference(propertyId);
            var segement = await folderBlob.ListBlobsSegmentedAsync(default);

            var dict = new Dictionary<string, ImageUrls>();
            foreach (var link in segement.Results.Select(x => x.Uri.ToString())) {
                if (link.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase)) {
                    var key = link.Split("/").Reverse().First().Split(".").First();
                    if (dict.TryGetValue(key, out var img)) {
                        this.UpdateImage(img, link);
                    } else {
                        dict[key] = this.UpdateImage(new ImageUrls(), link);
                    }
                } else {
                    var key = link.Split("/").Reverse().First();
                    if (dict.TryGetValue(key, out var img)) {
                        this.UpdateImage(img, link);
                    } else {
                        dict[key] = this.UpdateImage(new ImageUrls(), link);
                    }
                }
            }
            return this.Ok(dict.Values);
        }

        private ImageUrls UpdateImage(ImageUrls img, string link) {
            if (link.EndsWith(".thumbnail.png")) {
                img.ThumbnailUrl = link;
            } else {
                if (string.IsNullOrWhiteSpace(img.ThumbnailUrl))
                    img.ThumbnailUrl = link;
                img.Url = link;
            }
            return img;
        }

        [HttpDelete("{propertyId}/images/{imageId}")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> DeleteImage([FromRoute] string propertyId, [FromRoute] string imageId) {
            var folderBlob = this.GetBlobReference(propertyId);
            var blockBlob = folderBlob.GetBlockBlobReference(imageId);
            await blockBlob.DeleteAsync();
            return this.Ok();
        }

        [HttpPost("{propertyId}/images/upload")]
        [HttpPut("{propertyId}/images/upload")]
        public async Task<IActionResult> UploadImages([FromRoute] string propertyId) {
            var boundary = GetBoundary(Request.ContentType);

            if (boundary == null) {
                if (await UploadFileToStorage(Request.Body, propertyId, Guid.NewGuid().ToString())) {
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
                if (await UploadFileToStorage(stream, propertyId, Guid.NewGuid().ToString())) {
                    return this.Ok();
                }
                return this.BadRequest();
            }

        }

        private CloudBlobDirectory GetBlobReference(string directory) {
            var storageAccount = CloudStorageAccount.Parse(this.storageOptions.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageOptions.ImageContainer);
            return container.GetDirectoryReference(directory);
        }

        private async Task<bool> UploadFileToStorage(Stream fileStream, string folder, string fileName) {
            try {

                using(var copy = new MemoryStream()) {
                    await fileStream.CopyToAsync(copy);
                    copy.Position = 0;

                    using(var stream = new MemoryStream())
                    using(var image = Image.Load(copy)) {
                        var aspect = Math.Min(
                            Math.Min((double) image.Width, 1920.0) / image.Width,
                            Math.Min((double) image.Height, 1080.0) / image.Height);

                        using(Image<Rgba32> cropped = image.Clone(x => x.Resize(new ResizeOptions {
                            Size = new Size((int) (image.Width * aspect), (int) (image.Height * aspect)),
                                Mode = ResizeMode.Stretch
                        }))) {

                            cropped.Save(stream, new PngEncoder());
                            stream.Position = 0;
                            var folderBlob = this.GetBlobReference(folder);
                            var blockBlob = folderBlob.GetBlockBlobReference($"{fileName}.png");

                            await blockBlob.UploadFromStreamAsync(stream);
                        }
                    }

                    copy.Position = 0;
                    using(var stream = new MemoryStream())
                    using(var image = Image.Load(copy)) {
                        var aspect = Math.Min(
                            Math.Min((double) image.Width, 100.0) / image.Width,
                            Math.Min((double) image.Height, 100.0) / image.Height);

                        using(Image<Rgba32> cropped = image.Clone(x => x.Resize(new ResizeOptions {
                            Size = new Size((int) (image.Width * aspect), (int) (image.Height * aspect)),
                                Mode = ResizeMode.Stretch
                        }))) {

                            cropped.Save(stream, new PngEncoder());
                            stream.Position = 0;
                            var folderBlob = this.GetBlobReference(folder);
                            var blockBlob = folderBlob.GetBlockBlobReference($"{fileName}.thumbnail.png");

                            await blockBlob.UploadFromStreamAsync(stream);
                        }
                    }
                }
            } catch (Exception e) {
                System.Console.WriteLine(e.Message);
                return await Task.FromResult(false);
            }
            return await Task.FromResult(true);

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