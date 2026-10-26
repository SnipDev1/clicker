using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneController : Singleton<GameSceneController>
{
    public Button clickBtn; 
    public Button shopBtn;
    public TextMeshProUGUI tmp_moneyText;

    private UInt64 _money;
    private bool _moneyDirty;
    private float _refreshTimer;

    void Start() 
    {
        UiKit.ApplyFontToScene();

        clickBtn.onClick.AddListener(ClickerController.Instance.addMoney);
        shopBtn.onClick.AddListener(ShopController.Instance.SwitchPanel);

        UiKit.ApplyButtonStyle(clickBtn);
        UiKit.ApplyButtonStyle(shopBtn);
        ApplyGlassBalance();
    }

    /// <summary>Строка баланса тоже стеклянная: панель — это родитель текста денег.</summary>
    private void ApplyGlassBalance()
    {
        if (tmp_moneyText == null || tmp_moneyText.transform.parent == null)
            return;

        UiKit.ApplyGlass(tmp_moneyText.transform.parent.GetComponent<Image>(), UiKit.GlassTint);

        // на тёмном стекле серый текст из сцены читается плохо
        tmp_moneyText.color = UiKit.TitleColor;
    }

    /// <summary>Текст денег перерисовываем не чаще 10 раз в секунду — пассивный доход может капать каждый кадр.</summary>
    private void Update()
    {
        if (!_moneyDirty)
            return;

        _refreshTimer -= Time.unscaledDeltaTime;
        if (_refreshTimer > 0f)
            return;

        _refreshTimer = 0.1f;
        _moneyDirty = false;
        tmp_moneyText.SetText(_money.ToString() + "$");
    }

    public void setMoney(UInt64 money)
    {
        _money = money;
        _moneyDirty = true;
    }

}
