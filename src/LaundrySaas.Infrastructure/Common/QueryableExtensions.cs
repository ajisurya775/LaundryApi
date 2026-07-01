using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LaundrySaas.Application.Contracts.Common;

public static class QueryableExtensions
{
    /// <summary>
    /// Extension method to automatically paginate an IQueryable source and return a PagedResponse.
    /// </summary>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        string? message = null)
    {
        // Enforce boundary constraints
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;

        var totalRecords = await source.CountAsync();
        
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResponse<T>.Create(items, pageNumber, pageSize, totalRecords, message);
    }
}
