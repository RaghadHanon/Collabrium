using System.Globalization;
using System.Text;

namespace API.Utilities;

internal static class SimplePdfBuilder
{
    public static byte[] BuildReportDocument(IEnumerable<string> lines)
    {
        const int pageWidth = 595;
        const int pageHeight = 842;
        const int leftMargin = 40;
        const int topStart = 800;
        const int lineHeight = 16;
        const int maxCharsPerLine = 95;

        var flattenedLines = lines
            .SelectMany(l => WrapLine(l, maxCharsPerLine))
            .Select(EscapePdfText)
            .ToList();

        var linesPerPage = Math.Max(1, (topStart - 40) / lineHeight);
        var pages = flattenedLines
            .Select((line, idx) => new { line, idx })
            .GroupBy(x => x.idx / linesPerPage)
            .Select(g => g.Select(x => x.line).ToList())
            .ToList();

        if (pages.Count == 0)
            pages.Add([EscapePdfText("Report is empty")]);

        return BuildPdfWithPages(pages, pageWidth, pageHeight, leftMargin, topStart, lineHeight);
    }

    private static byte[] BuildPdfWithPages(
        IReadOnlyList<IReadOnlyList<string>> pages,
        int pageWidth,
        int pageHeight,
        int leftMargin,
        int topStart,
        int lineHeight)
    {
        var objects = new List<string>();

        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");

        var pageCount = pages.Count;
        var firstPageObjectIndex = 3;
        var firstFontObjectIndex = firstPageObjectIndex + pageCount;
        var firstContentObjectIndex = firstFontObjectIndex + pageCount;

        var kidRefs = Enumerable.Range(0, pageCount)
            .Select(i => $"{firstPageObjectIndex + i} 0 R");
        objects.Add($"<< /Type /Pages /Count {pageCount} /Kids [{string.Join(' ', kidRefs)}] >>");

        for (var i = 0; i < pageCount; i++)
        {
            var fontObjectRef = firstFontObjectIndex + i;
            var contentObjectRef = firstContentObjectIndex + i;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Resources << /Font << /F1 {fontObjectRef} 0 R >> >> /Contents {contentObjectRef} 0 R >>");
        }

        for (var i = 0; i < pageCount; i++)
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        foreach (var pageLines in pages)
        {
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("BT");
            contentBuilder.AppendLine("/F1 12 Tf");

            var currentY = topStart;
            foreach (var line in pageLines)
            {
                contentBuilder.AppendLine($"1 0 0 1 {leftMargin} {currentY} Tm ({line}) Tj");
                currentY -= lineHeight;
            }

            contentBuilder.AppendLine("ET");
            var contentStream = contentBuilder.ToString();
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>\nstream\n{contentStream}endstream");
        }

        return BuildPdf(objects);
    }

    private static byte[] BuildPdf(IReadOnlyList<string> objects)
    {
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");

        var offsets = new List<int> { 0 };

        for (var i = 0; i < objects.Count; i++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(sb.ToString()));
            sb.AppendLine($"{i + 1} 0 obj");
            sb.AppendLine(objects[i]);
            sb.AppendLine("endobj");
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(sb.ToString());

        sb.AppendLine("xref");
        sb.AppendLine($"0 {objects.Count + 1}");
        sb.AppendLine("0000000000 65535 f ");

        for (var i = 1; i <= objects.Count; i++)
            sb.AppendLine($"{offsets[i].ToString("D10", CultureInfo.InvariantCulture)} 00000 n ");

        sb.AppendLine("trailer");
        sb.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static IEnumerable<string> WrapLine(string value, int maxChars)
    {
        if (string.IsNullOrEmpty(value))
        {
            yield return string.Empty;
            yield break;
        }

        var line = value;
        while (line.Length > maxChars)
        {
            var splitAt = line.LastIndexOf(' ', Math.Min(maxChars, line.Length - 1));
            if (splitAt <= 0)
                splitAt = maxChars;

            yield return line[..splitAt].TrimEnd();
            line = line[splitAt..].TrimStart();
        }

        yield return line;
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
