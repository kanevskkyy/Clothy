using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class ClothesStockProfile : Profile
    {
        public ClothesStockProfile()
        {
            CreateMap<ClothesStock, SizeReadDTO>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(s => s.Size.Id))
                .ForMember(dto => dto.Name, opt => opt.MapFrom(s => s.Size.Name))
                .ForMember(dto => dto.CreatedAt, opt => opt.MapFrom(s => s.Size.CreatedAt))
                .ForMember(dto => dto.UpdatedAt, opt => opt.MapFrom(s => s.Size.UpdatedAt));

            CreateMap<ClothesStock, ColorReadDTO>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(s => s.Color.Id))
                .ForMember(dto => dto.HexCode, opt => opt.MapFrom(s => s.Color.HexCode))
                .ForMember(dto => dto.CreatedAt, opt => opt.MapFrom(s => s.Color.CreatedAt))
                .ForMember(dto => dto.UpdatedAt, opt => opt.MapFrom(s => s.Color.UpdatedAt));

            CreateMap<ClothesStockCreateDTO, ClothesStock>();

            CreateMap<ClothesStockUpdateDTO, ClothesStock>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<ClothesStock, ClothesStockReadDTO>();
        }
    }
}
