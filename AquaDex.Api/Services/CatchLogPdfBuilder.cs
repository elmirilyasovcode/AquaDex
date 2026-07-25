using System.Reflection.Metadata;
using AquaDex.Core.Entities;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace AquaDex.Api.Services;

public static class CatchLogPdfBuilder
{
    public static byte[] Build(List<CatchLog> logs)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("AquaDex — Catch Log Report").FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Species").Bold();
                        header.Cell().Text("Angler").Bold();
                        header.Cell().Text("Weight (kg)").Bold();
                        header.Cell().Text("Length (cm)").Bold();
                        header.Cell().Text("Date").Bold();
                    });

                    foreach (var log in logs)
                    {
                        table.Cell().Text(log.Species.CommonNameEn);
                        table.Cell().Text(log.User.DisplayName);
                        table.Cell().Text(log.WeightKg?.ToString() ?? "-");
                        table.Cell().Text(log.LengthCm?.ToString() ?? "-");
                        table.Cell().Text(log.CaughtAt.ToString("yyyy-MM-dd"));
                    }
                });
                page.Footer().AlignCenter().Text($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            });
        });

        return document.GeneratePdf();
    }
}