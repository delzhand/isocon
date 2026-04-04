using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class SystemTagModal
{
    public static void Open()
    {
        Player.Self().SetOp("Adding Table Tag");
        SelectionMenu.Hide();

        Modal2.CreateContext("ShunDialog1");
        var contents = Modal2.Contents("ShunDialog1");
        Modal2.AddDialogHeader("Add System Tag");

        var tagType = Modal2.AddInlineSelectField("Type", "Type", "Simple", StringUtility.CreateArray("Simple", "Number", "Clock").ToList<string>());
        tagType.Q<ShunSelect>().OnSelect += () =>
        {
            Modal2.ReadContext("ShunDialog1");
            var container = contents.Q<ShunContainer>("TagTypeContainer");
            container.Clear();
            string type = Modal2.GetSelectFieldValue("Type");
            if (type == "Number" || type == "Clock")
            {
                var initVal = Modal2.AddInlineIntField("InitialValue", "Initial Value", 0);
                Modal2.MoveToContainer(initVal, container);
            }
            if (type == "Clock")
            {
                var maxVal = Modal2.AddInlineIntField("MaxValue", "Max Value", 4);
                Modal2.MoveToContainer(maxVal, container);
            }
        };

        Modal2.AddInlineTextField("TagName", "Tag Name", "", "The text that will appear on the tag");
        Modal2.AddInlineComboboxField("Color", "Color", "Black", ColorUtility.CommonColors().ToList<string>());

        var typeContainer = new ShunContainer();
        typeContainer.name = "TagTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        contents.Add(typeContainer);

        var footer = Modal2.AddDialogFooter();

        var confirm = new ShunButton();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Create";
        confirm.clicked += () =>
        {
            AddSystemTagSubmit();
        };
        footer.Add(confirm);

        Modal2.Open();
        Modal2.AddCloseAction(() =>
        {
            Player.Self().ClearOp();
        });
    }

    private static void AddSystemTagSubmit()
    {
        Modal2.ReadContext("ShunDialog1");
        string tagName = Modal2.GetTextFieldValue("TagName");
        int tagValue = Modal2.GetIntFieldValue("InitialValue");
        int tagMaxValue = Modal2.GetIntFieldValue("MaxValue");
        string colorValue = Modal2.GetComboboxFieldValue("Color");
        string tagType = Modal2.GetSelectFieldValue("Type");
        GameSystemTag tag = new();
        tag.Name = tagName;
        tag.Value = tagValue;
        tag.Type = tagType;
        tag.MaxValue = tagMaxValue;
        tag.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag)}");
        Modal2.Dialog("ShunDialog1").Close();
    }
}