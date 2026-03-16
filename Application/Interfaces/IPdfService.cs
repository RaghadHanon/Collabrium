using Application.Features.Reports.DTOs;

public interface IPdfService
{
    byte[] GeneratePdf(CycleTimeReport data, byte[] chart1, byte[] chart2, byte[] chart3);
}