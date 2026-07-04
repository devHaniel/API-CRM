using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Interfaces;
using Hangfire;
using Infrastructure.Mensajeria;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RecordAppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

         services.AddHttpContextAccessor();

         services.AddHangfireServer();
         
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEventoRepository, EventoRepository>();
        services.AddScoped<IRecordatorioRepository, RecordatorioRepository>();
        services.AddScoped<IPlantillasMensajeRepository, PlantillasMensajeRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IMensajeService, TwilioMensajeService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
}
