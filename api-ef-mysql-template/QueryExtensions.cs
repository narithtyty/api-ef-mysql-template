using System.Collections.Generic;
using System.Linq;
using System;

namespace api_ef_mysql_template
{
    public class QueryExtensions<T>
    {
        public class PageResult<T>
        {
            public int PageNumber { get; set; }
            public long TotalDatas { get; set; }
            public int TotalPages { get; set; }
            public List<T> Data { get; set; }
        }

        public PageResult<T> GetPageResult(IQueryable<T> query, int pageNumber, int itemsPerPage)
        {
            // Get the total number of items in the result set.
            long totalData = query.Count();

            // Calculate the total number of pages.
            int totalPages = (int)Math.Ceiling((double)totalData / itemsPerPage);

            // Limit the results to the current page.
            query = query.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage);

            // Create a PageResult object and set the properties.
            PageResult<T> pageResult = new PageResult<T>
            {
                Data = query.ToList(),
                PageNumber = pageNumber,
                TotalDatas = totalData,
                TotalPages = totalPages
            };

            // Return the PageResult object.
            return pageResult;
        }
    }
}
