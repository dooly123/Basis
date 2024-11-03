using UnityEngine;

namespace BattlePhaze.SettingsManager.Types
{
    abstract public class SettingsManagerAbstractTypeSlider : SettingsManagerAbstractType
    {
        abstract public bool SliderOnValueChanged(SettingsManager Manager, int OptionIndex);
        abstract public bool SliderOptionSetValue(SettingsManager Manager, int OptionIndex, float Value, float Min, float Max);
        abstract public bool SliderOptionReadValue(SettingsManager Manager, int OptionIndex, out float Value);
        abstract public bool SliderOptionReadMinValue(SettingsManager Manager, int OptionIndex, out float Value);
        abstract public bool SliderOptionReadMaxValue(SettingsManager Manager, int OptionIndex, out float Value);
        abstract public bool SliderGetOptionsGameobject(SettingsManager Manager, int OptionIndex, out GameObject GameObject);
        abstract public bool SliderEnabledState(SettingsManager Manager, int OptionIndex, bool state);
    }
}