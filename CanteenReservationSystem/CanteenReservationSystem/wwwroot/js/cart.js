document.addEventListener("DOMContentLoaded", () => {

    // REMOVE ITEM
    window.removeItem = function (cartItemId) {
        fetch("/Cart/Remove", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: `cartItemId=${cartItemId}`
        }).then(() => {
            document.querySelector(`.figusta-cart-item[data-id="${cartItemId}"]`)?.remove();
            recalcTotal();
        });
    };

    // RECALCULATE TOTAL
    function recalcTotal() {
        let total = 0;

        document.querySelectorAll(".figusta-cart-item").forEach(item => {
            const checkbox = item.querySelector(".figusta-check");
            if (!checkbox || !checkbox.checked) return;

            const price = parseFloat(item.querySelector(".figusta-dish-price").dataset.price);
            const qty = parseInt(item.querySelector(".figusta-qty").value);

            if (!isNaN(price) && !isNaN(qty)) {
                total += price * qty;
            }
        });

        const totalElement = document.getElementById("figusta-total");
        if (totalElement) totalElement.innerText = total.toFixed(2);
    }

    // QUANTITY UPDATE
    document.querySelectorAll(".figusta-qty").forEach(input => {
        input.addEventListener("change", () => {
            const id = input.dataset.id;
            const quantity = input.value;

            fetch("/Cart/Update", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `dishId=${id}&quantity=${quantity}`
            }).then(() => {
                recalcTotal();
            });
        });
    });

    // NOTE UPDATE
    document.querySelectorAll(".figusta-note").forEach(textarea => {
        textarea.addEventListener("blur", () => {
            const id = textarea.dataset.id;
            const note = textarea.value;

            fetch("/Cart/Update", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `dishId=${id}&note=${encodeURIComponent(note)}`
            });
        });
    });

    // CHECKBOX UPDATE
    document.querySelectorAll(".figusta-check")
        .forEach(cb => cb.addEventListener("change", recalcTotal));

    recalcTotal();
});