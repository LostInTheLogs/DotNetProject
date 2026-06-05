using System.Globalization;
using ClinicManager.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicManager.Services;

public class PdfService : IPdfService
{
    private static readonly CultureInfo CurrencyCulture = CultureInfo.GetCultureInfo("en-US");

    public byte[] GenerateVisitSummaryPdf(VisitDetailsDto visit, PatientResponseDto patient, IEnumerable<ClinicalNoteResponseDto> notes)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                // Page Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("THE ABYSMAL MEDICAL CENTER").FontSize(20).Bold().FontColor(Color.FromHex("#1A365D"));
                        col.Item().Text("Ominously Professional Medical Care & Consultations").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(250).AlignRight().Column(col =>
                    {
                        col.Item().Text("VISIT SUMMARY").FontSize(16).Bold().FontColor(Color.FromHex("#1A365D")).AlignRight();
                        col.Item().Text($"Date: {visit.ScheduledDate.ToString("MMMM dd, yyyy HH:mm")}").FontSize(9).AlignRight();
                        col.Item().Text($"Visit ID: #{visit.Id}").FontSize(9).FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // Page Content
                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().Height(1.5f).Background(Color.FromHex("#1A365D"));
                    col.Item().PaddingBottom(15);

                    // Patient & Doctor Information Cards
                    col.Item().Row(row =>
                    {
                        // Patient Card (Left Column)
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(pCol =>
                        {
                            pCol.Item().Text("PATIENT DETAILS").FontSize(10).Bold().FontColor(Color.FromHex("#1A365D"));
                            pCol.Item().PaddingBottom(4);
                            pCol.Item().Text($"Name: {patient.FirstName} {patient.LastName}").Bold();
                            pCol.Item().Text($"PESEL: {patient.Pesel}");
                            pCol.Item().Text($"Insurance No: {patient.InsuranceNumber}");
                            pCol.Item().Text($"Phone: {patient.Phone}");
                            pCol.Item().Text($"Email: {patient.Email}");
                            pCol.Item().Text($"Address: {patient.Address}");
                        });

                        row.ConstantItem(20); // Spacing between columns

                        // Doctor & Appointment Card (Right Column)
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(10).Column(dCol =>
                        {
                            dCol.Item().Text("VISIT DETAILS").FontSize(10).Bold().FontColor(Color.FromHex("#1A365D"));
                            dCol.Item().PaddingBottom(4);
                            dCol.Item().Text($"Doctor: {visit.DoctorFullName}").Bold();
                            dCol.Item().Text($"Scheduled: {visit.ScheduledDate.ToString("yyyy-MM-dd HH:mm")}");
                            dCol.Item().Text($"Status: {visit.Status}");
                            dCol.Item().PaddingTop(8);
                            dCol.Item().Text("Reason for Visit:").Bold().FontSize(9);
                            dCol.Item().Text(visit.Reason).Italic();
                        });
                    });

                    col.Item().PaddingBottom(15);

                    // Clinical Notes Section
                    col.Item().Text("CLINICAL NOTES").FontSize(12).Bold().FontColor(Color.FromHex("#1A365D"));
                    col.Item().PaddingBottom(5);
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingBottom(10);

                    if (notes == null || !notes.Any())
                    {
                        col.Item().Text("No clinical notes recorded for this visit.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Column(notesCol =>
                        {
                            foreach (var note in notes)
                            {
                                notesCol.Item().Border(1).BorderColor(Colors.Grey.Lighten3).Background(Colors.Grey.Lighten5).Padding(8).Column(noteDetail =>
                                {
                                    noteDetail.Item().Row(noteRow =>
                                    {
                                        noteRow.RelativeItem().Text($"{note.NoteType}").Bold().FontColor(Color.FromHex("#2C5282"));
                                        noteRow.ConstantItem(150).AlignRight().Text($"{note.CreatedAt.ToString("yyyy-MM-dd HH:mm")}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                    });
                                    noteDetail.Item().PaddingTop(4);
                                    noteDetail.Item().Text(note.Content);
                                });
                                notesCol.Item().PaddingBottom(8);
                            }
                        });
                    }

                    col.Item().PaddingBottom(10);

                    // Procedures Performed
                    col.Item().Text("PROCEDURES PERFORMED").FontSize(12).Bold().FontColor(Color.FromHex("#1A365D"));
                    col.Item().PaddingBottom(5);
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingBottom(10);

                    if (visit.Procedures == null || !visit.Procedures.Any())
                    {
                        col.Item().Text("No procedures performed during this visit.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Name
                                columns.RelativeColumn(5); // Notes
                                columns.RelativeColumn(2); // Cost
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Procedure").Bold().FontColor(Colors.White);
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Notes").Bold().FontColor(Colors.White);
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Cost").Bold().FontColor(Colors.White).AlignRight();
                            });

                            foreach (var proc in visit.Procedures)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(proc.ProcedureName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(proc.Notes);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(proc.ActualCost.ToString("C", CurrencyCulture)).AlignRight();
                            }
                        });
                    }

                    col.Item().PaddingBottom(15);

                    // Prescribed Medications
                    col.Item().Text("PRESCRIBED MEDICATIONS").FontSize(12).Bold().FontColor(Color.FromHex("#1A365D"));
                    col.Item().PaddingBottom(5);
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingBottom(10);

                    if (visit.Prescriptions == null || !visit.Prescriptions.Any())
                    {
                        col.Item().Text("No medications prescribed during this visit.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Name
                                columns.RelativeColumn(4); // Dosage
                                columns.RelativeColumn(1); // Qty
                                columns.RelativeColumn(2); // Cost
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Medication").Bold().FontColor(Colors.White);
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Dosage").Bold().FontColor(Colors.White);
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Qty").Bold().FontColor(Colors.White).AlignCenter();
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(6).Text("Cost").Bold().FontColor(Colors.White).AlignRight();
                            });

                            foreach (var med in visit.Prescriptions)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(med.MedicationName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(med.Dosage);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(med.Quantity.ToString()).AlignCenter();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(med.TotalCost.ToString("C", CurrencyCulture)).AlignRight();
                            }
                        });
                    }

                    col.Item().PaddingBottom(20);

                    // Financial Summary Box
                    col.Item().AlignRight().Border(1).BorderColor(Color.FromHex("#1A365D")).Background(Colors.Grey.Lighten5).Padding(12).Width(200).Column(sumCol =>
                    {
                        sumCol.Item().Row(sumRow =>
                        {
                            sumRow.RelativeItem().Text("Total Cost:").Bold().FontSize(12).FontColor(Color.FromHex("#1A365D"));
                            sumRow.ConstantItem(100).AlignRight().Text(visit.TotalCost.ToString("C", CurrencyCulture)).Bold().FontSize(12).FontColor(Color.FromHex("#1A365D"));
                        });
                    });
                });

                // Page Footer
                page.Footer().Column(col =>
                {
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Clinic &bull; Confidential Medical Document").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        }).GeneratePdf();

        return document;
    }

    public byte[] GeneratePrescriptionPdf(VisitDetailsDto visit, PatientResponseDto patient)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10));

                // Page Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PRESCRIPTION").FontSize(24).Bold().FontColor(Color.FromHex("#0D9488"));
                        col.Item().Text("The Abysmal Medical Center").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text("Rx").FontSize(32).Bold().FontColor(Color.FromHex("#0D9488")).AlignRight();
                        col.Item().Text($"Date of Issue: {visit.ScheduledDate.ToString("yyyy-MM-dd")}").FontSize(9).AlignRight();
                        col.Item().Text($"Rx No: Rx-{visit.Id:D6}").FontSize(9).FontColor(Colors.Grey.Darken1).AlignRight();
                    });
                });

                // Page Content
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Height(2).Background(Color.FromHex("#0D9488"));
                    col.Item().PaddingBottom(15);

                    // Patient and Doctor Details
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(pCol =>
                        {
                            pCol.Item().Text("PATIENT").FontSize(11).Bold().FontColor(Color.FromHex("#0D9488"));
                            pCol.Item().PaddingBottom(4);
                            pCol.Item().Text($"Name: {patient.FirstName} {patient.LastName}").Bold().FontSize(11);
                            pCol.Item().Text($"PESEL: {patient.Pesel}");
                            pCol.Item().Text($"Address: {patient.Address}");
                        });

                        row.RelativeItem().Column(dCol =>
                        {
                            dCol.Item().Text("PRESCRIBER").FontSize(11).Bold().FontColor(Color.FromHex("#0D9488"));
                            dCol.Item().PaddingBottom(4);
                            dCol.Item().Text($"Dr. {visit.DoctorFullName}").Bold().FontSize(11);
                            dCol.Item().Text("Clinic Staff Doctor");
                            dCol.Item().Text($"Visit ID Reference: #{visit.Id}");
                        });
                    });

                    col.Item().PaddingVertical(20);
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingBottom(20);

                    // Medications List
                    col.Item().Text("PRESCRIBED MEDICATIONS").FontSize(12).Bold().FontColor(Color.FromHex("#0D9488"));
                    col.Item().PaddingBottom(10);

                    if (visit.Prescriptions == null || !visit.Prescriptions.Any())
                    {
                        col.Item().Text("No medications prescribed.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Column(medsCol =>
                        {
                            int count = 1;
                            foreach (var med in visit.Prescriptions)
                            {
                                medsCol.Item().PaddingBottom(12).Row(medRow =>
                                {
                                    medRow.ConstantItem(25).Text($"{count++}.").Bold().FontSize(11);
                                    medRow.RelativeItem().Column(medDetail =>
                                    {
                                        medDetail.Item().Row(titleRow =>
                                        {
                                            titleRow.RelativeItem().Text(med.MedicationName).Bold().FontSize(11);
                                            titleRow.ConstantItem(80).AlignRight().Text($"Qty: {med.Quantity}").Bold();
                                        });
                                        medDetail.Item().PaddingTop(2);
                                        medDetail.Item().Text($"Sig / Dosage: {med.Dosage}").Italic().FontColor(Colors.Grey.Darken3);
                                    });
                                });
                                medsCol.Item().PaddingBottom(5);
                                medsCol.Item().Height(0.5f).Background(Colors.Grey.Lighten3);
                                medsCol.Item().PaddingBottom(5);
                            }
                        });
                    }

                    col.Item().PaddingTop(30);

                    // Doctor signature stamp placeholder
                    col.Item().Row(sigRow =>
                    {
                        sigRow.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Dispense limit: 30 days from date of issue").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                        sigRow.ConstantItem(200).AlignRight().Column(stamp =>
                        {
                            stamp.Item().Height(1).Background(Colors.Grey.Darken1);
                            stamp.Item().PaddingTop(4);
                            stamp.Item().Text("Physician Signature & Stamp").FontSize(9).AlignCenter().FontColor(Colors.Grey.Darken1);
                            stamp.Item().PaddingTop(20);
                        });
                    });
                });

                // Page Footer
                page.Footer().Column(col =>
                {
                    col.Item().Height(1).Background(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5);
                    col.Item().Text("Clinic Rx System &bull; Barcode/Electronic verification system active").FontSize(8).FontColor(Colors.Grey.Darken1).AlignCenter();
                });
            });
        }).GeneratePdf();

        return document;
    }

    public byte[] GenerateServiceCostReportPdf(ServiceCostReportDto report)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("THE ABYSMAL MEDICAL CENTER").FontSize(16).Bold().FontColor(Color.FromHex("#1A365D"));
                        row.RelativeItem().AlignRight().Column(rCol =>
                        {
                            rCol.Item().Text("SERVICE COST REPORT").FontSize(14).Bold().FontColor(Color.FromHex("#1A365D")).AlignRight();
                            rCol.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1).AlignRight();
                        });
                    });
                    col.Item().Height(1.5f).Background(Color.FromHex("#1A365D"));
                    col.Item().PaddingBottom(8);
                    col.Item().Text(report.FilterDescription).FontSize(10).Italic().FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingBottom(6);
                });

                page.Content().Column(col =>
                {
                    col.Item().Row(summaryRow =>
                    {
                        summaryRow.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Column(sCol =>
                        {
                            sCol.Item().Text("SUMMARY").FontSize(10).Bold().FontColor(Color.FromHex("#1A365D"));
                            sCol.Item().PaddingBottom(4);
                            sCol.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Total Visits:").SemiBold();
                                r.RelativeItem().Text(report.TotalVisits.ToString()).AlignRight();
                            });
                            sCol.Item().Row(r =>
                            {
                                r.RelativeItem().Text("Grand Total:").SemiBold();
                                r.RelativeItem().Text(report.GrandTotal.ToString("C", CurrencyCulture)).AlignRight().Bold();
                            });
                        });
                    });

                    col.Item().PaddingBottom(10);

                    if (report.Lines.Count == 0)
                    {
                        col.Item().Text("No completed visits found matching the selected criteria.").Italic().FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                void HCell(string text)
                                {
                                    header.Cell().Background(Color.FromHex("#1A365D")).Padding(5)
                                        .Text(text).Bold().FontColor(Colors.White).FontSize(8);
                                }

                                HCell("Visit #");
                                HCell("Date");
                                HCell("Patient");
                                HCell("Doctor");
                                HCell("Procs");
                                HCell("Meds");
                                header.Cell().Background(Color.FromHex("#1A365D")).Padding(5)
                                    .Text("Total Cost").Bold().FontColor(Colors.White).FontSize(8).AlignRight();
                            });

                            foreach (var line in report.Lines)
                            {
                                void DCell(string text)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                        .Text(text).FontSize(8);
                                }

                                DCell($"#{line.VisitId}");
                                DCell(line.ScheduledDate.ToString("yyyy-MM-dd HH:mm"));
                                DCell(line.PatientName);
                                DCell(line.DoctorName);
                                DCell(line.ProcedureCount.ToString());
                                DCell(line.MedicationCount.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                    .Text(line.TotalCost.ToString("C", CurrencyCulture)).FontSize(8).AlignRight();
                            }
                        });
                    }
                });

                page.Footer().Column(fCol =>
                {
                    fCol.Item().Height(1).Background(Colors.Grey.Lighten2);
                    fCol.Item().PaddingTop(4);
                    fCol.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Clinic &bull; Confidential Financial Report").FontSize(7).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Darken1);
                            x.Span(" of ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            x.TotalPages().FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        }).GeneratePdf();

        return document;
    }
}
