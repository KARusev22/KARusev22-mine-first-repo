// Cashier AI pickup summary — async, never blocks the counter workflow.
(function () {
    const btn = document.getElementById("ai-cashier-btn");
    const result = document.getElementById("ai-cashier-result");
    if (!btn) return;

    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const esc = (s) => (s == null ? "" : String(s).replace(/[&<>"']/g, c =>
        ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])));

    btn.addEventListener("click", async function () {
        const code = btn.getAttribute("data-code");
        btn.disabled = true;
        const original = btn.innerHTML;
        btn.innerHTML = '<span class="ai-spinner"></span> Summarizing…';
        result.innerHTML = '<div class="ai-loading"><span class="ai-spinner ai-spinner--dark"></span> Preparing handover summary…</div>';

        try {
            const fd = new FormData();
            fd.append("code", code);
            const res = await fetch("/Ai/CashierSummary", {
                method: "POST",
                headers: { "RequestVerificationToken": tokenEl ? tokenEl.value : "" },
                body: fd
            });
            const data = await res.json();
            render(data);
        } catch (e) {
            result.innerHTML = '<div class="ai-error">Could not reach the AI service. Please try again.</div>';
        } finally {
            btn.disabled = false;
            btn.innerHTML = original;
        }
    });

    function render(payload) {
        if (!payload || !payload.ok || !payload.data) {
            result.innerHTML = '<div class="ai-error">' + esc(payload && payload.error ? payload.error : "No summary available.") + '</div>';
            return;
        }
        const d = payload.data;
        let html = '<div class="ai-summary"><i class="bi bi-chat-quote"></i> ' + esc(d.summary) + '</div>';
        if (d.alerts && d.alerts.length) {
            html += '<ul class="ai-note-list">';
            d.alerts.forEach(a => html += '<li>' + esc(a) + '</li>');
            html += '</ul>';
        }
        result.innerHTML = html;
    }
})();
