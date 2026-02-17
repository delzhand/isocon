namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Errors {
            // Error Messages
            public static string ErrorInvalidInput;
            public static string ErrorNetworkIssue;
            public static string ErrorDownloadFailed;
            public static string ErrorServerBinaryNotFound;
            public static string ErrorCopyingServerBinary;
            public static string ErrorDownloadSucceeded;
            public static string ErrorContactSupport;
            public static string ErrorEnterNumber;
            public static string ErrorEnterEmail;
            public static string ErrorValidEmail;
            public static string ErrorEnterPassword;
            public static string ErrorMailExtensions;
            public static string ErrorEnterInvoiceNumber;
            public static string ErrorInvalidEmailAddress;
            public static string ErrorLicenseInvoiceRedeemed;
            public static string ErrorEmailAlreadyUsed;
            public static string ErrorInvoiceNotFound;
            public static string ErrorInvoiceRefunded;
            public static string ErrorPromoCodeInvalid;
            public static string ErrorPromoCodeUsed;
            public static string ErrorPromoCodeExpired;
            public static string ErrorLicenseExtended;
            public static string ErrorPromoCodeActivation;
            public static string ErrorPromoCodeNetwork;
            
            // Warning Messages
            public static string WarningUnityJobHotReloaded;
            public static string WarningBuildSettingsNotSupported;
            public static string WarningInlinedMethods;
            public static string WarningMacOSVersionDetectionFailed;
            public static string WarningUnexpectedSaveProblem;
            public static string WarningRedeemStatusUnknown;
            public static string WarningRedeemUnknownError;
            public static string WarningFailedToRunServerCommand;
            public static string WarningVersionCheckException;
            public static string WarningVersionCheckFailed;
            public static string WarningUpdateIssueFailed;
            public static string WarningUpdatePackageFailed;
            public static string WarningUnableToFindPackage;
            public static string WarningCompileCheckerIssue;
            public static string WarningFailedToStartServer;
            public static string WarningNoSlnFileFound;
            public static string WarningPreparingBuildInfoFailed;
            public static string WarningInlineMethodChecker;
            public static string WarningRefreshingAssetFailed;
            public static string WarningFailedDeterminingRegistration;
            public static string WarningRedeemingLicenseFailed;
            public static string WarningInitializingEventEntries;
            public static string WarningPersistingEventEntries;
            public static string WarningIndicationTextNotFound;
            
            // Info Messages
            public static string InfoDebuggerAttached;
            public static string InfoInspectorFieldRecompile;
            public static string InfoDefaultProjectGeneration;
            public static string ErrorFreeChargesUnavailable;
            
            // Exception Messages
            public static string ExceptionExpectedZipFile;
            public static string ExceptionZipFileNotFound;
            public static string ExceptionUnzipFailed;
            public static string ExceptionDownloadFailed;
            public static string ExceptionUnableToFindManifest;
            public static string ErrorRedeemRequestFailed;
            public static string ErrorFailedDeserializingRedeem;
            public static string ErrorRedeemingWebException;
            public static string ExceptionDownloadContentLengthUnknown;
            public static string ExceptionDownloadFileCorrupted;
            public static string ExceptionFailedToFindAppDirectory;
            public static string ExceptionCouldNotStartCodePatcher;
            
            // Info/Debug Messages
            public static string InfoManifestSearch;
            public static string InfoOmitProjectsForPlayerBuild;
            public static string InfoSeparator;
            public static string InfoOmittedEditorProject;
            public static string InfoFoundProjectNamed;
            
            // Project Generation Warnings
            public static string WarningPostProcessorException;
            public static string WarningPostProcessorFailedProject;
            public static string WarningPostProcessorFailedSolution;
            public static string WarningPostProcessorNoDefaultConstructor;
            public static string WarningPostProcessorConstructorException;
            public static string WarningPostProcessorUnknownException;
            
            // Parse Errors
            public static string ErrorParseError;
            
            // Android Manifest Comments
            public static string CommentAndroidCleartextPermit;
            public static string CommentAndroidCleartextDevelopmentOnly;
            
            // Debug Messages
            public static string DebugDetouringMethodFailed;
            
            // Package Update Errors
            public static string ErrorRequestFailedStatusCode;
            public static string ErrorInvalidPackageJson;
            public static string ErrorInvalidVersionInPackageJson;
            public static string ErrorUnableToFindManifestJson;
            public static string ErrorNoDependenciesInManifest;
            public static string ErrorDependenciesNullInManifest;
            public static string ErrorNoDependenciesSpecified;
            
            public static void LoadEnglish() {
                // Error Messages
                ErrorInvalidInput = "Invalid input";
                ErrorNetworkIssue = "Something went wrong. Please check your internet connection.";
                ErrorContactSupport = "Something went wrong. Please contact support if the issue persists.";
                ErrorDownloadFailed = "Download attempt failed. If the issue persists please reach out to customer support for assistance. Exception: {0}";
                ErrorServerBinaryNotFound = "unable to find server binary for platform '{0}' at '{1}'. Will proceed with downloading the binary (default behavior)";
                ErrorCopyingServerBinary = "encountered exception when copying server binary in the specified custom executable path '{0}':\n{1}";
                ErrorDownloadSucceeded = "Download succeeded!";
                ErrorEnterNumber = "Please enter a number.";
                ErrorEnterEmail = "Please enter your email address.";
                ErrorValidEmail = "Please enter a valid email address.";
                ErrorEnterPassword = "Please enter your password.";
                ErrorMailExtensions = "Mail extensions (in a form of 'username+suffix@example.com') are not supported yet. Please provide your original email address (such as 'username@example.com' without '+suffix' part) as we're working on resolving this issue.";
                ErrorEnterInvoiceNumber = "Please enter invoice number / order ID.";
                ErrorInvalidEmailAddress = "Please enter a valid email address.";
                ErrorLicenseInvoiceRedeemed = "The invoice number/order ID you're trying to use has already been applied to redeem a license. Please enter a different invoice number/order ID. If you have already redeemed a license for another email, you may proceed to the next step.";
                ErrorEmailAlreadyUsed = "The provided email has already been used to redeem a license. If you have previously redeemed a license, you can proceed to the next step and use your existing credentials. If not, please input a different email address.";
                ErrorInvoiceNotFound = "The invoice was not found. Please ensure that you've entered the correct invoice number/order ID.";
                ErrorInvoiceRefunded = "The purchase has been refunded. Please enter a different invoice number/order ID.";
                ErrorPromoCodeInvalid = "Your promo code is invalid. Please ensure that you have entered the correct promo code.";
                ErrorPromoCodeUsed = "Your promo code has already been used.";
                ErrorPromoCodeExpired = "Your promo code has expired.";
                ErrorLicenseExtended = "Your license has already been activated with a promo code. Only one promo code activation per license is allowed.";
                ErrorPromoCodeActivation = "We encountered an error while activating your promo code. Please try again. If the issue persists, please contact our customer support team for assistance.";
                ErrorPromoCodeNetwork = "There is an issue connecting to our servers. Please check your internet connection or contact customer support if the issue persists.";
                
                // Warning Messages
                WarningUnityJobHotReloaded = "A unity job was hot reloaded. This will cause a harmless warning that can be ignored. More info about this can be found here: {0}";
                WarningBuildSettingsNotSupported = "Hot Reload was not included in the build because one or more build settings were not supported.";
                WarningInlinedMethods = "Unity Editor inlines simple methods when it's in \"Release\" mode, which Hot Reload cannot patch.\n\nSwitch to Debug mode to avoid this problem, or let Hot Reload fully recompile Unity when this issue occurs.";
                WarningMacOSVersionDetectionFailed = "Failed to detect MacOS version, if Hot Reload fails to start, please contact support.";
                WarningUnexpectedSaveProblem = "Unexpected problem unable to save HotReloadSettingsObject";
                WarningRedeemStatusUnknown = "Redeeming license failed: unknown status received";
                WarningRedeemUnknownError = "Redeeming a license failed: uknown error encountered";
                WarningFailedToRunServerCommand = "Failed to the run the start server command. ExitCode={0}\nFilepath: {1}";
                WarningVersionCheckException = "encountered exception when checking for new Hot Reload package version:\n{0}";
                WarningVersionCheckFailed = "version check failed: {0}";
                WarningUpdateIssueFailed = "Encountered issue when updating Hot Reload: {0}";
                WarningUpdatePackageFailed = "Failed to update package: {0}";
                WarningUnableToFindPackage = "Unable to find package. message: {0}";
                WarningCompileCheckerIssue = "compile checker encountered issue: {0} {1}";
                WarningFailedToStartServer = "Failed to start the Hot Reload Server. {0}";
                WarningNoSlnFileFound = "No .sln file found. Open any c# file to generate it so Hot Reload can work properly";
                WarningPreparingBuildInfoFailed = "Preparing build info failed! On-device functionality might not work. Exception: {0}";
                WarningInlineMethodChecker = "Inline method checker ran into an exception. Please contact support with the exception message to investigate the problem. Exception: {0}";
                WarningRefreshingAssetFailed = "Refreshing asset at path: {0} failed due to exception: {1}";
                WarningFailedDeterminingRegistration = "Failed determining registration outcome with {0}: {1}";
                WarningRedeemingLicenseFailed = "Redeeming a license failed with error: {0}";
                WarningInitializingEventEntries = "Failed initializing Hot Reload event entries on start: {0}";
                WarningPersistingEventEntries = "Failed persisting Hot Reload event entries: {0}";
                WarningIndicationTextNotFound = "Indication text not found for status {0}";
                
                // Info Messages
                InfoDebuggerAttached = "Debugger was attached. Hot Reload may interfere with your debugger session. Recompiling in order to get full debugger experience.";
                InfoInspectorFieldRecompile = "Some inspector field changes require recompilation in Unity. Auto recompiling Unity according to the settings.";
                InfoDefaultProjectGeneration = "Using default project generation. If you encounter any problem with Unity's default project generation consider disabling it to use custom project generation.";
                ErrorFreeChargesUnavailable = "Free charges unavailabe. Please contact support if the issue persists.";
                
                // Exception Messages
                ExceptionExpectedZipFile = "Expected to end with .zip, but it was: {0}";
                ExceptionZipFileNotFound = "zip file not found {0}";
                ExceptionUnzipFailed = "unzip failed with ExitCode {0}";
                ExceptionDownloadFailed = "Download failed with status code {0} and reason {1}";
                ExceptionUnableToFindManifest = "[{0}] Unable to find {1}";
                ErrorRedeemRequestFailed = "Redeem request failed. Status code: {0}, reason: {1}";
                ErrorFailedDeserializingRedeem = "Failed deserializing redeem response with exception: {0}: {1}";
                ErrorRedeemingWebException = "Redeeming license failed: WebException encountered {0}";
                ExceptionDownloadContentLengthUnknown = "Download failed: Content length unknown";
                ExceptionDownloadFileCorrupted = "Download failed: download file is corrupted";
                ExceptionFailedToFindAppDirectory = "Failed to find .app directory and move it to {0}";
                ExceptionCouldNotStartCodePatcher = "Could not start code patcher process.";
                
                // Info/Debug Messages
                InfoManifestSearch = "Did not find {0} at {1}, searching for manifest file inside {2}";
                InfoOmitProjectsForPlayerBuild = "To compile C# files same as a Player build, we must omit projects which aren't part of the selected Player build.";
                InfoSeparator = "---------";
                InfoOmittedEditorProject = "omitted editor/other project named: {0}";
                InfoFoundProjectNamed = "found project named {0}";
                
                // Project Generation Warnings
                WarningPostProcessorException = "Post processor '{0}' threw exception when calling OnGeneratedCSProjectFilesThreaded:\n{1}";
                WarningPostProcessorFailedProject = "Post processor '{0}' failed when processing project '{1}':\n{2}";
                WarningPostProcessorFailedSolution = "Post processor '{0}' failed when processing solution '{1}':\n{2}";
                WarningPostProcessorNoDefaultConstructor = "The type '{0}' was expected to have a public default constructor but it didn't";
                WarningPostProcessorConstructorException = "Exception occurred when invoking default constructor of '{0}':\n{1}";
                WarningPostProcessorUnknownException = "Unknown exception encountered when trying to create post processor '{0}':\n{1}";
                
                // Parse Errors
                ErrorParseError = "{0} Parse Error : {1}";
                
                // Android Manifest Comments
                CommentAndroidCleartextPermit = "[{0}] Added android:usesCleartextTraffic=\"true\" to permit connecting to the Hot Reload http server running on your machine.";
                CommentAndroidCleartextDevelopmentOnly = "[{0}] This change only happens in Unity development builds. You can disable this in the Hot Reload settings window.";
                
                // Debug Messages
                DebugDetouringMethodFailed = "Detouring {0} method failed. {1} {2}";
                
                // Package Update Errors
                ErrorRequestFailedStatusCode = "Request failed with statusCode: {0} {1}";
                ErrorInvalidPackageJson = "Invalid package.json";
                ErrorInvalidVersionInPackageJson = "Invalid version in package.json: '{0}'";
                ErrorUnableToFindManifestJson = "Unable to find manifest.json";
                ErrorNoDependenciesInManifest = "no dependencies object found in manifest.json";
                ErrorDependenciesNullInManifest = "dependencies object null in manifest.json";
                ErrorNoDependenciesSpecified = "no dependencies specified in manifest.json";
            }
            
            public static void LoadSimplifiedChinese() {
                // Error Messages
                ErrorInvalidInput = "无效输入";
                ErrorNetworkIssue = "出现问题。请检查您的网络连接。";
                ErrorContactSupport = "出现问题。如果问题仍然存在，请联系支持。";
                ErrorDownloadFailed = "下载尝试失败。如果问题仍然存在，请联系客户支持寻求帮助。异常：{0}";
                ErrorServerBinaryNotFound = "无法在“{1}”找到平台“{0}”的服务器二进制文件。将继续下载二进制文件（默认行为）";
                ErrorCopyingServerBinary = "在指定的自定义可执行路径“{0}”中复制服务器二进制文件时遇到异常：\n{1}";
                ErrorDownloadSucceeded = "下载成功！";
                ErrorEnterNumber = "请输入一个数字。";
                ErrorEnterEmail = "请输入您的电子邮件地址。";
                ErrorValidEmail = "请输入有效的电子邮件地址。";
                ErrorEnterPassword = "请输入您的密码。";
                ErrorMailExtensions = "尚不支持邮件扩展（格式为'username+suffix@example.com'）。请提供您原始的电子邮件地址（例如'username@example.com'，不含'+suffix'部分），我们正在努力解决此问题。";
                ErrorEnterInvoiceNumber = "请输入发票号码/订单 ID。";
                ErrorInvalidEmailAddress = "请输入有效的电子邮件地址。";
                ErrorLicenseInvoiceRedeemed = "您尝试使用的发票号码/订单 ID 已用于兑换许可证。请输入不同的发票号码/订单 ID。如果您已经为另一个电子邮件兑换了许可证，您可以继续下一步。";
                ErrorEmailAlreadyUsed = "提供的电子邮件已用于兑换许可证。如果您之前已兑换许可证，您可以继续下一步并使用您现有的凭据。如果没有，请输入不同的电子邮件地址。";
                ErrorInvoiceNotFound = "未找到发票。请确保您输入了正确的发票号码/订单 ID。";
                ErrorInvoiceRefunded = "购买已退款。请输入不同的发票号码/订单 ID。";
                ErrorPromoCodeInvalid = "您的促销代码无效。请确保您输入了正确的促销代码。";
                ErrorPromoCodeUsed = "您的促销代码已被使用。";
                ErrorPromoCodeExpired = "您的促销代码已过期。";
                ErrorLicenseExtended = "您的许可证已使用促销代码激活。每个许可证只允许激活一次促销代码。";
                ErrorPromoCodeActivation = "激活您的促销代码时遇到错误。请重试。如果问题仍然存在，请联系我们的客户支持团队寻求帮助。";
                ErrorPromoCodeNetwork = "连接到我们的服务器时出现问题。请检查您的网络连接，如果问题仍然存在，请联系客户支持。";

                // Warning Messages
                WarningUnityJobHotReloaded = "一个 unity 作业被热重载。这将导致一个可以忽略的无害警告。更多信息可以在这里找到：{0}";
                WarningBuildSettingsNotSupported = "由于一个或多个构建设置不受支持，Hot Reload 未包含在构建中。";
                WarningInlinedMethods = "Unity 编辑器在“发布”模式下会内联简单方法，Hot Reload 无法修补。\n\n切换到调试模式以避免此问题，或让 Hot Reload 在出现此问题时完全重新编译 Unity。";
                WarningMacOSVersionDetectionFailed = "检测 MacOS 版本失败，如果 Hot Reload 启动失败，请联系支持。";
                WarningUnexpectedSaveProblem = "无法保存 HotReloadSettingsObject 的意外问题";
                WarningRedeemStatusUnknown = "兑换许可证失败：收到未知状态";
                WarningRedeemUnknownError = "兑换许可证失败：遇到未知错误";
                WarningFailedToRunServerCommand = "运行启动服务器命令失败。退出代码={0}\n文件路径：{1}";
                WarningVersionCheckException = "检查新的 Hot Reload 软件包版本时遇到异常：\n{0}";
                WarningVersionCheckFailed = "版本检查失败：{0}";
                WarningUpdateIssueFailed = "更新 Hot Reload 时遇到问题：{0}";
                WarningUpdatePackageFailed = "更新软件包失败：{0}";
                WarningUnableToFindPackage = "无法找到软件包。消息：{0}";
                WarningCompileCheckerIssue = "编译检查器遇到问题：{0} {1}";
                WarningFailedToStartServer = "启动 Hot Reload 服务器失败。{0}";
                WarningNoSlnFileFound = "未找到 .sln 文件。打开任何 c# 文件以生成它，以便 Hot Reload 正常工作";
                WarningPreparingBuildInfoFailed = "准备构建信息失败！设备上功能可能无法工作。异常：{0}";
                WarningInlineMethodChecker = "内联方法检查器遇到异常。请联系支持并提供异常消息以调查问题。异常：{0}";
                WarningRefreshingAssetFailed = "刷新路径：{0} 的资产失败，原因异常：{1}";
                WarningFailedDeterminingRegistration = "确定注册结果失败，{0}：{1}";
                WarningRedeemingLicenseFailed = "兑换许可证失败，错误：{0}";
                WarningInitializingEventEntries = "启动时初始化 Hot Reload 事件条目失败：{0}";
                WarningPersistingEventEntries = "持久化 Hot Reload 事件条目失败：{0}";
                WarningIndicationTextNotFound = "未找到状态 {0} 的指示文本";

                // Info Messages
                InfoDebuggerAttached = "调试器已附加。Hot Reload 可能会干扰您的调试会话。正在重新编译以获得完整的调试器体验。";
                InfoInspectorFieldRecompile = "一些检查器字段更改需要 Unity 重新编译。根据设置自动重新编译 Unity。";
                InfoDefaultProjectGeneration = "使用默认项目生成。如果您遇到 Unity 默认项目生成的任何问题，请考虑禁用它以使用自定义项目生成。";
                ErrorFreeChargesUnavailable = "免费费用不可用。如果问题仍然存在，请联系支持。";

                // Exception Messages
                ExceptionExpectedZipFile = "预期以 .zip 结尾，但它是：{0}";
                ExceptionZipFileNotFound = "未找到 zip 文件 {0}";
                ExceptionUnzipFailed = "解压缩失败，退出代码 {0}";
                ExceptionDownloadFailed = "下载失败，状态码 {0}，原因 {1}";
                ExceptionUnableToFindManifest = "[{0}] 无法找到 {1}";
                ErrorRedeemRequestFailed = "兑换请求失败。状态码：{0}，原因：{1}";
                ErrorFailedDeserializingRedeem = "反序列化兑换响应失败，异常：{0}：{1}";
                ErrorRedeemingWebException = "兑换许可证失败：遇到 WebException {0}";
                ExceptionDownloadContentLengthUnknown = "下载失败：内容长度未知";
                ExceptionDownloadFileCorrupted = "下载失败：下载文件已损坏";
                ExceptionFailedToFindAppDirectory = "未能找到 .app 目录并将其移动到 {0}";
                ExceptionCouldNotStartCodePatcher = "无法启动代码修补程序进程。";

                // Info/Debug Messages
                InfoManifestSearch = "在 {1} 未找到 {0}，正在 {2} 内搜索清单文件";
                InfoOmitProjectsForPlayerBuild = "要像 Player 构建一样编译 C# 文件，我们必须省略不属于所选 Player 构建的项目。";
                InfoSeparator = "---------";
                InfoOmittedEditorProject = "省略的编辑器/其他项目名为：{0}";
                InfoFoundProjectNamed = "找到名为 {0} 的项目";

                // Project Generation Warnings
                WarningPostProcessorException = "后处理器“{0}”在调用 OnGeneratedCSProjectFilesThreaded 时引发异常：\n{1}";
                WarningPostProcessorFailedProject = "后处理器“{0}”在处理项目“{1}”时失败：\n{2}";
                WarningPostProcessorFailedSolution = "后处理器“{0}”在处理解决方案“{1}”时失败：\n{2}";
                WarningPostProcessorNoDefaultConstructor = "类型“{0}”应具有公共默认构造函数，但没有";
                WarningPostProcessorConstructorException = "调用“{0}”的默认构造函数时发生异常：\n{1}";
                WarningPostProcessorUnknownException = "尝试创建后处理器“{0}”时遇到未知异常：\n{1}";

                // Parse Errors
                ErrorParseError = "{0} 解析错误：{1}";

                // Android Manifest Comments
                CommentAndroidCleartextPermit = "[{0}] 添加了 android:usesCleartextTraffic=\"true\" 以允许连接到您机器上运行的 Hot Reload http 服务器。";
                CommentAndroidCleartextDevelopmentOnly = "[{0}] 此更改仅在 Unity 开发构建中发生。您可以在 Hot Reload 设置窗口中禁用此功能。";

                // Debug Messages
                DebugDetouringMethodFailed = "Detouring {0} 方法失败。{1} {2}";

                // Package Update Errors
                ErrorRequestFailedStatusCode = "请求失败，状态码：{0} {1}";
                ErrorInvalidPackageJson = "无效的 package.json";
                ErrorInvalidVersionInPackageJson = "package.json 中的版本无效：'{0}'";
                ErrorUnableToFindManifestJson = "无法找到 manifest.json";
                ErrorNoDependenciesInManifest = "在 manifest.json 中找不到依赖项对象";
                ErrorDependenciesNullInManifest = "manifest.json 中的依赖项对象为 null";
                ErrorNoDependenciesSpecified = "manifest.json 中未指定依赖项";
            }
        }
    }
}

