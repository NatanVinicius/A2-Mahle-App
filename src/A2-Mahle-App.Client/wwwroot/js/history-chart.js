window.historyChart = {
  productionChart: null,
  rejectRateChart: null,

  getExportHtml: async function (elementId) {
    const element = document.getElementById(elementId);

    if (!element) {
      throw new Error(`Element not found: ${elementId}`);
    }

    const clone = element.cloneNode(true);

    clone.classList.add("pdf-export-mode");

    clone.querySelectorAll(".apexcharts-tooltip, .apexcharts-xaxistooltip, .apexcharts-yaxistooltip, .apexcharts-toolbar, .apexcharts-legend").forEach((node) => {
      node.remove();
    });

    await window.historyChart.tryReplaceChartWithImage(clone, "production-chart", 250);
    await window.historyChart.tryReplaceChartWithImage(clone, "reject-rate-chart", 250);

    return clone.outerHTML;
  },

  tryReplaceChartWithImage: async function (cloneRoot, chartId, width) {
    try {
      await window.historyChart.replaceChartWithImage(cloneRoot, chartId, width);
    } catch (error) {
      console.warn(`HistoryChart: failed to rasterize ${chartId} for PDF export.`, error);
    }
  },

  replaceChartWithImage: async function (cloneRoot, chartId, width) {
    const liveChartElement = document.getElementById(chartId);
    const clonedChartElement = cloneRoot.querySelector(`#${chartId}`);

    if (!liveChartElement || !clonedChartElement) {
      return;
    }

    const liveSvg = liveChartElement.querySelector("svg");
    if (!liveSvg) {
      return;
    }

    const imageDataUrl = window.historyChart.svgToDataUrl(liveSvg);
    if (!imageDataUrl) {
      return;
    }

    clonedChartElement.innerHTML = "";
    clonedChartElement.className = "pdf-chart-image-container";

    const image = document.createElement("img");
    image.src = imageDataUrl;
    image.alt = chartId;
    image.width = width;
    image.height = width;
    image.className = "pdf-chart-image";

    clonedChartElement.appendChild(image);
  },

  svgToDataUrl: function (svgElement) {
    const clonedSvg = svgElement.cloneNode(true);
    const viewBox = clonedSvg.getAttribute("viewBox");
    const width = Number(clonedSvg.getAttribute("width")) || (viewBox ? Number(viewBox.split(" ")[2]) : 0) || 320;
    const height = Number(clonedSvg.getAttribute("height")) || (viewBox ? Number(viewBox.split(" ")[3]) : 0) || 260;

    clonedSvg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
    clonedSvg.setAttribute("width", String(width));
    clonedSvg.setAttribute("height", String(height));
    clonedSvg.setAttribute("preserveAspectRatio", "xMidYMid meet");

    clonedSvg.querySelectorAll("script, foreignObject").forEach((node) => {
      node.remove();
    });

    clonedSvg.querySelectorAll(
      ".apexcharts-tooltip, .apexcharts-toolbar, .apexcharts-legend, .apexcharts-grid, .apexcharts-xaxis, .apexcharts-yaxis, .apexcharts-series-markers"
    ).forEach((node) => {
      node.remove();
    });

    clonedSvg.querySelectorAll("line").forEach((node) => {
      node.remove();
    });

    const svgMarkup = new XMLSerializer().serializeToString(clonedSvg);
    return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svgMarkup)}`;
  },

  render: function (elementId, approved, rejected, rejectRate) {
    const element = document.getElementById(elementId);

    if (!element) {
      console.error(
        "HistoryChart: elemento não encontrado:",
        elementId
      );

      return;
    }

    if (typeof ApexCharts === "undefined") {
      console.error(
        "HistoryChart: ApexCharts não foi carregado."
      );

      return;
    }

    if (this.productionChart) {
      this.productionChart.destroy();
      this.productionChart = null;
    }

    const approvedValue = Number(approved) || 0;
    const rejectedValue = Number(rejected) || 0;

    /*
     * A taxa de rejeito NÃO entra como uma terceira
     * fatia no gráfico de produção.
     *
     * O donut representa somente:
     *
     * Aprovadas
     * Reprovadas
     */
    const total = approvedValue + rejectedValue;

    if (total <= 0) {
      element.innerHTML = `
        <div class="flex h-[300px] w-[420px] items-center justify-center text-sm text-gray-500">
          Nenhum dado de produção disponível
        </div>
      `;

      return;
    }

    const options = {
      series: [
        approvedValue,
        rejectedValue
      ],

      chart: {
        type: "donut",
        width: 320,
        height: 260,

        animations: {
          enabled: true,
          easing: "easeinout",
          speed: 800
        },

        toolbar: {
          show: false
        }
      },

      labels: [
        "Aprovadas",
        "Reprovadas"
      ],

      colors: [
        "#22c55e",
        "#ef4444"
      ],

      plotOptions: {
        pie: {
          expandOnClick: true,

          borderRadius: 10,

          donut: {
            size: "68%",

            labels: {
              show: true,

              name: {
                show: true,
                fontSize: "14px",
                fontWeight: 500,
                color: "#6b7280"
              },

              value: {
                show: true,
                fontSize: "24px",
                fontWeight: 700,
                color: "#111827",

                formatter: function (value) {
                  return Number(value).toLocaleString(
                    "pt-BR"
                  );
                }
              },

              total: {
                show: true,
                showAlways: true,

                label: "Produção",

                fontSize: "14px",
                fontWeight: 500,
                color: "#6b7280",

                formatter: function () {
                  return total.toLocaleString(
                    "pt-BR"
                  );
                }
              }
            }
          }
        }
      },

      stroke: {
        width: 4,
        colors: ["transparent"]
      },

      dataLabels: {
        enabled: false
      },

      legend: {
        show: false,
        position: "bottom",

        horizontalAlign: "center",

        fontSize: "14px",

        markers: {
          width: 10,
          height: 10,
          radius: 10
        },

        itemMargin: {
          horizontal: 12,
          vertical: 4
        }
      },

      tooltip: {
        enabled: true,

        y: {
          formatter: function (value) {
            return Number(value).toLocaleString(
              "pt-BR"
            );
          }
        }
      },

      responsive: [
        {
          breakpoint: 480,

          options: {
            chart: {
              width: 320,
              height: 320
            },

            legend: {
              position: "bottom"
            }
          }
        }
      ]
    };

    this.productionChart = new ApexCharts(
      element,
      options
    );

    this.productionChart.render();
  },

  renderRejectRate: function (elementId, rejectRate) {
    const element = document.getElementById(elementId);

    if (!element) {
      console.error(
        "HistoryChart: elemento não encontrado:",
        elementId
      );

      return;
    }

    if (typeof ApexCharts === "undefined") {
      console.error(
        "HistoryChart: ApexCharts não foi carregado."
      );

      return;
    }

    if (this.rejectRateChart) {
      this.rejectRateChart.destroy();
      this.rejectRateChart = null;
    }

    const rejectRateValue = Math.max(
      0,
      Math.min(100, Number(rejectRate) || 0)
    );

    const remainingValue = 100 - rejectRateValue;

    const options = {
      series: [
        rejectRateValue,
        remainingValue
      ],

      chart: {
        type: "donut",
        width: 320,
        height: 260,

        animations: {
          enabled: true,
          easing: "easeinout",
          speed: 800
        },

        toolbar: {
          show: false
        }
      },

      labels: [
        "Rejeito",
        "Sem rejeição"
      ],

      colors: [
        "#ff6905",
        "#b5b5b5"
      ],

      plotOptions: {
        pie: {
          expandOnClick: true,

          borderRadius: 10,

          donut: {
            size: "68%",

            labels: {
              show: true,

              name: {
                show: true,
                fontSize: "14px",
                fontWeight: 500,
                color: "#6b7280"
              },

              value: {
                show: true,
                fontSize: "24px",
                fontWeight: 700,
                color: "#111827",

                formatter: function () {
                  return rejectRateValue.toLocaleString(
                    "pt-BR",
                    {
                      minimumFractionDigits: 1,
                      maximumFractionDigits: 1
                    }
                  ) + "%";
                }
              },

              total: {
                show: true,
                showAlways: true,

                label: "Taxa de Rejeito",

                fontSize: "14px",
                fontWeight: 500,
                color: "#6b7280",

                formatter: function () {
                  return rejectRateValue.toLocaleString(
                    "pt-BR",
                    {
                      minimumFractionDigits: 1,
                      maximumFractionDigits: 1
                    }
                  ) + "%";
                }
              }
            }
          }
        }
      },

      stroke: {
        width: 4,
        colors: ["transparent"]
      },

      dataLabels: {
        enabled: false
      },

      legend: {
        show: false
      },

      tooltip: {
        enabled: true,

        y: {
          formatter: function (value, { seriesIndex }) {
            if (seriesIndex === 0) {
              return rejectRateValue.toLocaleString(
                "pt-BR",
                {
                  minimumFractionDigits: 1,
                  maximumFractionDigits: 1
                }
              ) + "%";
            }

            return remainingValue.toLocaleString(
              "pt-BR",
              {
                minimumFractionDigits: 1,
                maximumFractionDigits: 1
              }
            ) + "%";
          }
        }
      },

      responsive: [
        {
          breakpoint: 480,

          options: {
            chart: {
              width: 320,
              height: 320
            }
          }
        }
      ]
    };

    this.rejectRateChart = new ApexCharts(
      element,
      options
    );

    this.rejectRateChart.render();
  },

  destroy: function () {
    if (this.productionChart) {
      this.productionChart.destroy();
      this.productionChart = null;
    }

    if (this.rejectRateChart) {
      this.rejectRateChart.destroy();
      this.rejectRateChart = null;
    }
  }
};