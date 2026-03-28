using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.Shared.Helpers.CloudinaryConfig.ImageService
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folderPath = null, bool removeBackground = false);
        Task DeleteImageAsync(string publicId);
    }
}
