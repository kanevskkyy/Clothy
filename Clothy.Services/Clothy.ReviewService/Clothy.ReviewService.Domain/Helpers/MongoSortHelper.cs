using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Clothy.ReviewService.Domain.Helpers
{
    public class MongoSortHelper<T>
    {
        public SortDefinition<T> ApplySort(string? orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString)) return Builders<T>.Sort.Ascending("_id");

            string[] orderParams = orderByQueryString.Trim().Split(',');
            SortDefinition<T> sortBuilder = Builders<T>.Sort.Combine(new List<SortDefinition<T>>());

            foreach (string param in orderParams)
            {
                if (string.IsNullOrWhiteSpace(param)) continue;

                string trimmed = param.Trim();
                bool isDesc = trimmed.EndsWith(" desc", StringComparison.InvariantCultureIgnoreCase);
                string fieldName = trimmed.Split(" ")[0];

                SortDefinition<T> sortDef;
                if (isDesc) sortDef = Builders<T>.Sort.Descending(fieldName);
                else sortDef = Builders<T>.Sort.Ascending(fieldName);

                sortBuilder = Builders<T>.Sort.Combine(sortBuilder, sortDef);
            }

            return sortBuilder;
        }
    }
}
