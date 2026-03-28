using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.Helpers.ModelBinder
{
    public class ClothePhotosModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null) throw new ArgumentNullException(nameof(bindingContext));

            var form = await bindingContext.HttpContext.Request.ReadFormAsync();
            List<ClothePhotoCreateDTO> photos = new List<ClothePhotoCreateDTO>();

            var indices = form.Keys
                .Where(k => k.StartsWith($"{bindingContext.ModelName}["))
                .Select(k => int.Parse(k.Split('[', ']')[1]))
                .Distinct()
                .OrderBy(i => i)
                .ToList();

            foreach (int index in indices)
            {
                ClothePhotoCreateDTO photo = new ClothePhotoCreateDTO();

                string fileKey = $"{bindingContext.ModelName}[{index}].Photo";
                if (form.Files.GetFile(fileKey) is IFormFile file)
                {
                    photo.Photo = file;
                }

                string colorIdKey = $"{bindingContext.ModelName}[{index}].ColorId";
                if (form.TryGetValue(colorIdKey, out var colorIdValue) && Guid.TryParse(colorIdValue, out Guid colorId)) photo.ColorId = colorId;

                string isMainKey = $"{bindingContext.ModelName}[{index}].IsMain";
                if (form.TryGetValue(isMainKey, out var isMainValue) && bool.TryParse(isMainValue, out bool isMain)) photo.IsMain = isMain;

                photos.Add(photo);
            }

            bindingContext.Result = ModelBindingResult.Success(photos);
        }
    }
}
