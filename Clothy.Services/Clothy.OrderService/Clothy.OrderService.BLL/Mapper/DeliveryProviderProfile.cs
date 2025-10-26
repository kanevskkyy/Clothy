using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.DeliveryProviderDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.BLL.Mapper
{
    public class DeliveryProviderProfile : Profile
    {
        public DeliveryProviderProfile()
        {
            CreateMap<DeliveryProvider, DeliveryProviderReadDTO>();

            CreateMap<DeliveryProviderCreateDTO, DeliveryProvider>()
                .ForMember(entity => entity.IconUrl, map => map.Ignore())
                .ForMember(entity=> entity.CreatedAt, map=> map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<DeliveryProviderUpdateDTO, DeliveryProvider>()
                .ForMember(entity => entity.IconUrl, map => map.Ignore())
                .ForMember(entity => entity.UpdatedAt, map => map.MapFrom(dto => DateTime.UtcNow.ToUniversalTime()));
        }
    }
}
