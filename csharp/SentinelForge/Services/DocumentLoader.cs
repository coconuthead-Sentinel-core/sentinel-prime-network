// DocumentLoader.cs — single entry point the UI uses to load any document.
// Routes each format to the engine best suited to it:
//   .txt/.md/.log/.html/.htm -> native C++ core (SentinelForgeCore.dll)
//   .pdf                      -> PdfPig (C#)
//   .docx                     -> Open XML SDK (C#)
using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace SentinelForge.Services;

public static class DocumentLoader
{
    /// <summary>Load a document's full plain text, choosing the right engine for its type.</summary>
    public static string LoadText(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".pdf"  => LoadPdf(path),
            ".docx" => LoadDocx(path),
            // txt, md, log, html, htm — handled by the native C++ engine.
            _       => SentinelCore.LoadDocumentText(path),
        };
    }

    private static string LoadPdf(string path)
    {
        var sb = new StringBuilder();
        using var doc = PdfDocument.Open(path);
        int n = 1;
        foreach (var page in doc.GetPages())
        {
            sb.AppendLine(page.Text);
            sb.AppendLine();
            n++;
        }
        string text = sb.ToString().Trim();
        return text.Length == 0
            ? "(This PDF has no extractable text — it may be a scanned image. OCR is a future step.)"
            : text;
    }

    private static string LoadDocx(string path)
    {
        using var doc = WordprocessingDocument.Open(path, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return "(Empty Word document.)";

        var sb = new StringBuilder();
        foreach (var para in body.Descendants<Paragraph>())
            sb.AppendLine(para.InnerText);
        return sb.ToString().TrimEnd();
    }
}
