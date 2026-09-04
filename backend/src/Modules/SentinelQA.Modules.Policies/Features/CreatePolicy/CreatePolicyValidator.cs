using FluentValidation;

namespace SentinelQA.Modules.Policies.Features.CreatePolicy;

public sealed class CreatePolicyValidator : AbstractValidator<CreatePolicyCommand>
{
    public CreatePolicyValidator()
    {
        RuleFor(x => x.FirewallId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Rules).NotEmpty().WithMessage("A policy must contain at least one rule.");
        RuleForEach(x => x.Rules).ChildRules(rule =>
        {
            rule.RuleFor(r => r.Priority).GreaterThan(0);
            rule.RuleFor(r => r.Source).NotEmpty();
            rule.RuleFor(r => r.Destination).NotEmpty();
        });
    }
}