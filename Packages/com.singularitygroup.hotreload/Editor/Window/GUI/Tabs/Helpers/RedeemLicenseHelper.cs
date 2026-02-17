using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SingularityGroup.HotReload.DTO;
using SingularityGroup.HotReload.Editor.Localization;
using SingularityGroup.HotReload.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SingularityGroup.HotReload.Editor {
    internal enum RedeemStage {
        None,
        Registration,
        Redeem,
        Login
    }

    // IMPORTANT: don't rename
    internal enum RegistrationOutcome {
        None,
        Indie,
        Business,
    }

    internal class RedeemLicenseHelper {
        public static readonly RedeemLicenseHelper I = new RedeemLicenseHelper();

        private string _pendingCompanySize;
        private string _pendingInvoiceNumber;
        private string _pendingRedeemEmail;

        private const string registerFlagPath = PackageConst.LibraryCachePath + "/registerFlag.txt";
        public const string registerOutcomePath = PackageConst.LibraryCachePath + "/registerOutcome.txt";

        public RedeemStage RedeemStage { get; private set; }
        public RegistrationOutcome RegistrationOutcome { get; private set; }
        public bool RegistrationRequired => RedeemStage != RedeemStage.None;

        private string status;
        private string error;

        const string statusSuccess = "success";
        const string statusAlreadyClaimed = "already redeemed by this user/device";

        private GUILayoutOption[] secondaryButtonLayoutOptions = new[] { GUILayout.MaxWidth(100) };

        private bool requestingRedeem;
        private HttpClient redeemClient;
        const string redeemUrl = "https://vmhzj6jonn3qy7hk7tx7levpli0bstpj.lambda-url.us-east-1.on.aws/redeem";

        public RedeemLicenseHelper() {
            if (File.Exists(registerFlagPath)) {
                RedeemStage = RedeemStage.Registration;
            }
            try {
                if (File.Exists(registerOutcomePath)) {
                    RegistrationOutcome outcome;
                    if (Enum.TryParse(File.ReadAllText(registerOutcomePath), out outcome)) {
                        RegistrationOutcome = outcome;
                    }
                }
            } catch (Exception e) {
                Log.Warning(Translations.Errors.WarningFailedDeterminingRegistration, e.GetType().Name, e.Message);
            }
        }

        public void RenderStage(HotReloadRunTabState state) {
            if (state.redeemStage == RedeemStage.Registration) {
                RenderRegistration();
            } else if (state.redeemStage == RedeemStage.Redeem) {
                RenderRedeem();
            } else if (state.redeemStage == RedeemStage.Login) {
                RenderLogin(state);
            }
        }

        private void RenderRegistration() {
            var message = PackageConst.IsAssetStoreBuild
                ? Translations.Registration.MessageRegistrationProUsers
                : Translations.Registration.MessageRegistrationLicensingModel;
            if (error != null) {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            } else {
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Translations.Common.LabelCompanySize);
            GUI.SetNextControlName("company_size");
            _pendingCompanySize = EditorGUILayout.TextField(_pendingCompanySize)?.Trim();
            EditorGUILayout.Space();

            if (GUILayout.Button(Translations.Common.ButtonProceed)) {
                int companySize;
                if (!int.TryParse(_pendingCompanySize, out companySize)) {
                    error = Translations.Errors.ErrorEnterNumber;
                } else {
                    error = null;
                    HandleRegistration(companySize);
                }
            }
        }

        void HandleRegistration(int companySize) {
            RequestHelper.RequestEditorEvent(new Stat(StatSource.Client, StatLevel.Debug, StatFeature.Licensing, StatEventType.Register), new EditorExtraData { { StatKey.CompanySize, companySize } });
            if (companySize > 10) {
                FinishRegistration(RegistrationOutcome.Business);
                EditorCodePatcher.DownloadAndRun().Forget();
            } else if (PackageConst.IsAssetStoreBuild) {
                SwitchToStage(RedeemStage.Redeem);
            } else {
                FinishRegistration(RegistrationOutcome.Indie);
                EditorCodePatcher.DownloadAndRun().Forget();
            }
        }

        private void RenderRedeem() {
            if (error != null) {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            } else {
                EditorGUILayout.HelpBox(Translations.Registration.MessageRedeemInstructions, MessageType.Info);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Translations.Common.LabelInvoiceNumber);
            GUI.SetNextControlName("invoice_number");
            _pendingInvoiceNumber = EditorGUILayout.TextField(_pendingInvoiceNumber ?? HotReloadPrefs.RedeemLicenseInvoice)?.Trim();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(Translations.Common.LabelEmail);
            GUI.SetNextControlName("email_redeem");
            _pendingRedeemEmail = EditorGUILayout.TextField(_pendingRedeemEmail ?? HotReloadPrefs.RedeemLicenseEmail);
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(requestingRedeem)) {
                if (GUILayout.Button(Translations.Common.ButtonRedeem, HotReloadRunTab.bigButtonHeight)) {
                    RedeemLicense(email: _pendingRedeemEmail, invoiceNumber: _pendingInvoiceNumber).Forget();
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Translations.Common.ButtonSkip, secondaryButtonLayoutOptions)) {
                    SwitchToStage(RedeemStage.Login);
                }
                GUILayout.FlexibleSpace();
            }
        }

        async Task RedeemLicense(string email, string invoiceNumber) {
            string validationError;
            if (string.IsNullOrEmpty(invoiceNumber)) {
                validationError = Translations.Errors.ErrorEnterInvoiceNumber;
            } else {
                validationError = HotReloadRunTab.ValidateEmail(email);
            }
            if (validationError != null) {
                error = validationError;
                return;
            }
            var resp = await RequestRedeem(email: email, invoiceNumber: invoiceNumber);
            status = resp?.status;
            if (status != null) {
                if (status != statusSuccess && status != statusAlreadyClaimed) {
                    Log.Error(Translations.Errors.WarningRedeemStatusUnknown);
                    error = Translations.Registration.UnknownRedeemError;
                } else {
                    HotReloadPrefs.RedeemLicenseEmail = email;
                    HotReloadPrefs.RedeemLicenseInvoice = invoiceNumber;
                    // prepare data for login screen
                    HotReloadPrefs.LicenseEmail = email;
                    HotReloadPrefs.LicensePassword = null;

                    SwitchToStage(RedeemStage.Login);
                }
            } else if (resp?.error != null) {
                Log.Warning(Translations.Errors.WarningRedeemingLicenseFailed, resp.error);
                error = GetPrettyError(resp);
            } else {
                Log.Warning(Translations.Errors.WarningRedeemUnknownError);
                error = Translations.Registration.UnknownRedeemError;
            }
        }

        string GetPrettyError(RedeemResponse response) {
            var err = response?.error;
            if (err == null) {
                return Translations.Registration.UnknownRedeemError;
            }
            if (err.Contains("Invalid email")) {
                return Translations.Errors.ErrorInvalidEmailAddress;
            } else if (err.Contains("License invoice already redeemed")) {
                return Translations.Errors.ErrorLicenseInvoiceRedeemed;
            } else if (err.Contains("Different license already redeemed by given email")) {
                return Translations.Errors.ErrorEmailAlreadyUsed;
            } else if (err.Contains("Invoice not found")) {
                return Translations.Errors.ErrorInvoiceNotFound;
            } else if (err.Contains("Invoice refunded")) {
                return Translations.Errors.ErrorInvoiceRefunded;
            } else {
                return Translations.Registration.UnknownRedeemError;
            }
        }

        async Task<RedeemResponse> RequestRedeem(string email, string invoiceNumber) {
            requestingRedeem = true;
            await ThreadUtility.SwitchToThreadPool();
            try {
                redeemClient = redeemClient ?? (redeemClient = HttpClientUtils.CreateHttpClient());
                var input = new Dictionary<string, string> {
                    { "email", email },
                    { "invoice", invoiceNumber }
                };
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                using (var resp = await redeemClient.PostAsync(redeemUrl, content, HotReloadWindow.Current.cancelToken).ConfigureAwait(false)) {
                    if (resp.StatusCode != HttpStatusCode.OK) {
                        return new RedeemResponse(null, string.Format(Translations.Errors.ErrorRedeemRequestFailed, (int)resp.StatusCode, resp.ReasonPhrase));
                    }
                    var str = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    try {
                        return JsonConvert.DeserializeObject<RedeemResponse>(str);
                    } catch (Exception ex) {
                        return new RedeemResponse(null, string.Format(Translations.Errors.ErrorFailedDeserializingRedeem, ex.GetType().Name, ex.Message));
                    }
                }
            } catch (WebException ex) {
                return new RedeemResponse(null, string.Format(Translations.Errors.ErrorRedeemingWebException, ex.Message));
            } finally {
                requestingRedeem = false;
            }
        }

        private class RedeemResponse {
            public string status;
            public string error;

            public RedeemResponse(string status, string error) {
                this.status = status;
                this.error = error;
            }
        }

        private void RenderLogin(HotReloadRunTabState state) {
            if (status == statusSuccess) {
                EditorGUILayout.HelpBox(Translations.Registration.MessageRedeemSuccess, MessageType.Info);
            } else if (status == statusAlreadyClaimed) {
                EditorGUILayout.HelpBox(Translations.Registration.MessageRedeemAlreadyClaimed, MessageType.Info);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            HotReloadRunTab.RenderLicenseInnerPanel(state, renderLogout: false);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Translations.Common.ButtonGoBack, secondaryButtonLayoutOptions)) {
                    SwitchToStage(RedeemStage.Redeem);
                }
                GUILayout.FlexibleSpace();
            }
        }

        public void StartRegistration() {
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(registerFlagPath));
            using (File.Create(registerFlagPath)) {
            }
            RedeemStage = RedeemStage.Registration;
            RegistrationOutcome = RegistrationOutcome.None;
        }

        public void FinishRegistration(RegistrationOutcome outcome) {
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(registerFlagPath));
            File.WriteAllText(registerOutcomePath, outcome.ToString());
            File.Delete(registerFlagPath);
            RegistrationOutcome = outcome;
            SwitchToStage(RedeemStage.None);
            Cleanup();
        }

        void SwitchToStage(RedeemStage stage) {
            // remove focus so that the input field re-renders
            GUI.FocusControl(null);
            RedeemStage = stage;
        }

        void Cleanup() {
            redeemClient?.Dispose();
            redeemClient = null;
            _pendingCompanySize = null;
            _pendingInvoiceNumber = null;
            _pendingRedeemEmail = null;
            status = null;
            error = null;
        }
    }
}