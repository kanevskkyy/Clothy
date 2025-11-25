using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.BasketService.BLL.DTOs;
using Clothy.BaskteService.Domain.Entities;

namespace Clothy.BasketService.BLL.Mapper
{
    public class BasketProfile : Profile
    {
        public BasketProfile() 
        {
            CreateMap<BasketItem, BasketItemDTO>();
            
            CreateMap<BasketList, BasketDTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(opt => opt.BasketItems));
            
            CreateMap<BasketItemCreateDTO, BasketItem>();
        }
    }
}
