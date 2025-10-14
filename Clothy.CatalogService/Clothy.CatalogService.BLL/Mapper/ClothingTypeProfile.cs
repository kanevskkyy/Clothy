using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class ClothingTypeProfile : Profile
    {
        public ClothingTypeProfile()
        {
            CreateMap<ClothingType, ClothingTypeReadDTO>();

            CreateMap<ClothingTypeCreateDTO, ClothingType>();

            CreateMap<ClothingTypeUpdateDTO, ClothingType>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
