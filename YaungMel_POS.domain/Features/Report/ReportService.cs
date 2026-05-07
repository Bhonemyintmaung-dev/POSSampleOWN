using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YaungMel_POS.Domain.DTOs;
using YaungMel_POS.Domain.Features.Summary;

namespace YaungMel_POS.Domain.Features.Report
{
    public class ReportService : IReportService
    {
        private readonly ISummaryService _summaryService;
        private readonly IConverter _pdfConverter;

        public ReportService(ISummaryService summaryService, IConverter pdfConverter)
        {
            _summaryService = summaryService;
            _pdfConverter = pdfConverter;
        }

        public async Task<byte[]> GenerateAnalyticsReportPdfAsync(DateTime start, DateTime end)
        {
            var result = await _summaryService.GetSummariesByRangeAsync(start, end);
            if (!result.IsSuccess) return Array.Empty<byte>();

            var html = GenerateAnalyticsHtml(result.Data!, start, end);
            return ConvertHtmlToPdf(html, "Analytics Report");
        }

        public async Task<byte[]> GenerateDetailedDailyPdfAsync(DateTime date)
        {
            var result = await _summaryService.GetSummaryByDateAsync(date);
            if (!result.IsSuccess) return Array.Empty<byte>();

            var html = GenerateDailyHtml(result.Data!);
            return ConvertHtmlToPdf(html, $"Daily Report - {date:yyyy-MM-dd}");
        }

