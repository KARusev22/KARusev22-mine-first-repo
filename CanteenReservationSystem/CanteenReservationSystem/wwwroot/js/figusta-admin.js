(function () {
  if (typeof Chart === "undefined") {
    return;
  }

  const green = "#68b063";
  const greenFill = "rgba(104, 176, 99, 0.12)";
  const gridColor = "#e5e7eb";
  const textMuted = "#777777";

  Chart.defaults.font.family = "'Inter', system-ui, sans-serif";
  Chart.defaults.color = textMuted;

  const revenueCanvas = document.getElementById("figusta-revenue-chart");
  if (revenueCanvas) {
    new Chart(revenueCanvas, {
      type: "line",
      data: {
        labels: ["Jan", "Feb", "Mar", "Apr", "May"],
        datasets: [
          {
            data: [12500, 15800, 18200, 22100, 26500],
            borderColor: green,
            backgroundColor: greenFill,
            borderWidth: 2,
            pointBackgroundColor: green,
            pointBorderColor: green,
            pointRadius: 4,
            tension: 0.35,
            fill: true
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            grid: { color: gridColor },
            border: { display: false }
          },
          y: {
            beginAtZero: true,
            max: 26000,
            ticks: { stepSize: 6500 },
            grid: { color: gridColor },
            border: { display: false }
          }
        }
      }
    });
  }

  const peakCanvas = document.getElementById("figusta-peak-days-chart");
  if (peakCanvas) {
    new Chart(peakCanvas, {
      type: "bar",
      data: {
        labels: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        datasets: [
          {
            data: [45, 52, 58, 62, 78, 92, 75],
            backgroundColor: green,
            borderRadius: 4,
            barThickness: 28
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            grid: { display: false },
            border: { display: false }
          },
          y: {
            beginAtZero: true,
            max: 100,
            ticks: { stepSize: 25 },
            grid: { color: gridColor },
            border: { display: false }
          }
        }
      }
    });
  }

  const satisfactionCanvas = document.getElementById("figusta-satisfaction-chart");
  if (satisfactionCanvas) {
    new Chart(satisfactionCanvas, {
      type: "doughnut",
      data: {
        labels: ["Excellent", "Good", "Average", "Poor"],
        datasets: [
          {
            data: [45, 35, 15, 5],
            backgroundColor: ["#68b063", "#f0b429", "#333333", "#e74c3c"],
            borderWidth: 0,
            spacing: 2
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: "62%",
        plugins: { legend: { display: false } }
      }
    });
  }
})();
