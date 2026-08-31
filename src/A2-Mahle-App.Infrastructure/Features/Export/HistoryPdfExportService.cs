using System.Diagnostics;
using System.Text;

using A2MahleApp.Application.Features.Export;

namespace A2MahleApp.Infrastructure.Features.Export;

public sealed class HistoryPdfExportService : IHistoryExportPdfService
{
    private const string ClientProjectFileName = "A2-Mahle-App.Client.csproj";
    private static readonly string PuppeteerScriptRelativePath = Path.Combine("PdfExport", "render-history-pdf.cjs");
    private static readonly string AppStylesheetRelativePath = Path.Combine("wwwroot", "app.css");
    private static readonly string LogoRelativePath = Path.Combine("Features", "Export", "Assets", "mahle-logo.jpg");

    public async Task<byte[]> ExportProductionsAsync(
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        return await ExportHtmlAsync(htmlContent, "Histórico de Produção", cancellationToken);
    }

    public async Task<byte[]> ExportInspectionsAsync(
        string htmlContent,
        CancellationToken cancellationToken = default)
    {
        return await ExportHtmlAsync(htmlContent, "Histórico de Inspeções", cancellationToken);
    }

    private static async Task<byte[]> ExportHtmlAsync(
        string htmlContent,
        string documentTitle,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("O conteúdo HTML do relatório não pode ser vazio.", nameof(htmlContent));
        }

        string clientProjectDirectory = GetClientProjectDirectory();
        string stylesheetPath = Path.Combine(clientProjectDirectory, AppStylesheetRelativePath);
        string puppeteerScriptPath = Path.Combine(clientProjectDirectory, PuppeteerScriptRelativePath);
        string puppeteerPackageDirectory = Path.Combine(clientProjectDirectory, "node_modules", "puppeteer");
        string infrastructureProjectDirectory = GetInfrastructureProjectDirectory(clientProjectDirectory);
        string logoPath = Path.Combine(infrastructureProjectDirectory, LogoRelativePath);

        if (!File.Exists(stylesheetPath))
        {
            throw new FileNotFoundException("O arquivo de estilos da interface não foi encontrado para a exportação do PDF.", stylesheetPath);
        }

        if (!File.Exists(puppeteerScriptPath))
        {
            throw new FileNotFoundException("O script do Puppeteer para exportação do PDF não foi encontrado.", puppeteerScriptPath);
        }

        if (!Directory.Exists(puppeteerPackageDirectory))
        {
            throw new InvalidOperationException("A dependência 'puppeteer' não foi encontrada. Execute 'npm install' no projeto Client.");
        }

        if (!File.Exists(logoPath))
        {
            throw new FileNotFoundException("O arquivo da logo do relatório não foi encontrado para a exportação do PDF.", logoPath);
        }

