using System;
using System.Drawing;
using System.IO;
using Tesseract;

namespace Automation.Components
{
    public class OCRhelper
    {
        private readonly string _tessDataPath;

        public OCRhelper(string tessDataPath)
        {
            _tessDataPath = tessDataPath;
        }

        public string ExtractText(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new Exception($"Image not found: {imagePath}");

            var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);

            engine.DefaultPageSegMode = PageSegMode.SparseText;

            var original = new Bitmap(imagePath);

            // enlarge image
            var resized = new Bitmap(original, new Size(original.Width * 2, original.Height * 2));

            var pix = PixConverter.ToPix(resized);
            var page = engine.Process(pix);

            string text = page.GetText() ?? string.Empty;

            System.Diagnostics.Debug.WriteLine("------ OCR RAW RESULT ------");
            System.Diagnostics.Debug.WriteLine(text);

            string cleaned = text
                .Replace(" ", "")
                .Replace("\n", "")
                .Replace("\r", "");

            System.Diagnostics.Debug.WriteLine("------ OCR CLEANED RESULT ------");
            System.Diagnostics.Debug.WriteLine(cleaned);

            return text;
        }

        public bool ContainsText(string imagePath, string expected)
        {
            var text = ExtractText(imagePath);

            // Clean both sides for better matching
            string cleanText = text.Replace(" ", "").Replace("\n", "");
            string cleanExpected = expected.Replace(" ", "");

            return cleanText.IndexOf(cleanExpected, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}