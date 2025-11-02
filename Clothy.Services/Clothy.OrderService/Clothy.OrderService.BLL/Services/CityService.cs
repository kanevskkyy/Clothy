using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Exceptions;
using Clothy.Shared.Helpers;

namespace Clothy.OrderService.BLL.Services
{
    public class CityService : ICityService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public CityService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PagedList<CityReadDTO>> GetPagedAsync(CityFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var (cities, totalCount) = await unitOfWork.Cities.GetPagedAsync(filter, cancellationToken);
            List<CityReadDTO> dtos = mapper.Map<List<CityReadDTO>>(cities);

            return new PagedList<CityReadDTO>(dtos, totalCount, filter.PageNumber, filter.PageSize);
        }


        public async Task<CityReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {id}");

            return mapper.Map<CityReadDTO>(city);
        }

        public async Task<CityReadDTO> CreateAsync(CityCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Cities.ExistsByNameAsync(dto.Name, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"City with name '{dto.Name}' already exists.");

            City city = mapper.Map<City>(dto);
            city.Id = await unitOfWork.Cities.AddAsync(city, cancellationToken);
            await unitOfWork.CommitAsync();

            return mapper.Map<CityReadDTO>(city);
        }

        public async Task<CityReadDTO> UpdateAsync(Guid id, CityUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {id}");

            bool exists = await unitOfWork.Cities.ExistsByNameAsync(dto.Name, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"City with name '{dto.Name}' already exists.");

            mapper.Map(dto, city);
            await unitOfWork.Cities.UpdateAsync(city);
            await unitOfWork.CommitAsync();

            return mapper.Map<CityReadDTO>(city);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            City? city = await unitOfWork.Cities.GetByIdAsync(id, cancellationToken);
            if (city == null) throw new NotFoundException($"City not found with ID: {id}");

            await unitOfWork.Cities.DeleteAsync(city.Id);
            await unitOfWork.CommitAsync();
        }
    }
}
