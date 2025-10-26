using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class ClothesStockProfile : Profile
    {
        public ClothesStockProfile()
        {
            CreateMap<ClothesStockCreateDTO, ClothesStock>();

            CreateMap<ClothesStockUpdateDTO, ClothesStock>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<ClothesStock, ClothesStockReadDTO>();
        }
    }
}
