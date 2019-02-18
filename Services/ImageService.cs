using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using Vabulu.Models;

namespace Vabulu.Services {
    public class ImageService {
        private readonly StoreOption storageOptions;
        public ImageService(IOptions<StoreOption> storageOptions) {
            this.storageOptions = storageOptions.Value;
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

        public CloudBlobDirectory GetBlobReference(string directory) {
            var storageAccount = CloudStorageAccount.Parse(this.storageOptions.ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageOptions.ImageContainer);
            return container.GetDirectoryReference(directory);
        }

        public async Task DeleteImageAsync(string propertyId, string imageId) {
            var folderBlob = this.GetBlobReference(propertyId);
            var blockBlob = folderBlob.GetBlockBlobReference(imageId);
            await blockBlob.DeleteAsync();
        }

        public async Task<IEnumerable<ImageUrls>> GetImagesAsync(string propertyId) {
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
            return dict.Values;
        }
        public async Task<bool> UploadFileToStorageAsync(Stream fileStream, string folder, string fileName) {
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
    }
}