using BepInEx;
using BepInEx.Harmony;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

namespace AI_BetterFreeRoam
{
    [BepInPlugin(nameof(AI_BetterFreeRoam), nameof(AI_BetterFreeRoam), VERSION)][BepInProcess("AI-Syoujyo")]
    public class AI_BetterFreeRoam : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";

        private static Terrain terrain;
        private static bool inHousingIsland;
        
        private static ConfigEntry<bool> optimizeBaseMap { get; set; }
        private static ConfigEntry<bool> disableFoliage { get; set; }
        private void Awake()
        {
            optimizeBaseMap = Config.Bind("Performance Improvements", "Optimize Basemap", true, new ConfigDescription("Optimize Basemap (only works in housing island)"));
            disableFoliage = Config.Bind("Performance Improvements", "Disable Foliage", false, new ConfigDescription("Disable Foliage"));

            optimizeBaseMap.SettingChanged += delegate
            {
                if (terrain == null || !inHousingIsland)
                    return;

                terrain.basemapDistance = optimizeBaseMap.Value ? 0 : 1000;
            };
            
            disableFoliage.SettingChanged += delegate
            {
                if (terrain == null)
                    return;

                terrain.drawTreesAndFoliage = !disableFoliage.Value;
            };
            
            HarmonyWrapper.PatchAll(typeof(AI_BetterFreeRoam));
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(Manager.Map), "InitSearchActorTargetsAll")]
        public static void Map_InitSearchActorTargetsAll_Patch()
        {
            var map = GameObject.Find("map00_Beach/map00_terrain_data/map00_terrain");
            if (map == null)
            {
                map = GameObject.Find("map_01_data/terrin_island_medium");
                if (map == null)
                    return;
                
                inHousingIsland = true;
            }
            
            terrain = map.GetComponent<Terrain>();
            if (terrain == null)
                return;
            
            if(inHousingIsland)
                terrain.basemapDistance = optimizeBaseMap.Value ? 0 : 1000;
            
            terrain.drawTreesAndFoliage = !disableFoliage.Value;
        }
    }
}