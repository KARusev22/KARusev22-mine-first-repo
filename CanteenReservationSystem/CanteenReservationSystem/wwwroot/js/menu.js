(function () {
    const categorySelect = document.getElementById("figusta-category-filter");
    const searchInput = document.getElementById("figusta-search-input");
    const sortSelect = document.getElementById("figusta-sort-select");
    const grid = document.getElementById("figusta-menu-grid");
    const emptyState = document.getElementById("figusta-empty-state");
    const cards = Array.from(document.querySelectorAll(".figusta-dish-card[data-category]"));

    if (!grid || cards.length === 0) return;

    const getCardValue = (card, name) => card.getAttribute(name) ?? "";

    const filterAndSort = () => {
        const selectedCategory = categorySelect?.value ?? "all";
        const searchTerm = searchInput?.value.trim().toLowerCase() ?? "";
        const sortOrder = sortSelect?.value ?? "asc";

        cards.forEach(card => {
            const category = getCardValue(card, "data-category");
            const dishName = getCardValue(card, "data-name");
            const matchesCategory = selectedCategory === "all" || category === selectedCategory;
            const matchesSearch = searchTerm === "" || dishName.includes(searchTerm);
            const visible = matchesCategory && matchesSearch;

            card.classList.toggle("is-hidden", !visible);
        });

        const visibleCards = cards.filter(card => !card.classList.contains("is-hidden"));

        visibleCards.sort((a, b) => {
            const aPrice = parseFloat(getCardValue(a, "data-price")) || 0;
            const bPrice = parseFloat(getCardValue(b, "data-price")) || 0;
            return sortOrder === "desc" ? bPrice - aPrice : aPrice - bPrice;
        });

        visibleCards.forEach(card => grid.appendChild(card));

        if (emptyState) {
            emptyState.classList.toggle("visually-hidden", visibleCards.length > 0);
        }
    };

    categorySelect?.addEventListener("change", filterAndSort);
    searchInput?.addEventListener("input", filterAndSort);
    sortSelect?.addEventListener("change", filterAndSort);

    filterAndSort();
})();