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
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(ExclusiveFullscreen)} is loaded!", MessageType.Success);

            new Harmony("MegaPiggy.ExclusiveFullscreen").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        }
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

    private static readonly MethodInfo ReplacementSetResolution = typeof(GraphicSettings_ApplyAllGraphicSettings_Patch).GetMethod(
        nameof(SetResolution),
        BindingFlags.Static | BindingFlags.NonPublic);

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(OriginalSetResolution))
            {
                yield return new CodeInstruction(OpCodes.Call, ReplacementSetResolution)
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