using UnityEngine;
using Duckov;
using Duckov.BlackMarkets;
using Duckov.Quests;
using Duckov.Quests.Tasks;
using Duckov.MasterKeys;
using HarmonyLib;
using System.Collections.Generic;
using Duckov.PerkTrees;
using System.Reflection;
using System.Linq;
using Saves;
using ItemStatsSystem;
using System;

namespace BetterBlackMarket 
{
    [HarmonyPatch(typeof(BlackMarket), "GenerateDemandsAndSupplies")]
    public class PatchBlackMarketGenerateDemandsAndSupplies
    {
        static void Postfix(BlackMarket __instance)
        {
            //获取任务中需要的物品ID
            List<Quest> list = QuestManager.Instance.ActiveQuests;
            List<int> questsItemTypeIDs = new List<int>();
			foreach (Quest quest in list)
			{
				if (quest.Tasks != null)
				{
					foreach (Task task in quest.Tasks)
					{
                        if (task is SubmitItems submitItems && !submitItems.IsFinished())
                        {
                            questsItemTypeIDs.Add(submitItems.ItemTypeID);
                        }
					}
				}
			}

            //获取升级需要的物品ID
            List<int> perkUpgradeItemTypeIDs = new List<int>();
            PerkTreeManager.Instance.perkTrees.ForEach(perktree =>
            {
                foreach (Perk perk in perktree.Perks)
                {
                    if (perk == null)
                    {
                        continue;
                    }

                    if (!perk.Unlocked && !perk.Unlocking && perk.AreAllParentsUnlocked())
                    {
                        FieldInfo fieldInfo = perk.GetType().GetField("requirement", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (fieldInfo != null)
                        {
                            object value = fieldInfo.GetValue(perk);
                            PerkRequirement? perkRequirement = value as PerkRequirement;

                            perkRequirement?.cost.items.ToList().ForEach(item =>
                            {
                                if ((long)ItemUtilities.GetItemCount(item.id) < item.amount)
                                {
                                    perkUpgradeItemTypeIDs.Add(item.id);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log($"[BetterBlackMarket] [错误] 在 perk 中未找到名为 requirement 的字段。");
                        }
                    }
                }
            });

            //获取未获得的钥匙
            List<int> unobtainedKeys = new List<int>();
            MasterKeysManager.AllPossibleKeys.ForEach(key =>
            {
                if(!MasterKeysManager.IsActive(key))
                {
                    unobtainedKeys.Add(key);
                }
            });



            List<int> allNeededItemIDs = questsItemTypeIDs.Concat(perkUpgradeItemTypeIDs).Concat(unobtainedKeys).Distinct().ToList();
            Debug.Log($"[BetterBlackMarket] 处理前所有需要的物品的名字: {string.Join(", ", allNeededItemIDs.Select(id => ItemAssetsCollection.GetMetaData(id).DisplayName))}");

            //删除武器蓝图：基础 专业 进阶 终极
            allNeededItemIDs = allNeededItemIDs.Where(id => id != 314 && id != 315 && id != 316 && id != 317).ToList();
            //删除冷核碎片和赤核碎片
            if(ModBehaviour.DeleteColdNuclearFragment)
                allNeededItemIDs = allNeededItemIDs.Where(id => id != 308 && id != 309).ToList();
            //删除蓝色方块
            if(ModBehaviour.DeleteBlueBlock)
                allNeededItemIDs = allNeededItemIDs.Where(id => id != 1165).ToList();
            //删除鱼类
            if(ModBehaviour.DeleteFish)
                allNeededItemIDs = allNeededItemIDs.Where(id => id < 1097 || id > 1126).ToList();

            Debug.Log($"[BetterBlackMarket] 处理后所有需要的物品的名字: {string.Join(", ", allNeededItemIDs.Select(id => ItemAssetsCollection.GetMetaData(id).DisplayName))}");

            if (allNeededItemIDs.Count == 0)
            {
                return;
            }

            if (UnityEngine.Random.Range(0f, 1f) < ModBehaviour.BlackMarketSpecialRefreshProbability)
            {
                int weightedRandom = GetWeightedRandom(30, 50, 20);
                int itemIDToSupply = 0;
                System.Random random = new System.Random();
                switch (weightedRandom)
                {
                    case 1:
                        var questItems = allNeededItemIDs.Intersect(questsItemTypeIDs).ToList();
                        if (questItems.Count > 0)
                            itemIDToSupply = questItems[random.Next(questItems.Count)];
                        else
                            itemIDToSupply = allNeededItemIDs[random.Next(allNeededItemIDs.Count)];
                        break;
                    case 2:
                        var perkItems = allNeededItemIDs.Intersect(perkUpgradeItemTypeIDs).ToList();
                        if (perkItems.Count > 0)
                            itemIDToSupply = perkItems[random.Next(perkItems.Count)];
                        else
                            itemIDToSupply = allNeededItemIDs[random.Next(allNeededItemIDs.Count)];
                        break;
                    case 3:
                        var keyItems = allNeededItemIDs.Intersect(unobtainedKeys).ToList();
                        if (keyItems.Count > 0)
                            itemIDToSupply = keyItems[random.Next(keyItems.Count)];
                        else
                            itemIDToSupply = allNeededItemIDs[random.Next(allNeededItemIDs.Count)];
                        break;
                }

                List<BlackMarket.DemandSupplyEntry> supplies = Traverse.Create(__instance).Field("supplies").GetValue<List<BlackMarket.DemandSupplyEntry>>();


                BlackMarket.DemandSupplyEntry itemEntry = new BlackMarket.DemandSupplyEntry();
                Traverse traverse = Traverse.Create(itemEntry);
                traverse.Field("itemID").SetValue(itemIDToSupply);
                traverse.Field("priceFactor").SetValue(1);

                if(unobtainedKeys.Contains(itemIDToSupply))
                {
                    traverse.Field("remaining").SetValue(1);
                    traverse.Field("batchCount").SetValue(1);      
                }
                else
                {
                    traverse.Field("remaining").SetValue(GetWeightedRandom(1, 5));
                    traverse.Field("batchCount").SetValue(GetBatchCount(itemIDToSupply));       
                }
                

                Debug.Log($"[BetterBlackMarket] 商品 ID: {itemIDToSupply}, BatchCount: {GetBatchCount(itemIDToSupply)}");

                int index = UnityEngine.Random.Range(0, supplies.Count);
                supplies[index] = itemEntry; 

                Action action = Traverse.Create(__instance).Field("onAfterGenerateEntries").GetValue<Action>();
                action?.Invoke();

                Traverse.Create(LevelManager.Instance).Method("SaveMainCharacter").GetValue();
                SavesSystem.CollectSaveData();
                SavesSystem.SaveFile(true);   

                Debug.Log($"[BetterBlackMarket] 已将物品ID {itemIDToSupply} 强制添加到黑市供应 {index} 位置中。");
            }
            else
            {
                Debug.Log($"[BetterBlackMarket] 未触发特殊刷新。");
            }
        }

        static int GetBatchCount(int itemID)
        {
            if(itemID == 308 || itemID == 309) return UnityEngine.Random.Range(1, 4) * 5;
            else if(itemID == 1165) return UnityEngine.Random.Range(2, 5) * 100;
            else return UnityEngine.Random.Range(1, 3);
        }

        static int GetWeightedRandom(int i, int j)
        {
            if (i > j)
            {
                (j, i) = (i, j);
            }
            
            float r = UnityEngine.Random.Range(0f, 1f);
            float skewedR = Mathf.Pow(r, 2); 
            
            int range = j - i + 1;
            int result = i + (int)(skewedR * range);
            
            return result;
        }

        static int GetWeightedRandom(int i, int j, int k)
        {
            if (i + j + k != 100)
            {
                UnityEngine.Debug.LogWarning($"[BetterBlackMarket] 概率值总和不为100%: i={i}, j={j}, k={k}, 总和={i + j + k}");
                float total = i + j + k;
                i = Mathf.RoundToInt((float)i / total * 100);
                j = Mathf.RoundToInt((float)j / total * 100);
                k = 100 - i - j; 
            }

            int randomValue = UnityEngine.Random.Range(1, 101);

            if (randomValue <= i)
            {
                return 1;
            }
            else if (randomValue <= i + j)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}

//QuestTask SubmitItems

//BlackMarket

//肉体强化 黑市升级 生存技能 工作台 技能强化