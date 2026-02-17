namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Utility {
            // Compilation and Assembly
            public static string CompileError;
            public static string UnsupportedChange;
            public static string AssemblyFileEditError;
            public static string NativePluginEditError;
            public static string InspectorFieldChangeError;
            
            // Version and Project
            public static string InvalidVersionNoMinor;
            public static string InvalidVersionNoPatch;
            public static string FailedCreateCSharpProject;
            public static string ApplicationNotFound;
            
            // Download and Installation
            public static string StreamHasToBeReadable;
            public static string StreamHasToBeWritable;
            public static string UnableToLocateServer;
            public static string UnableToLocateServerDetail;
            public static string CannotFindSolutionFile;
            public static string UnableToUpdatePackageDifferentDrive;
            public static string UnableToLocateHotReloadPackage;
            
            // CLI and Build
            public static string FailedLocatingServer;
            public static string PreparingBuildInfoFailed;
            
            // Logs
            public static string PlayerAssemblyDebug;
            
            // Symbols
            public static string GenericParameterMismatch;
            public static string GenericParameterTypeDefinitionMismatch;
            
            // Method Compatibility
            public static string MethodCallWarning;
            public static string OnHotReloadLocalWarning;
            public static string OnHotReloadWarning;
            public static string OnHotReloadLocalCallWarning;
            
            public static void LoadEnglish() {
                CompileError = "Compile error";
                UnsupportedChange = "Unsupported change";
                AssemblyFileEditError = "errors: AssemblyFileEdit: Editing assembly files requires recompiling in Unity. in {0}";
                NativePluginEditError = "errors: NativePluginEdit: Editing native plugins requires recompiling in Unity. in {0}";
                InspectorFieldChangeError = "errors: Some inspector field changes require recompilation in Unity. Auto recompiling Unity according to the settings.";
                
                InvalidVersionNoMinor = "Invalid version (no minor version given in strict mode)";
                InvalidVersionNoPatch = "Invalid version (no patch version given in strict mode)";
                FailedCreateCSharpProject = "Failed creating c# project because the c# project header did not have the correct amount of arguments, which is {0}";
                ApplicationNotFound = "Application not found";
                
                StreamHasToBeReadable = "Has to be readable";
                StreamHasToBeWritable = "Has to be writable";
                UnableToLocateServer = "Unable to locate the 'Server' directory. ";
                UnableToLocateServerDetail = "Make sure the 'Server' directory is somewhere in the Assets folder inside a 'HotReload' folder or in the HotReload package";
                CannotFindSolutionFile = "Cannot find solution file. Please disable \"useBuiltInProjectGeneration\" in settings to enable custom project generation.";
                UnableToUpdatePackageDifferentDrive = "unable to update package because it is located on a different drive than the unity project";
                UnableToLocateHotReloadPackage = "unable to locate hot reload package";
                
                FailedLocatingServer = "Failed to locate Hot Reload server directory";
                PreparingBuildInfoFailed = "Preparing build info failed! On-device functionality might not work. Exception: {0}";
                
                PlayerAssemblyDebug = "player assembly named {0}";
                
                GenericParameterMismatch = "Generic parameter did not resolve to generic type definition";
                GenericParameterTypeDefinitionMismatch = "Generic parameter did not exist on the generic type definition";
                
                MethodCallWarning = "failed. Make sure it's a method with 0 parameters either static or defined on MonoBehaviour.";
                OnHotReloadLocalWarning = "failed to find method {0}. Make sure it exists within the same class.";
                OnHotReloadWarning = "failed. Make sure it has 0 parameters, or 1 parameter with type List<MethodPatch>. Exception:";
                OnHotReloadLocalCallWarning = "failed. Make sure it has 0 parameters. Exception:";
            }
            
            public static void LoadSimplifiedChinese() {
                CompileError = "编译错误";
                UnsupportedChange = "不支持的更改";
                AssemblyFileEditError = "错误：AssemblyFileEdit：编辑程序集文件需要在 Unity 中重新编译。在 {0} 中";
                NativePluginEditError = "错误：NativePluginEdit：编辑本机插件需要在 Unity 中重新编译。在 {0} 中";
                InspectorFieldChangeError = "错误：一些检查器字段更改需要在 Unity 中重新编译。根据设置自动重新编译 Unity。";

                InvalidVersionNoMinor = "无效版本（严格模式下未提供次要版本）";
                InvalidVersionNoPatch = "无效版本（严格模式下未提供补丁版本）";
                FailedCreateCSharpProject = "创建 C# 项目失败，因为 C# 项目头没有正确的参数数量，即 {0}";
                ApplicationNotFound = "未找到应用程序";

                StreamHasToBeReadable = "必须可读";
                StreamHasToBeWritable = "必须可写";
                UnableToLocateServer = "无法找到“服务器”目录。";
                UnableToLocateServerDetail = "确保“服务器”目录位于 Assets 文件夹内的“HotReload”文件夹中，或在 HotReload 包中";
                CannotFindSolutionFile = "找不到解决方案文件。请在设置中禁用“useBuiltInProjectGeneration”以启用自定义项目生成。";
                UnableToUpdatePackageDifferentDrive = "无法更新包，因为它位于与 unity 项目不同的驱动器上";
                UnableToLocateHotReloadPackage = "无法找到 hot reload 包";

                FailedLocatingServer = "未能找到 Hot Reload 服务器目录";
                PreparingBuildInfoFailed = "准备构建信息失败！设备上功能可能无法工作。异常：{0}";

                PlayerAssemblyDebug = "名为 {0} 的播放器程序集";

                GenericParameterMismatch = "泛型参数未解析为泛型类型定义";
                GenericParameterTypeDefinitionMismatch = "泛型参数在泛型类型定义上不存在";

                MethodCallWarning = "失败。请确保它是一个具有 0 个参数的方法，静态或在 MonoBehaviour 上定义。";
                OnHotReloadLocalWarning = "未能找到方法 {0}。请确保它存在于同一个类中。";
                OnHotReloadWarning = "失败。请确保它有 0 个参数，或 1 个类型为 List<MethodPatch> 的参数。异常：";
                OnHotReloadLocalCallWarning = "失败。请确保它有 0 个参数。异常：";
            }
        }
    }
}

