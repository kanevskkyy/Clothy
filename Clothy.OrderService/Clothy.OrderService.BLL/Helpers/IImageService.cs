using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Clothy.OrderService.BLL.Helpers
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file, string folderPath = null);
        Task DeleteImageAsync(string publicId);
    }
}
