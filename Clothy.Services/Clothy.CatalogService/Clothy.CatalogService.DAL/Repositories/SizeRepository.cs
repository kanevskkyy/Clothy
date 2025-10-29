using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.DAL.DB;
using Clothy.CatalogService.DAL.Interfaces;
using Clothy.CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clothy.CatalogService.DAL.Repositories
{
    public class SizeRepository : GenericRepository<Size>, ISizeRepository
    {
        public SizeRepository(ClothyCatalogDbContext context) : base(context) 
        {

        }

        public async Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return await dbSet.AnyAsync(property => property.Name.ToLower() == name.ToLower(), cancellationToken);
            }
            else
            {
                return await dbSet.AnyAsync(property => property.Name.ToLower() == name.ToLower() && property.Id != id, cancellationToken);
            }
        }
    }
}
