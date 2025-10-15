using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Clothy.CatalogService.DAL.Specification
{
    public class ClothesStockSpecification : Specification<ClothesStock>
    {
        public ClothesStockSpecification(ClothesStockSpecificationParameters parameters)
        {
            Query.Include(cs => cs.Clothe)
                 .ThenInclude(property => property.ClothyType)
                 .Include(cs => cs.Clothe)
                 .ThenInclude(property => property.Collection)
                 .Include(cs => cs.Size)
                 .Include(cs => cs.Color);

            if (parameters.ClotheId.HasValue)
            {
                Query.Where(property => property.ClotheId == parameters.ClotheId.Value);
            }

            if (parameters.SizeId.HasValue)
            {
                Query.Where(property => property.SizeId == parameters.SizeId.Value);
            }

            if (parameters.ColorId.HasValue)
            {
                Query.Where(property => property.ColorId == parameters.ColorId.Value);
            }

            if (parameters.MinQuantity.HasValue)
            {
                Query.Where(property => property.Quantity >= parameters.MinQuantity.Value);
            }

            if (parameters.MaxQuantity.HasValue)
            {
                Query.Where(property => property.Quantity <= parameters.MaxQuantity.Value);
            }

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "quantity":
                        if (parameters.SortDescending)
                        {
                            Query.OrderByDescending(cs => cs.Quantity);
                        }
                        else
                        {
                            Query.OrderBy(cs => cs.Quantity);
                        }
                        break;

                    case "createdat":
                        if (parameters.SortDescending)
                        {
                            Query.OrderByDescending(cs => cs.CreatedAt);
                        }
                        else
                        {
                            Query.OrderBy(cs => cs.CreatedAt);
                        }
                        break;

                    default:
                        Query.OrderBy(cs => cs.Id); 
                        break;
                }
            }
            else
            {
                Query.OrderByDescending(cs => cs.CreatedAt);
            }
        }
    }
}
