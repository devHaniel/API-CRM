using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMensajeService
    {
        Task<bool> EnviarWhatsAppAsync(string telefono, string mensaje, CancellationToken ct = default);
    }
}