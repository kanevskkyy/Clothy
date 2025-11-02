using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using Clothy.CatalogService.BLL.Exceptions;
using Clothy.CatalogService.BLL.Interfaces;
using Clothy.CatalogService.DAL.UOW;
using Clothy.CatalogService.Domain.Entities;
using Clothy.Shared.Exceptions;

namespace Clothy.CatalogService.BLL.Services
{
    public class ColorService : IColorService
    {
        private IUnitOfWork unitOfWork;
        private IMapper mapper;

        public ColorService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<ColorReadDTO> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Color? color = await unitOfWork.Colors.GetByIdAsync(id, cancellationToken);
            if (color == null) throw new NotFoundException($"Color not found with ID: {id}");
            
            return mapper.Map<ColorReadDTO>(color);
        }

        public async Task<List<ColorReadDTO>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return mapper.Map<List<ColorReadDTO>>(await unitOfWork.Colors.GetAllAsync(cancellationToken));
        }

        public async Task<List<ColorWithCountDTO>> GetAllWithCountAsync(CancellationToken cancellationToken = default)
        {
            return (await unitOfWork.Colors.GetColorsCountWithStockAsync(cancellationToken))
                .Select(pair => mapper.Map<ColorWithCountDTO>(pair))
                .ToList();
        }

        public async Task<ColorReadDTO> CreateAsync(ColorCreateDTO colorCreateDTO, CancellationToken cancellationToken = default)
        {
            bool exists = await unitOfWork.Colors.IsNameAlreadyExistsAsync(colorCreateDTO.HexCode, null, cancellationToken);
            if (exists) throw new AlreadyExistsException($"Color with hex code {colorCreateDTO.HexCode} already exists");

            Color color = mapper.Map<Color>(colorCreateDTO);
            await unitOfWork.Colors.AddAsync(color, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<ColorReadDTO>(color);
        }

        public async Task<ColorReadDTO> UpdateAsync(Guid id, ColorUpdateDTO colorUpdateDTO, CancellationToken cancellationToken = default)
        {
            Color? color = await unitOfWork.Colors.GetByIdAsync(id, cancellationToken);
            if (color == null) throw new NotFoundException($"Color not found with ID: {id}");

            bool exists = await unitOfWork.Colors.IsNameAlreadyExistsAsync(colorUpdateDTO.HexCode, id, cancellationToken);
            if (exists) throw new AlreadyExistsException($"Color with hex code {colorUpdateDTO.HexCode} already exists");

            mapper.Map(colorUpdateDTO, color);

            unitOfWork.Colors.Update(color);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<ColorReadDTO>(color);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Color? color = await unitOfWork.Colors.GetByIdAsync(id, cancellationToken);
            if (color == null) throw new NotFoundException($"Color not found with ID: {id}");

            unitOfWork.Colors.Delete(color);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
