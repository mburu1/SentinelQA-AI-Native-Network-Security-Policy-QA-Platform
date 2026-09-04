using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SentinelQA.Domain.ValueObjects;

namespace SentinelQA.Infrastructure.Persistence;

public sealed class CidrBlockConverter() : ValueConverter<CidrBlock, string>(
    cidr => cidr.ToString(),
    value => CidrBlock.Parse(value));

public sealed class PortRangeConverter() : ValueConverter<PortRange, string>(
    range => range.ToString(),
    value => PortRange.Parse(value));