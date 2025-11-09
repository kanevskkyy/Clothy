using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class RegionService : IRegionService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public RegionService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<RegionReadDTO> CreateAsync(RegionCreateDTO regionCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Region.ExistByNameAndCityIdAsync(regionCreateDTO.Name, regionCreateDTO.CityId, cancellationToken: cancellationToken);
            if (exists) throw new AlreadyExistsException($"Region with name '{regionCreateDTO.Name}' already exists.");

            City? city = await unitOfWork.Cities.GetByIdAsync(regionCreateDTO.CityId, cancellationToken);
            if(city == null) throw new NotFoundException($"City not found with ID: {regionCreateDTO.CityId}"); 

            Region region = mapper.Map<Region>(regionCreateDTO);
            
            region.Id = await unitOfWork.Region.AddAsync(region);
            await unitOfWork.CommitAsync();

            return mapper.Map<RegionReadDTO>(region);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {id}");

            await unitOfWork.Region.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();
        }

        public async Task<RegionReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {id}");

            return mapper.Map<RegionReadDTO>(region);
        }

        public async Task<PagedList<RegionReadDTO>> GetPagedAsync(RegionFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var (regions, totalCount) = await unitOfWork.Region.GetPagedAsync(filter, cancellationToken);
            List<RegionReadDTO> regionReadDTOs = mapper.Map<List<RegionReadDTO>>(regions);
            
            return new PagedList<RegionReadDTO>(regionReadDTOs, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<RegionReadDTO> UpdateAsync(Guid id, RegionUpdateDTO regionUpdateDTO, CancellationToken cancellationToken = default)
        {
            Region? region = await unitOfWork.Region.GetByIdAsync(id, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {id}");

            City? city = await unitOfWork.Cities.GetByIdAsync(regionUpdateDTO.CityId, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {regionUpdateDTO.CityId}");

            bool exists = await unitOfWork.Region.ExistByNameAndCityIdAsync(regionUpdateDTO.Name, regionUpdateDTO.CityId, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"Region with name '{regionUpdateDTO.Name}' already exists.");

            mapper.Map(regionUpdateDTO, region);

            await unitOfWork.Region.UpdateAsync(region, cancellationToken);
            await unitOfWork.CommitAsync();

            return mapper.Map<RegionReadDTO>(region);
        }
    }
}
