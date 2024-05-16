using System;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.Lastfm.Utils;

/// <summary>
/// Common functions to aide in string manipulation.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Compares a source and destination string for similarity.
    /// </summary>
    /// <param name="src">Source string.</param>
    /// <param name="tgt">Target string.</param>
    /// <returns>bool if string IsLike.</returns>
    public static bool IsLike(string src, string tgt)
    {
        // Placeholder until we have a better way
        var source = SanitiseString(src);
        var target = SanitiseString(tgt);

        return source.Equals(target, StringComparison.OrdinalIgnoreCase);
    }

    private static string SanitiseString(string s)
    {
        // This could also be [a-z][0-9]
        const string Pattern = "[\\~#%&*{}/:<>?,-.()|\"-]";

        // Remove invalid chars and then all spaces
        return Regex.Replace(new Regex(Pattern).Replace(s, string.Empty), @"\s+", string.Empty);
    }
}
