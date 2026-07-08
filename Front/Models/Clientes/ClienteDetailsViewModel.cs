using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Evento;

namespace Front.Models.Clientes
{
    public class ClienteDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Notas { get; set; }
        public List<EventoDto> Eventos { get; set; } = new List<EventoDto>();
    }
}