#if UNITY_EDITOR
using BattlePhaze.SettingsManager.DebugSystem;
using BattlePhaze.SettingsManager.Style;
using UnityEditor;
using UnityEngine;
using static BattlePhaze.SettingsManager.SettingsManagerOption;
#endif
namespace BattlePhaze.SettingsManager
{
    [CustomEditor(typeof(SettingsManagerOption), true)]
    public class SettingsManagerOptionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SettingsmanagerStyle.Style();
            SettingsManagerOption Target = (SettingsManagerOption)target;
            DisplaySelection(Target);
        }
        public static SettingsManager Instance;
        public static void DisplaySelection(SettingsManagerOption ModuleOption)
        {
            EditorGUILayout.BeginVertical(SettingsmanagerStyle.BackGroundStyling);
            Instance = SettingsManager.Instance;
            if (Instance == null)
            {
                Instance = MonoBehaviour.FindObjectOfType<SettingsManager>();
            }
#if UNITY_EDITOR
            if (Instance == null)
            {
                GUILayout.Label("Settings Manager could not be found please assign one (please make sure your setting settings manager up in a scene)");
                Instance = (SettingsManager)EditorGUILayout.ObjectField(Instance, typeof(SettingsManager), true);
            }
            if (Instance == null)
            {
                return;
            }
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Add Option", SettingsmanagerStyle.ButtonStyling))
            {
                ModuleOption.ManagerModuleOptions.Add(new SettingsManagerOptionValues());
            }
            if (Instance.Options.Count != 0 && ModuleOption != null && ModuleOption.ManagerModuleOptions.Count != 0)
            {
                for (int OptionValuesIndex = 0; OptionValuesIndex < ModuleOption.ManagerModuleOptions.Count; OptionValuesIndex++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Option Index " + FriendlyInteger(OptionValuesIndex + 1), SettingsmanagerStyle.DescriptorStyling);
                    int Value = GetIndex(ModuleOption.ManagerModuleOptions[OptionValuesIndex].OptionName);
                    SettingsManagerOptionValues Output = ModuleOption.ManagerModuleOptions[OptionValuesIndex];
                    int Current = EditorGUILayout.Popup(Value, GetArrayOfOptions());
                    SettingsMenuInput Input = Instance.Options[Current];
                    if (Input != null)
                    {
                        Output.Parse = Input.ParseController;
                        Output.Type = Input.Type;
                        Output.OptionName = Input.Name;
                        Output.Explanation = Input.Explanation;
                        if (GUILayout.Button("Remove Option",SettingsmanagerStyle.ButtonStyling))
                        {
                            ModuleOption.ManagerModuleOptions.RemoveAt(OptionValuesIndex);
                            SettingsManagerDebug.Log("Removing Option " + OptionValuesIndex);
                        }
                        GUILayout.EndHorizontal();
                        ModuleOption.ManagerModuleOptions[OptionValuesIndex] = Output;
                    }
                }
            }
            EditorGUILayout.EndVertical();
#endif
        }
        public static void DisplaySelection(ref string OptionName, ref string OptionExplanation, ref SettingsManagerEnums.ItemParse ItemParse, ref SettingsManagerEnums.IsType IsType, string Name)
        {
            if (Instance == null)
            {
                Instance = MonoBehaviour.FindObjectOfType<SettingsManager>();
            }
            if (Instance == null)
            {
                return;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Option " + Name);
#if UNITY_EDITOR
            if (Instance.Options.Count != 0)
            {
                int Value = GetIndex(OptionName);
                SettingsMenuInput Input = Instance.Options[EditorGUILayout.Popup(Value, GetArrayOfOptions())];
                ItemParse = Input.ParseController;
                IsType = Input.Type;
                OptionName = Input.Name;
                OptionExplanation = Input.Explanation;
            }
#endif
            GUILayout.EndHorizontal();
        }
        public static int GetIndex(string OptionName)
        {
            for (int OptionsIndex = 0; OptionsIndex < Instance.Options.Count; OptionsIndex++)
            {
                if (OptionName == Instance.Options[OptionsIndex].Name)
                {
                    return OptionsIndex;
                }
            }
            return 0;
        }
        public static string[] GetArrayOfOptions()
        {
            string[] OptionsArray = new string[Instance.Options.Count];
            for (int OptionsIndex = 0; OptionsIndex < Instance.Options.Count; OptionsIndex++)
            {
                OptionsArray[OptionsIndex] = Instance.Options[OptionsIndex].Name;
            }
            return OptionsArray;
        }
        static string[] ones = new string[] { string.Empty, "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
        static string[] teens = new string[] { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        static string[] tens = new string[] { "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        private static string FriendlyInteger(int n)
        {
            if (n == 0)
            {
                return "Zero";
            }

            if (n < 0)
            {
                return "Minus " + FriendlyInteger(-n);
            }

            string friendlyInt;
            if (n < 10)
            {
                friendlyInt = ones[n];
            }
            else if (n < 20)
            {
                friendlyInt = teens[n - 10];
            }
            else if (n < 100)
            {
                friendlyInt = tens[n / 10 - 2] + " " + FriendlyInteger(n % 10);
            }
            else if (n < 1000)
            {
                friendlyInt = ones[n / 100] + " Hundred " + FriendlyInteger(n % 100);
            }
            else if (n < 1000000)
            {
                friendlyInt = FriendlyInteger(n / 1000) + " Thousand " + FriendlyInteger(n % 1000);
            }
            else if (n < 1000000000)
            {
                friendlyInt = FriendlyInteger(n / 1000000) + " Million " + FriendlyInteger(n % 1000000);
            }
            else
            {
                friendlyInt = FriendlyInteger(n / 1000000000) + " Billion " + FriendlyInteger(n % 1000000000);
            }

            return friendlyInt.Trim();
        }
    }
}