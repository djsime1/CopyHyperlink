using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace CopyHyperlink;

public class CopyHyperlink : ResoniteMod
{
    public override string Name => "CopyHyperlink";
    public override string Author => "djsime1 / Zenuru";
    public override string Version => "1.0.1";
    public override string Link => "https://github.com/djsime1/CopyHyperlink";

    public override void OnEngineInit()
    {
        Harmony harmony = new("je.dj.CopyHyperlink");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(HyperlinkOpenDialog), "OnAttach")]
    class CopyHyperlinkPatch
    {
        public static void Postfix(HyperlinkOpenDialog __instance, ref SyncRef<Button> ____openButton, Sync<Uri> ___URL)
        {
            var ui = new UIBuilder(____openButton.Target.Slot.Parent);
            RadiantUI_Constants.SetupEditorStyle(ui);

            var btn = ui.Button("Interaction.CopyLink".AsLocaleKey(), RadiantUI_Constants.Sub.CYAN);
            var text = btn.Slot.GetComponentInChildren<Text>();
            btn.LocalPressed += (_, _) =>
            {
                __instance.InputInterface.Clipboard.SetText(___URL.Value.ToString());
                text.LocaleContent = "General.CopiedToClipboard".AsLocaleKey();
                __instance.RunInSeconds(2, () => text.LocaleContent = "Interaction.CopyLink".AsLocaleKey());
            };
        }
    }
}
