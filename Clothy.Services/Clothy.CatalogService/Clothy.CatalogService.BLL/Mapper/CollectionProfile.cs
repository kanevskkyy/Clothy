using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class CollectionProfile : Profile
    {
        public CollectionProfile()
        {
            CreateMap<Collection, CollectionReadDTO>();
            CreateMap<CollectionCreateDTO, Collection>();
            CreateMap<CollectionUpdateDTO, Collection>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime())); ;

            CreateMap<KeyValuePair<Collection, int>, CollectionWithCountDTO>()
                .ForMember(entity => entity.Id, map => map.MapFrom(dto => dto.Key.Id))
                .ForMember(entity => entity.Name, map => map.MapFrom(dto => dto.Key.Name))
                .ForMember(entity => entity.Slug, map => map.MapFrom(dto => dto.Key.Slug))
                .ForMember(entity => entity.ClotheItemCount, map => map.MapFrom(dto => dto.Value));
        }
    }
}
