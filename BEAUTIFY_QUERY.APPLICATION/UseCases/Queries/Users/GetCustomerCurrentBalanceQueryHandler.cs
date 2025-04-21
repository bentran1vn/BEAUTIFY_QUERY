using BEAUTIFY_QUERY.CONTRACT.Services.Users;
using User = BEAUTIFY_QUERY.DOMAIN.Entities.User;

namespace BEAUTIFY_QUERY.APPLICATION.UseCases.Queries.Users;
internal sealed class GetCustomerCurrentBalanceQueryHandler(IRepositoryBase<User, Guid> repositoryBase)
    : IQueryHandler<Query.GetCustomerCurrentBalance, string>
{
    public async Task<Result<string>> Handle(Query.GetCustomerCurrentBalance request,
        CancellationToken cancellationToken)
    {
        var User = await repositoryBase.FindSingleAsync(x => x.Id == request.UserId, cancellationToken);
        return User?.Balance.ToString() ?? Result.Failure<string>(new Error("404", "User Not Found"));
    }
}