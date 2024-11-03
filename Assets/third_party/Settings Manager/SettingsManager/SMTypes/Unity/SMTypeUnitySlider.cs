using BattlePhaze.SettingsManager.Types;
using UnityEngine;
namespace BattlePhaze.SettingsManager
{
    public class SMTypeUnitySlider : SettingsManagerAbstractTypeSlider
    {
        public void SetResetAction(SettingsManager Manager, int OptionIndex)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ResetToDefault, typeof(UnityEngine.UI.Button)))
            {
                UnityEngine.UI.Button Button = (UnityEngine.UI.Button)Manager.Options[OptionIndex].ResetToDefault;
                if (Manager.Options[OptionIndex].ResetAction != null)
                {
                    Button.onClick.RemoveListener(Manager.Options[OptionIndex].ResetAction);
                }
                Manager.Options[OptionIndex].ResetAction = delegate { SettingsManagerStorageManagement.SetDefault(Manager, OptionIndex, true); };
                Button.onClick.AddListener(Manager.Options[OptionIndex].ResetAction);
            }
        }
        public override bool SliderOptionSetValue(SettingsManager Manager, int OptionIndex, float Value, float Min, float Max)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                Slider.minValue = Min;
                Slider.maxValue = Max;
                Slider.value = Value;
                return true;
            }
            return false;
        }
        public override bool SliderOptionReadValue(SettingsManager Manager, int OptionIndex, out float Value)
        {
            Value = 0;
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                Value = Slider.value;
                return true;
            }
            return false;
        }
        public override bool SliderOptionReadMaxValue(SettingsManager Manager, int OptionIndex, out float Value)
        {
            Value = 0;
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                Value = Slider.maxValue;
                return true;
            }
            return false;
        }
        public override bool SliderOptionReadMinValue(SettingsManager Manager, int OptionIndex, out float Value)
        {
            Value = 0;
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                Value = Slider.minValue;
                return true;
            }
            return false; ;
        }
        public override bool SliderGetOptionsGameobject(SettingsManager Manager, int OptionIndex, out GameObject GameObject)
        {
            GameObject = null;
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                GameObject = Slider.gameObject;
                return true;
            }
            return false;
        }
        public override bool SliderEnabledState(SettingsManager Manager, int OptionIndex, bool outcome)
        {
            if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                Slider.gameObject.SetActive(outcome);
                return true;
            }
            return false;
        }
        public override bool SliderOnValueChanged(SettingsManager Manager, int OptionIndex)
        {
            if (SettingsManager.TypeCompare(Manager.Options[OptionIndex].ObjectInput, typeof(UnityEngine.UI.Slider)))
            {
                UnityEngine.UI.Slider Slider = (UnityEngine.UI.Slider)Manager.Options[OptionIndex].ObjectInput;
                if (Manager.Options[OptionIndex].ParseController == SettingsManagerEnums.ItemParse.NormalValue)
                {
                    Slider.wholeNumbers = false;
                }
                else
                {
                    Slider.wholeNumbers = true;
                }
                if (SettingsManagerTypesHelper.TypeCompare(Manager.Options[OptionIndex].ApplyInput, typeof(UnityEngine.UI.Button)))
                {
                    UnityEngine.UI.Button Button = (UnityEngine.UI.Button)Manager.Options[OptionIndex].ApplyInput;
                    Manager.Options[OptionIndex].ApplyAction = delegate { SettingsManagerSlider.SliderExecution(OptionIndex, Manager, Slider.value); };

                    if (Manager.Options[OptionIndex].ApplyAction != null)
                    {
                        Button.onClick.RemoveListener(Manager.Options[OptionIndex].ApplyAction);
                    }
                    Button.onClick.AddListener(Manager.Options[OptionIndex].ApplyAction);


                    if (Manager.Options[OptionIndex].FloatAction != null)
                    {
                        Slider.onValueChanged.RemoveListener(Manager.Options[OptionIndex].FloatAction);
                    }
                    Manager.Options[OptionIndex].FloatAction = delegate { SettingsManagerSlider.SetSliderDescription(OptionIndex, Manager, Slider.value); };
                    Slider.onValueChanged.AddListener(Manager.Options[OptionIndex].FloatAction);
                }
                else
                {
                    if (Manager.Options[OptionIndex].FloatAction != null)
                    {
                        Slider.onValueChanged.RemoveListener(Manager.Options[OptionIndex].FloatAction);
                    }
                    Manager.Options[OptionIndex].FloatAction = delegate { SettingsManagerSlider.SliderExecution(OptionIndex, Manager, Slider.value); };
                    Slider.onValueChanged.AddListener(Manager.Options[OptionIndex].FloatAction);
                }
            }
            SetResetAction(Manager, OptionIndex);
            return true;
        }
        public override SettingsManagerEnums.IsTypeInterpreter GetActiveType()
        {
            return SettingsManagerEnums.IsTypeInterpreter.Slider;
        }
    }
}