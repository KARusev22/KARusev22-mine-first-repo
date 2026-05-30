function confirmDelete(orderId, btn) {
    if (!confirm("Are you sure you want to delete this order?")) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch(`/Orders/DeleteConfirmed/${orderId}`, {
        method: "POST",
        headers: {
            "RequestVerificationToken": token
        }
    })
        .then(r => {
            if (r.ok) {
                btn.closest(".figusta-order-card").remove();
            }
        });
}