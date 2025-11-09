using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using Clothy.OrderService.BLL.Interfaces;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.DAL.UOW;
using Clothy.OrderService.Domain.Entities;
using Clothy.Shared.Helpers;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.OrderService.BLL.Services
{
    public class SettlementService : ISettlementService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public SettlementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<SettlementReadDTO> CreateAsync(SettlementCreateDTO settlementCreateDTO, CancellationToken cancellationToken = default)
        {
            bool nameAlreadyExists = await unitOfWork.Settlement.ExistsByNameAndRegionIdAsync(settlementCreateDTO.Name, settlementCreateDTO.RegionId, cancellationToken: cancellationToken);
            if (nameAlreadyExists) throw new AlreadyExistsException($"Settelement with name: {settlementCreateDTO.Name} with this RegionId already exists");

            Region? region = await unitOfWork.Region.GetByIdAsync(settlementCreateDTO.RegionId, cancellationToken);
            if (region == null) throw new NotFoundException($"Region with ID: {settlementCreateDTO.RegionId}");

            Settlement settlement = mapper.Map<Settlement>(settlementCreateDTO);
            settlement.Id = await unitOfWork.Settlement.AddAsync(settlement, cancellationToken);
            
            await unitOfWork.CommitAsync();
            return mapper.Map<SettlementReadDTO>(settlement);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");

            await unitOfWork.Settlement.DeleteAsync(id, cancellationToken);
            await unitOfWork.CommitAsync();
        }

        public async Task<SettlementReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");

            return mapper.Map<SettlementReadDTO>(settlement);
        }

        public async Task<PagedList<SettlementReadDTO>> GetPagedAsync(SettlementFilterDTO settlementFilterDTO, CancellationToken cancellationToken = default)
        {
            var (pagedSettlement, totalCount) = await unitOfWork.Settlement.GetPagedAsync(settlementFilterDTO, cancellationToken);
            List<SettlementReadDTO> settlementReadDTOs = mapper.Map<List<SettlementReadDTO>>(pagedSettlement);

            return new PagedList<SettlementReadDTO>(settlementReadDTOs, totalCount, settlementFilterDTO.PageNumber, settlementFilterDTO.PageSize);
        }

        public async Task<SettlementReadDTO> UpdateAsync(Guid id, SettlementUpdateDTO settlementUpdateDTO, CancellationToken cancellationToken = default)
        {
            Settlement? settlement = await unitOfWork.Settlement.GetByIdAsync(id, cancellationToken);
            if (settlement == null) throw new NotFoundException($"Settlement not found with ID: {id}");

            bool nameAlreadyExists = await unitOfWork.Settlement.ExistsByNameAndRegionIdAsync(settlementUpdateDTO.Name, settlementUpdateDTO.RegionId, id, cancellationToken: cancellationToken);
            if (nameAlreadyExists) throw new AlreadyExistsException($"Settelement with name: {settlementUpdateDTO.Name} with this RegionId already exists");

            Region? region = await unitOfWork.Region.GetByIdAsync(settlementUpdateDTO.RegionId, cancellationToken);
            if (region == null) throw new NotFoundException($"Region not found with ID: {settlementUpdateDTO.RegionId}");

            mapper.Map(settlementUpdateDTO, settlement);

            await unitOfWork.Settlement.UpdateAsync(settlement);
            await unitOfWork.CommitAsync();

            return mapper.Map<SettlementReadDTO>(settlement);
        }
    }
}
