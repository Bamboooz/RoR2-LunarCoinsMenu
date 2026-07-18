using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using UnityEngine;

namespace LunarCoinsMenu
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("___riskofthunder.RoR2BepInExPack", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.HardDependency)]
    public class LunarCoinsMenuPlugin : BaseUnityPlugin
    {
        internal static BepInEx.Logging.ManualLogSource Log;

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Bamboooz";
        public const string PluginName = "LunarCoinsMenu";
        public const string PluginVersion = "1.0.1";

        #region mod

        public void Awake()
        {
            Log = Logger;

            try
            {
                ModSettingsManager.SetModDescription("A Risk of Rain 2 mod made to modify your Lunar Coins.");

                string pluginDir = Path.GetDirectoryName(Info.Location);
                string iconPath = Path.Combine(pluginDir, "icon.png");

                if (!File.Exists(iconPath))
                {
                    iconPath = Path.Combine(Path.GetDirectoryName(pluginDir), "icon.png");
                }

                if (File.Exists(iconPath))
                {
                    Texture2D tex = new Texture2D(2, 2);

                    if (tex.LoadImage(File.ReadAllBytes(iconPath)))
                    {
                        ModSettingsManager.SetModIcon(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
                    }
                }

                ModConfig.Init(Config);

                On.RoR2.Run.Start += Run_Start;
            }
            catch (Exception e)
            {
                Log.LogError("Failed to initialize LunarCoinsMenu: " + e.Message);
            }
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, RoR2.Run self)
        {
            orig(self);

            if (ModConfig.ShouldUpdate.Value == ModConfig.UpdateMode.Once)
            {
                ModConfig.ShouldUpdate.Value = ModConfig.UpdateMode.Disabled;

                SetLunarCoins();
            }

            if (ModConfig.ShouldUpdate.Value == ModConfig.UpdateMode.OnEachRun)
            {
                SetLunarCoins();
            }
        }

        private static void SetLunarCoins()
        {
            try
            {
                var localUser = RoR2.LocalUserManager.GetFirstLocalUser();

                if (localUser == null)
                {
                    Log.LogWarning("LocalUser is null, couldn't set coins.");

                    return;
                }

                if (localUser.userProfile == null)
                {
                    Log.LogWarning("UserProfile is null, couldn't set coins.");

                    return;
                }

                uint amount = (uint) Mathf.Max(0, ModConfig.LunarCoinsAmount.Value);

                localUser.userProfile.coins = amount;

                Log.LogInfo($"Set Lunar Coins to {amount}");
            }
            catch (Exception e)
            {
                Log.LogError($"Error setting lunar coins: {e}");
            }
        }

        #endregion

        #region config

        public static class ModConfig
        {
            public static ConfigEntry<UpdateMode> ShouldUpdate;
            public static ConfigEntry<int> LunarCoinsAmount;

            public enum UpdateMode
            {
                Disabled,
                Once,
                OnEachRun
            }

            public static void Init(ConfigFile config)
            {
                ShouldUpdate = config.Bind(
                    "General",
                    "Update mode",
                    UpdateMode.Disabled,
                    "Select whether the game should update your lunar coins once, or on the start of each run, or not at all."
                );

                LunarCoinsAmount = config.Bind(
                    "General",
                    "Lunar Coins Amount",
                    0,
                    "Set lunar coins amount."
                );

                ModSettingsManager.AddOption(new ChoiceOption(ShouldUpdate));
                ModSettingsManager.AddOption(new IntFieldOption(LunarCoinsAmount, new IntFieldConfig { Min = 0, Max = int.MaxValue }));
            }
        }

        #endregion
    }
}
