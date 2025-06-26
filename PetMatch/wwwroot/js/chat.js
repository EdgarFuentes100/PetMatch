const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

let nombreReceptor = null;

document.addEventListener('DOMContentLoaded', () => {
    const chatModalEl = document.getElementById('chatModal');
    const chatModal = new bootstrap.Modal(chatModalEl);
    const chatNombre = document.getElementById('chatNombre');
    const mensajesLista = chatModalEl.querySelector('#mensajesLista');
    const mensajeInput = document.getElementById('mensajeInput');
    const enviarBtn = document.getElementById('enviarBtn');
    const nuevoMensajeBtn = document.getElementById('nuevoMensajeBtn');

    // Detectar scroll manual para ocultar botón si el scroll está abajo
    mensajesLista.addEventListener('scroll', () => {
        if (estaScrolleadoAbajo()) {
            nuevoMensajeBtn.classList.add('d-none');
        }
    });

    // Abrir chat
    document.querySelectorAll('.abrir-chat').forEach(btn => {
        btn.addEventListener('click', () => {
            nombreReceptor = btn.getAttribute('data-nombre');
            const receptorId = parseInt(btn.getAttribute('data-id'));

            chatNombre.textContent = nombreReceptor;
            mensajesLista.innerHTML = '';
            mensajeInput.value = '';
            chatModal.show();

            chatModalEl.addEventListener('shown.bs.modal', async function handleShown() {
                chatModalEl.removeEventListener('shown.bs.modal', handleShown);
                try {
                    await connection.invoke("SeleccionarReceptor", receptorId);
                    const historial = await connection.invoke("ObtenerHistorial", receptorId);

                    historial.forEach(mensaje => {
                        const nombre = mensaje.esPropio ? "Tú" : nombreReceptor;
                        agregarMensaje(nombre, mensaje.contenido, mensaje.esPropio);
                    });

                    requestAnimationFrame(() => {
                        mensajesLista.scrollTop = mensajesLista.scrollHeight;
                    });
                } catch (err) {
                    console.error("Error cargando historial:", err);
                }
            });
        });
    });

    // Enviar mensaje
    enviarBtn.addEventListener('click', async () => {
        const texto = mensajeInput.value.trim();
        if (!texto) return;

        try {
            await connection.invoke("EnviarMensaje", texto);
            mensajeInput.value = '';
        } catch (err) {
            console.error("Error al enviar:", err);
        }
    });

    // Recibir mensaje
    connection.on("RecibirMensaje", (emisorId, mensaje, esPropio) => {
        const nombre = esPropio ? "Tú" : nombreReceptor;
        agregarMensaje(nombre, mensaje, esPropio);

        if (esPropio) {
            // Si envío, hacer scroll automático abajo
            mensajesLista.scrollTop = mensajesLista.scrollHeight;
        } else {
            // Si recibo y no estoy abajo, mostrar botón
            if (!estaScrolleadoAbajo()) {
                nuevoMensajeBtn.classList.remove('d-none');
            } else {
                // Si estoy abajo, scroll automático
                mensajesLista.scrollTop = mensajesLista.scrollHeight;
            }
        }
    });

    // Click en botón para bajar al final
    nuevoMensajeBtn.addEventListener('click', () => {
        mensajesLista.scrollTop = mensajesLista.scrollHeight;
        nuevoMensajeBtn.classList.add('d-none');
    });

    // Función para agregar mensaje
    function agregarMensaje(nombre, texto, esPropio) {
        const li = document.createElement("li");
        li.className = `mb-3 d-flex justify-content-${esPropio ? "end" : "start"}`;

        const estadoTexto = esPropio ? "Enviado" : "Recibido";

        li.innerHTML = `
            <div class="d-flex align-items-start ${esPropio ? "text-end" : ""}">
                ${!esPropio ? '<i class="bi bi-person-circle fs-3 me-2 text-primary"></i>' : ""}
                <div>
                    <div class="fw-semibold d-flex justify-content-between align-items-center">
                        <span>${escapeHtml(nombre)}</span>
                        <small class="text-muted ms-2" style="font-size: 0.75rem;">${estadoTexto}</small>
                    </div>
                    <div class="${esPropio ? "bg-primary text-white" : "bg-light"} rounded p-2 shadow-sm">
                        ${escapeHtml(texto)}
                    </div>
                </div>
                ${esPropio ? '<i class="bi bi-person-circle fs-3 ms-2 text-secondary"></i>' : ""}
            </div>
        `;
        mensajesLista.appendChild(li);
    }

    // Verifica si scroll está abajo (dentro de un margen de 10px)
    function estaScrolleadoAbajo() {
        return mensajesLista.scrollTop + mensajesLista.clientHeight >= mensajesLista.scrollHeight - 10;
    }

    // Escapar texto para evitar inyección HTML
    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    // Iniciar conexión SignalR
    connection.start().catch(err => console.error("SignalR error:", err));
});
