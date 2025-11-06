using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class ColorProfile : Profile
    {
        public ColorProfile()
        {
            CreateMap<Color, ColorReadDTO>();

            CreateMap<ColorCreateDTO, Color>();

            CreateMap<ColorUpdateDTO, Color>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<KeyValuePair<Color, int>, ColorWithCountDTO>()
                .ForMember(dto => dto.Id, map => map.MapFrom(pair => pair.Key.Id))
                .ForMember(dto => dto.HexCode, map => map.MapFrom(pair => pair.Key.HexCode))
                .ForMember(dto => dto.ClotheItemCount, map => map.MapFrom(pair => pair.Value));
        }
    }
}
