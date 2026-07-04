using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain;

public partial class Cliente : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Notas { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual Tenant Tenant { get; set; } = null!;

    // public static implicit operator Cliente(Cliente v)
    // {
    //     throw new NotImplementedException();
    // }
}
