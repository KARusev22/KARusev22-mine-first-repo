// Admin poll AI suggestion — fills the form, never blocks it.
(function () {
    const btn = document.getElementById("ai-poll-btn");
    const status = document.getElementById("ai-poll-status");
    if (!btn) return;

    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const esc = (s) => (s == null ? "" : String(s).replace(/[&<>"']/g, c =>
        ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c])));

    btn.addEventListener("click", async function () {
        btn.disabled = true;
        const original = btn.innerHTML;
        btn.innerHTML = '<span class="ai-spinner"></span> Thinking…';
        status.innerHTML = '<div class="ai-loading"><span class="ai-spinner ai-spinner--dark"></span> Analyzing orders & comments…</div>';

        try {
            const fd = new FormData();
            const res = await fetch("/Ai/PollSuggestion", {
                method: "POST",
                headers: { "RequestVerificationToken": tokenEl ? tokenEl.value : "" },
                body: fd
            });
            const data = await res.json();
            if (!data || !data.ok || !data.data) {
                status.innerHTML = '<div class="ai-error">' + esc(data && data.error ? data.error : "No suggestion generated.") + '</div>';
                return;
            }
            fillForm(data.data);
            status.innerHTML = '<div class="ai-summary"><i class="bi bi-check-circle"></i> Suggestion applied — review and edit before creating.</div>';
        } catch (e) {
            status.innerHTML = '<div class="ai-error">Could not reach the AI service. Please try again.</div>';
        } finally {
            btn.disabled = false;
            btn.innerHTML = original;
        }
    });

    function fillForm(s) {
        const q = document.getElementById("poll-question");
        if (q) q.value = s.question || "";

        const container = document.getElementById("options");
        container.innerHTML = "";
        const opts = (s.options && s.options.length) ? s.options : ["", ""];
        opts.forEach(o => {
            const input = document.createElement("input");
            input.className = "form-control mb-2";
            input.name = "options";
            input.value = o;
            container.appendChild(input);
        });
    }
})();
