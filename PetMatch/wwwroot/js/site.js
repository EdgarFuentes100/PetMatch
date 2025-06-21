/*habre el modal de busqueda avanzada*/
document.addEventListener('DOMContentLoaded', function () {
    var modalBuscar = new bootstrap.Modal(document.getElementById('modalBuscar'));

    document.querySelectorAll('.abrir-modal-buscar').forEach(function (el) {
        el.addEventListener('click', function () {
            modalBuscar.show();
        });
        el.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                modalBuscar.show();
            }
        });
    });
});

/*habre el ver mas en las publicaciones*/
document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll(".historia-container").forEach(container => {
        const text = container.querySelector(".historia-recortada");
        const btn = container.querySelector(".btn-ver-mas");

        // Verificamos si el contenido tiene desbordamiento (más de 2 líneas)
        if (text.scrollHeight > text.clientHeight + 5) {
            btn.style.display = "inline";
        } else {
            btn.style.display = "none";
        }

        // Manejar clic en ver más / ver menos
        btn.addEventListener("click", function () {
            const expanded = text.classList.toggle("expanded");
            this.textContent = expanded ? "Ver menos" : "Ver más";
        });
    });
});