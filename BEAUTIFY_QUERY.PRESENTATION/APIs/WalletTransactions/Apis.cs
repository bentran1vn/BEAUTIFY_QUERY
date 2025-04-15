using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Constrants;
using BEAUTIFY_QUERY.CONTRACT.Services.WalletTransactions;

namespace BEAUTIFY_QUERY.PRESENTATION.APIs.WalletTransactions;
public class Apis : ApiEndpoint, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/wallet-transactions";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var gr1 = app.NewVersionedApi("WalletTransactions")
            .MapGroup(BaseUrl).HasApiVersion(1);

        // 1. Endpoint for a clinic to get its own transactions
        gr1.MapGet("clinic", GetClinicWalletTransactions)
            .RequireAuthorization(Constant.Role.CLINIC_STAFF)
            .WithName("GetClinicWalletTransactions")
            .WithSummary("Get wallet transactions for the current clinic")
            .WithDescription(
                "Retrieves wallet transactions for the authenticated clinic with filtering, sorting, and pagination options");

        // 2. Endpoint for a parent clinic to get transactions of its sub-clinics
        gr1.MapGet("sub-clinics", GetSubClinicWalletTransactions)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN)
            .WithName("GetSubClinicWalletTransactions")
            .WithSummary("Get wallet transactions for all sub-clinics of a parent clinic")
            .WithDescription(
                "Retrieves wallet transactions for all sub-clinics of the specified parent clinic with filtering, sorting, and pagination options");

        // 3. Endpoint for admin to get all clinic transactions
        gr1.MapGet("admin/all-clinics", GetAllClinicWalletTransactions)
            .RequireAuthorization(Constant.Role.SYSTEM_ADMIN)
            .WithName("GetAllClinicWalletTransactions")
            .WithSummary("Get wallet transactions for all clinics (admin only)")
            .WithDescription(
                "Retrieves wallet transactions for all clinics with filtering, sorting, and pagination options. Requires admin privileges.");

        // 4. Endpoint for customer to get their own transactions
        gr1.MapGet("customer", CustomerGetAllWalletTransactions)
            .RequireAuthorization(Constant.Role.CUSTOMER)
            .WithName("CustomerGetAllWalletTransactions")
            .WithSummary("Get wallet transactions for the current customer")
            .WithDescription(
                "Retrieves wallet transactions for the authenticated customer with filtering, sorting, and pagination options");
                
        // 5. Endpoint to get wallet history for a specific clinic by ID
        gr1.MapGet("clinics/{clinicId:guid}", GetWalletHistoryByClinicId)
            .RequireAuthorization(Constant.Role.CLINIC_ADMIN, Constant.Role.SYSTEM_ADMIN)
            .WithName("GetWalletHistoryByClinicId")
            .WithSummary("Get wallet history for a specific clinic by ID")
            .WithDescription(
                "Retrieves wallet transactions for a specific clinic. Clinic admins can only access their own clinic or sub-clinics. System admins can access any clinic.");
    }

    private static async Task<IResult> GetWalletHistoryByClinicId(
        ISender sender,
        Guid clinicId,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetWalletHistoryByClinicId(
            clinicId,
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex,
            pageSize));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetClinicWalletTransactions(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetClinicWalletTransactions(
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex,
            pageSize));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetSubClinicWalletTransactions(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetSubClinicWalletTransactions(
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex,
            pageSize));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> CustomerGetAllWalletTransactions(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.CustomerGetAllWalletTransactions(
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex,
            pageSize
        ));
        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }

    private static async Task<IResult> GetAllClinicWalletTransactions(
        ISender sender,
        string? searchTerm = null,
        string? sortColumn = null,
        string? sortOrder = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new Query.GetAllClinicWalletTransactions(
            searchTerm,
            sortColumn,
            SortOrderExtension.ConvertStringToSortOrder(sortOrder),
            pageIndex,
            pageSize));

        return result.IsFailure ? HandlerFailure(result) : Results.Ok(result);
    }
}
