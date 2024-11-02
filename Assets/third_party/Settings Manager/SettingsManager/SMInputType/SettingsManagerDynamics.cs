namespace BattlePhaze.SettingsManager
{
    public static class SettingsManagerDynamics
    {
        public static void DynamicExecution(int OptionIndex, SettingsManager Manager, int CurrentIndex)
        {

            Manager.Options[OptionIndex].SelectedValue = Manager.Options[OptionIndex].SelectableValueList[CurrentIndex].RealValue;
            SettingsManagerDescriptionSystem.TxtDescriptionSetText(Manager, OptionIndex);
            SettingsManagerStorageManagement.Save(Manager);
            Manager.SendOption(Manager.Options[OptionIndex]);
        }
    }
}