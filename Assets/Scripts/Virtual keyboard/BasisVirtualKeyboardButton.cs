using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct BasisVirtualKeyboardButton
{
    public RectTransform ButtonRect;
    public TextMeshProUGUI Text;
    public Button Button;
    public BasisVirtualKeyboardSpecialKey BasisVirtualKeyboardSpecialKey;
}
public enum BasisVirtualKeyboardSpecialKey
{
   NotSpecial, IsDeleteKey, IsCaseSwitchKey,IsEnterKey
}