using System.Collections.Generic;

namespace Application.DTOs.Common
{
    public record PagedResultDto<T>(
        IEnumerable<T> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages
    );
}
