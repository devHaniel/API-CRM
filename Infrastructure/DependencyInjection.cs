using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Hangfire;
using Infrastructure.Configuration;
using Infrastructure.Mensajeria;
using Infrastructure.Pagos;
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
         
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IEventoRepository, EventoRepository>();
        services.AddScoped<IRecordatorioRepository, RecordatorioRepository>();
        services.AddScoped<IPlantillasMensajeRepository, PlantillasMensajeRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IWhatsAppService, TwilioMensajeService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IPlanRepository, PlanRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.Configure<GmailSettings>(
            configuration.GetSection("Gmail"));
        services.AddScoped<IEmailService, GmailService>();

        return services;
    }
}
}
