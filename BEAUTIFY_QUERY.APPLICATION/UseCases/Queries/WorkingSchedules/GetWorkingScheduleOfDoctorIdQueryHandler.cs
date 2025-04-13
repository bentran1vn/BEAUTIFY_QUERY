using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.CONTRACT.Enumerations;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.WorkingSchedules;
using BEAUTIFY_QUERY.DOMAIN.Documents;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using MongoDB.Driver.Linq;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.WorkingSchedules;
internal sealed class GetWorkingScheduleOfDoctorIdQueryHandler(
    IRepositoryBase<WorkingSchedule, Guid> workingScheduleRepository,
    ICurrentUserService currentUserService) 
{
    public async Task<Result<PagedResult<Response.GetWorkingScheduleResponse>>> Handle(
        Query.GetWorkingScheduleOfDoctorId request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        var searchTerm = request.searchTerm?.Trim() ?? string.Empty;
        var query = workingScheduleRepository.FindAll(x =>
            !x.IsDeleted && x.DoctorClinic.UserId.Equals(userId) && x.CustomerScheduleId != null);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            if (searchTerm.Contains("to", StringComparison.OrdinalIgnoreCase))
            {
                var parts = searchTerm.Split("to", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var part1 = parts[0].Trim();
                    var part2 = parts[1].Trim();

                    // Try to parse as a date range
                    if (DateOnly.TryParse(part1, out var dateFrom) &&
                        DateOnly.TryParse(part2, out var dateTo))
                    {
                        query = query.Where(x => x.Date >= dateFrom && x.Date <= dateTo);
                    }
                    // Otherwise, try to parse as a time range
                    else if (TimeSpan.TryParse(part1, out var timeFrom) &&
                             TimeSpan.TryParse(part2, out var timeTo))
                    {
                        query = query.Where(x => x.StartTime >= timeFrom && x.EndTime <= timeTo);
                    }
                }
            }
            else
            {
                // Fallback to standard contains search with null checks
                query = query.Where(x =>
                    x.CustomerSchedule != null &&
                    x.CustomerSchedule.Customer != null &&
                    (
                        (x.CustomerSchedule.Status != null &&
                         x.CustomerSchedule.Status.ToLower().Contains(searchTerm.ToLower())) ||
                        (x.CustomerSchedule.Customer.FirstName != null &&
                         x.CustomerSchedule.Customer.FirstName.ToLower().Contains(searchTerm.ToLower())) ||
                        (x.CustomerSchedule.Customer.LastName != null &&
                         x.CustomerSchedule.Customer.LastName.ToLower().Contains(searchTerm.ToLower()))
                    ));
            }
        }

        query = request.SortOrder == SortOrder.Descending
            ? query.OrderByDescending(x => x.Date)
            : query.OrderBy(x => x.Date);
        var total = await PagedResult<WorkingSchedule>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize
        );
        var result = total.Items.Select(x => new Response.GetWorkingScheduleResponse
        {
            DoctorName = x.DoctorClinic.User.FullName,
            WorkingScheduleId = x.Id,
            ClinicId = x.DoctorClinic.ClinicId,
            DoctorId = x.DoctorClinic.UserId,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Date = x.Date,
            Status = x.CustomerSchedule.Status,
            StepIndex = x.CustomerSchedule.ProcedurePriceType.Procedure.StepIndex.ToString(),
            CustomerName = x.CustomerSchedule.Customer.FullName,
            CustomerId = x.CustomerSchedule.CustomerId,
            ServiceId = x.CustomerSchedule.ServiceId,
            ServiceName = x.CustomerSchedule.Service.Name,
            CustomerScheduleId = x.CustomerScheduleId,
            CurrentProcedureName = x.CustomerSchedule.ProcedurePriceType.Name,
        }).ToList();
        var mapped = new PagedResult<Response.GetWorkingScheduleResponse>(
            result,
            total.PageIndex,
            total.PageSize,
            total.TotalCount
        );
        return Result.Success(mapped);
    }
}