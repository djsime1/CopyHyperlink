using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Reflection;
using FrooxEngine;
using Elements.Core;
using FrooxEngine.UIX;

namespace CopyHyperlink;

public class CopyHyperlink : ResoniteMod
{
    public override string Name => "CopyHyperlink";
    public override string Author => "djsime1";
    public override string Version => "1.0.0";
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
            // UniLog.Log($"[CopyHyperlink] Injecting button!\n\t__instance: {__instance}\n\t____openButton: {____openButton}\n\t___URL: {___URL}", true);

            var ui = new UIBuilder(____openButton.Target.Slot.Parent);
            RadiantUI_Constants.SetupEditorStyle(ui);

            var btn = ui.Button("Interaction.CopyLink".AsLocaleKey(), RadiantUI_Constants.Sub.CYAN);
            var text = btn.Slot.GetComponentInChildren<Text>();
            btn.LocalPressed += (button, data) => {
                __instance.InputInterface.Clipboard.SetText(___URL.Value.ToString());
                text.LocaleContent = "General.CopiedToClipboard".AsLocaleKey();
                // HighlightHelper.FlashHighlight(__instance.Slot, null, colorX.Cyan);
                __instance.RunInSeconds(2, delegate
                {
                    text.LocaleContent = "Interaction.CopyLink".AsLocaleKey();
                });
            };

            // UniLog.Log($"[CopyHyperlink] Done.\n\tui: {ui}\n\tbtn: {btn}");
        }
    }
}
