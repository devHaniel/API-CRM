using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Usuario
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
