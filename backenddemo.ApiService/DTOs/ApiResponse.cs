namespace backenddemo.ApiService.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}

public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    public PaginationMeta? Pagination { get; set; }

    public static PagedApiResponse<T> Ok(
        IEnumerable<T> data,
        int page, int pageSize, int totalRecords,
        string message = "Success")
    {
        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        return new PagedApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = new PaginationMeta
            {
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages
            }
        };
    }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}
