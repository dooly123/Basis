using UnityEngine;
namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMDScreenmode : SettingsManagerOption
    {
        public SettingsManager Manager;
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager = null)
        {
            if (Manager == null)
            {
                Manager = SettingsManager.Instance;
            }
            if (NameReturn(0, Option))
            {
                SettingsManagerDropDown.Clear(Manager, Option.OptionIndex);
                Option.SelectableValueList.Clear();
                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Exclusive FullScreen");
                SMSelectableValues.AddSelection(ref Option.SelectableValueList, "Exclusive FullScreen", "Exclusive FullScreen");
                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Fullscreen");
                SMSelectableValues.AddSelection(ref Option.SelectableValueList, "Fullscreen", "Fullscreen");
                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Maximized Window");
                SMSelectableValues.AddSelection(ref Option.SelectableValueList, "Maximized Window", "Maximized Window");
                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Windowed");
                SMSelectableValues.AddSelection(ref Option.SelectableValueList, "Windowed", "Windowed");
                if (string.IsNullOrEmpty(Option.SelectedValue))
                {
                    SettingsManagerDropDown.SetOptionsValue(Manager, 0, 0, true);
                    Option.SelectedValue = Option.SelectableValueList[0].RealValue;
                    Screen.fullScreenMode = 0;
                }
                else
                {
                    for (int RealValuesIndex = 0; RealValuesIndex < Option.SelectableValueList.Count; RealValuesIndex++)
                    {
                        if (Option.SelectableValueList[RealValuesIndex].RealValue == Option.SelectedValue)
                        {
                            SettingsManagerDropDown.SetOptionsValue(Manager, Option.OptionIndex, RealValuesIndex, true);
                            Screen.fullScreenMode = (FullScreenMode)RealValuesIndex;
                            return;
                        }
                    }
                }
            }
        }
    }
}