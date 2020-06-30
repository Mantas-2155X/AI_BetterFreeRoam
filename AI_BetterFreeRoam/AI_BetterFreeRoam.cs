using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Manager;
using AIProject;
using AIProject.Player;

using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AI_BetterFreeRoam
{
    [BepInPlugin(nameof(AI_BetterFreeRoam), nameof(AI_BetterFreeRoam), VERSION)][BepInProcess("AI-Syoujyo")]
    public class AI_BetterFreeRoam : BaseUnityPlugin
    {
        public const string VERSION = "1.0.0";
        
        private static PlayerActor player;
        
        private static Light sun;
        private static Terrain terrain;
        private static GameObject titleCam;
        private static List<GameObject> greens;
        
        private static bool inHousingIsland;
        
        private static ConfigEntry<bool> optimizeBaseMap { get; set; }
        private static ConfigEntry<bool> disableFoliage { get; set; }
        private static ConfigEntry<bool> disableGreens { get; set; }
        private static ConfigEntry<bool> disableSunShadows { get; set; }
        private static ConfigEntry<bool> disableTitleScene { get; set; }
        
        private static ConfigEntry<KeyboardShortcut> runKey { get; set; }
        private static ConfigEntry<float> runSpeed { get; set; }

        private void Awake()
        {
            (optimizeBaseMap = Config.Bind("Performance Improvements", "Optimize basemap", true, new ConfigDescription("Optimize basemap (only works in housing island)"))).SettingChanged += delegate
            {
                if (terrain == null || !inHousingIsland)
                    return;

                terrain.basemapDistance = optimizeBaseMap.Value ? 0 : 1000;
            };
            (disableFoliage = Config.Bind("Performance Improvements", "Disable foliage", false, new ConfigDescription("Disable foliage"))).SettingChanged += delegate
            {
                if (terrain == null)
                    return;

                terrain.drawTreesAndFoliage = !disableFoliage.Value;
            };
            (disableGreens = Config.Bind("Performance Improvements", "Disable greens", false, new ConfigDescription("Disable greens (trees, weeds, bushes)"))).SettingChanged += delegate
            {
                if (greens == null)
                    return;

                foreach (var t in greens.Where(g => g != null))
                    t.SetActive(!disableGreens.Value);
            };
            (disableSunShadows = Config.Bind("Performance Improvements", "Disable sun shadows", false, new ConfigDescription("Disable sun shadows"))).SettingChanged += delegate
            {
                if (sun == null || HSceneManager.isHScene)
                    return;

                sun.shadows = disableSunShadows.Value ? LightShadows.None : LightShadows.Soft;
            };
            (disableTitleScene = Config.Bind("Performance Improvements", "Disable title scene", false, new ConfigDescription("Disable title scene"))).SettingChanged += delegate
            {
                if (titleCam == null)
                    return;

                titleCam.SetActive(!disableTitleScene.Value);
            };

            Events.Awake(Config);
            
            runKey = Config.Bind("QoL - Running", "Run key", new KeyboardShortcut(KeyCode.LeftAlt), new ConfigDescription("Key to enable running"));
            runSpeed = Config.Bind("QoL - Running", "Run speed", 10f, new ConfigDescription("Speed at which player can run"));
            
            var harmony = new Harmony(nameof(AI_BetterFreeRoam));
            harmony.PatchAll(typeof(AI_BetterFreeRoam));
            harmony.PatchAll(typeof(Events));
        }

        private void Update()
        {
            if (player == null || !runKey.Value.IsPressed() || HSceneManager.isHScene || player.PlayerController.State is Follow)
                return;
            
            var vec = new Vector3(player.StateInfo.move.x, 0f, player.StateInfo.move.z);
            vec *= runSpeed.Value;

            player.Locomotor.Move(vec * Time.deltaTime);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Map), "InitSearchActorTargetsAll")]
        public static void Map_InitSearchActorTargetsAll_Patch(Map __instance)
        {
            sun = GameObject.Find("CommonSpace/MapRoot/MapSimulation(Clone)/EnviroSkyGroup(Clone)/Enviro Directional Light").GetComponent<Light>();
            if (sun == null)
                return;
            
            sun.shadows = disableSunShadows.Value ? LightShadows.None : LightShadows.Soft;
            
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

            terrain.drawTreesAndFoliage = !disableFoliage.Value;
            
            if(inHousingIsland)
                terrain.basemapDistance = optimizeBaseMap.Value ? 0 : 1000;
            
            var gameObjects = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>();
            if (gameObjects == null)
                return;
            
            greens = new List<GameObject>();
            foreach (var t in gameObjects.Where(g => g != null))
            {
                if (!t.name.Contains("fern") && !t.name.Contains("grass") && !t.name.Contains("tree")) 
                    continue;
                
                greens.Add(t);
                t.SetActive(!disableGreens.Value);
            }

            player = __instance.Player;
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(TitleScene), "Start")]
        private static void TitleScene_Start_Patch(ref object __result)
        {
            __result = new[] { __result, TitleScene_Start() }.GetEnumerator();
        }
        
        private static IEnumerator TitleScene_Start()
        {
            titleCam = GameObject.Find("TitleScene/MainCamera");
            titleCam.SetActive(!disableTitleScene.Value);
                
            yield break;
        }
    }
}