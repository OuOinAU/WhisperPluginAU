using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine.SceneManagement;
using System;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace WhisperPlugin;

[BepInAutoPlugin]
[BepInProcess("Among Us.exe")]
public partial class WhisperPlugin : BasePlugin
{
    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        Harmony.PatchAll();

        SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>) ((scene, _) =>
        {
            if (scene.name == "MainMenu")
            {
                ModManager.Instance.ShowModStamp();
            }
        }));
    }
}

