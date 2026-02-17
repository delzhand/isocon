using System;
using System.Threading.Tasks;
using SingularityGroup.HotReload.Editor.Cli;
using SingularityGroup.HotReload.Editor.Localization;
using UnityEditor;

namespace SingularityGroup.HotReload.Editor {
    internal sealed class ExposeServerOption : ComputerOptionBase {

        public override string ShortSummary => Translations.Settings.OptionExposeServerShort;
        public override string Summary => Translations.Settings.OptionExposeServerFull;

        public override void InnerOnGUI() {
            string description;
            if (GetValue()) {
                description = Translations.Settings.OptionExposeServerDescriptionEnabled;
            } else {
                description = Translations.Settings.OptionExposeServerDescriptionDisabled;
            }
            EditorGUILayout.LabelField(description, HotReloadWindowStyles.WrapStyle);
        }

        public override bool GetValue() {
            return HotReloadPrefs.ExposeServerToLocalNetwork;
        }

        public override void SetValue(SerializedObject so, bool val) {
            // AllowAndroidAppToMakeHttpRequestsOption
            if (val == HotReloadPrefs.ExposeServerToLocalNetwork) {
                return;
            }

            HotReloadPrefs.ExposeServerToLocalNetwork = val;
            if (val) {
                // they allowed this one for mobile builds, so now we allow everything else needed for player build to work with HR
                new AllowAndroidAppToMakeHttpRequestsOption().SetValue(so, true);
            }
            RunTask(() => {
                RunOnMainThreadSync(() => {
                    var isRunningResult = ServerHealthCheck.I.IsServerHealthy;
                    if (isRunningResult) {
                        var restartServer = EditorUtility.DisplayDialog(Translations.Dialogs.DialogTitleHotReload,
                            string.Format(Translations.Dialogs.DialogMessageRestartExposeServer, Summary),
                            Translations.Dialogs.DialogButtonRestartServer, Translations.Dialogs.DialogButtonDontRestart);
                        if (restartServer) {
                            CodePatcher.I.ClearPatchedMethods();
                            EditorCodePatcher.RestartCodePatcher().Forget();
                        }
                    }
                });
            });
        }

        void RunTask(Action action) {
            var token = HotReloadWindow.Current.cancelToken;
            Task.Run(() => {
                if (token.IsCancellationRequested) return;
                try {
                    action();
                } catch (Exception ex) {
                    ThreadUtility.LogException(ex, token);
                }
            }, token);
        }

        void RunOnMainThreadSync(Action action) {
            ThreadUtility.RunOnMainThread(action, HotReloadWindow.Current.cancelToken);
        }
    }
}
