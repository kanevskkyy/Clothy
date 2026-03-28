using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class MaterialProfile : Profile
    {
        public MaterialProfile()
        {
            CreateMap<Material, MaterialReadDTO>();

            CreateMap<MaterialCreateDTO, Material>();

            CreateMap<MaterialUpdateDTO, Material>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
