using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

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

                ModConfig = LoadConfiguration(api);

                Mod.Logger.Debug("Lasting Armor: Looking for original Vintage Story code...");
                System.Reflection.MethodInfo OriginalApplyConfigMethod = typeof(SurvivalCoreSystem).GetMethod("applyConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                Mod.Logger.Debug("Lasting Armor: Applying world config Harmony postfix...");
                HI.Patch(OriginalApplyConfigMethod, postfix: new HarmonyMethod(typeof(ApplyConfigPostfix).GetMethod("Postfix")));
            }
        }

        public override void Start(ICoreAPI api)
        {
            //Mod.Logger.Event("Lasting Armor: Running init phase and loading config...");
            
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
            // TODO Loading the config file should check versions in future
            ModConfig = api.LoadModConfig<LastingArmorConfig>("LastingArmorConfig.json");
            //try
            //{
                if (ModConfig is null)
                {
                    api.Logger.Debug("Lasting Armor: No config found, creating default LastingArmorConfig.json");

                    ModConfig = new LastingArmorConfig();

                    api.StoreModConfig(ModConfig, "LastingArmorConfig.json");
                }
            //}
            //catch (Exception e)
            //{
            //Mod.Logger.Error("Lasting Armor: Error loading config: {0}", e.Message);
            //}
            return ModConfig;
        }
    }

    [HarmonyPatch(typeof(SurvivalCoreSystem), "applyConfig")]
    public class ApplyConfigPostfix
    {
        public static void Postfix(SurvivalCoreSystem __instance)
        {
            // Access the api field via reflection since it's private
            var apiField = typeof(SurvivalCoreSystem).GetField("api", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var api = apiField?.GetValue(__instance) as ICoreAPI;
            var worldConfig = api?.World?.Config;

            // Get the loaded mod system instance from the API
            var modSystem = api?.ModLoader.GetModSystem<LastingArmorModSystem>();
            int multiplier;
            if (modSystem?.ModConfig?.EnableWorldDurabilityScaling == true)
            {
                multiplier = worldConfig["toolDurability"].GetValue().ToString().ToInt();
                //api.Logger.Debug("LastingArmor: {0}",multiplier);
            }
            else
            {
                multiplier = modSystem?.ModConfig?.DurabilityMultiplier ?? 20;
            }
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
        public int ConfigVersion { get; set; } = 1; // Version of the config file
        public string CommentScaling { get; set; } = "Setting EnableWorldDurabilityScaling to true makes the mod ignore DurabilityMultiplier."; // Comment for the config file
        public int DurabilityMultiplier { get; set; } = 20; // Default multiplier
        public bool EnableWorldDurabilityScaling { get; set; } = false; // Default setting to enable durability scaling
    }
}
