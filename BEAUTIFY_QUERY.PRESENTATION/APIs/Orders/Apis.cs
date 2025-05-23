﻿using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.Orders;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.Orders;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/orders";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("Orders")
            .MapGroup(BaseUrl).HasApiVersion(1);
        gr1.MapGet(string.Empty, GetOrder).RequireAuthorization();
        gr1.MapGet("system", GetOrderSystems);
        gr1.MapGet("{id}", GetOrderById);
        gr1.MapGet("clinic", GetOrderByClinicId).RequireAuthorization(Constant.Role.CLINIC_STAFF);
        gr1.MapGet("clinic/branches", GetClinicOrderBranches)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN)
            .WithName("Get Clinic Order Branches")
            .WithSummary("Get all orders from clinic branches")
            .WithDescription("Retrieves orders from all branches of the parent clinic. Requires Clinic_Admin role.");
    }

    private static async Task<IResult> GetOrderByClinicId(
        ISender sender, string? searchTerm = null,
        string? sortColumn = null, string? sortOrder = null,
        Guid? liveStreamId = null, bool? isLiveStream = null,
        int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetOrdersByClinicId(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize, liveStreamId, isLiveStream));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetOrder(ISender sender,
        string? searchTerm = null, string? sortColumn = null,
        string? sortOrder = null, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(
            new Query.GetOrdersByCustomerId(searchTerm, sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder), pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
    
    private static async Task<IResult> GetOrderSystems(ISender sender,
        string? searchTerm = null, string? sortColumn = null,
        string? sortOrder = null, int pageIndex = 1, int pageSize = 10)
    {
        var result = await sender.Send(
            new Query.GetOrderSystems(searchTerm, sortColumn,
                SortOrderExtension.ConvertStringToSortOrder(sortOrder), pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetOrderById(ISender sender, Guid id)
    {
        var result = await sender.Send(new Query.GetOrderById(id));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetClinicOrderBranches(ISender sender, string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetClinicOrderBranchesQuery(searchTerm,
            sortColumn, SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex, pageSize));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}