        string tempDirectory = Path.Combine(Path.GetTempPath(), "A2MahleApp", "PdfExport", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        string inputHtmlPath = Path.Combine(tempDirectory, "history-export.html");
        string outputPdfPath = Path.Combine(tempDirectory, "history-export.pdf");

        try
        {
            string documentHtml = BuildHtmlDocument(htmlContent, documentTitle, stylesheetPath, logoPath);
            await File.WriteAllTextAsync(inputHtmlPath, documentHtml, Encoding.UTF8, cancellationToken);

            ProcessStartInfo startInfo = new()
            {
                FileName = "node",
                Arguments = $"\"{puppeteerScriptPath}\" \"{inputHtmlPath}\" \"{outputPdfPath}\"",
                WorkingDirectory = clientProjectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new()
            {
                StartInfo = startInfo
            };

            try
            {
                process.Start();
            }
            catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                throw new InvalidOperationException("Não foi possível iniciar o Node.js para gerar o PDF. Verifique se o Node.js está instalado e disponível no PATH.", exception);
            }

            string standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            string standardError = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                string processMessage = string.IsNullOrWhiteSpace(standardError)
                    ? standardOutput
                    : standardError;

                throw new InvalidOperationException($"Falha ao gerar o PDF com Puppeteer: {processMessage}".Trim());
            }

            if (!File.Exists(outputPdfPath))
            {
                throw new FileNotFoundException("O Puppeteer não gerou o arquivo PDF esperado.", outputPdfPath);
            }

            return await File.ReadAllBytesAsync(outputPdfPath, cancellationToken);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static string BuildHtmlDocument(
        string exportHtml,
        string documentTitle,
        string stylesheetPath,
        string logoPath)
    {
        string stylesheetUri = new Uri(stylesheetPath).AbsoluteUri;
        string logoUri = new Uri(logoPath).AbsoluteUri;

        return $$"""
                 <!DOCTYPE html>
                 <html lang="pt-BR">
                 <head>
                     <meta charset="utf-8" />
                     <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                     <title>{{documentTitle}}</title>
                     <link rel="stylesheet" href="{{stylesheetUri}}" />
                     <style>
                         @page {
                             size: A4;
                             margin: 2mm;
                         }

                         html,
                         body {
                             margin: 0;
                             padding: 0;
                             background: #ffffff;
                             height: 100%;
                         }

                         body {
                             color: #111827;
                             box-sizing: border-box;
                         }

                         .pdf-page {
                             height: 100%;
                             display: flex;
                             flex-direction: column;
                             box-sizing: border-box;
                             overflow: hidden;
                         }

                         .pdf-header {
                             display: flex;
                             align-items: flex-start;
                             justify-content: space-between;
                             gap: 16px;
                             padding: 20px;
                         }

                         .pdf-header__logo {
                             width: 148px;
                             height: auto;
                             display: block;
                             object-fit: contain;
                         }

                         .pdf-header__title {
                             flex: 1;
                             margin: 0;
                             text-align: center;
                             font-size: 24px;
                             font-weight: 700;
                             line-height: 1.2;
                             padding-top: 60px;
                             padding-right: 148px;
                         }

                         .pdf-content {
                             flex: 1;
                             min-height: 0;
                             padding-left: 20px;
                             padding-right: 20px;
                         }

                         #pdf-export-area {
                             overflow: visible !important;
                             min-height: auto !important;
                             height: auto !important;
                             flex: none !important;
                             box-shadow: none !important;
                             margin-top: 0 !important;
                             padding-right: 6px;
                         }

                         #pdf-export-area table {
                             width: 100% !important;
                             border-collapse: collapse;
                             table-layout: fixed;
                             margin-top: 50px;
                         }

                         #pdf-export-area thead {
                             position: static !important;
                         }

                         #pdf-export-area th,
                         #pdf-export-area td {
                             white-space: nowrap;
                             overflow: visible;
                         }

                         #history-chart-export {
                             margin-top: 40px !important;
                             margin-left: 10px;
                             display: flex !important;
                             align-items: flex-start !important;
                             justify-content: center !important;
                             gap: 26px !important;
                             flex-wrap: nowrap !important;
                         }

                         #history-chart-export > div {
                             display: flex !important;
                             align-items: center !important;
                             justify-content: center !important;
                             gap: 14px !important;
                             min-width: 0;
                             flex: 1 1 0;
                         }

                         #history-chart-export .text-body.text-sm {
                             font-size: 12px !important;
                             line-height: 1.35 !important;
                             white-space: nowrap !important;
                         }

                         #history-chart-export .pdf-chart-image-container,
                         #history-chart-export #production-chart,
                         #history-chart-export #reject-rate-chart {
                             display: flex !important;
                             align-items: center !important;
                             justify-content: center !important;
                             min-width: 250px !important;
                             width: 250px !important;
                             overflow: hidden !important;
                             border: 0 !important;
                             outline: 0 !important;
                             box-shadow: none !important;
                             background: transparent !important;
                         }

                         #history-chart-export .pdf-chart-image {
                             display: block;
                             width: 250px;
                             height: auto;
                             border: 0 !important;
                             outline: 0 !important;
                             box-shadow: none !important;
                             background: transparent !important;
                             object-fit: contain;
                         }

                         #history-chart-export .apexcharts-canvas,
                         #history-chart-export .apexcharts-svg {
                             overflow: visible !important;
                         }

                         .pdf-footer {
                             padding-top: 10px;
                             padding-bottom: 10px;
                             break-inside: avoid;
                             page-break-inside: avoid;
                         }

                         .pdf-footer__line {
                             height: 1px;
                             width: 100%;
                             margin-left: 0;
                             background: #d1d5db;
                         }

                         .pdf-footer__text {
                             margin-top: 16px;
                             text-align: center;
                             font-size: 12px;
                             color: #6b7280;
                         }
                     </style>
                 </head>
                 <body>
                     <div class="pdf-page">
                         <header class="pdf-header">
                             <img class="pdf-header__logo" src="{{logoUri}}" alt="A2 Vision Experts" />
                             <h1 class="pdf-header__title">{{documentTitle}}</h1>
                         </header>
                         <main class="pdf-content">
                             {{exportHtml}}
                         </main>
                         <footer class="pdf-footer">
                             <div class="pdf-footer__line"></div>
                             <div class="pdf-footer__text">A2 Vision Experts</div>
                         </footer>
                     </div>
                 </body>
                 </html>
                 """;
    }

    private static string GetClientProjectDirectory()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string projectFilePath = Path.Combine(currentDirectory.FullName, ClientProjectFileName);
            if (File.Exists(projectFilePath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Não foi possível localizar o diretório do projeto Client para gerar o PDF.");
    }

    private static string GetInfrastructureProjectDirectory(string clientProjectDirectory)
    {
        string? srcDirectory = Directory.GetParent(clientProjectDirectory)?.FullName;
        if (string.IsNullOrWhiteSpace(srcDirectory))
        {
            throw new DirectoryNotFoundException("Não foi possível localizar a pasta src para gerar o PDF.");
        }

        string infrastructureProjectDirectory = Path.Combine(srcDirectory, "A2-Mahle-App.Infrastructure");

        if (Directory.Exists(infrastructureProjectDirectory))
        {
            return infrastructureProjectDirectory;
        }

        throw new DirectoryNotFoundException("Não foi possível localizar o diretório do projeto Infrastructure para gerar o PDF.");
    }
}

