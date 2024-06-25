using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class BasisPlayerMenu : BasisUIBase
{
    public static string AddressableID = "BasisPlayerMenu";
    
    public Button HomePanelButton;
    public Button AvatarPanelButton;
    public Button SettingsPanelButton;
    public Button CloseMenuButton;

    public GameObject HomePanel;
    public GameObject AvatarPanel;
    public GameObject SettingsPanel;
    
    public static BasisPlayerMenu Instance;
    
    public void Start()
    {
        Instance = this;
        HomePanelButton.onClick.AddListener(() => OpenPanel(HomePanel));
        AvatarPanelButton.onClick.AddListener(() => OpenPanel(AvatarPanel));
        SettingsPanelButton.onClick.AddListener(() => OpenPanel(SettingsPanel));
        CloseMenuButton.onClick.AddListener(CloseThisMenu);
    }

    public static async Task OpenPlayerMenu()
    {
        await OpenThisMenu(AddressableID);
    }
    
    public void OpenPanel(GameObject panel)
    {
        HomePanel.SetActive(false);
        AvatarPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        panel.SetActive(true);
    }
}