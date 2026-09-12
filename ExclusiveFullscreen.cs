using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace ExclusiveFullscreen
{
    public class ExclusiveFullscreen : ModBehaviour
    {
        public static ExclusiveFullscreen Instance;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            ModHelper.Console.WriteLine(
                "Patching",
                MessageType.Info
            );

            new Harmony("MegaPiggy.ExclusiveFullscreen")
                .PatchAll(Assembly.GetExecutingAssembly());

            EnsureExclusiveFullscreen();
        }

        private void EnsureExclusiveFullscreen()
        {
            if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
                return;

            ModHelper.Console.WriteLine(
                "Changing borderless fullscreen to exclusive fullscreen.",
                MessageType.Info
            );

            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
    }

[HarmonyPatch(typeof(GraphicSettings), nameof(GraphicSettings.ApplyAllGraphicSettings))]
public static class GraphicSettings_ApplyAllGraphicSettings_Patch
{
    private static readonly MethodInfo OriginalSetResolution = typeof(Screen).GetMethod(
        nameof(Screen.SetResolution),
        new[]
        {
            typeof(int),
            typeof(int),
            typeof(bool)
        });

    private static readonly MethodInfo ReplacementSetResolution =
        typeof(GraphicSettings_ApplyAllGraphicSettings_Patch).GetMethod(
            nameof(SetResolution),
            BindingFlags.Static | BindingFlags.NonPublic
        );

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(OriginalSetResolution))
            {
                yield return new CodeInstruction(
                    OpCodes.Call,
                    ReplacementSetResolution
                )
                .WithLabels(instruction.labels)
                .WithBlocks(instruction.blocks);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void SetResolution(int width, int height, bool fullScreen)
    {
        Screen.SetResolution(
            width,
            height,
            fullScreen
                ? FullScreenMode.ExclusiveFullScreen
                : FullScreenMode.Windowed
        );
    }
}
}