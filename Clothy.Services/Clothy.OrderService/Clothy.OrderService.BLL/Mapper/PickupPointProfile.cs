using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.BLL.Mapper
{
    public class PickupPointProfile : Profile
    {
        public PickupPointProfile()
        {
            CreateMap<PickupPoints, PickupPointReadDTO>();

            CreateMap<PickupPointCreateDTO, PickupPoints>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(time => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<PickupPointUpdateDTO, PickupPoints>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(time => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