        private byte[] ConvertHtmlToPdf(string html, string title)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    DocumentTitle = title,
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = html,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "YaungMel POS - System Generated Report" }
                    }
                }
            };

            return _pdfConverter.Convert(doc);
        }

        private string GetCommonStyles()
        {
            return @"
            <style>
                @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
                body {
                    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
                    background-color: #f8f9fc;
                    color: #0f1729;
                    margin: 0;
                    padding: 40px;
                }
                .header {
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    border-bottom: 2px solid #6366f1;
                    padding-bottom: 20px;
                    margin-bottom: 30px;
                }
                .brand {
                    font-size: 24px;
                    font-weight: 800;
                    color: #6366f1;
                    letter-spacing: -0.5px;
                }
                .report-title {
                    text-align: right;
                }
                .report-title h1 {
                    margin: 0;
                    font-size: 18px;
                    color: #4b5563;
                }
                .report-title p {
                    margin: 5px 0 0;
                    font-size: 14px;
                    color: #9ca3af;
                }
                .card {
                    background: white;
                    border-radius: 12px;
                    padding: 20px;
                    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
                    margin-bottom: 25px;
                }
                .stats-grid {
                    display: grid;
                    grid-template-columns: repeat(3, 1fr);
                    gap: 20px;
                    margin-bottom: 30px;
                }
                .stat-box {
                    background: white;
                    padding: 15px;
                    border-radius: 10px;
                    border: 1px solid rgba(0,0,0,0.05);
                }
                .stat-label {
                    font-size: 12px;
                    color: #64748b;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                    margin-bottom: 5px;
                }
                .stat-value {
                    font-size: 20px;
                    font-weight: 700;
                    color: #0f1729;
                }
                .table-container {
                    width: 100%;
                    overflow: hidden;
                    border-radius: 10px;
                    border: 1px solid #eef0f5;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    background: white;
                }
                th {
                    background-color: #f1f5f9;
                    color: #475569;
                    font-weight: 600;
                    font-size: 12px;
                    text-align: left;
                    padding: 12px 15px;
                    text-transform: uppercase;
                }
                td {
                    padding: 12px 15px;
                    border-bottom: 1px solid #f1f5f9;
                    font-size: 13px;
                    color: #334155;
                }
                .mono {
                    font-family: 'Courier New', Courier, monospace;
                    font-weight: 600;
                }
                .badge {
                    display: inline-block;
                    padding: 4px 8px;
                    border-radius: 6px;
                    font-size: 11px;
                    font-weight: 600;
                    background: rgba(99, 102, 241, 0.1);
                    color: #6366f1;
                }
                .text-right { text-align: right; }
                .total-row {
                    font-weight: 700;
                    background: #f8fafc;
                }
            </style>";
        }

        private string GenerateAnalyticsHtml(List<SummaryDTO> items, DateTime start, DateTime end)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append(GetCommonStyles());
            sb.Append("</head><body>");

            // Header
            sb.Append($@"
            <div class='header'>
                <div class='brand'>YaungMel POS</div>
                <div class='report-title'>
                    <h1>Analytics Report</h1>
                    <p>{start:MMM dd, yyyy} - {end:MMM dd, yyyy}</p>
                </div>
            </div>");

            // Overview Stats
            var totalSales = items.Sum(x => x.TotalSale);
            var totalRevenue = items.Sum(x => x.TotalAmount);
            var avgRevenue = items.Count > 0 ? totalRevenue / items.Count : 0;

            sb.Append($@"
            <div class='stats-grid'>
                <div class='stat-box'>
                    <div class='stat-label'>Total Transactions</div>
                    <div class='stat-value'>{totalSales}</div>
                </div>
                <div class='stat-box'>
                    <div class='stat-label'>Total Revenue</div>
                    <div class='stat-value'>{totalRevenue:N0} MMK</div>
                </div>
                <div class='stat-box'>
                    <div class='stat-label'>Avg. Daily Revenue</div>
                    <div class='stat-value'>{avgRevenue:N0} MMK</div>
                </div>
            </div>");

            // Table
            sb.Append(@"
            <div class='table-container'>
                <table>
                    <thead>
                        <tr>
                            <th>Date</th>
                            <th>Sales Count</th>
                            <th>Top Product</th>
                            <th class='text-right'>Daily Revenue</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var item in items)
            {
                sb.Append($@"
                <tr>
                    <td>{item.Date:yyyy-MM-dd}</td>
                    <td>{item.TotalSale}</td>
                    <td><span class='badge'>{item.TopSaleProductName ?? "-"}</span></td>
                    <td class='text-right mono'>{item.TotalAmount:N0} MMK</td>
                </tr>");
            }

            sb.Append($@"
                    </tbody>
                    <tfoot>
                        <tr class='total-row'>
                            <td colspan='3'>GRAND TOTAL</td>
                            <td class='text-right mono'>{totalRevenue:N0} MMK</td>
                        </tr>
                    </tfoot>
                </table>
            </div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }

        private string GenerateDailyHtml(SummaryDetailDto detail)
        {
            var sb = new StringBuilder();
            sb.Append("<html><head>");
            sb.Append(GetCommonStyles());
            sb.Append("</head><body>");

            // Header
            sb.Append($@"
            <div class='header'>
                <div class='brand'>YaungMel POS</div>
                <div class='report-title'>
                    <h1>Detailed Daily Report</h1>
                    <p>{detail.Summary.Date:MMMM dd, yyyy}</p>
                </div>
            </div>");

            // Summary Card
            sb.Append($@"
            <div class='card'>
                <div style='display: flex; justify-content: space-between;'>
                    <div>
                        <div class='stat-label'>Daily Total Revenue</div>
                        <div class='stat-value' style='font-size: 28px; color: #6366f1;'>{detail.Summary.TotalAmount:N0} MMK</div>
                    </div>
                    <div style='text-align: right;'>
                        <div class='stat-label'>Total Transactions</div>
                        <div class='stat-value'>{detail.Summary.TotalSale}</div>
                    </div>
                </div>
                <div style='margin-top: 15px; padding-top: 15px; border-top: 1px solid #f1f5f9;'>
                    <span class='stat-label'>Top Selling Product:</span>
                    <span style='font-weight: 600; color: #0f1729; margin-left: 10px;'>{detail.Summary.TopSaleProductName ?? "N/A"}</span>
                </div>
            </div>");

            // Sales Breakdown
            sb.Append("<h2 style='font-size: 16px; margin-bottom: 15px; color: #4b5563;'>Transaction Breakdown</h2>");
            sb.Append(@"
            <div class='table-container'>
                <table>
                    <thead>
                        <tr>
                            <th>Voucher</th>
                            <th>Items</th>
                            <th class='text-right'>Amount</th>
                        </tr>
                    </thead>
                    <tbody>");

            foreach (var sale in detail.Sales)
            {
                var itemsList = string.Join(", ", sale.SaleItems.Select(i => $"{i.ProductName} (x{i.Quantity})"));
                sb.Append($@"
                <tr>
                    <td><span class='badge'>{sale.VoucherCode}</span></td>
                    <td style='font-size: 11px; color: #64748b;'>{itemsList}</td>
                    <td class='text-right mono'>{sale.TotalPrice:N0} MMK</td>
                </tr>");
            }

            sb.Append(@"
                    </tbody>
                </table>
            </div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
