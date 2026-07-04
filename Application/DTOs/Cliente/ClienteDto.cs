using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.DTOs.Cliente
{
    public record ClienteDto(Guid Id, string Nombre, 
    string? Telefono, 
    string? Email, 
    string? Notas,
    DateTime FechaCreacion,
    bool Activo);
}