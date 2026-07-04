using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IEventoService, EventoService>();
            services.AddScoped<IRecordatorioService, RecordatorioService>();
            services.AddScoped<IPlantillasMensajeService, PlantillasMensajeService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
