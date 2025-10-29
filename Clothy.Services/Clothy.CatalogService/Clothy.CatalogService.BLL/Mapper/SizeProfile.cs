using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using Clothy.CatalogService.Domain.Entities;

namespace Clothy.CatalogService.BLL.Mapper
{
    public class SizeProfile : Profile
    {
        public SizeProfile()
        {
            CreateMap<Size, SizeReadDTO>();

            CreateMap<SizeCreateDTO, Size>();

            CreateMap<SizeUpdateDTO, Size>()
                .ForMember(size => size.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
