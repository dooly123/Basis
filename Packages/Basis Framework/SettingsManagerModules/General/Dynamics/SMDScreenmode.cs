using UnityEngine;
namespace BattlePhaze.SettingsManager.Intergrations
{
    public class SMDScreenmode : SettingsManagerOption
    {
        public SettingsManager Manager;
        public bool OnlySetScreenModeOnce = true;
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager = null)
        {
            if (Manager == null)
            {
                Manager = SettingsManager.Instance;
            }
            /*
            if (NameReturn(0, Option))
            {
                SettingsManagerDropDown.Clear(Manager, Option.OptionIndex);
                Option.SelectableValueList.Clear();
                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Fullscreen");
                SMSelectableValues.AddSelection( Option.SelectableValueList, "Fullscreen", "Fullscreen");

                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Maximized Window");
                SMSelectableValues.AddSelection( Option.SelectableValueList, "Maximized Window", "Maximized Window");

                SettingsManagerDropDown.AddDropDownOption(Manager, Option.OptionIndex, "Windowed");
                SMSelectableValues.AddSelection( Option.SelectableValueList, "Windowed", "Windowed");
                if (string.IsNullOrEmpty(Option.SelectedValue))
                {
                    SettingsManagerDropDown.SetOptionsValue(Manager, 0, 0, true);
                    Option.SelectedValue = Option.SelectableValueList[0].RealValue;
                    if (OnlySetScreenModeOnce)
                    {
                        Screen.fullScreenMode = 0;
                        OnlySetScreenModeOnce = false;
                    }
                }
                else
                {
                    for (int RealValuesIndex = 0; RealValuesIndex < Option.SelectableValueList.Count; RealValuesIndex++)
                    {
                        if (Option.SelectableValueList[RealValuesIndex].RealValue == Option.SelectedValue)
                        {
                            SettingsManagerDropDown.SetOptionsValue(Manager, Option.OptionIndex, RealValuesIndex, true);
                            if (OnlySetScreenModeOnce)
                            {
                                Screen.fullScreenMode = (FullScreenMode)RealValuesIndex;
                                OnlySetScreenModeOnce = false;
                            }
                            return;
                        }
                    }
                }
            }
            */
        }
    }
}