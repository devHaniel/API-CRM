using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Plane
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public decimal PrecioMensual { get; set; }

    public int LimiteRecordatoriosMes { get; set; }

    public decimal PrecioRecordatorioExtra { get; set; }

    public int MaxUsuarios { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
