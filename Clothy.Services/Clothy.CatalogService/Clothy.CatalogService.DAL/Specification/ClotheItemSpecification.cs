using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using Clothy.CatalogService.Domain.Entities;
using Clothy.CatalogService.Domain.QueryParameters;

namespace Clothy.CatalogService.DAL.Specification
{
    public class ClotheItemSpecification : Specification<ClotheItem>
    {
        public ClotheItemSpecification(ClotheItemSpecificationParameters parameters)
        {
            Query.Include(property => property.Stocks);
            Query.Include(property => property.ClotheTags);
            Query.Include(property => property.ClothyType);
            Query.Include(property => property.Collection);
            Query.Include(property => property.Brand);
            Query.Include(property => property.Photos);

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                Query.Where(property => property.Name.ToLower().Contains(parameters.Name.ToLower()));
            }

            if (parameters.MinPrice.HasValue)
            {
                Query.Where(property => property.Price >= parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice.HasValue)
            {
                Query.Where(property => property.Price <= parameters.MaxPrice.Value);
            }

            if (parameters.BrandIds != null && parameters.BrandIds.Any())
            {
                Query.Where(property => parameters.BrandIds.Contains(property.BrandId.Value));
            }

            if (parameters.CollectionIds != null && parameters.CollectionIds.Any())
            {
                Query.Where(property => parameters.CollectionIds.Contains(property.CollectionId.Value));
            }

            if (parameters.ClothingTypeIds != null && parameters.ClothingTypeIds.Any())
            {
                Query.Where(property => parameters.ClothingTypeIds.Contains(property.ClothingTypeId.Value));
            }

            if (parameters.SizeIds != null && parameters.SizeIds.Any())
            {
                Query.Where(property => property.Stocks.Any(s => parameters.SizeIds.Contains(s.SizeId)));
            }

            if (parameters.TagIds != null && parameters.TagIds.Any())
            {
                Query.Where(property => property.ClotheTags.Any(ct => parameters.TagIds.Contains(ct.TagId)));
            }

            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "price":
                        if (parameters.SortDescending)
                        {
                            Query.OrderByDescending(property => property.Price);
                        }
                        else
                        {
                            Query.OrderBy(property => property.Price);
                        }
                        break;

                    case "name":
                        if (parameters.SortDescending)
                        {
                            Query.OrderByDescending(property => property.Name);
                        }
                        else
                        {
                            Query.OrderBy(property => property.Name);
                        }
                        break;
                }
            }
            else
            {
                Query.OrderByDescending(property => property.CreatedAt);
            }
        }
    }
}
