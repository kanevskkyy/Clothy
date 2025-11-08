using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.Domain.Entities;


namespace Clothy.OrderService.BLL.Mapper
{
    public class RegionProfile : Profile
    {
        public RegionProfile()
        {
            CreateMap<Region, RegionReadDTO>();

            CreateMap<RegionCreateDTO, Region>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<RegionUpdateDTO, Region>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
