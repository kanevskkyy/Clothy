using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.Helpers;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;

namespace Clothy.CatalogService.BLL.Services
{
    public class ClothesStockService : IClothesStockService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public ClothesStockService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<PagedList<ClothesStockReadDTO>> GetPagedClothesStockAsync(ClothesStockSpecificationParameters parameters, CancellationToken cancellationToken = default)
        {
            PagedList<ClothesStock> paged = await unitOfWork.ClothesStocks.GetPagedClothesStockAsync(parameters, cancellationToken);
            List<ClothesStockReadDTO> mapped = mapper.Map<List<ClothesStockReadDTO>>(paged.Items);

            return new PagedList<ClothesStockReadDTO>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<ClothesStockReadDTO> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            return mapper.Map<ClothesStockReadDTO>(stock);
        }

        public async Task<ClothesStockReadDTO> CreateAsync(ClothesStockCreateDTO dto, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.ClothesStocks.IsSizeAndColorAndClotheIdsExists(dto.SizeId, dto.ColorId, dto.ClotheId, cancellationToken);
            if (exists) throw new AlreadyExistsException("Clothes stock with this Size, Color and Clothe already exists");

            ClothesStock stock = mapper.Map<ClothesStock>(dto);
            await unitOfWork.ClothesStocks.AddAsync(stock, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task<ClothesStockReadDTO> UpdateAsync(Guid id, ClothesStockUpdateDTO dto, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            mapper.Map(dto, stock);

            unitOfWork.ClothesStocks.Update(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetByIdWithDetailsAsync(stock.Id, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ClothesStock? stock = await unitOfWork.ClothesStocks.GetByIdWithDetailsAsync(id, cancellationToken);
            if (stock == null) throw new NotFoundException($"Clothes stock not found with ID: {id}");

            unitOfWork.ClothesStocks.Delete(stock);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
