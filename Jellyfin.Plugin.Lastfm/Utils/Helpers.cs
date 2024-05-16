using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Jellyfin.Plugin.Lastfm.Resources;
using MediaBrowser.Controller.Entities.Audio;

namespace Jellyfin.Plugin.Lastfm.Utils;

/// <summary>
/// Common functions that generate Last.FM metadata.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Creates an MD5 hash from a string.
    /// </summary>
    /// <param name="input">The input string to calculate MD5 hash.</param>
    /// <returns>MD5 hash of the input string.</returns>
    public static string CreateMd5Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
#pragma warning disable CA5351
        var hashBytes = MD5.HashData(inputBytes);
#pragma warning restore CA5351

        // Convert the byte array to hexadecimal string
        var sb = new StringBuilder();
        foreach (byte b in hashBytes)
        {
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Adds the API signature to the data dictionary.
    /// </summary>
    /// <param name="data">Key->Vaule data used to build api_sig.</param>
    public static void AppendSignature(ref Dictionary<string, string> data)
    {
        data.Add("api_sig", CreateSignature(data));
    }

    /// <summary>
    /// Converts a DateTime to a Unix timestamp.
    /// </summary>
    /// <param name="date">DataTime object to convert to timestamp.</param>
    /// <returns>A Unixtimestamp as an int.</returns>
    public static int ToTimestamp(DateTime date)
    {
        return Convert.ToInt32((date - new DateTime(1970, 1, 1)).TotalSeconds);
    }

    /// <summary>
    /// Converts a Unix timestamp to a DateTime.
    /// </summary>
    /// <param name="timestamp">Timestamp value to convert to DateTime (localTime).</param>
    /// <returns>A DataTime object representing local time.</returns>
    public static DateTime FromTimestamp(double timestamp)
    {
        var date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return date.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>
    /// Generates a Unix timestamp for the current time.
    /// </summary>
    /// <returns>A Unixtimestamp as an int.</returns>
    public static int CurrentTimestamp()
    {
        return ToTimestamp(DateTime.UtcNow);
    }

    /// <summary>
    /// Converts input Dictionory into a query string.
    /// </summary>
    /// <param name="data">Dictionary of key value pairs to convert to a query string.</param>
    /// <returns>A Unixtimestamp as an int.</returns>
    public static string DictionaryToQueryString(Dictionary<string, string> data)
    {
        return string.Join("&", data.Where(k => !string.IsNullOrWhiteSpace(k.Value)).Select(kvp => string.Format(CultureInfo.InvariantCulture, "{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value))));
    }

    private static string CreateSignature(Dictionary<string, string> data)
    {
        var s = new StringBuilder();
        foreach (var item in data.OrderBy(x => x.Key))
        {
            s.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1}", item.Key, item.Value));
        }

        // Append seceret
        s.Append(Strings.Keys.LastfmApiSeceret);

        return CreateMd5Hash(s.ToString());
    }

    /// <summary>
    /// Retrieve MusicBrainZ artist ID from a MusicArtist object.
    /// </summary>
    /// <param name="artist">Artist to lookup.</param>
    /// <returns>Musicbrainz artist id.</returns>
    public static string? GetMusicBrainzArtistId(MusicArtist artist)
    {
        if (artist.ProviderIds == null)
        {
            return null;
        }

        if (artist.ProviderIds.TryGetValue("MusicBrainzArtist", out string? mbArtistId))
        {
            return mbArtistId;
        }

        return null;
    }
}
