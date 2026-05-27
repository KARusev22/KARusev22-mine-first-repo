(function () {
  const filterSelect = document.getElementById("figusta-category-filter");
  const cards = document.querySelectorAll(".figusta-dish-card[data-category]");

  if (!filterSelect || cards.length === 0) {
    return;
  }

  filterSelect.addEventListener("change", function () {
    const selected = filterSelect.value;

    cards.forEach(function (card) {
      const category = card.getAttribute("data-category");
      const show = selected === "all" || category === selected;
      card.classList.toggle("is-hidden", !show);
    });
  });
})();
