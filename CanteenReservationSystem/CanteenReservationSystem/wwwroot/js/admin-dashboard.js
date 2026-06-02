document.addEventListener("DOMContentLoaded", () => {

    const d = window.dashboardData;

    /*ORDERS PER DAY*/
    new Chart(document.getElementById("ordersPerDayChart"), {
        type: "line",
        data: {
            labels: d.ordersPerDayLabels,
            datasets: [{
                label: "Orders",
                data: d.ordersPerDayValues,
                borderColor: "rgba(47,129,39,0.77)",
                backgroundColor: "rgba(39,174,96,0.25)",
                fill: true,
                tension: 0.4
            }]
        }
    });

    /*STATUS*/
    new Chart(document.getElementById("statusChart"), {
        type: "doughnut",
        data: {
            labels: d.statusLabels,
            datasets: [{
                data: d.statusValues,
                backgroundColor: ["rgba(255,195,100,0.91)", "#27AE60", "#E74C3C"],
                borderWidth: 1
            }]
        },
        options: { cutout: "55%" }
    });

    /*TOP DISHES*/
    new Chart(document.getElementById("topDishesChart"), {
        type: "bar",
        data: {
            labels: d.topDishesLabels,
            datasets: [{
                label: "Quantity",
                data: d.topDishesValues,
                backgroundColor: "rgba(255, 230, 140, 0.8)",
                borderColor: "rgb(223,184,83)",
                borderWidth: 1.5
            }]
        },
        options: { indexAxis: "y" }
    });

    /*REVENUE*/
    new Chart(document.getElementById("revenueChart"), {
        type: "line",
        data: {
            labels: d.revenueLabels,
            datasets: [{
                label: "Revenue",
                data: d.revenueValues,
                borderColor: "#4A90E2",
                backgroundColor: "rgba(74,144,226,0.25)",
                fill: true,
                tension: 0.4
            }]
        }
    });

    /*BLACK POINTS*/
    new Chart(document.getElementById("blackPointsChart"), {
        type: "bar",
        data: {
            labels: d.blackPointsLabels,
            datasets: [{
                label: "Black Points",
                data: d.blackPointsValues,
                backgroundColor: "rgba(0,0,0,0)",
                borderColor: "rgb(214,123,123)",
                borderWidth: 2,
                borderDash: [4, 4]
            }]
        }
    });

    /*ACTIVE USERS*/
    new Chart(document.getElementById("activeUsersChart"), {
        type: "bar",
        data: {
            labels: d.activeUsersLabels,
            datasets: [{
                label: "Orders",
                data: d.activeUsersValues,
                backgroundColor: "rgba(0,0,0,0)",
                borderColor: "rgb(241,196,107)",
                borderWidth: 2,
                borderDash: [4, 4]
            }]
        }
    });

});