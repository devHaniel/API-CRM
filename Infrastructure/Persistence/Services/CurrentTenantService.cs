using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistence.Services
{
    public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenant_id")?.Value;

            if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var tenantId))
                throw new UnauthorizedAccessException("No se pudo determinar el tenant actual.");

            return tenantId;
        }
    }
}
}