using BattlePhaze.SettingsManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMTypeFallbackDataHolder : MonoBehaviour
{
    [SerializeField]
    public List<SMWorkAround> WorkArounds = new List<SMWorkAround>();
    public SettingsManager Manager;
    public void Awake()
    {
        if (Manager == null)
        {
            Manager = SettingsManager.Instance;
        }
        Initalize();
    }
    public void Initalize()
    {
        WorkArounds.Clear();
        for (int WorkAroundIndex = 0; WorkAroundIndex < Manager.Options.Count; WorkAroundIndex++)
        {
            SMWorkAround WorkAround = new SMWorkAround();
            WorkAround.Name = Manager.Options[WorkAroundIndex].Name;
            WorkAround.Index = WorkAroundIndex;
            WorkAround.SelectedValue = Manager.Options[WorkAroundIndex].SelectedValue;
            WorkAround.DefaultValue = Manager.Options[WorkAroundIndex].SelectedValueDefault;
            WorkAround.SelectableValueList = Manager.Options[WorkAroundIndex].SelectableValueList;

            WorkArounds.Add(WorkAround);
        }
    }
    public int FindOrAddOption(int Index)
    {
        if (WorkArounds.Count != Manager.Options.Count)
        {
            Initalize();
        }
        return Index;
    }
}
