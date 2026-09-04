using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Modules.Defects.Features.CreateDefect;

internal sealed class CreateDefectCommandHandler(
    IRepository<Defect, Guid> defects,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<CreateDefectCommand, Guid>
{
    public async Task<Guid> Handle(CreateDefectCommand request, CancellationToken cancellationToken)
    {
        var defect = Defect.Report(
            currentUser.TenantId,
            request.Title,
            request.Description,
            Enum.Parse<Severity>(request.Severity, ignoreCase: true),
            Enum.Parse<PriorityLevel>(request.Priority, ignoreCase: true),
            request.Environment,
            currentUser.IsAuthenticated ? currentUser.UserId : null,
            request.TestRunId,
            request.Component,
            request.Steps,
            request.Expected,
            request.Actual);

        await defects.AddAsync(defect, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return defect.Id;
    }
}