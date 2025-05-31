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
                api.Logger.Debug("Lasting Armor: Getting original method...");
                System.Reflection.MethodInfo OriginalApplyConfigMethod = typeof(SurvivalCoreSystem).GetMethod("applyConfig", BindingFlags.Instance | BindingFlags.NonPublic);

                api.Logger.Debug("Lasting Armor: Applying world config transpiler...");
                //HI.Patch(OriginalApplyConfigMethod, transpiler: new HarmonyMethod(typeof(ApplyConfigTranspiler).GetMethod("Transpiler")));
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
            System.Diagnostics.Debug.Print("Lasting Armor: Postfix");
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
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(SurvivalCoreSystem), "applyConfig")]
    public class ApplyConfigTranspiler
    {
        public static IEnumerable<CodeInstruction> TranspilerDebug(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                System.Diagnostics.Debug.Print("Lasting Armor: Looking IN: {0}", code);
                yield return code;
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            //for
                
            return codes; // No need to patch if the expected instruction is not present
        }

        public static IEnumerable<CodeInstruction> TranspilerOld(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            //var objLocalIndex = -1;

            // Find the local variable index for 'obj'
            /*for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stloc_S && codes[i].operand is LocalBuilder lb)
                {
                    objLocalIndex = lb.LocalIndex;
                    break;
                }
            }
            if (objLocalIndex == -1) objLocalIndex = 4; // fallback*/

            //var toolField = AccessTools.Field(typeof(CollectibleObject), "Tool");
            //var durabilityField = AccessTools.Field(typeof(CollectibleObject), "Durability");
            //var itemWearableType = typeof(Vintagestory.GameContent.ItemWearable);

            //Label? labelFalse = null;
            //bool pendingLabelFalse = false;

            for (int i = 0; i < codes.Count; i++)
            {
                System.Diagnostics.Debug.Print("Lasting Armor: Looking IN: opcode[{1}] {0}", codes[i],i);
                // Look for: ldloc.s, ldfld Tool, call get_HasValue, brfalse.s
                if (
                    i + 3 < codes.Count &&
                    codes[i].opcode == OpCodes.Ldloc_1 && // or OpCodes.Ldloc_S, depending on local index
                    codes[i + 1].opcode == OpCodes.Ldflda &&
                    ((FieldInfo)codes[i + 1].operand).Name == "Tool" &&
                    codes[i + 2].opcode == OpCodes.Call &&
                    ((MethodInfo)codes[i + 2].operand).Name == "get_HasValue" &&
                    codes[i + 3].opcode == OpCodes.Brfalse_S
                )
                {
                    System.Diagnostics.Debug.Print("Lasting Armor: FOUND: opcode[{1}] {0}", codes[i],i);
                    // Save the label for the false branch
                    //labelFalse = (Label)codes[i + 3].operand;
                    //var labelTrue = new Label();
                    //System.Diagnostics.Debug.Print("Lasting Armor: labelFalse: {0}", labelFalse);
                    //System.Diagnostics.Debug.Print("Lasting Armor: labelTrue: {0}", labelTrue);
                    // if (collectible.Tool.HasValue) goto labelTrue;
                    //yield return codes[i]; // ldloc.1
                    //yield return codes[i + 1]; // ldflda Tool
                    //yield return codes[i + 2]; // call get_HasValue
                    //yield return new CodeInstruction(OpCodes.Brtrue_S, labelTrue);

                    // if (!(collectible is ItemWearable)) goto labelFalse;
                    //yield return new CodeInstruction(OpCodes.Ldloc_1)/*(OpCodes.Ldloc_S, ((LocalBuilder)codes[i].operand).LocalIndex)*/;
                    //yield return new CodeInstruction(OpCodes.Isinst, typeof(Vintagestory.GameContent.ItemWearable));
                    //yield return new CodeInstruction(OpCodes.Brfalse_S, labelFalse);

                    // if (collectible.GetMaxDurability() >= 1) goto labelTrue;
                    //yield return new CodeInstruction(OpCodes.Ldloc_1)/*(OpCodes.Ldloc_S, ((LocalBuilder)codes[i].operand).LocalIndex)*/;
                    //yield return new CodeInstruction(OpCodes.Callvirt, typeof(CollectibleObject).GetMethod("GetMaxDurability"));
                    //yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    //yield return new CodeInstruction(OpCodes.Bge_S, labelTrue);

                    // labelTrue:
                    //codes[i + 4].labels.Add(labelTrue);

                    //i += 3; // Skip the original 4 instructions
                    //continue;
                }

                // Attach labelFalse to the first non-label, non-nop instruction after the injected block
                /*if (pendingLabelFalse && labelFalse.HasValue)
                {
                    // Only attach to a real instruction (not a label or Nop)
                    if (codes[i].opcode != OpCodes.Nop)
                    {
                        codes[i].labels.Add(labelFalse.Value);
                        pendingLabelFalse = false;
                        labelFalse = null;
                    }
                }*/

                yield return codes[i];
            }

            /*for (int i = 0; i < codes.Count; i++)
            {
                // Look for the original: if (obj.Tool != null)
                if (
                    codes[i].opcode == OpCodes.Ldloc_S &&
                    codes[i].operand is LocalBuilder lb2 &&
                    lb2.LocalIndex == objLocalIndex &&
                    i + 1 < codes.Count &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    ((FieldInfo)codes[i + 1].operand).Name == "Tool" &&
                    i + 2 < codes.Count &&
                    codes[i + 2].opcode == OpCodes.Ldnull
                )
                {
                    // Remove the next 3 instructions (ldloc.s, ldfld Tool, ldnull)
                    i += 2;

                    // Insert our compound condition:
                    // if (obj.Tool != null || (obj is ItemWearable && obj.Durability > 0))
                    var labelTrue = codes[i + 1].labels.Count > 0 ? codes[i + 1].labels[0] : new Label();
                    labelFalse = new Label();
                    pendingLabelFalse = true;

                    // if (obj.Tool != null) goto labelTrue;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objLocalIndex);
                    yield return new CodeInstruction(OpCodes.Ldfld, toolField);
                    yield return new CodeInstruction(OpCodes.Brtrue_S, labelTrue);

                    // if (!(obj is ItemWearable)) goto labelFalse;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objLocalIndex);
                    yield return new CodeInstruction(OpCodes.Isinst, itemWearableType);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, labelFalse);

                    // if (obj.Durability >= 1) goto labelTrue;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, objLocalIndex);
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(CollectibleObject).GetMethod("GetMaxDurability"));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Blt_S, labelTrue);


                    // labelTrue:
                    codes[i + 1].labels.Add(labelTrue);

                    continue;
                }

                // Attach labelFalse to the first non-label, non-nop instruction after the injected block
                if (pendingLabelFalse && labelFalse.HasValue)
                {
                    // Only attach to a real instruction (not a label or Nop)
                    if (codes[i].opcode != OpCodes.Nop)
                    {
                        codes[i].labels.Add(labelFalse.Value);
                        pendingLabelFalse = false;
                        labelFalse = null;
                    }
                }

                yield return codes[i];
            }*/
        }
    }


}
