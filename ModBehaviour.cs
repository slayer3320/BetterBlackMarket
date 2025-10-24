using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using HarmonyLib;
using ItemStatsSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BetterBlackMarket
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private const string Id = "Slayer3320.BetterBlackMarket";
        private Harmony? harmony;

        public enum RefreshTypeEnum
        {
            //按时间
            Time,
            //按对局
            Round,
        }

        public static RefreshTypeEnum RefreshType = RefreshTypeEnum.Time;


        //黑市特殊刷新的概率
        public static float BlackMarketSpecialRefreshProbability = 0.75f;
        //是否删除冷核碎片和赤核碎片
        public static bool DeleteColdNuclearFragment = true;
        //是否删除蓝色方块
        public static bool DeleteBlueBlock = true;
        //是否删除鱼类
        public static bool DeleteFish = true;
        //黑市刷新时间的乘数
        public static float BlackMarketRefreshTimeMultiplier = 1f;
        
        public static string ProbString = "0.75";
        public static string MultString = "1.000";

        private void OnEnable()
        {
            UnityEngine.Debug.Log("[BetterBlackMarket] OnEnable");

            gameObject.AddComponent<ModUI>();
            LoadSettings();

            harmony = new Harmony(Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnDisable()
        {
            harmony?.UnpatchAll(Id);
        }

        public static void SaveSettings()
        {
            PlayerPrefs.SetInt("BBM_RefreshType", (int)RefreshType);
            PlayerPrefs.SetFloat("BBM_Prob", BlackMarketSpecialRefreshProbability);
            PlayerPrefs.SetInt("BBM_DelCold", DeleteColdNuclearFragment ? 1 : 0);
            PlayerPrefs.SetInt("BBM_DelBlue", DeleteBlueBlock ? 1 : 0);
            PlayerPrefs.SetInt("BBM_DelFish", DeleteFish ? 1 : 0);
            PlayerPrefs.SetFloat("BBM_Multiplier", BlackMarketRefreshTimeMultiplier);
            PlayerPrefs.SetString("BBM_ProbString", ProbString);
            PlayerPrefs.SetString("BBM_MultString", MultString);
            PlayerPrefs.Save();
            UnityEngine.Debug.Log("[BetterBlackMarket] Settings saved to PlayerPrefs.");
        }

        public static void LoadSettings()
        {
            RefreshType = (RefreshTypeEnum)PlayerPrefs.GetInt("BBM_RefreshType", (int)RefreshTypeEnum.Time);
            BlackMarketSpecialRefreshProbability = PlayerPrefs.GetFloat("BBM_Prob", 0.75f);
            DeleteColdNuclearFragment = PlayerPrefs.GetInt("BBM_DelCold", 1) == 1;
            DeleteBlueBlock = PlayerPrefs.GetInt("BBM_DelBlue", 1) == 1;
            DeleteFish = PlayerPrefs.GetInt("BBM_DelFish", 1) == 1;
            BlackMarketRefreshTimeMultiplier = PlayerPrefs.GetFloat("BBM_Multiplier", 1f);
            
            ProbString = PlayerPrefs.GetString("BBM_ProbString", "0.75");
            MultString = PlayerPrefs.GetString("BBM_MultString", "1.000");

            UnityEngine.Debug.Log("[BetterBlackMarket] Settings loaded from PlayerPrefs.");
        }
    }
}

