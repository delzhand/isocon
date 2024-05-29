#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
using System;
using SingularityGroup.HotReload.Newtonsoft.Json;
using UnityEngine;

namespace SingularityGroup.HotReload {
    [Serializable]
    class PatchServerInfo {
        public readonly string hostName;
        public readonly int port;
        public readonly string commitHash;
        public readonly string rootPath;
        public readonly bool isRemote;

        public const string UnknownCommitHash = "unknown";

        /// <param name="hostName">an ip address or "localhost"</param>
        public PatchServerInfo(string hostName, string commitHash, string rootPath, bool isRemote = false) {
            this.hostName = hostName;
            this.commitHash = commitHash ?? UnknownCommitHash;
            this.rootPath = rootPath;
            this.isRemote = isRemote;
            this.port = RequestHelper.port;
        }
        
        /// <param name="hostName">an ip address or "localhost"</param>
        // constructor should (must?) have a param for each field
        [JsonConstructor]
        public PatchServerInfo(string hostName, int port, string commitHash, string rootPath, bool isRemote = false) {
            this.hostName = hostName;
            this.port = port;
            this.commitHash = commitHash ?? UnknownCommitHash;
            this.rootPath = rootPath;
            this.isRemote = isRemote;
        }

        /// <inheritdoc cref="TryParse(Uri,out SingularityGroup.HotReload.PatchServerInfo)"/>
        public static string TryParse(string uriString, out PatchServerInfo info) {
            return TryParse(new Uri(uriString), out info);
        }

        /// <summary>
        /// Extract server info from deeplink uri
        /// </summary>
        /// <returns>Error message string, or null on success</returns>
        public static string TryParse(Uri uri, out PatchServerInfo info) {
            info = null;
            if (!uri.IsWellFormedOriginalString()) {
                return "!IsWellFormedOriginalString";
            }

            if (!uri.AbsolutePath.EndsWith("/connect")) {
                return $"Uri path is {uri.AbsolutePath} but should end with /connect";
            }

            try {
                var commitHash = Uri.UnescapeDataString(uri.Query.TrimStart('?'));
                // success
                info = new PatchServerInfo(uri.Host, uri.Port, commitHash, null, true);
                return null;
            } catch (Exception ex) {
                Log.Exception(ex);
                return $"Parsing uri failed with an exception: {ex}";
        }
    }

        /// <summary>
        /// Convert server info into a uri that launches an app via a deeplink.
        /// </summary>
        /// <returns>Uri that you can display as a QR-Code</returns>
        public Uri ToUri(string applicationId) {
            // dont need rootPath in the uri - it is only used by EditorCodePatcher
            var builder = new UriBuilder($"hotreload-{applicationId}", hostName, port) {
                Path = "connect",
                Query = Uri.EscapeDataString(commitHash),
            };
            return builder.Uri;
        }

        public string ToUriString(string applicationId) => ToUri(applicationId).AbsoluteUri;
    }
}
#endif
