using System;
using System.Collections.Generic;
using System.Linq;

namespace NovaCRM.Domain.Clients;

public static class ClientStatusRules
{
    public static readonly string[] StatusPriority =
    {
        "BLOCKED",
        "PROBLEM",
        "RISK",
        "VIP",
        "NEW",
        "REGULAR",
    };

    private static readonly HashSet<string> StatusNames = new(
        StatusPriority
            .Concat(new[] { "AT RISK", "AT-RISK", "ATRISK" })
            .Select(name => name.ToUpperInvariant()),
        StringComparer.OrdinalIgnoreCase);

    public static bool IsStatusName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return StatusNames.Contains(name.Trim());
    }
}
