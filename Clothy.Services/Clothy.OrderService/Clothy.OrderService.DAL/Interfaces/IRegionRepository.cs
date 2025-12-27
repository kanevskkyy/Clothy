using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.DAL.FilterDTOs;
using Clothy.OrderService.Domain.Entities;

namespace Clothy.OrderService.DAL.Interfaces
{
    public interface IRegionRepository : IGenericRepository<Region>
    {
        Task<bool> ExistByNameAndCityIdAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    }
}
