using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameSceneController : Singleton<GameSceneController>
{
    public Button clickBtn; 
    public Button shopBtn;
    public TextMeshProUGUI tmp_moneyText;
    private ClickerController clickerController;
    private ShopController shopController;

    void Start() 
    {
        clickerController = ClickerController.Instance;
        shopController = ShopController.Instance;

        clickBtn.onClick.AddListener(clickerController.addMoney);
        shopBtn.onClick.AddListener(shopController.SwitchPanel);
        
    }

    private void Test()
    {
        Debug.Log("BTN CLICKED!");
    }
    
    public void setMoney(UInt64 money)
    {
        string moneyText = money.ToString() + "$";
        Debug.Log(moneyText);
        tmp_moneyText.SetText(moneyText);
    }

}
