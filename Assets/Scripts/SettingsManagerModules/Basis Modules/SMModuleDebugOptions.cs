using BattlePhaze.SettingsManager;
public class SMModuleDebugOptions : SettingsManagerOption
{
    public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
    {
        if (NameReturn(0, Option))
        {
            Popcron.Gizmos.Enabled = CheckIsOn(Option.SelectedValue);
        }
    }
}