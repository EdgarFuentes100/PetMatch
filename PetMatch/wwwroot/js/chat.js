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

    document.querySelectorAll('.abrir-chat').forEach(btn => {
        btn.addEventListener('click', async () => {
            nombreReceptor = btn.getAttribute('data-nombre');
            const receptorId = btn.getAttribute('data-id');

            chatNombre.textContent = nombreReceptor;
            mensajesLista.innerHTML = '';
            mensajeInput.value = '';
            chatModal.show();

            // Solo seleccionas al receptor. No guardas ni usas IDs en JS.
            await connection.invoke("SeleccionarReceptor", parseInt(receptorId));
        });
    });

    enviarBtn.addEventListener('click', async () => {
        const texto = mensajeInput.value.trim();
        if (!texto) return;

        await connection.invoke("EnviarMensaje", texto);
        mensajeInput.value = '';
    });

    connection.on("RecibirMensaje", (emisorId, mensaje, esPropio) => {
        const nombre = esPropio ? "Tú" : nombreReceptor;
        agregarMensaje(nombre, mensaje, esPropio);
    });

    connection.start().catch(err => console.error("SignalR error:", err));

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

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }
});
