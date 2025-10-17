using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.BLL.Mapper
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<City, CityReadDTO>();

            CreateMap<CityCreateDTO, City>()
                .ForMember(entity => entity.CreatedAt, map => map.MapFrom(_ => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<CityUpdateDTO, City>()
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(_ => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
