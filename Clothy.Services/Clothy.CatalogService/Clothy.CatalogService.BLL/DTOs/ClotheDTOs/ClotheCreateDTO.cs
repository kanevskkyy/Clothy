using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.BLL.Helpers.ModelBinder;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.DTOs.ClotheDTOs
{
    public class ClotheCreateDTO
    {
        public string? Name { get; set; } 
        public string? Slug { get; set; } 
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public Gender Gender { get; set; }

        public Guid BrandId { get; set; }
        public Guid ClothingTypeId { get; set; }
        public Guid CollectionId { get; set; }


        [ModelBinder(BinderType = typeof(ClothePhotosModelBinder))]
        public List<ClothePhotoCreateDTO> AdditionalPhotos { get; set; } = new();

        public List<Guid> TagIds { get; set; } = new();

        [ModelBinder(BinderType = typeof(JsonFormModelBinder<List<ClotheMaterialCreateDTO>>))]
        public List<ClotheMaterialCreateDTO> Materials { get; set; } = new();
    }
}