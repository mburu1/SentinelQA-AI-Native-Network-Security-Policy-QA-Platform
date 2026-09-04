namespace SentinelQA.Domain.Enums;

public enum Protocol { Tcp, Udp, Icmp, Any }
public enum RuleAction { Allow, Deny }
public enum Direction { Inbound, Outbound, Any }
public enum PolicyStatus { Draft, Validated, Deployed, Retired }