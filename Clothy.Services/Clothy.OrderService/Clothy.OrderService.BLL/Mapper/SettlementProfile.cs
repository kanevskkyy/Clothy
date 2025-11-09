using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.BLL.Mapper
{
    public class SettlementProfile : Profile
    {
        public SettlementProfile() 
        {
            CreateMap<Settlement, SettlementReadDTO>();

            CreateMap<SettlementCreateDTO, Settlement>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(time => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<SettlementUpdateDTO, Settlement>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(time => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
