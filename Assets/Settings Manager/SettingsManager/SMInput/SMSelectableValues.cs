namespace BattlePhaze.SettingsManager
{
    using System.Collections.Generic;
    using UnityEngine;
    [System.Serializable]
    public class SMSelectableValues
    {
        [SerializeField]
        private string realValue;
        [SerializeField]
        public string UserValue;
        public string RealValue
        {
            get
            {
                return realValue.ToLower();
            }

            set
            {
                realValue = value.ToLower();
            }
        }
        public static List<string> GetRealValueArray(List<SMSelectableValues> SelectableValues)
        {
            List<string> ListOfRealValueValues = new List<string>();
            for (int SelectableValuesIndex = 0; SelectableValuesIndex < SelectableValues.Count; SelectableValuesIndex++)
            {
                ListOfRealValueValues.Add(SelectableValues[SelectableValuesIndex].RealValue);
            }
            return ListOfRealValueValues;
        }
        public static List<string> GetUserValueArray(List<SMSelectableValues> SelectableValues)
        {
            List<string> ListOfUserValues = new List<string>();
            for (int SelectableValuesIndex = 0; SelectableValuesIndex < SelectableValues.Count; SelectableValuesIndex++)
            {
                ListOfUserValues.Add(SelectableValues[SelectableValuesIndex].UserValue);
            }
            return ListOfUserValues;
        }
        public static void AddSelection(ref List<SMSelectableValues> SelectableValues, string RealValue, string UserValue)
        {
            SMSelectableValues Selectable = new SMSelectableValues();
            Selectable.RealValue = RealValue;
            Selectable.UserValue = UserValue;
            SelectableValues.Add(Selectable);

        }
    }
}