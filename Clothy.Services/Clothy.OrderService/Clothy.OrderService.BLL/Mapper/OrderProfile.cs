using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;
using Clothy.OrderService.Domain.Entities.AdditionalEntities;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.BLL.Mapper
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderReadDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<OrderWithDetailsData, OrderDetailDTO>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.DeliveryDetail, opt => opt.MapFrom(src => src.DeliveryDetail));

            CreateMap<OrderSummaryData, OrderReadDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.UserFirstName))
                .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.UserLastName))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<OrderCreateDTO, Order>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow.ToUniversalTime()))
                .ForMember(dest => dest.StatusId, opt => opt.Ignore());

            CreateMap<OrderItemCreateDTO, OrderItem>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<DeliveryDetailCreateDTO, DeliveryDetail>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow.ToUniversalTime()));

            CreateMap<OrderItemData, OrderItemDTO>();

            CreateMap<DeliveryDetailData, DeliveryDetailDTO>();
        }
    }
}