using System.Reflection;
using ForraControl.API.Models.Dtos.Admin;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ForraControl.API.Services;

// Arma el PDF del reporte completo con QuestPDF. Sin dependencias de UI —
// solo recibe el DTO ya calculado por AdminService.ObtenerReporteCompletoAsync.
public static class ReportePdfBuilder
{
    private static readonly byte[] LogoBytes = LoadLogo();

    private static byte[] LoadLogo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().First(n => n.EndsWith("logo_splash.png"));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public static byte[] Build(ReporteCompletoDto r)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                // "DejaVu Sans" en vez de Calibri: es la que se instala en el
                // contenedor de Railway (ver Dockerfile) — Calibri no existe en Linux
                // y solo "funcionaba" en local porque Windows sí la trae preinstalada.
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("DejaVu Sans"));

                page.Header().Element(c => ComposeHeader(c, r));
                page.Content().Element(c => ComposeContent(c, r));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static string PeriodoLabel(string periodo) => periodo.ToLower() switch
    {
        "semana" => "Últimos 7 días",
        "mes" => "Este mes",
        _ => "Hoy",
    };

    private static void ComposeHeader(IContainer container, ReporteCompletoDto r)
    {
        container.PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Row(row =>
        {
            row.Spacing(10);
            row.ConstantItem(36).Height(36).Image(LogoBytes).FitArea();
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Forra Store").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                col.Item().Text($"Reporte de ventas — {PeriodoLabel(r.Periodo)}").FontSize(12).FontColor(Colors.Grey.Darken1);
            });
            row.ConstantItem(160).AlignRight().Text($"Generado: {r.GeneradoEn:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeContent(IContainer container, ReporteCompletoDto r)
    {
        container.PaddingVertical(12).Column(col =>
        {
            col.Spacing(18);

            col.Item().Element(c => ComposeKpis(c, r));
            col.Item().Element(c => ComposeDesglose(c, r));
            if (r.TopProductos.Count > 0) col.Item().Element(c => ComposeTopProductos(c, r));
            if (r.VentasPorCategoria.Count > 0) col.Item().Element(c => ComposeVentasPorCategoria(c, r));
            if (r.AlertasStock.Count > 0) col.Item().Element(c => ComposeAlertas(c, r));
            col.Item().Element(c => ComposeInventario(c, r));
            col.Item().Element(c => ComposeVentas(c, r));
        });
    }

    // ── KPIs ─────────────────────────────────────────────────────────

    private static void ComposeKpis(IContainer container, ReporteCompletoDto r)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            row.RelativeItem().Element(c => KpiCard(c, "Total vendido", $"${r.TotalVentas:0.00}", Colors.Blue.Darken2));
            row.RelativeItem().Element(c => KpiCard(c, "Ventas", r.NumVentas.ToString(), Colors.Green.Darken2));
            row.RelativeItem().Element(c => KpiCard(c, "Ticket promedio", $"${r.TicketPromedio:0.00}", Colors.Orange.Darken2));
            row.RelativeItem().Element(c => KpiCard(c, "Descuentos", $"${r.DescuentoTotal:0.00}", Colors.Red.Darken2));
        });
    }

    private static void KpiCard(IContainer container, string label, string valor, string color)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
        {
            col.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(2).Text(valor).FontSize(16).Bold().FontColor(color);
        });
    }

    // ── Desglose (barras) ───────────────────────────────────────────

    private static void ComposeDesglose(IContainer container, ReporteCompletoDto r)
    {
        var max = r.DesgloseDiario.Count > 0 ? r.DesgloseDiario.Max(d => d.Total) : 0;

        container.Column(col =>
        {
            col.Item().Text("Desglose de ventas").Bold().FontSize(13);
            col.Item().PaddingTop(6);

            foreach (var d in r.DesgloseDiario)
            {
                var pct = max > 0 ? Math.Max(0.001f, (float)(d.Total / max)) : 0.001f;
                col.Item().PaddingBottom(5).Row(row =>
                {
                    row.ConstantItem(45).AlignMiddle().Text(d.Etiqueta).FontSize(8);
                    row.RelativeItem().Height(12).Background(Colors.Grey.Lighten3).Row(barRow =>
                    {
                        barRow.RelativeItem(pct).Background(Colors.Blue.Medium);
                        barRow.RelativeItem(Math.Max(0.001f, 1 - pct));
                    });
                    row.ConstantItem(65).AlignMiddle().AlignRight().Text(d.Total > 0 ? $"${d.Total:0.00}" : "—").FontSize(8);
                });
            }
        });
    }

    // ── Tablas genéricas ─────────────────────────────────────────────

    private static IContainer HeaderCell(IContainer c) => c
        .Background(Colors.Blue.Darken2)
        .Padding(5)
        .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));

    private static IContainer BodyCell(IContainer c) => c
        .BorderBottom(1)
        .BorderColor(Colors.Grey.Lighten2)
        .PaddingVertical(4)
        .PaddingHorizontal(5)
        .DefaultTextStyle(x => x.FontSize(9));

    private static void ComposeTopProductos(IContainer container, ReporteCompletoDto r)
    {
        container.Column(col =>
        {
            col.Item().Text("Top productos vendidos").Bold().FontSize(13);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Producto");
                    h.Cell().Element(HeaderCell).Text("Presentación");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Cant. vendida");
                });

                foreach (var p in r.TopProductos)
                {
                    table.Cell().Element(BodyCell).Text(p.NombreProducto);
                    table.Cell().Element(BodyCell).Text(p.DescripcionPresentacion);
                    table.Cell().Element(BodyCell).AlignRight().Text(p.TotalVendido.ToString());
                }
            });
        });
    }

    private static void ComposeVentasPorCategoria(IContainer container, ReporteCompletoDto r)
    {
        container.Column(col =>
        {
            col.Item().Text("Ventas por categoría").Bold().FontSize(13);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Categoría");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Unidades");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                });

                foreach (var vc in r.VentasPorCategoria)
                {
                    table.Cell().Element(BodyCell).Text(vc.Categoria);
                    table.Cell().Element(BodyCell).AlignRight().Text(vc.Cantidad.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text($"${vc.Total:0.00}");
                }
            });
        });
    }

    private static void ComposeAlertas(IContainer container, ReporteCompletoDto r)
    {
        container.Column(col =>
        {
            col.Item().Text("Alertas de stock bajo").Bold().FontSize(13).FontColor(Colors.Red.Darken2);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Producto");
                    h.Cell().Element(HeaderCell).Text("Presentación");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Stock");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Mínimo");
                });

                foreach (var producto in r.AlertasStock)
                {
                    foreach (var pres in producto.Presentaciones)
                    {
                        table.Cell().Element(BodyCell).Text(producto.NombreProducto);
                        table.Cell().Element(BodyCell).Text(pres.Descripcion);
                        table.Cell().Element(BodyCell).AlignRight().Text(pres.Stock.ToString())
                            .FontColor(Colors.Red.Darken2);
                        table.Cell().Element(BodyCell).AlignRight().Text(pres.StockMinimo.ToString());
                    }
                }
            });
        });
    }

    private static void ComposeInventario(IContainer container, ReporteCompletoDto r)
    {
        container.Column(col =>
        {
            col.Item().Text("Inventario completo").Bold().FontSize(13);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Producto");
                    h.Cell().Element(HeaderCell).Text("Categoría");
                    h.Cell().Element(HeaderCell).Text("Presentación");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Stock");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Mínimo");
                    h.Cell().Element(HeaderCell).AlignCenter().Text("Estado");
                });

                foreach (var item in r.Inventario)
                {
                    var esAlerta = item.Estado == "alerta";
                    table.Cell().Element(BodyCell).Text(item.NombreProducto);
                    table.Cell().Element(BodyCell).Text(item.Categoria);
                    table.Cell().Element(BodyCell).Text(item.Presentacion);
                    table.Cell().Element(BodyCell).AlignRight().Text(item.Stock.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text(item.StockMinimo.ToString());
                    table.Cell().Element(BodyCell).AlignCenter().Text(esAlerta ? "Bajo" : "OK")
                        .FontColor(esAlerta ? Colors.Red.Darken2 : Colors.Green.Darken2);
                }
            });
        });
    }

    private static void ComposeVentas(IContainer container, ReporteCompletoDto r)
    {
        container.Column(col =>
        {
            col.Item().Text("Detalle de ventas del período").Bold().FontSize(13);
            col.Item().PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderCell).Text("Fecha");
                    h.Cell().Element(HeaderCell).Text("Cliente");
                    h.Cell().Element(HeaderCell).AlignRight().Text("# Prod.");
                    h.Cell().Element(HeaderCell).AlignRight().Text("Total");
                });

                foreach (var v in r.Ventas)
                {
                    table.Cell().Element(BodyCell).Text(v.Fecha.ToString("dd/MM/yyyy HH:mm"));
                    table.Cell().Element(BodyCell).Text(v.NombreCliente);
                    table.Cell().Element(BodyCell).AlignRight().Text(v.NumProductos.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text($"${v.TotalFinal:0.00}");
                }
            });
        });
    }
}
