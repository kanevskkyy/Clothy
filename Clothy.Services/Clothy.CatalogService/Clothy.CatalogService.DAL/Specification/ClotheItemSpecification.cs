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
                .ThenInclude(property => property.Size);

            Query.Include(property => property.Stocks)
                .ThenInclude(property => property.Color);

            Query.Include(property => property.ClotheTags)
                .ThenInclude(property => property.Tag);

            Query.Include(property => property.ClothyType);

            Query.Include(property => property.Collection);

            Query.Include(property => property.Brand);

            Query.Include(property => property.Photos)
                .ThenInclude(property => property.Color);

            Query.Include(property => property.ClotheMaterials)
                .ThenInclude(property => property.Material);

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                string filterName = parameters.Name.ToLower();

                Query.Where(property => property.Name.ToLower().Contains(filterName)
                || property.Brand.Name.Contains(filterName)
                || property.ClothyType.Name.Contains(filterName));
            }

            if (parameters.Gender.HasValue)
            {
                Query.Where(property => property.Gender == parameters.Gender);
            }

            if (parameters.ShowOnlyWithDiscounts)
            {
                Query.Where(property => property.OldPrice.HasValue);
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
                Query.Where(property => parameters.Brands.Contains(property.Brand.Slug));
            }

            if (parameters.Collections != null && parameters.Collections.Any())
            {
                Query.Where(property => parameters.Collections.Contains(property.Collection.Slug));
            }

            if (parameters.ClothingTypes != null && parameters.ClothingTypes.Any())
            {
                Query.Where(property => parameters.ClothingTypes.Contains(property.ClothyType.Slug));
            }

            if (parameters.Sizes != null && parameters.Sizes.Any())
            {
                Query.Where(property => property.Stocks.Any(size => parameters.Sizes.Contains(size.Size.Slug)));
            }

            if (parameters.Colors != null && parameters.Colors.Any())
            {
                Query.Where(property => property.Stocks.Any(color => parameters.Colors.Contains(color.Color.Slug)));
            }

            if (parameters.Tags != null && parameters.Tags.Any())
            {
                Query.Where(property => property.ClotheTags.Any(clotheTags => parameters.Tags.Contains(clotheTags.Tag.Slug)));
            }

            if (parameters.Materials != null && parameters.Materials.Any())
            {
                Query.Where(property => property.ClotheMaterials.Any(clotheMaterials => parameters.Materials.Contains(clotheMaterials.Material.Slug)));
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
