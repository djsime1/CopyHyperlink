using System.Reflection.Emit;
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
    public override string Version => "1.1.0";
    public override string Link => "https://github.com/djsime1/CopyHyperlink";
    
    private static ModConfiguration Config;
    
    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> skipOpenDelay = new("Skip Hyperlink 'Open' delay", "(Requires restart to change) Skips the 3 second delay before the 'Open' button enables.", () => true);
    public static bool SkipOpenDelay => Config!.GetValue(skipOpenDelay);

    public override void OnEngineInit()
    {
        Harmony harmony = new("je.dj.CopyHyperlink");
        Config = GetConfiguration()!;
        Config.Save();
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(HyperlinkOpenDialog))]
    class CopyHyperlinkPatches
    {
        [HarmonyPatch("OnAttach")]
        [HarmonyPostfix]
        public static void HyperlinkOpenDialog_OnAttach_Postfix(HyperlinkOpenDialog __instance, ref SyncRef<Button> ____openButton, Sync<Uri> ___URL)
        {
            if (__instance.InputInterface.Clipboard is null)
            {
                Warn("InputInterface has no Clipboard, CopyHyperlink can't do anything.");
                return;
            }

            ____openButton.Target.Slot.Parent.Children[^1].OrderOffset = 2; // Keep Cancel as last button
            var ui = new UIBuilder(____openButton.Target.Slot.Parent);
            RadiantUI_Constants.SetupEditorStyle(ui);

            var btn = ui.Button("Interaction.CopyLink".AsLocaleKey(), RadiantUI_Constants.Sub.CYAN);
            btn.Enabled = btn.InputInterface.IsClipboardSupported;
            var text = btn.Slot.GetComponentInChildren<Text>();
            btn.LocalPressed += (_, _) =>
            {
                __instance.InputInterface.Clipboard.SetText(___URL.Value.ToString());
                text.LocaleContent = "General.CopiedToClipboard".AsLocaleKey();
                __instance.RunInSeconds(2, () => text.LocaleContent = "Interaction.CopyLink".AsLocaleKey());
            };
        }

        [HarmonyPatch("Setup")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> HyperlinkOpenDialog_Setup_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.LoadsConstant(HyperlinkOpenDialog.BUTTON_TIMEOUT_SECONDS) && SkipOpenDelay)
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                else
                    yield return instruction;
            }
        }
    }
}
