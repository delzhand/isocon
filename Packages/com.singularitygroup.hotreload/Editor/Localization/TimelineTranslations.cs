namespace SingularityGroup.HotReload.Editor.Localization {
    internal static partial class Translations {
        public static class Timeline {
            // Timeline/Events
            public static string TimelineTitle;
            public static string SuggestionsTitle;
            public static string MessageCompleteRegistration;
            public static string MessageUseStartButton;
            public static string MessageEnableFilters;
            public static string MessageMakeCodeChanges;
            public static string MessageOnly40EntriesShown;
            public static string EventsTooltip;
            public static string LabelSuggestionsFormat;
            public static string LabelTimeline;
            public static string ButtonIgnoreEventType;
            public static string MessageStartHotReload;
            
            // Partially Supported Change Descriptions
            public static string PartiallySupportedLambdaClosure;
            public static string PartiallySupportedEditAsyncMethod;
            public static string PartiallySupportedAddMonobehaviourMethod;
            public static string PartiallySupportedEditMonobehaviourField;
            public static string PartiallySupportedEditCoroutine;
            public static string PartiallySupportedEditGenericFieldInitializer;
            public static string PartiallySupportedAddEnumMember;
            public static string PartiallySupportedEditFieldInitializer;
            public static string PartiallySupportedAddMethodWithAttributes;
            public static string PartiallySupportedAddFieldWithAttributes;
            public static string PartiallySupportedGenericMethodInGenericClass;
            public static string PartiallySupportedNewCustomSerializableField;
            public static string PartiallySupportedMultipleFieldsEditedInTheSameType;
            
            // Event Entry Titles and Descriptions
            public static string EventTitleFailedApplyingPatch;
            public static string EventDescriptionInlinedMethods;
            public static string EventDescriptionNoIssuesFound;
            public static string EventDescriptionSeeUnsupportedChangesBelow;
            public static string EventDescriptionSeeDetailedEntriesBelow;
            public static string EventDescriptionSeePartiallyAppliedChangesBelow;
            public static string EventDescriptionUndetectedChange;
            public static string EventTitleChangePartiallyApplied;
            public static string EventDescriptionFailedApplyingPatchTapForMore;
            
            public static void LoadEnglish() {
                // Timeline/Events
                TimelineTitle = "Timeline";
                SuggestionsTitle = "Suggestions";
                MessageCompleteRegistration = "Complete registration before using Hot Reload";
                MessageUseStartButton = "Use the Start button to activate Hot Reload";
                MessageEnableFilters = "Enable filters to see events";
                MessageMakeCodeChanges = "Make code changes to see events";
                MessageOnly40EntriesShown = "Only last 40 entries are shown";
                EventsTooltip = "Events";
                LabelSuggestionsFormat = "Suggestions ({0})";
                LabelTimeline = "Timeline";
                ButtonIgnoreEventType = "Ignore this event type ";
                MessageStartHotReload = "Press Start to begin using Hot Reload";
                
                // Partially Supported Change Descriptions
                PartiallySupportedLambdaClosure = "A lambda closure was edited (captured variable was added or removed). Changes to it will only be visible to the next created lambda(s).";
                PartiallySupportedEditAsyncMethod = "An async method was edited. Changes to it will only be visible the next time this method is called.";
                PartiallySupportedAddMonobehaviourMethod = "A new method was added or made public. It will not show up in the Inspector until the next full recompilation.";
                PartiallySupportedEditMonobehaviourField = "A field in a MonoBehaviour was removed or reordered. The inspector will not notice this change until the next full recompilation.";
                PartiallySupportedEditCoroutine = "An IEnumerator/IEnumerable was edited. When used as a coroutine, changes to it will only be visible the next time the coroutine is created.";
                PartiallySupportedEditGenericFieldInitializer = "A field initializer inside generic class was edited. Field initializer will not have any effect until the next full recompilation.";
                PartiallySupportedAddEnumMember = "An enum member was added. ToString and other reflection methods work only after the next full recompilation. Additionally, changes to the enum order may not apply until you patch usages in other places of the code.";
                PartiallySupportedEditFieldInitializer = "A field initializer was edited. Changes will only apply to new instances of that type, since the initializer for an object only runs when it is created.";
                PartiallySupportedAddMethodWithAttributes = "A method with attributes was added. Method attributes will not have any effect until the next full recompilation.";
                PartiallySupportedAddFieldWithAttributes = "A field with attributes was added. Field attributes will not have any effect until the next full recompilation.";
                PartiallySupportedGenericMethodInGenericClass = "A generic method was edited. Usages in non-generic classes applied, but usages in the generic classes are not supported.";
                PartiallySupportedNewCustomSerializableField = "A new custom serializable field was added. The inspector will not notice this change until the next full recompilation.";
                PartiallySupportedMultipleFieldsEditedInTheSameType = "Multiple fields modified in the same type during a single patch. Their values have been reset.";
                
                // Event Entry Titles and Descriptions
                EventTitleFailedApplyingPatch = "Failed applying patch to method";
                EventDescriptionInlinedMethods = "Some methods got inlined by the Unity compiler and cannot be patched by Hot Reload. Switch to Debug mode to avoid this problem.";
                EventDescriptionNoIssuesFound = "No issues found";
                EventDescriptionSeeUnsupportedChangesBelow = "See unsupported changes below";
                EventDescriptionSeeDetailedEntriesBelow = "See detailed entries below";
                EventDescriptionSeePartiallyAppliedChangesBelow = "See partially applied changes below";
                EventDescriptionUndetectedChange = "Code semantics didn't change (e.g. whitespace) or the change requires manual recompile.\n\nRecompile to force-apply changes.";
                EventTitleChangePartiallyApplied = "Change partially applied";
                EventDescriptionFailedApplyingPatchTapForMore = "{0}: {1}, tap here to see more.";
            }
            
            public static void LoadSimplifiedChinese() {
                // Timeline/Events
                TimelineTitle = "时间线";
                SuggestionsTitle = "建议";
                MessageCompleteRegistration = "在使用 Hot Reload 前完成注册";
                MessageUseStartButton = "使用开始按钮激活 Hot Reload";
                MessageEnableFilters = "启用过滤器以查看事件";
                MessageMakeCodeChanges = "进行代码更改以查看事件";
                MessageOnly40EntriesShown = "仅显示最后 40 个条目";
                EventsTooltip = "事件";
                LabelSuggestionsFormat = "建议 ({0})";
                LabelTimeline = "时间线";
                ButtonIgnoreEventType = "忽略此事件类型 ";
                MessageStartHotReload = "按开始以开始使用 Hot Reload";

                // Partially Supported Change Descriptions
                PartiallySupportedLambdaClosure = "编辑了 lambda 闭包（添加或删除了捕获的变量）。对其的更改仅对下一个创建的 lambda 可见。";
                PartiallySupportedEditAsyncMethod = "编辑了异步方法。对其的更改仅在下次调用此方法时可见。";
                PartiallySupportedAddMonobehaviourMethod = "添加了新方法或将其设为公共。在下次完全重新编译之前，它不会显示在检查器中。";
                PartiallySupportedEditMonobehaviourField = "删除了 MonoBehaviour 中的字段或重新排序。在下次完全重新编译之前，检查器不会注意到此更改。";
                PartiallySupportedEditCoroutine = "编辑了 IEnumerator/IEnumerable。当用作协程时，对其的更改仅在下次创建协程时可见。";
                PartiallySupportedEditGenericFieldInitializer = "编辑了泛型类中的字段初始化器。在下次完全重新编译之前，字段初始化器不会有任何效果。";
                PartiallySupportedAddEnumMember = "添加了枚举成员。ToString 和其他反射方法仅在下次完全重新编译后才起作用。此外，在您修补代码中其他地方的用法之前，对枚举顺序的更改可能不会应用。";
                PartiallySupportedEditFieldInitializer = "编辑了字段初始化器。更改仅适用于该类型的新实例，因为对象的初始化器仅在创建时运行。";
                PartiallySupportedAddMethodWithAttributes = "添加了带属性的方法。在下次完全重新编译之前，方法属性不会有任何效果。";
                PartiallySupportedAddFieldWithAttributes = "添加了带属性的字段。在下次完全重新编译之前，字段属性不会有任何效果。";
                PartiallySupportedGenericMethodInGenericClass = "编辑了泛型方法。在非泛型类中的用法已应用，但不支持在泛型类中的用法。";
                PartiallySupportedNewCustomSerializableField = "添加了新的自定义可序列化字段。在下次完全重新编译之前，检查器不会注意到此更改。";
                PartiallySupportedMultipleFieldsEditedInTheSameType = "在单个补丁期间，在同一类型中修改了多个字段。它们的值已被重置。";

                // Event Entry Titles and Descriptions
                EventTitleFailedApplyingPatch = "将补丁应用于方法失败";
                EventDescriptionInlinedMethods = "某些方法被 Unity 编译器内联，无法被 Hot Reload 修补。切换到调试模式以避免此问题。";
                EventDescriptionNoIssuesFound = "未发现问题";
                EventDescriptionSeeUnsupportedChangesBelow = "请参阅下面的不支持的更改";
                EventDescriptionSeeDetailedEntriesBelow = "请参阅下面的详细条目";
                EventDescriptionSeePartiallyAppliedChangesBelow = "请参阅下面部分应用的更改";
                EventDescriptionUndetectedChange = "代码语义未更改（例如空格）或更改需要手动重新编译。\n\n重新编译以强制应用更改。";
                EventTitleChangePartiallyApplied = "更改部分应用";
                EventDescriptionFailedApplyingPatchTapForMore = "{0}: {1}，点击此处查看更多。";
            }
        }
    }
}

