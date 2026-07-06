using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Cliente;
using Application.Interfaces;
using Domain.Interfaces;
using Front.Models.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Front.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly IClienteService _clienteService;
        private readonly ICurrentTenantService _currentTenantService;

        public ClientesController(IClienteService clienteService, ICurrentTenantService currentTenantService)
        {
            _clienteService = clienteService;
            _currentTenantService = currentTenantService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
        {
            var tenantId = _currentTenantService.TenantId;

            var resultado = await _clienteService.ObtenerTodosAsync(tenantId, pageNumber, pageSize, ct);

            var model = new ClientesIndexViewModel
            {
                Clientes = resultado.Items.Select(c => new CllienteListItemViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = true
                }).ToList(),
                PageNumber = resultado.PageNumber,
                PageSize = resultado.PageSize,
                TotalCount = resultado.TotalCount,
                TotalPages = resultado.TotalPages
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View(new ClienteFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearClienteDto(model.Nombre, model.Telefono, model.Email, model.Notas);
                await _clienteService.CrearAsync(tenantId, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            var cliente = await _clienteService.ObtenerPorIdAsync(tenantId, id, ct);
            if (cliente is null)
                return NotFound();

            var model = new ClienteFormViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono ?? string.Empty,
                Email = cliente.Email ?? string.Empty,
                Notas = cliente.Notas
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClienteFormViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = _currentTenantService.TenantId;

            try
            {
                var dto = new CrearClienteDto(model.Nombre, model.Telefono, model.Email, model.Notas);
                await _clienteService.ActualizarAsync(tenantId, model.Id, dto, ct);

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var tenantId = _currentTenantService.TenantId;

            try
            {
                await _clienteService.EliminarAsync(tenantId, id, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}