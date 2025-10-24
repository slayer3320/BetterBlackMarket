using UnityEngine;

namespace BetterBlackMarket
{
    public class ModUI : MonoBehaviour
    {
        private GUISkin? customSkin;
        private bool isSkinInitialized = false;
        private string focusedControl = "";
        private bool showMenu = false;
        private Rect windowRect = new Rect(20, 20, 400, 520);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                showMenu = !showMenu;
            }
            
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Semicolon))
            {
                showMenu = !showMenu;
            }
        }

        private void OnGUI()
        {
            if (!isSkinInitialized)
            {
                customSkin = Instantiate(GUI.skin);
                var fontSize = 18;
                customSkin.window.fontSize = 18;
                customSkin.label.fontSize = fontSize;
                customSkin.button.fontSize = fontSize;
                customSkin.toggle.fontSize = fontSize;
                customSkin.box.fontSize = fontSize;
                customSkin.textField.fontSize = fontSize;
                isSkinInitialized = true;
            }
 
 
            string currentFocus = GUI.GetNameOfFocusedControl();
            if (focusedControl != currentFocus)
            {
                if (focusedControl == "probTextField")
                {
                    ValidateAndApplyProb();
                }
                if (focusedControl == "multTextField")
                {
                    ValidateAndApplyMult();
                }
                focusedControl = currentFocus;
            }

            if (showMenu)
            {
                var originalSkin = GUI.skin;
                if (customSkin != null)
                {
                    GUI.skin = customSkin;
                }
                
                windowRect = GUILayout.Window(0, windowRect, DrawMenu, "BetterBlackMarket Settings");

                GUI.skin = originalSkin;
            }
        }

        void DrawMenu(int windowID)
        {
            bool isEnterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            // --- 刷新类型选择 ---
            GUILayout.Space(10);
            GUILayout.Label("刷新类型");
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(ModBehaviour.RefreshType == ModBehaviour.RefreshTypeEnum.Time, "按时间"))
            {
                if (ModBehaviour.RefreshType != ModBehaviour.RefreshTypeEnum.Time)
                {
                    ModBehaviour.RefreshType = ModBehaviour.RefreshTypeEnum.Time;
                    ModBehaviour.SaveSettings();
                }
            }
            if (GUILayout.Toggle(ModBehaviour.RefreshType == ModBehaviour.RefreshTypeEnum.Round, "按对局"))
            {
                if (ModBehaviour.RefreshType != ModBehaviour.RefreshTypeEnum.Round)
                {
                    ModBehaviour.RefreshType = ModBehaviour.RefreshTypeEnum.Round;
                    ModBehaviour.SaveSettings();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            // --- 刷新概率部分 ---
            GUILayout.Space(10);
            GUILayout.Label("刷新概率设置");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("特殊刷新概率", GUILayout.Width(200));
            GUI.SetNextControlName("probTextField");
            ModBehaviour.ProbString = GUILayout.TextField(ModBehaviour.ProbString, GUILayout.Width(80), GUILayout.Height(30));
            GUILayout.EndHorizontal();

            var newProbSlider = GUILayout.HorizontalSlider(ModBehaviour.BlackMarketSpecialRefreshProbability, 0f, 1f);
            if (newProbSlider != ModBehaviour.BlackMarketSpecialRefreshProbability)
            {
                ModBehaviour.BlackMarketSpecialRefreshProbability = newProbSlider;
                ModBehaviour.ProbString = ModBehaviour.BlackMarketSpecialRefreshProbability.ToString("F2");
                ModBehaviour.SaveSettings();
            }

            if (focusedControl == "probTextField" && isEnterPressed)
            {
                GUI.FocusControl(null);
            }
            
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            var newDeleteCold = GUILayout.Toggle(ModBehaviour.DeleteColdNuclearFragment, "过滤冷核/赤核碎片");
            if (newDeleteCold != ModBehaviour.DeleteColdNuclearFragment)
            {
                ModBehaviour.DeleteColdNuclearFragment = newDeleteCold;
                ModBehaviour.SaveSettings();
            }
            var newDeleteBlue = GUILayout.Toggle(ModBehaviour.DeleteBlueBlock, "过滤蓝色方块");
            if (newDeleteBlue != ModBehaviour.DeleteBlueBlock)
            {
                ModBehaviour.DeleteBlueBlock = newDeleteBlue;
                ModBehaviour.SaveSettings();
            }
            var newDeleteFish = GUILayout.Toggle(ModBehaviour.DeleteFish, "过滤鱼类");
            if (newDeleteFish != ModBehaviour.DeleteFish)
            {
                ModBehaviour.DeleteFish = newDeleteFish;
                ModBehaviour.SaveSettings();
            }
            GUILayout.EndVertical();

            GUILayout.Space(20);

            // --- 刷新时间部分 ---
            GUILayout.Label("刷新时间设置");
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label("刷新时间倍率", GUILayout.Width(200));
            GUI.SetNextControlName("multTextField");
            ModBehaviour.MultString = GUILayout.TextField(ModBehaviour.MultString, GUILayout.Width(80), GUILayout.Height(30));
            GUILayout.EndHorizontal();

            var newMultSlider = GUILayout.HorizontalSlider(ModBehaviour.BlackMarketRefreshTimeMultiplier, 0.001f, 1f);
            if (newMultSlider != ModBehaviour.BlackMarketRefreshTimeMultiplier)
            {
                ModBehaviour.BlackMarketRefreshTimeMultiplier = newMultSlider;
                ModBehaviour.MultString = ModBehaviour.BlackMarketRefreshTimeMultiplier.ToString("F3");
                ModBehaviour.SaveSettings();
            }

            if (focusedControl == "multTextField" && isEnterPressed)
            {
                GUI.FocusControl(null);
            }

            // 将关闭按钮推到底部
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("关闭"))
            {
                showMenu = false;
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void ValidateAndApplyProb()
        {
            if (float.TryParse(ModBehaviour.ProbString, out var parsedProb))
            {
                var clampedProb = Mathf.Clamp(parsedProb, 0f, 1f);
                ModBehaviour.ProbString = clampedProb.ToString("F2");
                if (Mathf.Abs(clampedProb - ModBehaviour.BlackMarketSpecialRefreshProbability) > float.Epsilon)
                {
                    ModBehaviour.BlackMarketSpecialRefreshProbability = clampedProb;
                    ModBehaviour.SaveSettings();
                }
            }
            else
            {
                ModBehaviour.ProbString = ModBehaviour.BlackMarketSpecialRefreshProbability.ToString("F2");
            }
        }

        private void ValidateAndApplyMult()
        {
            if (float.TryParse(ModBehaviour.MultString, out var parsedMult))
            {
                var clampedMult = Mathf.Clamp(parsedMult, 0.001f, 1f);
                ModBehaviour.MultString = clampedMult.ToString("F3");
                if (Mathf.Abs(clampedMult - ModBehaviour.BlackMarketRefreshTimeMultiplier) > float.Epsilon)
                {
                    ModBehaviour.BlackMarketRefreshTimeMultiplier = clampedMult;
                    ModBehaviour.SaveSettings();
                }
            }
            else
            {
                ModBehaviour.MultString = ModBehaviour.BlackMarketRefreshTimeMultiplier.ToString("F3");
            }
        }
    }
}
