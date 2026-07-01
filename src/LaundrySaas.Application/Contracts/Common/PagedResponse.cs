using System;
using System.Collections.Generic;

namespace LaundrySaas.Application.Contracts.Common;

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public PagedResponse()
    {
    }

    public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        Success = true;
        Message = message;
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        Errors = null;
    }

    public static PagedResponse<T> Create(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        return new PagedResponse<T>(data, pageNumber, pageSize, totalRecords, message);
    }
}
