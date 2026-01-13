using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class ClotheProfile : Profile
    {
        public ClotheProfile()
        {
            CreateMap<ClotheItem, ClotheSummaryDTO>()
                .ForMember(dto => dto.AdditionalPhotosCount, map => map.MapFrom(c => c.Photos.Count))
                .ForMember(dto => dto.ColorsCount, map => map.MapFrom(c => c.Stocks
                    .Where(s => s.Quantity > 0) 
                    .Select(s => s.ColorId)
                    .Distinct()
                    .Count()))
                .ForMember(dto => dto.IsAvailable, map => map.MapFrom(c => c.Stocks.Any(s => s.Quantity > 0)))
                .ForMember(dto => dto.Brand, map => map.MapFrom(c => c.Brand))
                .ForMember(dto => dto.Collection, map => map.MapFrom(c => c.Collection))
                .ForMember(dto => dto.ClothyType, map => map.MapFrom(c => c.ClothyType));

            CreateMap<ClotheItem, ClotheDetailDTO>()
                .ForMember(dto => dto.AdditionalPhotos, map => map.MapFrom(c => c.Photos))
                .ForMember(dto => dto.Tags, map => map.MapFrom(c => c.ClotheTags))
                .ForMember(dto => dto.Materials, map => map.MapFrom(c => c.ClotheMaterials))
                .ForMember(dto => dto.Stocks, map => map.MapFrom(c => c.Stocks))
                .ForMember(dto => dto.Brand, map => map.MapFrom(c => c.Brand))
                .ForMember(dto => dto.Collection, map => map.MapFrom(c => c.Collection))
                .ForMember(dto => dto.ClothyType, map => map.MapFrom(c => c.ClothyType));

            CreateMap<ClothesStock, ClotheStockDTO>()
                .ForMember(dest => dest.StockId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))   
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color)) 
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

            CreateMap<PhotoClothes, PhotoReadDTO>()
                .ForMember(dto => dto.Id, map => map.MapFrom(c => c.Id))
                .ForMember(dto => dto.PhotoURL, map => map.MapFrom(c => c.PhotoURL));

            CreateMap<ClotheTag, TagReadDTO>()
                .ForMember(dto => dto.Id, map => map.MapFrom(ct => ct.Tag.Id))
                .ForMember(dto => dto.Name, map => map.MapFrom(ct => ct.Tag.Name))
                .ForMember(dto => dto.CreatedAt, map => map.MapFrom(ct => ct.Tag.CreatedAt));

            CreateMap<ClotheCreateDTO, ClotheItem>()
                .ForMember(dto => dto.Photos, map => map.Ignore())
                .ForMember(dto => dto.ClotheMaterials, map => map.Ignore())
                .ForMember(dto => dto.ClotheTags, map => map.Ignore());

            CreateMap<ClotheMaterial, MaterialWithPercentageDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Material.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Material.Name))
                .ForMember(dest => dest.Percentage, opt => opt.MapFrom(src => src.Percentage));

            CreateMap<ClotheUpdateDTO, ClotheItem>()
                .ForMember(dest => dest.UpdatedAt, map => map.MapFrom(time => DateTime.UtcNow.ToUniversalTime()))
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
                .ForMember(dest => dest.CollectionId, opt => opt.MapFrom(src => src.CollectionId))
                .ForMember(dest => dest.ClothingTypeId, opt => opt.MapFrom(src => src.ClothingTypeId));
        }
    }
}
