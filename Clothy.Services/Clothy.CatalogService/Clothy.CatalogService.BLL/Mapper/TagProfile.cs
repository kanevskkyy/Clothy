using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<Tag, TagReadDTO>();

            CreateMap<TagCreateDTO, Tag>();

            CreateMap<TagUpdateDTO, Tag>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<KeyValuePair<Tag, int>, TagWithCountDTO>()
                .ForMember(tagWithCount => tagWithCount.Id, map => map.MapFrom(pair => pair.Key.Id))
                .ForMember(tagWithCount => tagWithCount.Name, map => map.MapFrom(pair => pair.Key.Name))
                .ForMember(tagWithCount => tagWithCount.ClotheItemCount, map => map.MapFrom(pair => pair.Value));
        }
    }
}
