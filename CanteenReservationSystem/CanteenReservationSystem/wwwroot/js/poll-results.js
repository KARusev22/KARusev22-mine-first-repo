document.addEventListener("DOMContentLoaded", () => {
    const labels = window.pollData.labels;
    const votes = window.pollData.votes;

    new Chart(document.getElementById("pollChart"), {
        type: "bar",
        data: {
            labels,
            datasets: [{
                label: "Votes",
                data: votes,
                backgroundColor: "rgba(46, 204, 113, 0.35)",
                borderColor: "rgba(39, 174, 96, 0.9)",
                borderWidth: 2,
                borderRadius: 10,
                barThickness: 40,
                maxBarThickness: 50,
                hoverBackgroundColor: "rgba(46, 204, 113, 0.55)"
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 900 },
            plugins: { legend: { display: false } },
            scales: {
                y: { beginAtZero: true, ticks: { stepSize: 1 } },
                x: { grid: { display: false } }
            }
        }
    });

    function animateValue(el, start, end, duration) {
        let range = end - start;
        let current = start;
        let increment = end > start ? 1 : -1;
        let stepTime = Math.abs(Math.floor(duration / range));

        let timer = setInterval(() => {
            current += increment;
            el.textContent = current + "%";
            if (current == end) clearInterval(timer);
        }, stepTime);
    }

    const totalVotes = votes.reduce((a, b) => a + b, 0);
    const maxPossible = labels.length * 10;
    const score = Math.min(100, Math.round((totalVotes / maxPossible) * 100));

    const scoreEl = document.getElementById("engScore");
    const barFill = document.getElementById("engBarFill");
    const levelEl = document.getElementById("engLevel");
    const trendEl = document.getElementById("engTrend");

    animateValue(scoreEl, 0, score, 800);

    barFill.style.width = score + "%";

    if (score < 30) {
        levelEl.textContent = "Low Participation";
        trendEl.textContent = "↓";
        trendEl.style.color = "#c0392b";
    } else if (score < 70) {
        levelEl.textContent = "Moderate Participation";
        trendEl.textContent = "→";
        trendEl.style.color = "#f1c40f";
    } else {
        levelEl.textContent = "High Participation";
        trendEl.textContent = "↑";
        trendEl.style.color = "#27ae60";
    }
});