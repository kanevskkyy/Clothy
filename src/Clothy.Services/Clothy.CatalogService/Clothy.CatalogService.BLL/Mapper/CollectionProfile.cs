using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using Clothy.CatalogService.Domain.Entities.Catalog;

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
        }
    }
}
