#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
using System;
using System.Threading.Tasks;
using SingularityGroup.HotReload.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace SingularityGroup.HotReload {
    class QuestionDialog : MonoBehaviour {

        [Header(Localization.Translations.MenuItems.Information)]
        public Text textSummary;
        public Text textSuggestion;

        [Header(Localization.Translations.MenuItems.UIControls)]
        public Button buttonContinue;
        public Button buttonCancel;
        public Button buttonMoreInfo;
        
        public TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();

        public void UpdateView(Config config) {
            textSummary.text = config.summary;
            textSuggestion.text = config.suggestion;

            if (string.IsNullOrEmpty(config.continueButtonText)) {
                buttonContinue.enabled = false;
            } else {
                buttonContinue.GetComponentInChildren<Text>().text = config.continueButtonText;
                buttonContinue.onClick.AddListener(() => {
                    completion.TrySetResult(true);
                    Hide();
                });
            }

            if (string.IsNullOrEmpty(config.cancelButtonText)) {
                buttonCancel.enabled = false;
            } else {
                buttonCancel.GetComponentInChildren<Text>().text = config.cancelButtonText;
                buttonCancel.onClick.AddListener(() => {
                    completion.TrySetResult(false);
                    Hide();
                });
            }
            
            buttonMoreInfo.onClick.AddListener(() => {
                Application.OpenURL(config.moreInfoUrl);
            });
        }

        internal class Config {
            public string summary;
            public string suggestion;
            public string continueButtonText = Localization.Translations.Dialogs.ContinueButtonText;
            public string cancelButtonText = Localization.Translations.Dialogs.CancelButtonText;
            public string moreInfoUrl = PackageConst.DefaultLocale == Locale.SimplifiedChinese ?
                "https://hotreload.net/zh/documentation/on-device#处理不同的提交" :
                "https://hotreload.net/documentation/on-device#handling-different-commits";
        }

        /// hide this dialog
        void Hide() {
            gameObject.SetActive(false); // this should disable the Update loop?
        }
    }
}
#endif
