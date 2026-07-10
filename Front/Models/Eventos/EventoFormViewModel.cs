using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Front.Models.Eventos
{
    public class EventoFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Selecciona un cliente")]
        [Display(Name = "Cliente")]
        public Guid ClienteId { get; set; }

        public string Tipo { get; set; } = "Cita";

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? Descripcion { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser mayor o igual a 0")]
        public decimal? Monto { get; set; }

        public string Estado { get; set; } = "Pendiente";

        // Para poblar el <select> de clientes — no se envía al servidor, solo para renderizar
        public List<SelectListItem> ClientesDisponibles { get; set; } = [];

        public List<SelectListItem> EstadosDisponibles { get; } =
        [
            new("Pendiente", "Pendiente"),
            new("Pagado", "Pagado"),
            new("Cancelado", "Cancelado")
        ];
    }
}