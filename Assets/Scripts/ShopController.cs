using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : Singleton<ShopController>
{
    public GameObject shopPanel;
    public Button buyBtn;
    public Button shopBtn;
    [SerializeField] private UInt64 baseUpgradePrice = 10;
    private PlayerController playerController;
    private TextMeshProUGUI btnText;

    private UInt64 _currentPrice = 10;


    private UInt64 currentPrice
    {
        get 
        { 
            UInt64.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1Price), out UInt64 cPrice);
            if (cPrice == 0)
                return baseUpgradePrice; 
            return cPrice;
        }
        set 
        { 
            _currentPrice = value;
            SavingSystem.Instance.SaveString(SaveNames.upgrade1Price, value.ToString());
            UpdateUI();
        }
    }

    void Awake()
    {
        btnText = buyBtn.GetComponentInChildren<TextMeshProUGUI>();
        shopBtn.onClick.AddListener(SwitchPanel);
        buyBtn.onClick.AddListener(BuyUpgrade);
        playerController = PlayerController.Instance;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        btnText.text = $"{currentPrice.ToString()}$";
    }

    public void SwitchPanel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
        UpdateUI();
    } 
    
    private void BuyUpgrade()
    {
        
        if (!playerController.SpendMoney(currentPrice))
            return;
        int.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1), out int result);
        

        currentPrice *= 2;
        result += 1;
        SavingSystem.Instance.SaveString(SaveNames.upgrade1, result.ToString());
        UpgradesController.Instance.UpdateUpgrades();

        
    }



}
