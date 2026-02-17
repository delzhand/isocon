#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class Logging {
			// Server and Connection
			public static string HotReloadUnreachableDisconnecting;
			public static string RequestHandshakeToServer;
			public static string ServerHealthyAfterHandshake;
			
			// Polling Errors
			public static string PollMethodPatchesFailed;
			public static string PollPatchStatusFailed;
			public static string PollAssetChangesFailed;
			
			// Request Errors
			public static string DeserializingResponseFailed;
			public static string RequestTimeout;
			
			// Method Invocation
			public static string InvokeOnHotReloadFailed;
			public static string InvokeOnHotReloadLocalFailed;
			
			// Build and Player
			public static string HotReloadNotAvailableBuildSettings;
			public static string BuildInfoNotFound;
			
			// Method Compatibility
			public static string UnknownIssue;
			
			// Patch Loading/Saving
			public static string LoadingPatchesFromDiskError;
			public static string LoadingPatchesFromFile;
			public static string LoadedPatchesFromDisk;
			public static string SavingAppliedPatches;
			
			// Patch Registration/Application
			public static string RegisterPatches;
			public static string ApplyPatchesPending;
			public static string DetourMethod;
			
			// Exceptions
			public static string ExceptionHandlingMethodPatch;
			public static string ExceptionApplyingPatch;
			public static string ExceptionEnsureUnityEventMethod;
			public static string ExceptionRemoveUnityEventMethod;
			public static string InvalidPath;
			
			// Field Operations
			public static string FailedRegisteringInitializerInvalidMethod;
			public static string FailedRegisteringInitializerException;
			public static string FailedRegisteringNewFieldDefinitions;
			public static string FailedRemovingInitializer;
			public static string FailedRemovingFieldValue;
			public static string FailedMovingFieldValue;
			public static string FailedUpdatingFieldAttributes;
			public static string FailedAddingFieldToInspector;
			public static string FailedHidingFieldFromInspector;
			
			// Method Patching
			public static string DebuggerAttachedNotAllowed;
			public static string MethodMismatch;
			public static string FailedToApplyPatchForMethod;
			public static string HotReloadApplyTook;
			
			// Unity Events
			public static string SceneLoadedWithNewUnityEventMethods;
            
			public static void LoadEnglish() {
				HotReloadUnreachableDisconnecting = "Hot Reload was unreachable for {0} seconds, disconnecting";
				RequestHandshakeToServer = "Request handshake to Hot Reload server with hostname: {0}";
				ServerHealthyAfterHandshake = "Server is healthy after first handshake? {0}";
				
				PollMethodPatchesFailed = "PollMethodPatches failed with code {0} {1} {2}";
				PollPatchStatusFailed = "PollPatchStatus failed with code {0} {1} {2}";
				PollAssetChangesFailed = "PollAssetChanges failed with code {0} {1} {2}";
				
				DeserializingResponseFailed = "Deserializing response failed with {0}: {1}";
				RequestTimeout = "Request timeout";
				
				InvokeOnHotReloadFailed = "[InvokeOnHotReload] {0} {1} failed. Exception:\n{2}";
				InvokeOnHotReloadLocalFailed = "[InvokeOnHotReloadLocal] {0} {1} failed. Exception:\n{2}";
				
				HotReloadNotAvailableBuildSettings = "Hot Reload is not available in this build because one or more build settings were not supported.";
				BuildInfoNotFound = "Build info not found";
				
				UnknownIssue = "unknown issue";
				
				LoadingPatchesFromDiskError = "Encountered exception when loading patches from disk:";
				LoadingPatchesFromFile = "Loading patches from file {0}";
				LoadedPatchesFromDisk = "Loaded {0} patches from disk";
				SavingAppliedPatches = "Saving {0} applied patches to {1}";
				
				RegisterPatches = "Register patches.\nWarnings: {0} \nMethods:\n{1}";
				ApplyPatchesPending = "ApplyPatches. {0} patches pending.";
				DetourMethod = "Detour method {0:X8} {1}, offset: {2}";
				
				ExceptionHandlingMethodPatch = "Exception occured when handling method patch. Exception:";
				ExceptionApplyingPatch = "Edit requires full recompile to apply: Encountered exception when applying a patch.\nCommon causes: editing code that failed to patch previously, an unsupported change, or a real bug in Hot Reload.\nIf you think this is a bug, please report the issue on Discord and include a code-snippet before/after.";
				ExceptionEnsureUnityEventMethod = "Encountered exception in EnsureUnityEventMethod: {0} {1}";
				ExceptionRemoveUnityEventMethod = "Encountered exception in RemoveUnityEventMethod: {0} {1}";
				InvalidPath = "Invalid path: {0}";
				
				FailedRegisteringInitializerInvalidMethod = "Failed registering initializer for field {0} in {1}. Field value might not be initialized correctly. Invalid method.";
				FailedRegisteringInitializerException = "Failed registering initializer for field {0} in {1}. Field value might not be initialized correctly. Exception: {2}";
				FailedRegisteringNewFieldDefinitions = "Failed registering new field definitions for field {0} in {1}. Exception: {2}";
				FailedRemovingInitializer = "Failed removing initializer for field {0} in {1}. Field value might not be initialized correctly. Exception: {2}";
				FailedRemovingFieldValue = "Failed removing field value from {0} in {1}. Field value in code might not be up to date. Exception: {2}";
				FailedMovingFieldValue = "Failed moving field value from {0} to {1} in {2}. Field value in code might not be up to date. Exception: {3}";
				FailedUpdatingFieldAttributes = "Failed updating field attributes of {0} in {1}. Updates might not reflect in the inspector. Exception: {2}";
				FailedAddingFieldToInspector = "Failed adding field {0}:{1} to the inspector. Field will not be displayed. Exception: {2}";
				FailedHidingFieldFromInspector = "Failed hiding field {0}:{1} from the inspector. Exception: {2}";
				
				DebuggerAttachedNotAllowed = "Patching methods is not allowed while the Debugger is attached. You can change this behavior in settings if Hot Reload is compatible with the debugger you're running.";
				MethodMismatch = "Edit requires full recompile to apply: Method mismatch: {0}, patch: {1}. \nCommon causes: editing code that failed to patch previously, an unsupported change, or a real bug in Hot Reload.\nIf you think this is a bug, please report the issue on Discord and include a code-snippet before/after.";
				FailedToApplyPatchForMethod = "Edit requires full recompile to apply: Failed to apply patch for method {0} in assembly {1}.\nCommon causes: editing code that failed to patch previously, an unsupported change, or a real bug in Hot Reload.\nIf you think this is a bug, please report the issue on Discord and include a code-snippet before/after.\nException: {2}";
				HotReloadApplyTook = "Hot Reload apply took {0}";
				
				SceneLoadedWithNewUnityEventMethods = "A new Scene was loaded while new unity event methods were added at runtime. MonoBehaviours in the Scene will not trigger these new events.";
			}
            
			public static void LoadSimplifiedChinese() {
				HotReloadUnreachableDisconnecting = "Hot Reload {0} 秒内无法访问，正在断开连接";
				RequestHandshakeToServer = "向 Hot Reload 服务器请求握手，主机名：{0}";
				ServerHealthyAfterHandshake = "第一次握手后服务器是否健康？{0}";

				PollMethodPatchesFailed = "PollMethodPatches 失败，代码 {0} {1} {2}";
				PollPatchStatusFailed = "PollPatchStatus 失败，代码 {0} {1} {2}";
				PollAssetChangesFailed = "PollAssetChanges 失败，代码 {0} {1} {2}";

				DeserializingResponseFailed = "反序列化响应失败，{0}：{1}";
				RequestTimeout = "请求超时";

				InvokeOnHotReloadFailed = "[InvokeOnHotReload] {0} {1} 失败。异常：\n{2}";
				InvokeOnHotReloadLocalFailed = "[InvokeOnHotReloadLocal] {0} {1} 失败。异常：\n{2}";

				HotReloadNotAvailableBuildSettings = "由于一个或多个构建设置不受支持，Hot Reload 在此构建中不可用。";
				BuildInfoNotFound = "未找到构建信息";

				UnknownIssue = "未知问题";

				LoadingPatchesFromDiskError = "从磁盘加载补丁时遇到异常：";
				LoadingPatchesFromFile = "从文件 {0} 加载补丁";
				LoadedPatchesFromDisk = "从磁盘加载了 {0} 个补丁";
				SavingAppliedPatches = "将 {0} 个已应用的补丁保存到 {1}";

				RegisterPatches = "注册补丁。\n警告：{0} \n方法：\n{1}";
				ApplyPatchesPending = "ApplyPatches。{0} 个补丁待处理。";
				DetourMethod = "Detour 方法 {0:X8} {1}，偏移量：{2}";

				ExceptionHandlingMethodPatch = "处理方法补丁时发生异常。异常：";
				ExceptionApplyingPatch = "编辑需要完全重新编译才能应用：应用补丁时遇到异常。\n常见原因：编辑之前修补失败的代码、不支持的更改或 Hot Reload 中的真正错误。\n如果您认为这是一个错误，请在 Discord 上报告问题并附上之前/之后的代码片段。";
				ExceptionEnsureUnityEventMethod = "在 EnsureUnityEventMethod 中遇到异常：{0} {1}";
				ExceptionRemoveUnityEventMethod = "在 RemoveUnityEventMethod 中遇到异常：{0} {1}";
				InvalidPath = "无效路径：{0}";

				FailedRegisteringInitializerInvalidMethod = "在 {1} 中为字段 {0} 注册初始化程序失败。字段值可能未正确初始化。方法无效。";
				FailedRegisteringInitializerException = "在 {1} 中为字段 {0} 注册初始化程序失败。字段值可能未正确初始化。异常：{2}";
				FailedRegisteringNewFieldDefinitions = "在 {1} 中为字段 {0} 注册新字段定义失败。异常：{2}";
				FailedRemovingInitializer = "在 {1} 中为字段 {0} 删除初始化程序失败。字段值可能未正确初始化。异常：{2}";
				FailedRemovingFieldValue = "从 {1} 中的 {0} 删除字段值失败。代码中的字段值可能不是最新的。异常：{2}";
				FailedMovingFieldValue = "在 {2} 中将字段值从 {0} 移动到 {1} 失败。代码中的字段值可能不是最新的。异常：{3}";
				FailedUpdatingFieldAttributes = "在 {1} 中更新 {0} 的字段属性失败。更新可能不会反映在检查器中。异常：{2}";
				FailedAddingFieldToInspector = "将字段 {0}:{1} 添加到检查器失败。字段将不会显示。异常：{2}";
				FailedHidingFieldFromInspector = "从检查器中隐藏字段 {0}:{1} 失败。异常：{2}";

				DebuggerAttachedNotAllowed = "附加调试器时不允许修补方法。如果 Hot Reload 与您正在运行的调试器兼容，您可以在设置中更改此行为。";
				MethodMismatch = "编辑需要完全重新编译才能应用：方法不匹配：{0}，补丁：{1}。\n常见原因：编辑之前修补失败的代码、不支持的更改或 Hot Reload 中的真正错误。\n如果您认为这是一个错误，请在 Discord 上报告问题并附上之前/之后的代码片段。";
				FailedToApplyPatchForMethod = "编辑需要完全重新编译才能应用：为程序集 {1} 中的方法 {0} 应用补丁失败。\n常见原因：编辑之前修补失败的代码、不支持的更改或 Hot Reload 中的真正错误。\n如果您认为这是一个错误，请在 Discord 上报告问题并附上之前/之后的代码片段。\n异常：{2}";
				HotReloadApplyTook = "Hot Reload 应用耗时 {0}";

				SceneLoadedWithNewUnityEventMethods = "在运行时添加新的 unity 事件方法时加载了新场景。场景中的 MonoBehaviours 不会触发这些新事件。";
			}
		}
	}

}
#endif
