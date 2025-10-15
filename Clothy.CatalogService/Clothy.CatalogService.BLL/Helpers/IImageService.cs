using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.Helpers
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folderPath = null);
        Task DeleteImageAsync(string publicId);
    }
}
