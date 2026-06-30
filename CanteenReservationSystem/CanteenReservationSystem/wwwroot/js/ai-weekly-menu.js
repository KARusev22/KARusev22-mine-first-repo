// AI weekly menu planner — runs entirely via fetch so the page stays responsive.
(function () {
    const form = document.getElementById("ai-weekly-form");
    const btn = document.getElementById("ai-generate-btn");
    const result = document.getElementById("ai-weekly-result");
    if (!form) return;

    const esc = (s) => (s == null ? "" : String(s).replace(/[&<>"']/g, c =>
        ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])));

    form.addEventListener("submit", async function () {
        const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
        const fd = new FormData(form);

        btn.disabled = true;
        const original = btn.innerHTML;
        btn.innerHTML = '<span class="ai-spinner"></span> Generating…';
        result.innerHTML = '<div class="ai-loading"><span class="ai-spinner ai-spinner--dark"></span> Building your weekly plan…</div>';

        try {
            const res = await fetch("/Ai/WeeklyMenu", {
                method: "POST",
                headers: { "RequestVerificationToken": token },
                body: fd
            });
            const data = await res.json();
            render(data);
        } catch (e) {
            result.innerHTML = '<div class="ai-error">Something went wrong while contacting the AI. Please try again.</div>';
        } finally {
            btn.disabled = false;
            btn.innerHTML = original;
        }
    });

    function render(payload) {
        if (!payload || !payload.ok || !payload.data) {
            result.innerHTML = '<div class="ai-error">' + esc(payload && payload.error ? payload.error : "No plan was generated.") + '</div>';
            return;
        }
        const plan = payload.data;
        let html = '<div class="ai-plan-grid">';
        (plan.days || []).forEach(day => {
            html += '<div class="ai-day-card"><div class="ai-day-head"><span>' + esc(day.day) + '</span>';
            if (day.totalCalories) html += '<span class="kcal">' + esc(day.totalCalories) + ' kcal</span>';
            html += '</div><div class="ai-day-body">';
            (day.items || []).forEach(item => {
                html += '<div class="ai-meal"><div class="ai-meal-name">' + esc(item.dish) + '</div>';
                if (item.category) html += '<div class="ai-meal-cat">' + esc(item.category) + '</div>';
                if (item.reason) html += '<div class="ai-meal-reason">' + esc(item.reason) + '</div>';
                html += '</div>';
            });
            if (day.note) html += '<div class="ai-day-note">' + esc(day.note) + '</div>';
            html += '</div></div>';
        });
        html += '</div>';
        if (plan.summary) html += '<div class="ai-summary"><i class="bi bi-lightbulb"></i> ' + esc(plan.summary) + '</div>';
        result.innerHTML = html;
    }
})();
