using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Specification;
using Clothy.CatalogService.Domain.Entities.Clothe;
using Clothy.CatalogService.Domain.QueryParameters;

namespace Clothy.CatalogService.DAL.Specification
{
    public class ClotheItemSpecification : Specification<ClotheItem>
    {
        public ClotheItemSpecification(ClotheItemSpecificationParameters parameters)
        {
            Query.Include(property => property.Stocks)
                .ThenInclude(property => property.Color);
            Query.Include(property => property.ClotheTags);
            Query.Include(property => property.Photos);
            Query.Include(property => property.ClotheMaterials);

            if (parameters.Gender.HasValue)
            {
                Query.Where(property => property.Gender == parameters.Gender);
            }

            if (parameters.MinPrice.HasValue)
            {
                Query.Where(property => property.Price >= parameters.MinPrice.Value);
            }

            if (parameters.MaxPrice.HasValue)
            {
                Query.Where(property => property.Price <= parameters.MaxPrice.Value);
            }

            if (parameters.Brands != null && parameters.Brands.Any())
            {
                Query.Where(property => parameters.Brands.Contains(property.Brand.Id));
            }

            if (parameters.Collections != null && parameters.Collections.Any())
            {
                Query.Where(property => parameters.Collections.Contains(property.Collection.Id));
            }

            if (parameters.ClothingTypes != null && parameters.ClothingTypes.Any())
            {
                Query.Where(property => parameters.ClothingTypes.Contains(property.ClothyType.Id));
            }

            if (parameters.Colors != null && parameters.Colors.Any())
            {
                Query.Where(property => property.Stocks.Any(color => parameters.Colors.Contains(color.ColorId)));
            }

            if (parameters.Tags != null && parameters.Tags.Any())
            {
                Query.Where(property => property.ClotheTags.Any(clotheTags => parameters.Tags.Contains(clotheTags.TagId)));
            }

            if (parameters.Materials != null && parameters.Materials.Any())
            {
                Query.Where(property => property.ClotheMaterials.Any(clotheMaterials => parameters.Materials.Contains(clotheMaterials.MaterialId)));
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
