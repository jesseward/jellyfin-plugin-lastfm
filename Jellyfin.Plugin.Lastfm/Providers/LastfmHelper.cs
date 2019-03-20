namespace Jellyfin.Plugin.Lastfm.Providers
{
    using MediaBrowser.Common.Configuration;
    using System;
    using System.IO;
    using System.Linq;

    public static class LastfmHelper
    {
        public static string GetImageUrl(IHasLastFmImages data, out string size)
        {
            size = null;

            if (data.image == null)
            {
                return null;
            }

            var validImages = data.image
                .Where(i => !string.IsNullOrWhiteSpace(i.url))
                .ToList();

            var img = validImages
                .FirstOrDefault(i => string.Equals(i.size, "mega", StringComparison.OrdinalIgnoreCase)) ??
                data.image.FirstOrDefault(i => string.Equals(i.size, "extralarge", StringComparison.OrdinalIgnoreCase)) ??
                data.image.FirstOrDefault(i => string.Equals(i.size, "large", StringComparison.OrdinalIgnoreCase)) ??
                data.image.FirstOrDefault(i => string.Equals(i.size, "medium", StringComparison.OrdinalIgnoreCase)) ??
                data.image.FirstOrDefault();

            if (img != null)
            {
                size = img.size;
                return img.url;
            }

            return null;
        }

        /// <exception cref="ArgumentNullException">thrown when string params are null.</exception>
        /// <exception cref="IOException">Unable to perform file operation on cachePath.</exception>
        public static void SaveImageInfo(IApplicationPaths appPaths, string musicBrainzId, string url, string size)
        {
            if (appPaths == null)
            {
                throw new ArgumentNullException("appPaths");
            }
            if (string.IsNullOrEmpty(musicBrainzId))
            {
                throw new ArgumentNullException("musicBrainzId");
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            var cachePath = Path.Combine(appPaths.CachePath, "lastfm", musicBrainzId, "image.txt");

            if (string.IsNullOrEmpty(url))
            {
                File.Delete(cachePath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                File.WriteAllText(cachePath, url + "|" + size);
            }

        }
    }
}
