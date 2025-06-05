using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.Common;
using Vintagestory.API.Config;
using Vintagestory.Datastructures;
using Vintagestory.GameContent;
using System.Text.Json;
using System;
using System.Diagnostics;
using Vintagestory.ServerMods;

namespace LastingArmor
{
    public class LastingArmorModSystem : ModSystem
    {
        private Harmony HI => new Harmony(Mod.Info.ModID);
        public LastingArmorConfig ModConfig { get; private set; }

        public override void StartPre(ICoreAPI api)
        {
            if (api.Side.IsServer()) // only apply the patch server side
            {
                Mod.Logger.Event("Lasting Armor: Running pre-init phase...");

                Mod.Logger.Debug("Lasting Armor: Looking for original Vintage Story code...");
                System.Reflection.MethodInfo OriginalApplyConfigMethod = typeof(SurvivalCoreSystem).GetMethod("applyConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                Mod.Logger.Debug("Lasting Armor: Applying world config Harmony postfix...");
                HI.Patch(OriginalApplyConfigMethod, postfix: new HarmonyMethod(typeof(ApplyConfigPostfix).GetMethod("Postfix")));
            }
        }

        public override void Start(ICoreAPI api)
        {
            api.Logger.Event("Lasting Armor: Running init phase and loading config...");
            // Load the 
            ModConfig = LoadConfiguration(api);
            //api.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            //api.Logger.Notification("Hello from template mod server side: " + Lang.Get("lastingarmor:hello"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            //api.Logger.Notification("Hello from template mod client side: " + Lang.Get("lastingarmor:hello"));
        }

        public override void Dispose()
        {
            HI.UnpatchAll(HI.Id);
        }

        private LastingArmorConfig LoadConfiguration(ICoreAPI api)
        {
            ModConfig = api.LoadModConfig<LastingArmorConfig>("LastingArmorConfig.json");
            try
            {
                if (ModConfig is null)
                {
                    api.Logger.Debug("Lasting Armor: No config found, creating default LastingArmorConfig.json");

                    ModConfig = new LastingArmorConfig();

                    api.StoreModConfig(ModConfig, "LastingArmorConfig.json");
                }
            }
            catch (Exception e)
            {
                Mod.Logger.Error("Lasting Armor: Error loading config: {0}", e.Message);
            }
            return ModConfig;
        }
    }

    [HarmonyPatch(typeof(SurvivalCoreSystem), "applyConfig")]
    public class ApplyConfigPostfix
    {
        public static void Postfix(SurvivalCoreSystem __instance)
        {
            //System.Diagnostics.Debug.Print("Lasting Armor: Postfix");
            var multiplier = 20;
            // Access the api field via reflection since it's private
            var apiField = typeof(SurvivalCoreSystem).GetField("api", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var api = apiField?.GetValue(__instance) as ICoreAPI;

            if (api.Side.IsServer())
            {
                foreach (CollectibleObject collectible in api.World.Collectibles)
                {
                    if (collectible.Class == "ItemWearable" && collectible.Durability > 1)
                    {
                        collectible.Durability = (int)((float)collectible.Durability * multiplier);
                        api.Logger.Debug("Lasting Armor: Final durability set to {0} for [{1}]", collectible.Durability, collectible.Code);
                    }
                }
            }
        }
    }

    public class LastingArmorConfig
    {
        public int DurabilityMultiplier { get; set; } = 20; // Default multiplier
        public bool EnableDurabilityScaling { get; set; } = false; // Default setting to enable durability scaling
    }
}
