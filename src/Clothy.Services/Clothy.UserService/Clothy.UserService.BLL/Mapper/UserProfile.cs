using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.UserService.BLL.DTOs.AuthDTOs;
using Clothy.UserService.BLL.DTOs.UserDTOs;
using Clothy.UserService.Domain.Entities;

namespace Clothy.UserService.BLL.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserReadDTO>()
                .ForMember(dest => dest.Roles, option => option.Ignore());

            CreateMap<UserUpdateDTO, ApplicationUser>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<RegisterDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(opt => opt.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
        }
    }
}
