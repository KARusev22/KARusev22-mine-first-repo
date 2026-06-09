document.addEventListener("DOMContentLoaded", () => {
    const addBtn = document.getElementById("add-ingredient-btn");
    const container = document.getElementById("ingredients-container");

    if (addBtn && container) {

        let index = container.querySelectorAll(".row").length;

        addBtn.addEventListener("click", () => {
            console.log("Add ingredient clicked");

            const row = document.createElement("div");
            row.className = "row mb-2";

            row.innerHTML = `
                <input type="hidden" name="IngredientIds[${index}]" value="0" />
                <div class="col">
                    <input class="form-control" name="IngredientNames[${index}]" placeholder="Ingredient name" />
                </div>
                <div class="col">
                    <input class="form-control" name="IngredientGrams[${index}]" placeholder="Grams" />
                </div>
            `;

            container.appendChild(row);
            index++;
        });
    }

    const deleteButtons = document.querySelectorAll(".delete-btn");
    const deleteForm = document.getElementById("deleteForm");
    const dishNameSpan = document.getElementById("dishName");

    if (deleteButtons && deleteForm && dishNameSpan) {

        deleteButtons.forEach(btn => {
            btn.addEventListener("click", () => {
                const id = btn.dataset.id;
                const name = btn.dataset.name;

                dishNameSpan.textContent = name;
                deleteForm.action = `/AdminDishes/Delete/${id}`;
            });
        });
    }

    const restoreButtons = document.querySelectorAll(".restore-btn");
    const restoreForm = document.getElementById("restoreForm");
    const restoreDishName = document.getElementById("restoreDishName");

    if (restoreButtons && restoreForm && restoreDishName) {
        restoreButtons.forEach(btn => {
            btn.addEventListener("click", () => {
                const id = btn.dataset.id;
                const name = btn.dataset.name;

                restoreDishName.textContent = name;
                restoreForm.action = `/AdminDishes/Restore/${id}`;
            });
        });
    }
});


