const charts = new Map();

const defaultColors = ["#0d6efd", "#198754", "#dc3545", "#fd7e14", "#6f42c1"];

export function renderChart(config) {
    const canvas = document.getElementById(config.canvasId);
    if (!canvas || typeof Chart === "undefined") {
        return;
    }

    destroyChart(config.canvasId);

    const isNarrowScreen = window.matchMedia("(max-width: 575.98px)").matches;

    const chart = new Chart(canvas, {
        type: "line",
        data: {
            labels: config.labels,
            datasets: config.datasets.map((dataset, index) => ({
                label: dataset.label,
                data: dataset.data,
                borderColor: dataset.borderColor ?? defaultColors[index % defaultColors.length],
                backgroundColor: dataset.backgroundColor ?? "transparent",
                tension: 0.2,
                fill: false,
                pointRadius: 4,
                pointHoverRadius: 6
            }))
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: config.datasets.length > 1
                }
            },
            scales: {
                y: {
                    title: {
                        display: Boolean(config.yAxisLabel),
                        text: config.yAxisLabel ?? ""
                    }
                },
                x: {
                    title: {
                        display: Boolean(config.xAxisLabel),
                        text: config.xAxisLabel ?? ""
                    },
                    ticks: {
                        autoSkip: true,
                        maxRotation: isNarrowScreen ? 45 : 0,
                        minRotation: isNarrowScreen ? 45 : 0
                    }
                }
            }
        }
    });

    charts.set(config.canvasId, chart);
}

export function destroyChart(canvasId) {
    const chart = charts.get(canvasId);
    if (!chart) {
        return;
    }

    chart.destroy();
    charts.delete(canvasId);
}
