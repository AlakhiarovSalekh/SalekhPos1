// Copyright (c) SalekhPos contributors. All rights reserved.
// Licensed under the proprietary license. See LICENSE in the project root.

namespace SalekhPos.Application.Common;

/// <summary>
/// Bundled top-N common-password list. Loaded from the embedded
/// <c>CommonPasswords.txt</c> resource so it is reproducible across
/// environments and never requires a network call at startup.
///
/// The list is a curated subset of the public "SecLists/Common-Credentials"
/// top 1,000, used here as a fast pre-check. We intentionally do NOT
/// depend on a network download at runtime — defence in depth.
/// </summary>
public static class CommonPasswordList
{
    private static readonly Lazy<HashSet<string>> Set = new(Load);

    public static bool IsCommon(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return Set.Value.Contains(password);
    }

    private static HashSet<string> Load()
    {
        var asm = typeof(CommonPasswordList).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("CommonPasswords.txt", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                "Embedded resource 'CommonPasswords.txt' was not found in the Application assembly.");

        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Failed to open CommonPasswords.txt embedded resource.");
        using var reader = new StreamReader(stream);
        var set = new HashSet<string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            set.Add(trimmed);
        }

        return set;
    }
}
