using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Экран ачивок: каркас и строки те же, что в магазине.</summary>
public class AchievementView
{
    private readonly AchievementsController controller;
    private readonly GameObject root;
    private readonly TextMeshProUGUI summary;
    private readonly List<ShopRow> rows = new List<ShopRow>();

    public bool IsVisible => root.activeSelf;

    public AchievementView(AchievementsController controller, Transform canvas)
    {
        this.controller = controller;

        RectTransform rootRect = UiKit.CreateRect("AchievementsScreen", canvas);
        UiKit.Fill(rootRect);
        root = rootRect.gameObject;

        ShopScreenContent screen = ShopScreenBuilder.Build(root.transform, "ACHIEVEMENTS");
        summary = screen.Balance;

        for (int i = 0; i < AchievementsCatalog.All.Length; i++)
            rows.Add(new ShopRow(screen.List, AchievementsCatalog.All[i].id));

        controller.StateChanged += Refresh;

        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        Refresh();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void Refresh()
    {
        summary.SetText($"{controller.UnlockedCount} / {controller.TotalCount}");

        for (int i = 0; i < rows.Count; i++)
        {
            AchievementDefinition definition = AchievementsCatalog.All[i];
            ShopRow row = rows[i];
            bool unlocked = controller.IsUnlocked(i);

            row.SetCardColor(unlocked ? UiKit.CardActiveColor : UiKit.CardColor);
            row.SetIcon(null, unlocked ? UiKit.GlassTintAccent : UiKit.GlassTintDisabled, definition.badge);
            row.SetTitle(definition.title, $"keep {definition.requirement}$ on your balance");

            if (unlocked)
                row.SetStatus("unlocked", UiKit.GlassTintAccent);
            else
                row.SetStatus("locked", UiKit.GlassTintDisabled);
        }
    }
}
