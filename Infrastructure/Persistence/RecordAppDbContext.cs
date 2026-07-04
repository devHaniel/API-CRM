using System;
using System.Collections.Generic;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class RecordAppDbContext : DbContext
{
    public RecordAppDbContext(DbContextOptions<RecordAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<PlantillasMensaje> PlantillasMensajes { get; set; }

    public virtual DbSet<Recordatorio> Recordatorios { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clientes_pkey");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.TenantId, "idx_clientes_tenant");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Notas).HasColumnName("notas");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("clientes_tenant_id_fkey");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("eventos_pkey");

            entity.ToTable("eventos");

            entity.HasIndex(e => e.ClienteId, "idx_eventos_cliente");

            entity.HasIndex(e => new { e.TenantId, e.Fecha, e.Estado }, "idx_eventos_fecha_estado");

            entity.HasIndex(e => e.TenantId, "idx_eventos_tenant");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Pendiente'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Descripcion)
            .HasMaxLength(200)
            .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Monto)
                .HasPrecision(12, 2)
                .HasColumnName("monto");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("eventos_cliente_id_fkey");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("eventos_tenant_id_fkey");
        });

        modelBuilder.Entity<PlantillasMensaje>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("plantillas_mensaje_pkey");

            entity.ToTable("plantillas_mensaje");

            entity.HasIndex(e => e.TenantId, "idx_plantillas_tenant");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Contenido).HasColumnName("contenido");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Tenant).WithMany(p => p.PlantillasMensajes)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("plantillas_mensaje_tenant_id_fkey");
        });

        modelBuilder.Entity<Recordatorio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("recordatorios_pkey");

            entity.ToTable("recordatorios");

            entity.HasIndex(e => e.EventoId, "idx_recordatorios_evento");

            entity.HasIndex(e => new { e.FechaProgramada, e.Estado }, "idx_recordatorios_pendientes").HasFilter("((estado)::text = 'Pendiente'::text)");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CanalEnvio)
                .HasMaxLength(10)
                .HasColumnName("canal_envio");
            entity.Property(e => e.DetalleError).HasColumnName("detalle_error");
            entity.Property(e => e.Estado)
                .HasMaxLength(15)
                .HasDefaultValueSql("'Pendiente'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.EventoId).HasColumnName("evento_id");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.FechaProgramada).HasColumnName("fecha_programada");
            entity.Property(e => e.PlantillaId).HasColumnName("plantilla_id");

            entity.HasOne(d => d.Evento).WithMany(p => p.Recordatorios)
                .HasForeignKey(d => d.EventoId)
                .HasConstraintName("recordatorios_evento_id_fkey");

            entity.HasOne(d => d.Plantilla).WithMany(p => p.Recordatorios)
                .HasForeignKey(d => d.PlantillaId)
                .HasConstraintName("recordatorios_plantilla_id_fkey");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tenants_pkey");

            entity.ToTable("tenants");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.PlanActivo)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Free'::character varying")
                .HasColumnName("plan_activo");
            entity.Property(e => e.Rubro)
                .HasMaxLength(80)
                .HasColumnName("rubro");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.TenantId, "idx_usuarios_tenant");

            entity.HasIndex(e => new { e.TenantId, e.Email }, "usuarios_tenant_id_email_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Empleado'::character varying")
                .HasColumnName("rol");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("usuarios_tenant_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
