using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Extensions;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.PRESENTATION.Abstractions;
using BEAUTIFY_QUERY.CONTRACT.Services.DoctorCertificates;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.DoctorCertificates;
public class DoctorCertificateApi : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "/api/v{version:apiVersion}/doctor-certificates";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Doctor Certificates").MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet("{doctorId:guid}", GetDoctorCertificateByDoctorId);
        gr1.MapGet("doctor-certificate/{id:guid}", GetDoctorCertificateById);
        gr1.MapGet(string.Empty, GetAllDoctorCertificates);
    }


    private static async Task<IResult> GetDoctorCertificateByDoctorId(
        ISender sender,
        Guid doctorId)
    {
        var result = await sender.Send(new Query.GetDoctorCertificateByDoctorId(doctorId));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetDoctorCertificateById(
        ISender sender,
        Guid id)
    {
        var result = await sender.Send(new Query.GetDoctorCertificateById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    private static async Task<IResult> GetAllDoctorCertificates(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllDoctorCertificates(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}