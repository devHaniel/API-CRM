using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain;

public partial class Evento : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid ClienteId { get; set; }

    public string Tipo { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public decimal? Monto { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();

    public virtual Tenant Tenant { get; set; } = null!;
}
