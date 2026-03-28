using AutoMapper;
using Clothy.AuthService.BLL.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.Mapper
{
    public class KeycloakMappingProfile : Profile
    {
        public KeycloakMappingProfile()
        {
            CreateMap<KeycloakTokenResponse, TokenResponseDTO>();
        }
    }
}
