document.addEventListener("DOMContentLoaded", () => {

    const itemsBody = document.getElementById("itemsBody");
    const addDishBtn = document.getElementById("addDishBtn");
    const grandTotal = document.getElementById("grandTotal");

    const allDishes = JSON.parse(document.getElementById("allDishesJson").value);

    function recalcTotals() {
        let total = 0;

        document.querySelectorAll(".order-row").forEach(row => {
            const select = row.querySelector(".dish-select");
            const qty = row.querySelector(".qty-input");
            const price = parseFloat(
                select.selectedOptions[0].dataset.price.replace(",", ".")
            );

            const rowTotal = price * parseInt(qty.value);
            row.querySelector(".row-total").innerText = rowTotal.toFixed(2) + " €";

            total += rowTotal;
        });

        grandTotal.innerText = total.toFixed(2) + " €";
    }

    addDishBtn.addEventListener("click", () => {
        const index = itemsBody.querySelectorAll("tr").length;

        let options = "";
        allDishes.forEach(d => {
            options += `<option value="${d.Id}" data-price="${d.Price}">${d.Name}</option>`;
        });

        const firstDish = allDishes[0];

        const newRow = document.createElement("tr");
        newRow.classList.add("order-row");

        newRow.innerHTML = `
            <td>
                <select name="Items[${index}].DishId" class="figusta-select dish-select">
                    ${options}
                </select>
            </td>

            <td>
                <input type="number" name="Items[${index}].Quantity"
                       class="figusta-input qty-input" value="1" min="1" />
            </td>

            <td class="price-cell">${firstDish.Price.toFixed(2)} €</td>

            <td class="row-total">${firstDish.Price.toFixed(2)} €</td>

            <td>
                <input type="text"
                       name="Items[${index}].Note"
                       class="figusta-input note-input"
                       placeholder="Note..." />
            </td>

            <td>
                <button type="button" class="figusta-btn-delete-small remove-row">
                    <i class="bi bi-x-circle"></i>
                </button>
            </td>
        `;

        itemsBody.appendChild(newRow);
        recalcTotals();
    });

    itemsBody.addEventListener("change", (e) => {

        if (e.target.classList.contains("dish-select")) {
            const row = e.target.closest("tr");
            const price = parseFloat(
                e.target.selectedOptions[0].dataset.price.replace(",", ".")
            );

            row.querySelector(".price-cell").innerText = price.toFixed(2) + " €";

            const qty = parseInt(row.querySelector(".qty-input").value);
            row.querySelector(".row-total").innerText = (price * qty).toFixed(2) + " €";

            recalcTotals();
        }

        if (e.target.classList.contains("qty-input")) {
            const row = e.target.closest("tr");
            const price = parseFloat(
                row.querySelector(".dish-select").selectedOptions[0].dataset.price.replace(",", ".")
            );

            const qty = parseInt(e.target.value);
            row.querySelector(".row-total").innerText = (price * qty).toFixed(2) + " €";

            recalcTotals();
        }
    });

    itemsBody.addEventListener("click", (e) => {
        if (e.target.closest(".remove-row")) {
            e.target.closest("tr").remove();
            recalcTotals();
        }
    });

    recalcTotals();
});
