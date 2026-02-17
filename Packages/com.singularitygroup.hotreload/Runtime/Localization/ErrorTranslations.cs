#if ENABLE_MONO && (DEVELOPMENT_BUILD || UNITY_EDITOR)
namespace SingularityGroup.HotReload.Localization {

	internal static partial class Translations {
		public static class Errors {
			public static string MethodNameMismatch;
			public static string DeclaringTypeNameMismatch;
			public static string IsGenericMethodDefinitionMismatch;
			public static string MissingThisParameter;
			public static string ThisParameterTypeMismatch;
			public static string ParameterCountMismatch;
			public static string ParameterTypeMismatch;
			public static string ReturnTypeMismatch;
			public static string GenericParameterNotGenericType;
			public static string GenericParameterDidNotExist;
			public static string IsPlayerWithHotReloadFalse;
			public static string UnknownExceptionReadingBuildInfo;
			public static string BuildInfoNotFound;
			public static string FailedPromptsPrefab;
			public static string HandshakeFailedInvalidBuildTarget;
			public static string BuildTargetMismatch;
			public static string UnableToResolveMethodInAssembly;
			public static string UnableToResolveTypeInAssembly;
			public static string UnableToResolveFieldInAssembly;
            
			public static void LoadEnglish() {
				MethodNameMismatch = "Method name mismatch";
				DeclaringTypeNameMismatch = "Declaring type name mismatch";
				IsGenericMethodDefinitionMismatch = "IsGenericMethodDefinition mismatch";
				MissingThisParameter = "missing this parameter";
				ThisParameterTypeMismatch = "this parameter type mismatch";
				ParameterCountMismatch = "parameter count mismatch";
				ParameterTypeMismatch = "parameter type mismatch";
				ReturnTypeMismatch = "Return type mismatch";
				GenericParameterNotGenericType = "Generic parameter did not resolve to generic type definition";
				GenericParameterDidNotExist = "Generic parameter did not exist on the generic type definition";
				IsPlayerWithHotReloadFalse = "IsPlayerWithHotReload() is false";
				UnknownExceptionReadingBuildInfo = "Uknown exception happened when reading build info";
				BuildInfoNotFound = "Uknown issue happened when reading build info.";
				FailedPromptsPrefab = "Failed to find PromptsPrefab (are you using Hot Reload as a package?";
				HandshakeFailedInvalidBuildTarget = "Server did not declare its current Unity activeBuildTarget in the handshake response. Will assume it is {0}.";
				BuildTargetMismatch = "Your Unity project is running on {0}. You may need to switch it to {1} for Hot Reload to work.";
				UnableToResolveMethodInAssembly = "Unable to resolve method {0} in assembly {1}";
				UnableToResolveTypeInAssembly = "Unable to resolve type with name: {0} in assembly {1}";
				UnableToResolveFieldInAssembly = "Unable to resolve field with name: {0} in assembly {1}";
			}
            
			public static void LoadSimplifiedChinese() {
				MethodNameMismatch = "方法名称不匹配";
				DeclaringTypeNameMismatch = "声明类型名称不匹配";
				IsGenericMethodDefinitionMismatch = "IsGenericMethodDefinition 不匹配";
				MissingThisParameter = "缺少 this 参数";
				ThisParameterTypeMismatch = "this 参数类型不匹配";
				ParameterCountMismatch = "参数数量不匹配";
				ParameterTypeMismatch = "参数类型不匹配";
				ReturnTypeMismatch = "返回类型不匹配";
				GenericParameterNotGenericType = "泛型参数未解析为泛型类型定义";
				GenericParameterDidNotExist = "泛型参数在泛型类型定义上不存在";
				IsPlayerWithHotReloadFalse = "IsPlayerWithHotReload() 为 false";
				UnknownExceptionReadingBuildInfo = "读取构建信息时发生未知异常";
				BuildInfoNotFound = "读取构建信息时发生未知问题。";
				FailedPromptsPrefab = "未能找到 PromptsPrefab（您是否将 Hot Reload 作为软件包使用？";
				HandshakeFailedInvalidBuildTarget = "服务器在握手响应中未声明其当前的 Unity activeBuildTarget。将假定为 {0}。";
				BuildTargetMismatch = "您的 Unity 项目正在 {0} 上运行。您可能需要将其切换到 {1} 才能使 Hot Reload 工作。";
				UnableToResolveMethodInAssembly = "无法在程序集 {1} 中解析方法 {0}";
				UnableToResolveTypeInAssembly = "无法在程序集 {1} 中解析名称为 {0} 的类型";
				UnableToResolveFieldInAssembly = "无法在程序集 {1} 中解析名称为 {0} 的字段";
			}
		}
	}

}
#endif
