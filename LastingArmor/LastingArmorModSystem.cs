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

        public override void StartPre(ICoreAPI api)
        {
            // Find and patch durability in all "itemtypes/wearables" JSON files
            if (api is ICoreServerAPI sapi)
            {
/*
                //var modPaths = sapi.GetModPaths();
                *//*foreach (var modPath in modPaths)
                {*//*
                //var wearablesDir = System.IO.Path.Combine(api.GetOrCreateDataPath("cache"), "assets", "itemtypes", "wearables");
                var CachePath = api.GetOrCreateDataPath("Cache");
                if (!System.IO.Directory.Exists(CachePath)) api.Logger.Debug("'{0}' does not exist?",CachePath);
                api.Logger.Debug("Lasting Armor: Cache Path: '{0}'", CachePath);

                foreach (var key in System.IO.Directory.GetFiles(CachePath, "*.json", System.IO.SearchOption.AllDirectories))
                {
                    api.Logger.Debug("Lasting Armor: Array Key Value: {0}", key);
                }

                foreach (var file in System.IO.Directory.GetFiles(CachePath, "*.json", System.IO.SearchOption.AllDirectories))
                {
                    api.Logger.Debug("Lasting Armor: Processing file {0}", file);
                    try
                    {
                        var json = System.IO.File.ReadAllText(file);
                        var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("class", out var JsonItemClass) && JsonItemClass.GetString() == "ItemWearable")
                        {
                            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("durabilityByType", out var durabilityByTypeProp))
                            {
                                // Deserialize to a modifiable structure
                                var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                                if (dict.TryGetValue("durabilityByType", out var durabilityObj) && durabilityObj is JsonElement durabilityElem && durabilityElem.ValueKind == JsonValueKind.Object)
                                {
                                    var durabilityDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                                    foreach (var prop in durabilityElem.EnumerateObject())
                                    {
                                        durabilityDict[prop.Name] = prop.Value.GetInt32() * 20;
                                    }
                                    dict["durabilityByType"] = durabilityDict;
                                    var newJson = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
                                    System.IO.File.WriteAllText(file, newJson);
                                }
                            }
                        }
                    }
                    catch { *//* Ignore malformed files *//* }
                }
                //}
*/
            }


            if (api.Side.IsServer()) // only apply the patch server side
            {
                api.Logger.Debug("Lasting Armor: Looking for original Vintage Story code...");
                System.Reflection.MethodInfo OriginalApplyConfigMethod = typeof(SurvivalCoreSystem).GetMethod("applyConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                api.Logger.Debug("Lasting Armor: Applying world config Harmony postfix...");
                HI.Patch(OriginalApplyConfigMethod, postfix: new HarmonyMethod(typeof(ApplyConfigPostfix).GetMethod("Postfix")));
            }
        }

        public override void Start(ICoreAPI api)
        {
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
}
