const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

let nombreReceptor = null;

document.addEventListener('DOMContentLoaded', () => {
    const chatModalEl = document.getElementById('chatModal');
    const chatModal = new bootstrap.Modal(chatModalEl);
    const chatNombre = document.getElementById('chatNombre');
    const mensajesLista = document.getElementById('mensajesLista');
    const mensajeInput = document.getElementById('mensajeInput');
    const enviarBtn = document.getElementById('enviarBtn');

    // Botones para abrir chat
    document.querySelectorAll('.abrir-chat').forEach(btn => {
        btn.addEventListener('click', async () => {
            nombreReceptor = btn.getAttribute('data-nombre');
            const receptorId = parseInt(btn.getAttribute('data-id'));

            chatNombre.textContent = nombreReceptor;
            mensajesLista.innerHTML = '';
            mensajeInput.value = '';
            chatModal.show();

            try {
                // Establecer sesión
                await connection.invoke("SeleccionarReceptor", receptorId);

                // Traer historial ya marcado con EsPropio por el servidor
                const historial = await connection.invoke("ObtenerHistorial", receptorId);

                historial.forEach(mensaje => {
                    const nombre = mensaje.esPropio ? "Tú" : nombreReceptor;
                    agregarMensaje(nombre, mensaje.contenido, mensaje.esPropio);
                });
            } catch (err) {
                console.error("Error cargando historial:", err);
            }
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

    // Escuchar mensajes
    connection.on("RecibirMensaje", (emisorId, mensaje, esPropio) => {
        const nombre = esPropio ? "Tú" : nombreReceptor;
        agregarMensaje(nombre, mensaje, esPropio);
    });

    // Iniciar conexión
    connection.start().catch(err => console.error("SignalR error:", err));

    // Función de UI para mostrar mensaje
    function agregarMensaje(nombre, texto, esPropio) {
        const li = document.createElement("li");
        li.className = `mb-3 d-flex justify-content-${esPropio ? "end" : "start"}`;
        li.innerHTML = `
            <div class="d-flex align-items-start ${esPropio ? "text-end" : ""}">
                ${!esPropio ? '<i class="bi bi-person-circle fs-3 me-2 text-primary"></i>' : ""}
                <div>
                    <div class="fw-semibold">${escapeHtml(nombre)}</div>
                    <div class="${esPropio ? "bg-primary text-white" : "bg-light"} rounded p-2 shadow-sm">
                        ${escapeHtml(texto)}
                    </div>
                </div>
                ${esPropio ? '<i class="bi bi-person-circle fs-3 ms-2 text-secondary"></i>' : ""}
            </div>
        `;
        mensajesLista.appendChild(li);
        mensajesLista.scrollTop = mensajesLista.scrollHeight;
    }

    // Evita inyecciones de HTML
    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
});
