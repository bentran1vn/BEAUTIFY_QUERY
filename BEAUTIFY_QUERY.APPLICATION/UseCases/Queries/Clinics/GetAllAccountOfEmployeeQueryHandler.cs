using AutoMapper;
using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Repositories;
using BEAUTIFY_QUERY.CONTRACT.Services.Clinics;
using BEAUTIFY_QUERY.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Clinics;
internal sealed class GetAllAccountOfEmployeeQueryHandler(
    IRepositoryBase<User, Guid> userRepository,
    IRepositoryBase<Clinic, Guid> clinicRepository)
    : IQueryHandler<Query.GetAllAccountOfEmployeeQuery,
        List<Response.GetAccountOfEmployee>>
{
    public async Task<Result<List<Response.GetAccountOfEmployee>>> Handle(
        Query.GetAllAccountOfEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        var clinic = await clinicRepository.FindByIdAsync(request.ClinicId, cancellationToken, x => x.UserClinics);
        if (clinic == null)
        {
            return Result.Failure<List<Response.GetAccountOfEmployee>>(new Error("404", "Clinic Not Found"));
        }

        var userIds = clinic.UserClinics.Select(x => x.UserId).ToList();
        var users = await userRepository.FindAll(x => userIds.Contains(x.Id)).ToListAsync(cancellationToken);

        var mappedUsers = users.Select(u => new Response.GetAccountOfEmployee
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Address = u.Address,
            ProfilePictureUrl = u.ProfilePicture,
            Role = u.Role?.Name
        }).ToList();

        return Result.Success(mappedUsers);
    }
}