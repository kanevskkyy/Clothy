using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Clothy.Shared.Helpers
{
    public class PagedList<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public List<T> Items { get; set; }

        public PagedList() { }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public static async Task<PagedList<T>> ToPagedListAsync(IFindFluent<T, T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            int count = (int)await source.CountDocumentsAsync(cancellationToken: cancellationToken);
            List<T> items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        public static PagedList<T> ToPagedList<T>(List<T> source, int pageNumber, int pageSize)
        {
            int count = source.Count;
            List<T> items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}