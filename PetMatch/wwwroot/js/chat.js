const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

let nombreReceptor = null;
let receptorId = null;
let userId = null; // ID del usuario actual

document.addEventListener('DOMContentLoaded', () => {
    const chatModalEl = document.getElementById('chatModal');
    const chatModal = new bootstrap.Modal(chatModalEl);
    const chatNombre = document.getElementById('chatNombre');
    const mensajesLista = chatModalEl.querySelector('#mensajesLista');
    const mensajeInput = document.getElementById('mensajeInput');
    const enviarBtn = document.getElementById('enviarBtn');
    const nuevoMensajeBtn = document.getElementById('nuevoMensajeBtn');

    mensajesLista.addEventListener('scroll', () => {
        if (estaScrolleadoAbajo()) {
            nuevoMensajeBtn.classList.add('d-none');
        }
    });

    function abrirModalChat(id, nombre) {
        receptorId = id;
        nombreReceptor = nombre;

        chatNombre.textContent = nombreReceptor;
        mensajesLista.innerHTML = '';
        mensajeInput.value = '';
        chatModal.show();

        chatModalEl.addEventListener('shown.bs.modal', async function handleShown() {
            chatModalEl.removeEventListener('shown.bs.modal', handleShown);
            try {
                await connection.invoke("SeleccionarReceptor", receptorId);
                const result = await connection.invoke("ObtenerHistorial", receptorId);

                userId = result.userId;

                result.mensajes.forEach(m => {
                    const nombreMsg = m.esPropio ? "Tú" : nombreReceptor;
                    agregarMensaje(nombreMsg, m.contenido, m.esPropio, m.leido);
                });

                mensajesLista.scrollTop = mensajesLista.scrollHeight;
                await connection.invoke("MarcarMensajesComoLeidos", receptorId, userId);
            } catch (err) {
                console.error("Error cargando historial:", err);
            }
        });
    }

    document.querySelectorAll('.abrir-chat').forEach(btn => {
        btn.addEventListener('click', () => {
            const id = parseInt(btn.getAttribute('data-id'));
            const nombre = btn.getAttribute('data-nombre');
            abrirModalChat(id, nombre);
        });
    });

    chatModalEl.addEventListener('hidden.bs.modal', async () => {
        try {
            await connection.invoke('CerrarChat');
        } catch (err) {
            console.error('Error al cerrar chat:', err);
        }
    });

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

    connection.on("RecibirMensaje", (emisorId, mensaje, esPropio, leido) => {
        const nombre = esPropio ? "Tú" : nombreReceptor;
        agregarMensaje(nombre, mensaje, esPropio, leido);

        if (!esPropio && !estaScrolleadoAbajo()) {
            nuevoMensajeBtn.classList.remove('d-none');
        } else {
            mensajesLista.scrollTop = mensajesLista.scrollHeight;
        }
    });

    connection.on("ActualizarEstadoLeidos", (emisorId) => {
        const mensajes = mensajesLista.querySelectorAll('li[data-espropio="true"] .check-leido');
        mensajes.forEach(span => span.textContent = '✔✔');
    });

    nuevoMensajeBtn.addEventListener('click', () => {
        mensajesLista.scrollTop = mensajesLista.scrollHeight;
        nuevoMensajeBtn.classList.add('d-none');
    });

    connection.on("RecibirMensajeNuevo", async () => {
        await actualizarListaChats();
    });

    connection.on("ActualizarListaChats", async () => {
        console.log("Evento ActualizarListaChats recibido");
        await actualizarListaChats();
    });

    async function actualizarListaChats() {
        try {
            const chats = await connection.invoke("ObtenerMisChats");
            const lista = document.getElementById("lista-chats");
            const userId = parseInt(document.getElementById('userIdHidden').value);
            lista.innerHTML = "";

            chats.forEach(chat => {
                const esNoLeido = (chat.receptorId === userId && !chat.leido);
                const estadoMensaje = chat.emisorId === userId ? "Enviado" : "Recibido";

                const li = document.createElement("li");
                li.className = "list-group-item d-flex justify-content-between align-items-center";
                li.setAttribute("data-id", chat.otroUsuarioId);

                li.innerHTML = `
                    <div>
                        <strong ${esNoLeido ? 'class="text-success fw-bold"' : ''}>${chat.otroNombre}</strong>
                        ${esNoLeido ? '<span class="badge bg-success ms-2">Nuevo mensaje</span>' : ''}
                        <br />
                        <small class="text-muted">${estadoMensaje} - <span style="font-style: italic;">${chat.contenido}</span></small>
                    </div>
                    <a class="btn btn-sm btn-primary abrir-chat"
                       data-id="${chat.otroUsuarioId}"
                       data-nombre="${chat.otroNombre}">
                        <i class="bi bi-chat-dots"></i> Abrir chat
                    </a>
                `;

                lista.appendChild(li);
            });

            document.querySelectorAll('.abrir-chat').forEach(btn => {
                btn.addEventListener('click', () => {
                    const id = parseInt(btn.getAttribute('data-id'));
                    const nombre = btn.getAttribute('data-nombre');
                    abrirModalChat(id, nombre);
                });
            });
        } catch (error) {
            console.error("Error al actualizar chats:", error);
        }
    }

    function agregarMensaje(nombre, texto, esPropio, leido = false) {
        const li = document.createElement("li");
        li.className = `mb-3 d-flex justify-content-${esPropio ? "end" : "start"}`;
        li.setAttribute('data-espropio', esPropio.toString());

        const estadoTexto = esPropio ? "Enviado" : "Recibido";
        const check = esPropio ? (leido ? '✔✔' : '✔') : '';

        li.innerHTML = `
            <div class="d-flex align-items-start ${esPropio ? "text-end" : ""}">
                ${!esPropio ? '<i class="bi bi-person-circle fs-3 me-2 text-primary"></i>' : ""}
                <div>
                    <div class="fw-semibold d-flex justify-content-between align-items-center">
                        <span>${escapeHtml(nombre)}</span>
                        <small class="text-muted ms-2" style="font-size: 0.75rem;">
                            ${estadoTexto} ${esPropio ? `<span class="check-leido">${check}</span>` : ''}
                        </small>
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

    function estaScrolleadoAbajo() {
        return mensajesLista.scrollTop + mensajesLista.clientHeight >= mensajesLista.scrollHeight - 10;
    }

    function escapeHtml(text) {
        const div = document.createElement("div");
        div.textContent = text;
        return div.innerHTML;
    }

    connection.start()
        .then(() => {
            actualizarListaChats(); // 👈 Cargar lista inicial
        })
        .catch(err => console.error("SignalR error:", err));
});