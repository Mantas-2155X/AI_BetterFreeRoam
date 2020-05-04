using BepInEx;
using BepInEx.Harmony;
using BepInEx.Configuration;

using HarmonyLib;

using Manager;

using UnityEngine;

namespace AI_BetterFreeRoam
{
    [BepInPlugin(nameof(AI_BetterFreeRoam), nameof(AI_BetterFreeRoam), VERSION)][BepInProcess("AI-Syoujyo")]
    public class AI_BetterFreeRoam : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";

        private static Terrain terrain;
        private static GameObject mapSimulation;
        private static bool inHousingIsland;
        
        private static ConfigEntry<bool> optimizeBaseMap { get; set; }
        private static ConfigEntry<bool> disableFoliage { get; set; }
        private static ConfigEntry<bool> disableMapSimulation { get; set; }
        
        private void Awake()
        {
            optimizeBaseMap = Config.Bind("Performance Improvements", "Optimize Basemap", true, new ConfigDescription("Optimize Basemap (only works in housing island)"));
            disableFoliage = Config.Bind("Performance Improvements", "Disable Foliage", false, new ConfigDescription("Disable Foliage"));
            disableMapSimulation = Config.Bind("Performance Improvements", "Disable Map simulation", false, new ConfigDescription("Disable Map simulation (may disable some effects)"));

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
            
            disableMapSimulation.SettingChanged += delegate
            {
                if (mapSimulation == null || HSceneManager.isHScene)
                    return;

                mapSimulation.SetActive(!disableMapSimulation.Value);
            };

            HarmonyWrapper.PatchAll(typeof(AI_BetterFreeRoam));
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(Map), "InitSearchActorTargetsAll")]
        public static void Map_InitSearchActorTargetsAll_Patch()
        {
            mapSimulation = GameObject.Find("CommonSpace/MapRoot/MapSimulation(Clone)/EnviroSkyGroup(Clone)");
            if (mapSimulation == null)
                return;
            
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
            
            mapSimulation.SetActive(!disableMapSimulation.Value);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(HSceneManager), "EndHScene")]
        public static void HSceneManager_EndHScene_Patch()
        {
            mapSimulation.SetActive(!disableMapSimulation.Value);
        }
    }
}