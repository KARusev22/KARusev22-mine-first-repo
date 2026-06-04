document.addEventListener("DOMContentLoaded", function () {

    const taken = parseInt(document.getElementById("TakenOrdersValue").value);
    const notTaken = parseInt(document.getElementById("NotTakenOrdersValue").value);

    const topDays = JSON.parse(document.getElementById("Top3Days").value);
    const topCategories = JSON.parse(document.getElementById("Top3Categories").value);

    /* PIE */
    new Chart(document.getElementById('statusChart'), {
        type: 'pie',
        data: {
            labels: ['Taken', 'Not Taken'],
            datasets: [{
                data: [taken, notTaken],
                backgroundColor: ['#4a90e2', '#f39c12'],
                borderColor: function(context) {
                    const bg = context.dataset.backgroundColor[context.dataIndex];
                    return shadeColor(bg, -20);
                },
                borderWidth: 1.2
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    });

    /* TOP 3 DAYS */
    new Chart(document.getElementById('topDaysChart'), {
        type: 'bar',
        data: {
            labels: Object.keys(topDays),
            datasets: [{
                label: 'Orders',
                data: Object.values(topDays),
                backgroundColor: ['#4a90e2', '#9b59b6', '#2f6f3e'],
                borderColor: function(context) {
                    const bg = context.dataset.backgroundColor[context.dataIndex];
                    return shadeColor(bg, -20);
                },
                borderWidth: 1.2,
                barThickness: 14,
                maxBarThickness: 14
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: { beginAtZero: true },
                x: { grid: { color: '#e8d98a33' } }
            }
        }
    });

    /* TOP 3 CATEGORIES */
    new Chart(document.getElementById('topCategoriesChart'), {
        type: 'bar',
        data: {
            labels: Object.keys(topCategories),
            datasets: [{
                label: 'Quantity',
                data: Object.values(topCategories),
                backgroundColor: ['#f39c12', '#4a90e2', '#9b59b6'],
                borderColor: function(context) {
                    const bg = context.dataset.backgroundColor[context.dataIndex];
                    return shadeColor(bg, -20);
                },
                borderWidth: 1.2,
                barThickness: 14,
                maxBarThickness: 14,
                borderRadius: 8
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { beginAtZero: true, grid: { color: '#e8d98a33' } }
            }
        }
    });

});

/* Darker outline generator */
function shadeColor(color, percent) {
    const f = parseInt(color.slice(1),16),
        t = percent<0?0:255,
        p = percent<0?percent*-1:percent,
        R = f>>16,
        G = f>>8&0x00FF,
        B = f&0x0000FF;
    return "#" + (
        0x1000000 +
        (Math.round((t-R)*p)+R)*0x10000 +
        (Math.round((t-G)*p)+G)*0x100 +
        (Math.round((t-B)*p)+B)
    ).toString(16).slice(1);
}