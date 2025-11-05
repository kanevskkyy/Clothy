using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using Clothy.CatalogService.Domain.Entities;

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

            CreateMap<KeyValuePair<Material, int>, MaterialWithCountDTO>()
                .ForMember(dest => dest.Id, map => map.MapFrom(pair => pair.Key.Id))
                .ForMember(dest => dest.Name, map => map.MapFrom(pair => pair.Key.Name))
                .ForMember(dest => dest.ClotheItemCount, map => map.MapFrom(pair => pair.Value));
        }
    }
}
