using UnityEditor;

namespace SingularityGroup.HotReload.Editor {
    internal static class HotReloadState {
        private const string LastPatchIdKey = "HotReloadWindow.LastPatchId";
        private const string ShowingRedDotKey = "HotReloadWindow.ShowingRedDot";
        private const string RecompiledUnsupportedChangesOnExitPlaymodeKey = "HotReloadWindow.RecompiledUnsupportedChangesOnExitPlaymode";
        
        public static string LastPatchId {
            get { return SessionState.GetString(LastPatchIdKey, string.Empty); }
            set { SessionState.SetString(LastPatchIdKey, value); }
        }
        
        public static bool ShowingRedDot {
            get { return SessionState.GetBool(ShowingRedDotKey, false); }
            set { SessionState.SetBool(ShowingRedDotKey, value); }
        }
        
        public static bool RecompiledUnsupportedChangesOnExitPlaymode {
            get { return SessionState.GetBool(RecompiledUnsupportedChangesOnExitPlaymodeKey, false); }
            set { SessionState.SetBool(RecompiledUnsupportedChangesOnExitPlaymodeKey, value); }
        }
    }

}
