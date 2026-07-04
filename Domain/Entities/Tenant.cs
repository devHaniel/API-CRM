using System;
using System.Collections.Generic;

namespace Domain;

public partial class Tenant
{
    public Guid Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Rubro { get; set; } = null!;

    public string PlanActivo { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    public virtual ICollection<PlantillasMensaje> PlantillasMensajes { get; set; } = new List<PlantillasMensaje>();

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
