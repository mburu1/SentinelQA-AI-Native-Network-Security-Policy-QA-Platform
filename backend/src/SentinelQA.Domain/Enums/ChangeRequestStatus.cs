using System;
using System.Collections.Generic;
using System.Text;

// Required supporting types (Enums + Exception) – place in the appropriate projects

namespace SentinelQA.Domain.Enums;

public enum ChangeRequestStatus
{
    Draft,
    Submitted,
    Validating,
    Validated,
    Testing,
    Tested,
    Approved,
    Rejected,
    Deploying,
    Deployed,
    Completed,
    Failed
}