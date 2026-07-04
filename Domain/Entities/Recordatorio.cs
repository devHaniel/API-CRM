using System;
using System.Collections.Generic;
using Domain.Interfaces;

namespace Domain;

public partial class Recordatorio : IEntity
{
    public Guid Id { get; set; }

    public Guid EventoId { get; set; }

    public Guid? PlantillaId { get; set; }

    public string CanalEnvio { get; set; } = null!;

    public DateTime FechaProgramada { get; set; }

    public DateTime? FechaEnvio { get; set; }

    public string Estado { get; set; } = null!;

    public string? DetalleError { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Evento Evento { get; set; } = null!;

    public virtual PlantillasMensaje? Plantilla { get; set; }
}
