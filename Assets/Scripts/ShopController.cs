using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : Singleton<ShopController>
{
    public GameObject shopPanel;
    public Button buyBtn;
    public Button shopBtn;

    void Awake()
    {
        shopBtn.onClick.AddListener(SwitchPanel);
        buyBtn.onClick.AddListener(BuyUpgrade);
    }


    public void SwitchPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    } 
    
    private void BuyUpgrade()
    {
        PlayerController.Instance.money -= 10;
        int.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1), out int result);
        result += 1;
        SavingSystem.Instance.SaveString(SaveNames.upgrade1, result.ToString());
        UpgradesController.Instance.UpdateUpgrades();
        
    }



}
