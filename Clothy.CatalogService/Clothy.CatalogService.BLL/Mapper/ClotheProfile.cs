using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
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
                .ForMember(dto => dto.AdditionalColorsCount, map => map.MapFrom(c => c.Stocks.Count))
                .ForMember(dto => dto.IsAvailable, map => map.MapFrom(c => c.Stocks.Any(s => s.Quantity > 0)))
                .ForMember(dto => dto.Brand, map => map.MapFrom(c => c.Brand))
                .ForMember(dto => dto.Collection, map => map.MapFrom(c => c.Collection))
                .ForMember(dto => dto.ClothyType, map => map.MapFrom(c => c.ClothyType));

            CreateMap<ClotheItem, ClotheDetailDTO>()
                .ForMember(dto => dto.AdditionalPhotos, map => map.MapFrom(c => c.Photos))
                .ForMember(dto => dto.Tags, map => map.MapFrom(c => c.ClotheTags))
                .ForMember(dto => dto.Materials, map => map.MapFrom(c => c.ClotheMaterials))
                .ForMember(dto => dto.Colors, map => map.MapFrom(c => c.Stocks))
                .ForMember(dto => dto.Brand, map => map.MapFrom(c => c.Brand))
                .ForMember(dto => dto.Collection, map => map.MapFrom(c => c.Collection))
                .ForMember(dto => dto.ClothyType, map => map.MapFrom(c => c.ClothyType));

            CreateMap<ClothesStock, ColorReadDTO>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(s => s.Color.Id))
                .ForMember(dto => dto.HexCode, opt => opt.MapFrom(s => s.Color.HexCode))
                .ForMember(dto => dto.CreatedAt, opt => opt.MapFrom(s => s.Color.CreatedAt))
                .ForMember(dto => dto.UpdatedAt, opt => opt.MapFrom(s => s.Color.UpdatedAt));

            CreateMap<PhotoClothes, PhotoReadDTO>()
                .ForMember(dto => dto.Id, map => map.MapFrom(c => c.Id))
                .ForMember(dto => dto.PhotoURL, map => map.MapFrom(c => c.PhotoURL));

            CreateMap<ClotheMaterial, MaterialWithPercentageDTO>()
                .ForMember(dto => dto.Name, map => map.MapFrom(cm => cm.Material.Name))
                .ForMember(dto => dto.Percentage, map => map.MapFrom(cm => cm.Percentage));

            CreateMap<ClotheTag, TagReadDTO>()
                .ForMember(dto => dto.Id, map => map.MapFrom(ct => ct.Tag.Id))
                .ForMember(dto => dto.Name, map => map.MapFrom(ct => ct.Tag.Name));

            CreateMap<ClotheCreateDTO, ClotheItem>()
                .ForMember(dto => dto.Photos, map => map.Ignore())
                .ForMember(dto => dto.ClotheMaterials, map => map.Ignore())
                .ForMember(dto => dto.ClotheTags, map => map.Ignore());

            CreateMap<ClotheUpdateDTO, ClotheItem>()
                .ForMember(dto => dto.Photos, map => map.Ignore())
                .ForMember(dto => dto.UpdatedAt, map => DateTime.UtcNow.ToUniversalTime())
                .ForMember(dto => dto.ClotheMaterials, map => map.Ignore())
                .ForMember(dto => dto.ClotheTags, map => map.Ignore());
        }
    }
}
