var calendar;

document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');
    if (!calendarEl) return;

    calendar = new FullCalendar.Calendar(calendarEl, {
        locale: 'es',
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        buttonText: {
            today: 'Hoy',
            month: 'Mes',
            week: 'Semana',
            day: 'Día',
            list: 'Lista'
        },
        events: '/Eventos/ObtenerEventosCalendario',
        eventDidMount: function (info) {
            var estado = info.event.extendedProps.estado;
            if (estado === 'Pagado') info.el.style.backgroundColor = '#22c55e';
            else if (estado === 'Cancelado') info.el.style.backgroundColor = '#ef4444';
            else info.el.style.backgroundColor = '#f59e0b';
        },
        eventClick: function (info) {
            info.jsEvent.preventDefault();
            mostrarEvento(info.event);
        },
        dateClick: function (info) {
            abrirCrearModal(info.date);
        }
    });

    calendar.render();
    poblarClientes();
});

function poblarClientes() {
    var select = document.getElementById('campo-cliente');
    if (!select || !window.clientes) return;
    window.clientes.forEach(function (c) {
        var opt = document.createElement('option');
        opt.value = c.id;
        opt.textContent = c.nombre;
        select.appendChild(opt);
    });
}

function abrirCrearModal(fecha) {
    var y = fecha.getFullYear();
    var m = String(fecha.getMonth() + 1).padStart(2, '0');
    var d = String(fecha.getDate()).padStart(2, '0');
    var h = String(fecha.getHours()).padStart(2, '0');
    var min = String(fecha.getMinutes()).padStart(2, '0');
    document.getElementById('campo-fecha').value = y + '-' + m + '-' + d + 'T' + h + ':' + min;
    document.getElementById('crear-error').classList.add('hidden');
    document.getElementById('crear-evento-form').reset();
    document.getElementById('campo-fecha').value = y + '-' + m + '-' + d + 'T' + h + ':' + min;
    document.getElementById('crear-modal').classList.remove('hidden');
}

function cerrarCrearModal() {
    document.getElementById('crear-modal').classList.add('hidden');
}

document.addEventListener('click', function (e) {
    var modal = document.getElementById('evento-modal');
    if (e.target === modal) modal.classList.add('hidden');
    var crearModal = document.getElementById('crear-modal');
    if (e.target === crearModal) crearModal.classList.add('hidden');
});

document.getElementById('crear-evento-form').addEventListener('submit', function (e) {
    e.preventDefault();
    var fd = new FormData(this);
    var data = {
        clienteId: fd.get('clienteId'),
        fecha: document.getElementById('campo-fecha').value,
        descripcion: fd.get('descripcion') || null,
        monto: fd.get('monto') ? parseFloat(fd.get('monto')) : null
    };
    fetch('/Eventos/CrearDesdeCalendario', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
        .then(function (r) { return r.json(); })
        .then(function (r) {
            if (r.success) {
                cerrarCrearModal();
                calendar.refetchEvents();
            } else {
                var err = document.getElementById('crear-error');
                err.textContent = r.message || 'Error al crear el evento';
                err.classList.remove('hidden');
            }
        })
        .catch(function () {
            var err = document.getElementById('crear-error');
            err.textContent = 'Error de conexión';
            err.classList.remove('hidden');
        });
});

function mostrarEvento(event) {
    var props = event.extendedProps;

    document.getElementById('modal-titulo').textContent = event.title;

    var html = '';
    html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Cliente:</span><span class="text-gray-900">' + (props.clienteNombre || '—') + '</span></div>';
    html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Tipo:</span><span class="text-gray-900">' + (props.tipo || '—') + '</span></div>';
    html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Fecha:</span><span class="text-gray-900">' + event.start.toLocaleString('es-MX', { dateStyle: 'long', timeStyle: 'short' }) + '</span></div>';

    if (props.monto != null) {
        html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Monto:</span><span class="text-gray-900">$' + Number(props.monto).toFixed(2) + '</span></div>';
    }
    if (props.descripcion) {
        html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Descripción:</span><span class="text-gray-900">' + props.descripcion + '</span></div>';
    }

    var estadoColors = {
        Pendiente: 'bg-yellow-100 text-yellow-800',
        Pagado: 'bg-green-100 text-green-800',
        Cancelado: 'bg-red-100 text-red-800'
    };
    var colorClass = estadoColors[props.estado] || 'bg-gray-100 text-gray-800';
    html += '<div class="flex items-center gap-2"><span class="text-sm font-medium text-gray-500 w-24">Estado:</span><span class="px-2.5 py-0.5 rounded-full text-xs font-semibold ' + colorClass + '">' + (props.estado || '—') + '</span></div>';

    html += '<div class="pt-4 flex gap-2">';
    html += '<a href="/Eventos/Edit/' + event.id + '" class="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm hover:bg-blue-700">Editar</a>';
    html += '<a href="/Eventos/Details/' + event.id + '" class="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg text-sm hover:bg-gray-200">Ver detalle</a>';
    html += '</div>';

    document.getElementById('modal-contenido').innerHTML = html;
    document.getElementById('evento-modal').classList.remove('hidden');
}

function cerrarModal() {
    document.getElementById('evento-modal').classList.add('hidden');
}
