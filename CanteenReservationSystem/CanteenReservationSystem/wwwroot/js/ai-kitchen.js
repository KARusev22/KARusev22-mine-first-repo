// Kitchen AI assistant + inventory auto-save. All async — never blocks the dashboard.
(function () {
    const token = () => {
        const el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    };
    const esc = (s) => (s == null ? "" : String(s).replace(/[&<>"']/g, c =>
        ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])));

    // ---- AI availability analysis ----
    const btn = document.getElementById("ai-kitchen-btn");
    const result = document.getElementById("ai-kitchen-result");

    if (btn) {
        btn.addEventListener("click", async function () {
            const date = btn.getAttribute("data-date");
            btn.disabled = true;
            const original = btn.innerHTML;
            btn.innerHTML = '<span class="ai-spinner"></span> Analyzing…';
            result.innerHTML = '<div class="ai-loading"><span class="ai-spinner ai-spinner--dark"></span> Checking stock against all orders…</div>';

            try {
                const fd = new FormData();
                fd.append("date", date);
                const res = await fetch("/Ai/KitchenInsights", {
                    method: "POST",
                    headers: { "RequestVerificationToken": token() },
                    body: fd
                });
                const data = await res.json();
                renderInsight(data);
            } catch (e) {
                result.innerHTML = '<div class="ai-error">Could not reach the AI service. Please try again.</div>';
            } finally {
                btn.disabled = false;
                btn.innerHTML = original;
            }
        });
    }

    function renderInsight(payload) {
        if (!payload || !payload.ok || !payload.data) {
            result.innerHTML = '<div class="ai-error">' + esc(payload && payload.error ? payload.error : "No analysis available.") + '</div>';
            return;
        }
        const d = payload.data;
        const ok = (d.verdict || "").toLowerCase() === "ok";
        let html = '<div class="ai-verdict ' + (ok ? "ai-verdict--ok" : "ai-verdict--shortage") + '">' +
            '<i class="bi ' + (ok ? "bi-check-circle-fill" : "bi-exclamation-triangle-fill") + '"></i> ' +
            (ok ? "All ingredients available" : "Shortage detected") + '</div>';
        html += '<div>' + esc(d.summary) + '</div>';

        if (d.shortages && d.shortages.length) {
            html += '<table class="ai-shortage-table"><thead><tr><th>Ingredient</th><th>Needed</th><th>Available</th><th>Deficit</th></tr></thead><tbody>';
            d.shortages.forEach(s => {
                html += '<tr><td>' + esc(s.ingredient) + '</td><td>' + esc(s.needed) + ' g</td><td>' +
                    esc(s.available) + ' g</td><td class="deficit">-' + esc(s.deficit) + ' g</td></tr>';
            });
            html += '</tbody></table>';
        }
        if (d.notes && d.notes.length) {
            html += '<ul class="ai-note-list">';
            d.notes.forEach(n => html += '<li>' + esc(n) + '</li>');
            html += '</ul>';
        }
        result.innerHTML = html;
    }

    // ---- Inventory auto-save on change ----
    document.querySelectorAll(".ai-stock-input").forEach(input => {
        input.addEventListener("change", async function () {
            const id = input.getAttribute("data-ingredient-id");
            const val = Math.max(0, parseInt(input.value, 10) || 0);
            input.value = val;
            input.style.outline = "2px solid #facc15";
            try {
                const fd = new FormData();
                fd.append("ingredientId", id);
                fd.append("availableGrams", val);
                const res = await fetch("/Kitchen/UpdateStock", {
                    method: "POST",
                    headers: { "RequestVerificationToken": token() },
                    body: fd
                });
                const data = await res.json();
                input.style.outline = data && data.ok ? "2px solid #22c55e" : "2px solid #ef4444";
            } catch (e) {
                input.style.outline = "2px solid #ef4444";
            }
            setTimeout(() => { input.style.outline = "none"; }, 900);
        });
    });
})();
