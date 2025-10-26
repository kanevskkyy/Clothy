using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Clothy.CatalogService.BLL.Helpers
{
    public class ImageService : IImageService
    {
        private Cloudinary cloudinary;

        public ImageService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            CloudinarySettings settings = cloudinarySettings.Value;
            Account cloudinaryAccount = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret
            );
            cloudinary = new Cloudinary(cloudinaryAccount);
        }

        public async Task<string> UploadAsync(IFormFile file, string folderPath = null)
        {
            Stream fileStream = file.OpenReadStream();

            try
            {
                ImageUploadParams uploadParameters = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, fileStream),
                    Folder = folderPath
                };

                ImageUploadResult uploadResult = await cloudinary.UploadAsync(uploadParameters);
                return uploadResult.SecureUrl.ToString();
            }
            finally
            {
                fileStream.Dispose();
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return;
            }

            try
            {
                string imagePublicId = ExtractPublicIdFromUrl(imageUrl);

                if (string.IsNullOrEmpty(imagePublicId))
                {
                    return;
                }

                DeletionParams deletionParameters = new DeletionParams(imagePublicId);
                DeletionResult deletionResult = await cloudinary.DestroyAsync(deletionParameters);

                if (deletionResult?.Result != "ok")
                {
                    Console.WriteLine($"Warning: Failed to delete image with publicId: {imagePublicId}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error deleting image: {exception.Message}");
            }
        }

        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                Uri imageUri = new Uri(imageUrl);
                string[] urlSegments = imageUri.Segments;
                bool uploadSegmentFound = false;
                List<string> publicIdParts = new List<string>();

                for (int segmentIndex = 0; segmentIndex < urlSegments.Length; segmentIndex++)
                {
                    string currentSegment = urlSegments[segmentIndex].TrimEnd('/');

                    if (currentSegment == "upload")
                    {
                        uploadSegmentFound = true;
                        continue;
                    }

                    if (uploadSegmentFound)
                    {
                        bool isVersionSegment = currentSegment.StartsWith("v") &&
                                               currentSegment.Length > 1 &&
                                               char.IsDigit(currentSegment[1]);

                        if (isVersionSegment)
                        {
                            continue;
                        }
                        publicIdParts.Add(currentSegment);
                    }
                }

                if (publicIdParts.Count == 0)
                {
                    return string.Empty;
                }

                string fullPublicId = string.Join("/", publicIdParts);
                int extensionDotIndex = fullPublicId.LastIndexOf('.');
                if (extensionDotIndex > 0)
                {
                    string publicIdWithoutExtension = fullPublicId.Substring(0, extensionDotIndex);
                    return publicIdWithoutExtension;
                }

                return fullPublicId;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error extracting public ID from URL: {exception.Message}");
                return string.Empty;
            }
        }
    }
}
