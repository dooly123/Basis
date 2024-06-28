using UnityEngine;
using UnityEngine.EventSystems;
namespace BattlePhaze.SettingsManager.ExplanationManagement
{
    public class SettingsManagerExplanation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region PublicFields
        public int OptionIndex;
        public SettingsManager Manager;
        #endregion
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Manager)
            {
                SettingsManagerDescriptionSystem.ExplanationSystem(Manager, Manager.Options[OptionIndex].Explanation);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Manager)
            {
                SettingsManagerDescriptionSystem.ExplanationSystem(Manager, string.Empty);
            }
        }
    }
}