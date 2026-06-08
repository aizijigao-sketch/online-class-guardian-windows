namespace Guardian.Shared.Models;

public enum RuleDecisionKind
{
    Ignore,
    Allow,
    Block
}

public sealed record RuleDecision(RuleDecisionKind Kind, string Reason)
{
    public static RuleDecision Ignore(string reason) => new(RuleDecisionKind.Ignore, reason);
    public static RuleDecision Allow(string reason) => new(RuleDecisionKind.Allow, reason);
    public static RuleDecision Block(string reason) => new(RuleDecisionKind.Block, reason);
}
