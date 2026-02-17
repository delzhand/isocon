namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Suggestions {
            // Button texts
            public static string ButtonDocs;
            public static string ButtonMoreInfo;
            public static string ButtonVote;
            public static string ButtonUseBuildTimeOnlyAtlas;
            public static string ButtonOpenSettings;
            public static string ButtonIgnoreSuggestion;
            public static string ButtonLearnMore;
            public static string ButtonStopHotReload;
            public static string ButtonDontShowAgain;
            public static string ButtonOK;
            public static string ButtonOKPadded;
            public static string ButtonDisable;
            public static string ButtonTurnOff;
            public static string ButtonAutoRecompile;
            public static string ButtonSwitchToDebugMode;
            public static string ButtonKeepEnabledDuringDebugging;
            public static string ButtonRecompile;
            
            // Unity Best Development Tool Award 2023
            public static string Award2023Title;
            public static string Award2023Message;
            
            // Unsupported Changes
            public static string UnsupportedChangesTitle;
            public static string UnsupportedChangesMessage;
            
            // Unsupported Packages
            public static string UnsupportedPackagesTitle;
            public static string UnsupportedPackagesMessage;
            
            // Auto Recompiled When Playmode State Changes
            public static string AutoRecompiledPlaymodeTitle;
            public static string AutoRecompiledPlaymodeMessage;
            
            // Auto Recompiled When Playmode State Changes 2022
            public static string AutoRecompiled2022Title;
            public static string AutoRecompiled2022Message;
            
            // Multidimensional Arrays
            public static string MultidimensionalArraysTitle;
            public static string MultidimensionalArraysMessage;
            
            // Editors Without HR Running
            public static string EditorsWithoutHRTitle;
            public static string EditorsWithoutHRMessage;
            
            // Field Initializer With Side Effects
            public static string FieldInitializerSideEffectsTitle;
            public static string FieldInitializerSideEffectsMessage;
            
            // Detailed Error Reporting Is Enabled
            public static string DetailedErrorReportingTitle;
            public static string DetailedErrorReportingMessage;
            
            // Field Initializer Existing Instances Edited
            public static string FieldInitializerEditedTitle;
            public static string FieldInitializerEditedMessage;
            
            // Field Initializer Existing Instances Unedited
            public static string FieldInitializerUneditedTitle;
            public static string FieldInitializerUneditedMessage;
            
            // Add Monobehaviour Method
            public static string AddMonobehaviourMethodTitle;
            public static string AddMonobehaviourMethodMessage;
            
            // Switch To Debug Mode For Inlined Methods
            public static string SwitchToDebugModeTitle;
            public static string SwitchToDebugModeMessage;
            public static string SwitchToDebugModeConfirmation;
            
            // Hot Reload While Debugger Is Attached
            public static string DebuggerAttachedTitle;
            public static string DebuggerAttachedMessage;
            
            // Hot Reloaded Methods When Debugger Is Attached
            public static string DebuggerMethodsTitle;
            public static string DebuggerMethodsMessage;
            public static string DebuggerMethodsConfirmation;
            
            // Hot Reloaded Requires UTF8 Encoding
            public static string UTF8EncodingRequiredTitle;
            public static string UTF8EncodingRequiredMessage;
            
            public static void LoadEnglish() {
                // Button texts
                ButtonDocs = "Docs";
                ButtonMoreInfo = "More Info";
                ButtonVote = " Vote ";
                ButtonUseBuildTimeOnlyAtlas = " Use \"Build Time Only Atlas\" ";
                ButtonOpenSettings = " Open Settings ";
                ButtonIgnoreSuggestion = " Ignore suggestion ";
                ButtonLearnMore = " Learn more ";
                ButtonStopHotReload = " Stop Hot Reload ";
                ButtonDontShowAgain = " Don't show again ";
                ButtonOK = " OK ";
                ButtonOKPadded = "    OK    ";
                ButtonDisable = " Disable ";
                ButtonTurnOff = " Turn off ";
                ButtonAutoRecompile = " Auto Recompile ";
                ButtonSwitchToDebugMode = " Switch to Debug mode ";
                ButtonKeepEnabledDuringDebugging = " Keep enabled during debugging ";
                ButtonRecompile = " Recompile ";
                
                // Unity Best Development Tool Award 2023
                Award2023Title = "Vote for the \"Best Development Tool\" Award!";
                Award2023Message = "Hot Reload was nominated for the \"Best Development Tool\" Award. Please consider voting. Thank you!";
                
                // Unsupported Changes
                UnsupportedChangesTitle = "Which changes does Hot Reload support?";
                UnsupportedChangesMessage = "Hot Reload supports most code changes, but there are some limitations. Generally, changes to methods and fields are supported. Things like adding new types is not (yet) supported. See the documentation for the list of current features and our current roadmap";
                
                // Unsupported Packages
                UnsupportedPackagesTitle = "Unsupported package detected";
                UnsupportedPackagesMessage = "The following packages are only partially supported: ECS, Mirror, Fishnet, and Photon. Hot Reload will work in the project, but changes specific to those packages might not hot-reload";
                
                // Auto Recompiled When Playmode State Changes
                AutoRecompiledPlaymodeTitle = "Unity recompiles on enter/exit play mode?";
                AutoRecompiledPlaymodeMessage = "If you have an issue with the Unity Editor recompiling when the Play Mode state changes, more info is available in the docs. Feel free to reach out if you require assistance. We'll be glad to help.";
                
                // Auto Recompiled When Playmode State Changes 2022
                AutoRecompiled2022Title = "Unsupported setting detected";
                AutoRecompiled2022Message = "The 'Sprite Packer Mode' setting can cause unintended recompilations if set to 'Sprite Atlas V1 - Always Enabled'";
                
                // Multidimensional Arrays
                MultidimensionalArraysTitle = "Use jagged instead of multidimensional arrays";
                MultidimensionalArraysMessage = "Hot Reload doesn't support methods with multidimensional arrays ([,]). You can work around this by using jagged arrays ([][])";
                
                // Editors Without HR Running
                EditorsWithoutHRTitle = "Some Unity instances don't have Hot Reload running.";
                EditorsWithoutHRMessage = "Make sure that either: \n1) Hot Reload is installed and running on all Editor instances, or \n2) Hot Reload is stopped in all Editor instances where it is installed.";
                
                // Field Initializer With Side Effects
                FieldInitializerSideEffectsTitle = "Field initializer with side-effects detected";
                FieldInitializerSideEffectsMessage = "A field initializer update might have side effects, e.g. calling a method or creating an object.\n\nWhile Hot Reload does support this, it can sometimes be confusing when the initializer logic runs at 'unexpected times'.";
                
                // Detailed Error Reporting Is Enabled
                DetailedErrorReportingTitle = "Detailed error reporting is enabled";
                DetailedErrorReportingMessage = "When an error happens in Hot Reload, the exception stacktrace is sent as telemetry to help diagnose and fix the issue.\nThe exception stack trace is only included if it originated from the Hot Reload package or binary. Stacktraces from your own code are not sent.\nYou can disable detailed error reporting to prevent telemetry from including any information about your project.";
                
                // Field Initializer Existing Instances Edited
                FieldInitializerEditedTitle = "Field initializer edit updated the value of existing class instances";
                FieldInitializerEditedMessage = "By default, Hot Reload updates field values of existing object instances when new field initializer has constant value.\n\nIf you want to change this behavior, disable the \"Apply field initializer edits to existing class instances\" option in Settings or click the button below.";
                
                // Field Initializer Existing Instances Unedited
                FieldInitializerUneditedTitle = "Field initializer edits don't apply to existing objects";
                FieldInitializerUneditedMessage = "By default, Hot Reload applies field initializer edits of existing fields only to new objects (newly instantiated classes), just like normal C#.\n\nFor rapid prototyping, you can use static fields which will update across all instances.";
                
                // Add Monobehaviour Method
                AddMonobehaviourMethodTitle = "New MonoBehaviour methods are not shown in the inspector";
                AddMonobehaviourMethodMessage = "New methods in MonoBehaviours are not shown in the inspector until the script is recompiled. This is a limitation of Hot Reload handling of Unity's serialization system.\n\nYou can use the button below to auto recompile partially supported changes such as this one.";
                
                // Switch To Debug Mode For Inlined Methods
                SwitchToDebugModeTitle = "Switch code optimization to Debug Mode";
                SwitchToDebugModeMessage = "In Release Mode some methods are inlined, which prevents Hot Reload from applying changes. A clear warning is always shown when this happens, but you can use Debug Mode to avoid the issue altogether";
                SwitchToDebugModeConfirmation = "Switching code optimization will stop Play Mode.\n\nDo you wish to proceed?";
                
                // Hot Reload While Debugger Is Attached
                DebuggerAttachedTitle = "Hot Reload is disabled while a debugger is attached";
                DebuggerAttachedMessage = "Hot Reload automatically disables itself while a debugger is attached, as it can otherwise interfere with certain debugger features.\nWhile disabled, every code change will trigger a full Unity recompilation.\n\nYou can choose to keep Hot Reload enabled while a debugger is attached, though some features like debugger variable inspection might not always work as expected.";
                
                // Hot Reloaded Methods When Debugger Is Attached
                DebuggerMethodsTitle = "Hot Reload may interfere with your debugger session";
                DebuggerMethodsMessage = "Some debugger features, like variable inspection, might not work as expected for methods patched during the Hot Reload session. A full Unity recompile is required to get the full debugger experience.";
                DebuggerMethodsConfirmation = "Using the Recompile button will stop Play Mode.\n\nDo you wish to proceed?";
                
                // Hot Reloaded Requires UTF8 Encoding
                UTF8EncodingRequiredTitle = "Change file encoding to UTF-8";
                UTF8EncodingRequiredMessage = "Unknown source file encoding detected. Change the encoding of the code editor to UTF-8 to resolve this problem.";
            }
            
            public static void LoadSimplifiedChinese() {
                // Button texts
                ButtonDocs = "文档";
                ButtonMoreInfo = "更多信息";
                ButtonVote = " 投票 ";
                ButtonUseBuildTimeOnlyAtlas = " 使用“仅构建时图集” ";
                ButtonOpenSettings = " 打开设置 ";
                ButtonIgnoreSuggestion = " 忽略建议 ";
                ButtonLearnMore = " 了解更多 ";
                ButtonStopHotReload = " 停止 Hot Reload ";
                ButtonDontShowAgain = " 不再显示 ";
                ButtonOK = " 确定 ";
                ButtonOKPadded = "    确定    ";
                ButtonDisable = " 禁用 ";
                ButtonTurnOff = " 关闭 ";
                ButtonAutoRecompile = " 自动重新编译 ";
                ButtonSwitchToDebugMode = " 切换到调试模式 ";
                ButtonKeepEnabledDuringDebugging = " 在调试期间保持启用 ";
                ButtonRecompile = " 重新编译 ";

                // Unity Best Development Tool Award 2023
                Award2023Title = "为“最佳开发工具”奖投票！";
                Award2023Message = "Hot Reload 被提名为“最佳开发工具”奖。请考虑投票。谢谢！";

                // Unsupported Changes
                UnsupportedChangesTitle = "Hot Reload 支持哪些更改？";
                UnsupportedChangesMessage = "Hot Reload 支持大多数代码更改，但存在一些限制。通常支持对方法和字段的更改。尚不支持添加新类型等操作。有关当前功能列表和我们当前路线图，请参阅文档";

                // Unsupported Packages
                UnsupportedPackagesTitle = "检测到不支持的包";
                UnsupportedPackagesMessage = "以下包仅部分支持：ECS、Mirror、Fishnet 和 Photon。Hot Reload 可以在项目中使用，但针对这些包的特定更改可能无法热重载";

                // Auto Recompiled When Playmode State Changes
                AutoRecompiledPlaymodeTitle = "Unity 在进入/退出播放模式时重新编译？";
                AutoRecompiledPlaymodeMessage = "如果您在播放模式状态更改时遇到 Unity 编辑器重新编译的问题，可以在文档中找到更多信息。如果您需要帮助，请随时与我们联系。我们很乐意提供帮助。";

                // Auto Recompiled When Playmode State Changes 2022
                AutoRecompiled2022Title = "检测到不支持的设置";
                AutoRecompiled2022Message = "如果“精灵打包器模式”设置为“精灵图集 V1 - 始终启用”，则可能导致意外的重新编译";

                // Multidimensional Arrays
                MultidimensionalArraysTitle = "使用交错数组代替多维数组";
                MultidimensionalArraysMessage = "Hot Reload 不支持带有二维数组 ([,]) 的方法。您可以通过使用交错数组 ([][]) 来解决此问题";

                // Editors Without HR Running
                EditorsWithoutHRTitle = "一些 Unity 实例没有运行 Hot Reload。";
                EditorsWithoutHRMessage = "请确保：\n1) 在所有编辑器实例上安装并运行 Hot Reload，或\n2) 在所有安装了 Hot Reload 的编辑器实例中停止它。";

                // Field Initializer With Side Effects
                FieldInitializerSideEffectsTitle = "检测到带有副作用的字段初始化器";
                FieldInitializerSideEffectsMessage = "字段初始化器更新可能有副作用，例如调用方法或创建对象。\n\n虽然 Hot Reload 支持此功能，但当初始化器逻辑在“意外时间”运行时，有时可能会令人困惑。";

                // Detailed Error Reporting Is Enabled
                DetailedErrorReportingTitle = "已启用详细错误报告";
                DetailedErrorReportingMessage = "当 Hot Reload 中发生错误时，异常堆栈跟踪将作为遥测数据发送，以帮助诊断和修复问题。\n仅当异常堆栈跟踪源自 Hot Reload 包或二进制文件时才会包含。不会发送您自己代码的堆栈跟踪。\n您可以禁用详细错误报告，以防止遥测数据包含有关您项目的任何信息。";

                // Field Initializer Existing Instances Edited
                FieldInitializerEditedTitle = "字段初始化器编辑更新了现有类实例的值";
                FieldInitializerEditedMessage = "默认情况下，当新的字段初始化器具有常量值时，Hot Reload 会更新现有对象实例的字段值。\n\n如果您想更改此行为，请在“设置”中禁用“将字段初始化器编辑应用于现有类实例”选项，或单击下面的按钮。";

                // Field Initializer Existing Instances Unedited
                FieldInitializerUneditedTitle = "字段初始化器编辑不适用于现有对象";
                FieldInitializerUneditedMessage = "默认情况下，Hot Reload 仅将现有字段的字段初始化器编辑应用于新对象（新实例化的类），就像普通的 C# 一样。\n\n为了快速原型制作，您可以使用静态字段，它将在所有实例中更新。";

                // Add Monobehaviour Method
                AddMonobehaviourMethodTitle = "新的 MonoBehaviour 方法不会显示在检查器中";
                AddMonobehaviourMethodMessage = "在重新编译脚本之前，MonoBehaviours 中的新方法不会显示在检查器中。这是 Hot Reload 处理 Unity 序列化系统的限制。\n\n您可以使用下面的按钮自动重新编译部分支持的更改，例如此更改。";

                // Switch To Debug Mode For Inlined Methods
                SwitchToDebugModeTitle = "将代码优化切换到调试模式";
                SwitchToDebugModeMessage = "在发布模式下，某些方法是内联的，这会阻止 Hot Reload 应用更改。发生这种情况时总是会显示明确的警告，但您可以使用调试模式完全避免此问题";
                SwitchToDebugModeConfirmation = "切换代码优化将停止播放模式。\n\n您希望继续吗？";

                // Hot Reload While Debugger Is Attached
                DebuggerAttachedTitle = "附加调试器时禁用 Hot Reload";
                DebuggerAttachedMessage = "附加调试器时，Hot Reload 会自动禁用自身，因为它可能会干扰某些调试器功能。\n禁用后，每次代码更改都会触发完整的 Unity 重新编译。\n\n您可以选择在附加调试器时保持 Hot Reload 启用，但某些功能（如调试器变量检查）可能不总是按预期工作。";

                // Hot Reloaded Methods When Debugger Is Attached
                DebuggerMethodsTitle = "Hot Reload 可能会干扰您的调试会话";
                DebuggerMethodsMessage = "某些调试器功能，例如变量检查，对于在 Hot Reload 会话期间修补的方法可能无法按预期工作。需要完整的 Unity 重新编译才能获得完整的调试器体验。";
                DebuggerMethodsConfirmation = "使用“重新编译”按钮将停止播放模式。\n\n您希望继续吗？";
                
                // Hot Reloaded Requires UTF8 Encoding
                UTF8EncodingRequiredTitle = "将文件编码更改为 UTF-8";
                UTF8EncodingRequiredMessage = "检测到未知的源文件编码。请将代码编辑器的编码更改为 UTF-8 以解决此问题。";
            }
        }
    }
}

