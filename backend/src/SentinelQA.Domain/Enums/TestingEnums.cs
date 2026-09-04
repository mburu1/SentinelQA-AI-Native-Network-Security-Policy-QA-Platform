namespace SentinelQA.Domain.Enums;

public enum TestRunStatus { Pending, Running, Completed, Failed, Cancelled }
public enum TestResultStatus { Passed, Failed, Skipped, Error }
public enum TestCaseType { Positive, Negative, Boundary, Security, Performance }