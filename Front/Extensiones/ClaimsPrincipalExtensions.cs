using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Front.Extensiones
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetNombreNegocio(this ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == "NombreNegocio")?.Value ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == "Email")?.Value ?? string.Empty;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value == "Admin";
        }

        public static Guid GetPlanId(this ClaimsPrincipal user)
        {
            var planIdClaim = user?.Claims.FirstOrDefault(c => c.Type == "PlanId")?.Value;
            return Guid.TryParse(planIdClaim, out var planId) ? planId : Guid.Empty;
        }
    }
}