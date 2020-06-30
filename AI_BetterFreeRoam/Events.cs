using BepInEx.Configuration;

using HarmonyLib;

using AIProject;
using BehaviorDesigner.Runtime.Tasks;

namespace AI_BetterFreeRoam
{
    public static class Events
    {
        private static ConfigEntry<bool> disableRandomClothChange { get; set; }
        private static ConfigEntry<bool> disableRandomRevRape { get; set; }
        private static ConfigEntry<bool> disableRandomInvite { get; set; }
        private static ConfigEntry<bool> disableRandomGift { get; set; }
        private static ConfigEntry<bool> disableRandomLes { get; set; }
        private static ConfigEntry<bool> disableRandomMas { get; set; }
        
        public static void Awake(ConfigFile Config)
        {
            disableRandomClothChange = Config.Bind("QoL - Events", "Disable random clothchange", false, new ConfigDescription("Disable girls random changing clothes (eliminates lag spikes)"));
            disableRandomRevRape = Config.Bind("QoL - Events", "Disable random reverse rape", false, new ConfigDescription("Disable girls random reverse rape"));
            disableRandomInvite = Config.Bind("QoL - Events", "Disable random invites", false, new ConfigDescription("Disable girls random inviting"));
            disableRandomGift = Config.Bind("QoL - Events", "Disable random gifts", false, new ConfigDescription("Disable girls random giving gifts"));
            disableRandomLes = Config.Bind("QoL - Events", "Disable random lesbian", false, new ConfigDescription("Disable girls random lesbian (eliminates lag spikes)"));
            disableRandomMas = Config.Bind("QoL - Events", "Disable random masturbation", false, new ConfigDescription("Disable girls random masturbation (eliminates lag spikes)"));
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CanClothChange), "OnUpdate")]
        private static bool CanClothChange_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomClothChange.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CanRevRape), "OnUpdate")]
        private static bool CanRevRape_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomRevRape.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CanInvitation), "OnUpdate")]
        private static bool CanInvitation_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomInvite.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CanGift), "OnUpdate")]
        private static bool CanGift_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomGift.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CanLesbian), "OnUpdate")]
        private static bool CanLesbian_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomLes.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch(typeof(CanMasturbation), "OnUpdate")]
        private static bool CanMasturbation_OnUpdate_Patch(ref TaskStatus __result)
        {
            if (!disableRandomMas.Value)
                return true;
            
            __result = TaskStatus.Failure;
            
            return false;
        }
    }
}