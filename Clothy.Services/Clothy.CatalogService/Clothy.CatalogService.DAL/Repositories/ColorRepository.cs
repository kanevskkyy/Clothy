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
    public class ColorRepository : GenericRepository<Color>, IColorRepository
    {
        public ColorRepository(ClothyCatalogDbContext context) : base(context)
        {

        }

        public async Task<Dictionary<Color, int>> GetColorsCountWithStockAsync(CancellationToken cancellationToken = default)
        {
            List<Color> colors = await dbSet
                .Include(property => property.ClothesStocks)
                .ToListAsync();
            Dictionary<Color, int> result = new Dictionary<Color, int>();

            foreach (Color color in colors)
            {
                int colorsQuantity = color.ClothesStocks.Count;
                result.Add(color, colorsQuantity);
            }

            return result;
        }

        public async Task<bool> IsNameAlreadyExistsAsync(string name, Guid? id = null, CancellationToken cancellationToken = default)
        {
            if (id == null)
            {
                return await dbSet.AnyAsync(property => property.HexCode.ToLower() == name.ToLower(), cancellationToken);
            }
            else
            {
                return await dbSet.AnyAsync(property => property.HexCode.ToLower() == name.ToLower() && property.Id != id, cancellationToken);
            }
        }
    }
}
