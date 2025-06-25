const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

let receptorId = null;

document.addEventListener('DOMContentLoaded', () => {
    const chatModalEl = document.getElementById('chatModal');
    const chatModal = new bootstrap.Modal(chatModalEl);
    const chatNombre = document.getElementById('chatNombre');
    const mensajesLista = document.getElementById('mensajesLista');
    const mensajeInput = document.getElementById('mensajeInput');
    const enviarBtn = document.getElementById('enviarBtn');

    document.querySelectorAll('.abrir-chat').forEach(btn => {
        btn.addEventListener('click', () => {
            const nombre = btn.getAttribute('data-nombre');
            receptorId = btn.getAttribute('data-id');

            chatNombre.textContent = nombre;
            mensajesLista.innerHTML = '';
            mensajeInput.value = '';
            chatModal.show();
        });
    });

    enviarBtn.addEventListener('click', async () => {
        const texto = mensajeInput.value.trim();
        if (!texto || !receptorId) return;

        await connection.invoke("EnviarMensaje", receptorId, texto);

        const li = document.createElement('li');
        li.textContent = `Tú: ${texto}`;
        li.classList.add('text-end', 'mb-2');
        mensajesLista.appendChild(li);

        mensajeInput.value = '';
        mensajesLista.scrollTop = mensajesLista.scrollHeight;
    });

    connection.on("RecibirMensaje", (emisorId, mensaje) => {
        const li = document.createElement('li');
        li.textContent = `${emisorId === receptorId ? chatNombre.textContent : 'Tú'}: ${mensaje}`;
        li.classList.add(emisorId === receptorId ? 'text-start' : 'text-end', 'mb-2');
        mensajesLista.appendChild(li);
        mensajesLista.scrollTop = mensajesLista.scrollHeight;
    });

    connection.start().catch(err => console.error(err.toString()));
});
