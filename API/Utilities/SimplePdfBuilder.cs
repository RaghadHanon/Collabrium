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

        var escapedLines = lines.Select(EscapePdfText).ToList();

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine("BT");
        contentBuilder.AppendLine("/F1 12 Tf");

        var currentY = topStart;
        foreach (var line in escapedLines)
        {
            contentBuilder.AppendLine($"1 0 0 1 {leftMargin} {currentY} Tm ({line}) Tj");
            currentY -= lineHeight;
        }

        contentBuilder.AppendLine("ET");
        var contentStream = contentBuilder.ToString();

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Count 1 /Kids [3 0 R] >>",
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {pageWidth} {pageHeight}] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(contentStream)} >>\nstream\n{contentStream}endstream"
        };

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
        {
            sb.AppendLine($"{offsets[i].ToString("D10", CultureInfo.InvariantCulture)} 00000 n ");
        }

        sb.AppendLine("trailer");
        sb.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
        sb.AppendLine("startxref");
        sb.AppendLine(xrefOffset.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("%%EOF");

        return Encoding.ASCII.GetBytes(sb.ToString());
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
    }
}
