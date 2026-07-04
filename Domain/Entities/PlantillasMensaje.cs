using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain;

public partial class PlantillasMensaje : ITenantOwned
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Recordatorio> Recordatorios { get; set; } = new List<Recordatorio>();

    public virtual Tenant Tenant { get; set; } = null!;
}
