using BattlePhaze.SettingsManager.DebugSystem;
using BattlePhaze.SettingsManager.Types;
using System;
using UnityEngine;
namespace BattlePhaze.SettingsManager
{
    public class SMTypeFallBackSlider : SettingsManagerAbstractTypeSlider
    {
        public SettingsManager SettingsManager;
        /// <summary>
        /// Slider Option Set Value
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="Value"></param>
        public override bool SliderOptionSetValue(SettingsManager Manager, int OptionIndex, float Value, float Min, float Max)
        {
            try
            {
                if (Manager != null)
                {
                    if (Manager.Options[OptionIndex] != null)
                    {
                        if (Manager.Options[OptionIndex].TextDescription == null)
                        {
                            int Index = SettingsManager.FindOrAddOption(OptionIndex);
                            SMWorkAround WorkAround = SettingsManager.WorkArounds[Index];
                            WorkAround.SelectedValue = Value.ToString(Manager.ManagerSettings.CInfo);
                            WorkAround.Min = Min;
                            WorkAround.Max = Max;
                            Debug.Log("setting value to " + WorkAround.SelectedValue + " for " + WorkAround.Name);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch(Exception E)
            {
                SettingsManagerDebug.LogError(E.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Slider Option Set Value
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="Value"></param>
        public override bool SliderOptionReadValue(SettingsManager Manager, int OptionIndex, out float Value)
        {
            Value = 0f;
            if (Manager.Options[OptionIndex].ObjectInput == null)
            {
                int Index = SettingsManager.FindOrAddOption(OptionIndex);
                SMWorkAround WorkAround = SettingsManager.WorkArounds[Index];
                if (float.TryParse(WorkAround.SelectedValue, System.Globalization.NumberStyles.Any, Manager.ManagerSettings.CInfo, out float FloatValue))
                {
                    Value = FloatValue;
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to Parse!");
                }
            }
            return false;
        }
        /// <summary>
        /// Slider Option Set Value
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="Value"></param>
        public override bool SliderOptionReadMaxValue(SettingsManager Manager, int OptionIndex,  out float Value)
        {
            Value = 0f;
            if (Manager.Options[OptionIndex].ObjectInput == null)
            {
                SMWorkAround WorkAround = SettingsManager.WorkArounds[SettingsManager.FindOrAddOption(OptionIndex)];
                Value = WorkAround.Max;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Slider Option Set Value
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="Value"></param>
        public override bool SliderOptionReadMinValue(SettingsManager Manager, int OptionIndex, out float Value)
        {
            Value = 0f;
            if (Manager.Options[OptionIndex].ObjectInput == null)
            {
                SMWorkAround WorkAround = SettingsManager.WorkArounds[SettingsManager.FindOrAddOption(OptionIndex)];
                Value = WorkAround.Min;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Slider Option Return Gameobject
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="Value"></param>
        public override bool SliderGetOptionsGameobject(SettingsManager Manager, int OptionIndex, out GameObject GameObject)
        {
            GameObject = null;
            return false;
        }
        /// <summary>
        /// Slider Enabled State
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="outcome"></param>
        public override bool SliderEnabledState(SettingsManager Manager, int OptionIndex, bool outcome)
        {
            if (Manager.Options[OptionIndex].ObjectInput == null)
            {
                return false;
            }
            return false;
        }
        /// <summary>
        /// Slider On Value Changed
        /// </summary>
        /// <param name="Manager"></param>
        /// <param name="OptionIndex"></param>
        /// <param name="FunctionName"></param>
        public override bool SliderOnValueChanged(SettingsManager Manager, int OptionIndex)
        {
            if (Manager.Options[OptionIndex].ObjectInput == null)
            {
                return true;
            }
            return false;
        }
        public override SettingsManagerEnums.IsTypeInterpreter GetActiveType()
        {
            return SettingsManagerEnums.IsTypeInterpreter.Slider;
        }
    }
}