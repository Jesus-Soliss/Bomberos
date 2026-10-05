// Confirmación de acciones críticas: un <form data-confirm="mensaje"> muestra el modal antes de enviarse.
(function () {
    const modalEl = document.getElementById('modalConfirmar');
    if (!modalEl) return;

    const modal = new bootstrap.Modal(modalEl);
    const mensaje = document.getElementById('modalConfirmarMensaje');
    const aceptar = document.getElementById('modalConfirmarAceptar');
    let pendiente = null;

    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form.dataset.confirm) return;

        e.preventDefault();
        pendiente = form;
        mensaje.textContent = form.dataset.confirm;
        modal.show();
    });

    aceptar.addEventListener('click', function () {
        if (!pendiente) return;
        aceptar.disabled = true;
        pendiente.submit();   // submit() no vuelve a disparar el evento, así que no se pide confirmar dos veces
    });

    modalEl.addEventListener('hidden.bs.modal', function () {
        pendiente = null;
        aceptar.disabled = false;
    });
})();
