using Duckov.BlackMarkets;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;

namespace BetterBlackMarket
{
    /// <summary>
    /// 这个 Transpiler 补丁修改了 BlackMarket.TimeToRefresh 属性的 get 方法。
    /// 原始代码 (大致):
    ///   ...
    ///   float num = Mathf.Max(onRequestRefreshTimeFactorEventContext.Value, 0.01f);
    ///   return TimeSpan.FromTicks((long)((float)this.timeToRefresh * num));
    ///
    /// 补丁后代码 (大致):
    ///   ...
    ///   float num = onRequestRefreshTimeFactorEventContext.Value * ModBehaviour.BlackMarketRefreshTimeMultiplier;
    ///   return TimeSpan.FromTicks((long)((float)this.timeToRefresh * num));
    /// </summary>
    [HarmonyPatch(typeof(BlackMarket), "TimeToRefresh", MethodType.Getter)]
    public class PatchBlackMarketTimeToRefresh
    {
        // 目标方法：我们要寻找的 call 指令是 Mathf.Max(float, float)
        private static readonly MethodInfo TargetMethod = AccessTools.Method
        (
            typeof(Mathf), 
            nameof(Mathf.Max), 
            new[] { typeof(float), typeof(float) }
        );

        // 目标字段：我们要加载的静态乘数字段
        private static readonly FieldInfo MultiplierField = AccessTools.Field(
            typeof(ModBehaviour), 
            nameof(ModBehaviour.BlackMarketRefreshTimeMultiplier)
        );

        // 我们要查找的常量值
        private const float ValueToReplace = 0.01f;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);
            bool patched = false;

            if (TargetMethod == null)
            {
                Debug.LogError("[BetterBlackMarket] [Transpiler] 无法找到目标方法 Mathf.Max。");
                return instructions;
            }

            if (MultiplierField == null)
            {
                Debug.LogError("[BetterBlackMarket] [Transpiler] 无法找到字段 ModBehaviour.BlackMarketRefreshTimeMultiplier。");
                return instructions;
            }

            // 使用 for 循环遍历指令
            for (int i = 0; i < code.Count; i++)
            {
                // 1. 寻找对 Mathf.Max(float, float) 的调用
                if (code[i].Calls(TargetMethod))
                {
                    // 2. 找到了 call 指令。现在检查 *前一条* 指令 (i-1)
                    //    它应该是加载常数 0.01f (ldc.r4 0.01)
                    if (i > 0 && code[i - 1].opcode == OpCodes.Ldc_R4 && (float)code[i - 1].operand == ValueToReplace)
                    {
                        // 3. 匹配成功！
                        
                        // a. 修改 'ldc.r4 0.01' (i-1) 为 'ldsfld ModBehaviour.BlackMarketRefreshTimeMultiplier'
                        //    (Ldsfld = Load static field)
                        code[i - 1].opcode = OpCodes.Ldsfld;
                        code[i - 1].operand = MultiplierField;

                        // b. 修改 'call Mathf.Max' (i) 为 'mul' (乘法)
                        code[i].opcode = OpCodes.Mul;
                        code[i].operand = null; // Mul 指令不需要操作数

                        // 4. 标记为已修补并退出循环
                        patched = true;
                        Debug.Log("[BetterBlackMarket] [Transpiler] 成功修补");
                        break; 
                    }
                }
            }

            if (!patched)
            {
                Debug.LogError("[BetterBlackMarket] [Transpiler] 未能成功应用补丁到 BlackMarket.TimeToRefresh.Get()。未找到 'Mathf.Max(..., 0.01f)' 模式。");
            }

            // 返回修改后的指令列表
            return code.AsEnumerable();
        }
    }
}