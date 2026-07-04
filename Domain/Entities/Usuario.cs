using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain;

public partial class Usuario : IEntity
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
