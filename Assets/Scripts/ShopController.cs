using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Магазин: сила клика и пассивный доход. Экран собирается из кода тем же каркасом,
/// что и магазин фонов, строки — одинаковые (ShopRow).
/// </summary>
public class ShopController : Singleton<ShopController>
{
    public GameObject shopPanel;
    [SerializeField] private UInt64 baseUpgradePrice = 10;

    private static readonly Color ClickTileColor = new Color32(44, 44, 44, 255);
    private static readonly Color PassiveTileColor = new Color32(31, 46, 36, 255);

    private PlayerController playerController;
    private ShopRow clickRow;
    private ShopRow passiveRow;
    private TextMeshProUGUI balanceText;
    private float refreshTimer;

    public bool IsOpen => shopPanel != null && shopPanel.activeSelf;

    void Awake()
    {
        playerController = PlayerController.Instance;
    }

    private void Start()
    {
        BuildScreen();
    }

    private void Update()
    {
        if (shopPanel == null || !shopPanel.activeSelf)
            return;

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = 0.5f;
        Refresh();
    }

    public void SwitchPanel()
    {
        if (shopPanel == null)
            return;

        bool open = !shopPanel.activeSelf;
        shopPanel.SetActive(open);

        if (!open)
            return;

        shopPanel.transform.SetAsLastSibling();
        refreshTimer = 0.5f;
        Refresh();
    }

    private void BuildScreen()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopController: не задан shopPanel");
            return;
        }

        ShopScreenContent screen = ShopScreenBuilder.Build(shopPanel.transform, "SHOP", clearExistingChildren: true);
        balanceText = screen.Balance;

        clickRow = new ShopRow(screen.List, "click_power");
        clickRow.Action.onClick.AddListener(BuyClickUpgrade);

        passiveRow = new ShopRow(screen.List, "passive_income");
        passiveRow.Action.onClick.AddListener(BuyPassiveIncome);

        Refresh();
    }

    private void Refresh()
    {
        if (clickRow == null || passiveRow == null)
            return;

        ulong money = playerController != null ? playerController.money : 0UL;
        balanceText.SetText($"{money}$");

        RefreshClickRow(money);
        RefreshPassiveRow(money);
    }

    private void RefreshClickRow(ulong money)
    {
        ulong increase = UpgradesController.Instance != null ? UpgradesController.Instance.clickIncrease : 0UL;
        ulong price = CurrentClickPrice;
        bool affordable = money >= price;

        // одна покупка = один уровень
        clickRow.SetIcon(null, ClickTileColor, "+1");
        clickRow.SetTitle("click power", $"lvl {increase} · +{increase + 1}$ per click");
        clickRow.SetAction($"{(increase == 0 ? "buy" : "upgrade")} {price}$", affordable,
            affordable ? UiKit.GlassTint : UiKit.GlassTintDisabled);
    }

    private void RefreshPassiveRow(ulong money)
    {
        PassiveIncomeController passive = PassiveIncomeController.Instance;

        passiveRow.SetIcon(null, PassiveTileColor, "$/s");

        if (!passive.IsUnlocked)
            passiveRow.SetTitle("passive income", "not unlocked · +1$/sec");
        else
            passiveRow.SetTitle("passive income", $"lvl {passive.Level} · +{passive.IncomePerSecond}$/sec");

        if (passive.IsMaxed)
        {
            passiveRow.SetAction("max", false, UiKit.GlassTintDisabled);
            return;
        }

        ulong price = passive.NextPrice;
        bool affordable = money >= price;
        passiveRow.SetAction($"{(passive.IsUnlocked ? "upgrade" : "buy")} {price}$", affordable,
            affordable ? UiKit.GlassTint : UiKit.GlassTintDisabled);
    }

    private void BuyClickUpgrade()
    {
        ulong price = CurrentClickPrice;
        if (playerController == null || !playerController.SpendMoney(price))
            return;

        int.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1), out int count);
        count++;

        SavingSystem.Instance.SaveString(SaveNames.upgrade1, count.ToString());
        SavingSystem.Instance.SaveString(SaveNames.upgrade1Price, DoublePrice(price).ToString());

        UpgradesController.Instance.UpdateUpgrades();
        Refresh();
    }

    private void BuyPassiveIncome()
    {
        PassiveIncomeController.Instance.TryBuyOrUpgrade();
        Refresh();
    }

    /// <summary>Цена следующего апгрейда силы клика (хранится в PlayerPrefs).</summary>
    private ulong CurrentClickPrice
    {
        get
        {
            if (!ulong.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1Price), out ulong price) || price == 0)
                return baseUpgradePrice;

            return price;
        }
    }

    private static ulong DoublePrice(ulong price)
    {
        return price > ulong.MaxValue / 2 ? ulong.MaxValue : price * 2;
    }
}
