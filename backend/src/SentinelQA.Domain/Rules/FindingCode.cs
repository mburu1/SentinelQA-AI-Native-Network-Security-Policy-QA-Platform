namespace SentinelQA.Domain.Rules;

public enum FindingCode
{
    InvalidCidr,
    InvalidPort,
    DuplicateRule,
    OverlappingRules,
    ConflictingActions,
    ShadowedRule,
    UnreachableRule,
    OverlyBroadRule,
    PublicExposure,
    MissingJustification
}