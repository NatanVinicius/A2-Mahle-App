const path = require("node:path");
const { pathToFileURL } = require("node:url");
const puppeteer = require("puppeteer");

async function main() {
    const [, , inputHtmlPath, outputPdfPath] = process.argv;

    if (!inputHtmlPath || !outputPdfPath) {
        throw new Error("Expected input HTML path and output PDF path.");
    }

    const browser = await puppeteer.launch({
        headless: true,
        args: ["--allow-file-access-from-files"]
    });

    try {
        const page = await browser.newPage();

        await page.setViewport({
            width: 1440,
            height: 1800,
            deviceScaleFactor: 1
        });

        await page.emulateMediaType("screen");
        await page.goto(pathToFileURL(path.resolve(inputHtmlPath)).href, {
            waitUntil: "networkidle0"
        });

        await page.evaluate(async () => {
            if (document.fonts && document.fonts.ready) {
                await document.fonts.ready;
            }
        });

        await page.pdf({
            path: path.resolve(outputPdfPath),
            format: "A4",
            scale: 0.86,
            printBackground: true,
            preferCSSPageSize: true,
            margin: {
                top: "0",
                right: "0",
                bottom: "0",
                left: "0"
            }
        });
    } finally {
        await browser.close();
    }
}

main().catch((error) => {
    console.error(error instanceof Error ? error.stack ?? error.message : String(error));
    process.exit(1);
});
