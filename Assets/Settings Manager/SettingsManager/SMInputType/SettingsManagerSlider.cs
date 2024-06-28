using BattlePhaze.SettingsManager.DebugSystem;
using UnityEngine;
namespace BattlePhaze.SettingsManager
{
    public static class SettingsManagerSlider
    {
        public static void SliderExecution(int OptionIndex, SettingsManager Manager, float CurrentValue, bool Save)
        {
            if (Manager.Options[OptionIndex].Type == SettingsManagerEnums.IsType.Slider)
            {
                if (Manager.Options[OptionIndex].ParseController == SettingsManagerEnums.ItemParse.intValue)
                {
                    CurrentValue = (int)CurrentValue;
                }
                Manager.Options[OptionIndex].SelectedValue = CurrentValue.ToString(Manager.ManagerSettings.CInfo);
                if (float.TryParse(Manager.Options[OptionIndex].SliderMaxValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float SliderMaxValue))
                {
                    if (Manager.Options[OptionIndex].ReturnedValueTextType == SettingsManagerEnums.TextReturn.SliderPercentage)
                    {
                        SetTextDescription(Manager, OptionIndex, CurrentValue, SliderMaxValue, Manager.Options[OptionIndex].Round, true, Manager.Options[OptionIndex].RoundTo, Manager.Options[OptionIndex].MaxPercentage, Manager.Options[OptionIndex].MinPercentage);
                    }
                    else
                    {
                        SetTextDescription(Manager, OptionIndex, CurrentValue, SliderMaxValue, Manager.Options[OptionIndex].Round, false, Manager.Options[OptionIndex].RoundTo, Manager.Options[OptionIndex].MaxPercentage, Manager.Options[OptionIndex].MinPercentage);
                    }
                    if (Save)
                    {
                        SettingsManagerStorageManagement.Save(Manager);
                    }
                    Manager.SendOption(Manager.Options[OptionIndex]);
                }
                else
                {
                    SettingsManagerDebug.LogError("Could not parse Slider Max Value SliderExecution Failed!");
                }
            }
        }
        public static void SetSliderDescription(int OptionIndex, SettingsManager Manager, float CurrentValue)
        {
            if (Manager.Options[OptionIndex].Type == SettingsManagerEnums.IsType.Slider)
            {
                if (Manager.Options[OptionIndex].ParseController == SettingsManagerEnums.ItemParse.intValue)
                {
                    CurrentValue = (int)CurrentValue;
                }
                if (float.TryParse(Manager.Options[OptionIndex].SliderMaxValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float SliderMaxValue))
                {
                    if (Manager.Options[OptionIndex].ReturnedValueTextType == SettingsManagerEnums.TextReturn.SliderPercentage)
                    {
                        SetTextDescription(Manager, OptionIndex, CurrentValue, SliderMaxValue, Manager.Options[OptionIndex].Round, true, Manager.Options[OptionIndex].RoundTo, Manager.Options[OptionIndex].MaxPercentage, Manager.Options[OptionIndex].MinPercentage);
                    }
                    else
                    {
                        SetTextDescription(Manager, OptionIndex, CurrentValue, SliderMaxValue, Manager.Options[OptionIndex].Round, false, Manager.Options[OptionIndex].RoundTo, Manager.Options[OptionIndex].MaxPercentage, Manager.Options[OptionIndex].MinPercentage);
                    }
                }
                else
                {
                    SettingsManagerDebug.LogError("Could not parse Slider Max Value SliderExecution Failed!");
                }
            }
        }
        public static void SetTextDescription(SettingsManager Manager, int OptionIndex, float SliderValue, float SliderMaxValue, bool Round, bool Percentage, int RoundTo, float MaxPercentage, float MinPercentageOffset)
        {
            if (Manager.Options[OptionIndex].TextDescription != null)
            {
                if (Manager.Options[OptionIndex].ReturnedValueTextType == SettingsManagerEnums.TextReturn.SliderPercentage)
                {
                    Manager.Options[OptionIndex].ValueDescriptor = Information.SettingsManagerInformationConverter.PostProcessValue(Round, Percentage, RoundTo, SliderValue, SliderMaxValue, MaxPercentage, MinPercentageOffset);
                }
                else
                {
                    Manager.Options[OptionIndex].ValueDescriptor = SliderValue.ToString(Manager.ManagerSettings.CInfo);
                }
                SettingsManagerDescriptionSystem.TxtDescriptionSetText(Manager, OptionIndex);
            }
        }
        public static void SliderOptionReadValue(SettingsManager Manager, int OptionIndex, out bool HasValue, out float Value)
        {
            HasValue = false;
            Value = 0;
            for (int textsIndex = 0; textsIndex < SettingsManager.Instance.SettingsManagerAbstractTypeSlider.Count; textsIndex++)
            {
                if (Manager.SettingsManagerAbstractTypeSlider[textsIndex] != null)
                {
                    Manager.SettingsManagerAbstractTypeSlider[textsIndex].SliderOptionReadValue(Manager, OptionIndex, out HasValue, out Value);
                    if (HasValue)
                    {
                        return;
                    }
                }
            }
        }
        public static void SliderOptionReadValue(SettingsManager Manager, SettingsMenuInput Option, out bool HasValue, out float Value)
        {
            HasValue = false;
            Value = 0;
            for (int OptionsIndex = 0; OptionsIndex < Manager.Options.Count; OptionsIndex++)
            {
                if (Manager.Options[OptionsIndex] == Option)
                {
                    for (int textsIndex = 0; textsIndex < SettingsManager.Instance.SettingsManagerAbstractTypeSlider.Count; textsIndex++)
                    {
                        if (Manager.SettingsManagerAbstractTypeSlider[textsIndex] != null)
                        {
                            Manager.SettingsManagerAbstractTypeSlider[textsIndex].SliderOptionReadValue(Manager, OptionsIndex, out HasValue, out Value);
                            if (HasValue)
                            {
                                return;
                            }
                        }
                    }
                    return;
                }
            }
        }
        public static void SliderEnabledState(SettingsManager Manager, int OptionIndex, bool outcome)
        {
            for (int textsIndex = 0; textsIndex < SettingsManager.Instance.SettingsManagerAbstractTypeSlider.Count; textsIndex++)
            {
                if (Manager.SettingsManagerAbstractTypeSlider[textsIndex] != null)
                {
                    Manager.SettingsManagerAbstractTypeSlider[textsIndex].SliderEnabledState(Manager, OptionIndex, outcome);
                }
            }
        }
        public static void SliderGetOptionsGameobject(SettingsManager Manager, int OptionIndex, out bool HasValue, out GameObject Value)
        {
            HasValue = false;
            Value = null;
            for (int textsIndex = 0; textsIndex < SettingsManager.Instance.SettingsManagerAbstractTypeSlider.Count; textsIndex++)
            {
                if (Manager.SettingsManagerAbstractTypeSlider[textsIndex] != null)
                {
                    Manager.SettingsManagerAbstractTypeSlider[textsIndex].SliderGetOptionsGameobject(Manager, OptionIndex, out HasValue, out Value);
                    if (HasValue)
                    {
                        return;
                    }
                }
            }
        }
        public static void InitalizeSlider(SettingsManager Manager, int OptionIndex)
        {
            SettingsMenuInput Option = Manager.Options[OptionIndex];
            float.TryParse(Option.SelectedValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float SliderValue);
            float.TryParse(Option.SliderMinValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float SliderMinValue);
            float.TryParse(Option.SliderMaxValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float SliderMaxValue);
            SetTextDescription(Manager, Option.OptionIndex, SliderValue, SliderMaxValue, Option.Round, Option.ReturnedValueTextType == SettingsManagerEnums.TextReturn.SliderPercentage, Option.RoundTo, Option.MaxPercentage, Option.MinPercentage);
            for (int AbstractModulesListIndex = 0; AbstractModulesListIndex < Manager.SettingsManagerAbstractTypeSlider.Count; AbstractModulesListIndex++)
            {
                if (Manager.SettingsManagerAbstractTypeSlider[AbstractModulesListIndex] != null)
                {
                    Manager.SettingsManagerAbstractTypeSlider[AbstractModulesListIndex].SliderOptionSetValue(Manager, OptionIndex, SliderValue, SliderMinValue, SliderMaxValue);
                    Manager.SettingsManagerAbstractTypeSlider[AbstractModulesListIndex].SliderOnValueChanged(Manager, OptionIndex, out bool RequestSuccessful);
                    if (RequestSuccessful)
                    {
                        return;
                    }
                }
            }
        }
    }
}