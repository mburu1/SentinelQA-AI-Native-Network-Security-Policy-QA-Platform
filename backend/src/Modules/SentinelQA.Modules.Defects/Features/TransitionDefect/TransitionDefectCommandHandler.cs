using MediatR;
using SentinelQA.Application.Abstractions;
using SentinelQA.Application.Abstractions.Persistence;
using SentinelQA.Application.Common;
using SentinelQA.Domain.Aggregates;
using SentinelQA.Domain.Enums;

namespace SentinelQA.Modules.Defects.Features.TransitionDefect;

internal sealed class TransitionDefectCommandHandler(
    IRepository<Defect, Guid> defects,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser) : IRequestHandler<TransitionDefectCommand, bool>
{
    public async Task<bool> Handle(TransitionDefectCommand request, CancellationToken cancellationToken)
    {
        var defect = await defects.GetByIdAsync(request.DefectId, cancellationToken)
            ?? throw new NotFoundException($"Defect '{request.DefectId}' was not found.");

        defect.TransitionTo(Enum.Parse<DefectStatus>(request.TargetStatus, ignoreCase: true), currentUser.UserId, request.Note);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}