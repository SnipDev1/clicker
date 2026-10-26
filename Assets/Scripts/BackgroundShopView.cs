using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Экран магазина фонов. Собирается из кода, чтобы не переписывать сцену руками.
/// Каркас общий с магазином апгрейдов (ShopScreenBuilder), строки — тоже (ShopRow).
/// </summary>
public class BackgroundShopView
{
    private readonly BackgroundsController controller;
    private readonly GameObject root;
    private readonly ScrollRect scroll;
    private readonly RectTransform content;
    private readonly TextMeshProUGUI balanceText;
    private readonly List<Row> rows = new List<Row>();
    private readonly HashSet<string> builtIds = new HashSet<string>();
    private float refreshTimer;

    private class Row
    {
        public BackgroundDefinition definition;
        public ShopRow view;
        public Button delete;
    }

    public bool IsVisible => root.activeSelf;

    public BackgroundShopView(BackgroundsController controller, Transform canvas)
    {
        this.controller = controller;

        RectTransform rootRect = UiKit.CreateRect("BackgroundsScreen", canvas);
        UiKit.Fill(rootRect);
        root = rootRect.gameObject;

        ShopScreenContent screen = ShopScreenBuilder.Build(root.transform, "BACKGROUNDS");
        content = screen.List;
        scroll = screen.Scroll;
        balanceText = screen.Balance;

        // внизу два столбца: «добавить картинку» и «закрыть»
        UiKit.SetAnchored((RectTransform)screen.Close.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(-200f, 120f), new Vector2(360f, 150f));

        Button addImage = UiKit.CreateButton("add_image", root.transform, "add image", 44f);
        UiKit.SetAnchored((RectTransform)addImage.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(200f, 120f), new Vector2(360f, 150f));
        addImage.onClick.AddListener(AddImage);

        SyncRows();

        controller.StateChanged += Refresh;

        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        SyncRows();
        Refresh();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        scroll.verticalNormalizedPosition = 1f;
        refreshTimer = 0.5f;
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void Toggle()
    {
        if (IsVisible)
            Hide();
        else
            Show();
    }

    /// <summary>Пока экран открыт, раз в полсекунды обновляем баланс и кнопки.</summary>
    public void Tick()
    {
        if (!IsVisible)
            return;

        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer > 0f)
            return;

        refreshTimer = 0.5f;
        Refresh();
    }

    /// <summary>Перерисовать строки под текущее состояние игрока.</summary>
    public void Refresh()
    {
        ulong money = PlayerController.Instance != null ? PlayerController.Instance.money : 0UL;
        balanceText.SetText($"{money}$");

        foreach (Row row in rows)
        {
            BackgroundDefinition definition = row.definition;
            bool owned = controller.IsOwned(definition.id);
            bool current = controller.CurrentId == definition.id;

            row.view.SetCardColor(current ? UiKit.CardActiveColor : UiKit.CardColor);

            if (current)
            {
                row.view.SetTitle(definition.displayName, "in use");
                row.view.SetAction("selected", false, UiKit.GlassTintAccent);
            }
            else if (owned)
            {
                row.view.SetTitle(definition.displayName, "owned");
                row.view.SetAction("select", true, UiKit.GlassTint);
            }
            else
            {
                bool affordable = money >= definition.price;
                row.view.SetTitle(definition.displayName, "not owned");
                row.view.SetAction($"buy {definition.price}$", affordable,
                    affordable ? UiKit.GlassTint : UiKit.GlassTintDisabled);
            }
        }
    }

    private void OnAction(BackgroundDefinition definition)
    {
        if (definition == null || controller.CurrentId == definition.id)
            return;

        if (controller.IsOwned(definition.id))
            controller.Select(definition.id);
        else
            controller.TryBuy(definition);
    }

    private void AddImage()
    {
        if (NativeFileDialog.IsAvailable)
        {
            string path;
            if (!NativeFileDialog.TryPickImage(out path))
                return;      // отменили выбор

            BackgroundDefinition imported = CustomBackgrounds.Import(path);
            if (imported == null)
                return;

            BackgroundsCatalog.Register(imported);
            SyncRows();
            controller.Select(imported.id);
        }
        else
        {
            // системного диалога нет — показываем папку и подхватываем то, что положили
            CustomBackgrounds.OpenFolder();

            if (CustomBackgrounds.RegisterFolder() == 0)
                return;

            SyncRows();
            controller.Select(BackgroundsCatalog.All[BackgroundsCatalog.All.Count - 1].id);
        }

        Refresh();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    /// <summary>Добавляет строки для фонов, которых ещё нет в списке (например, своих картинок).</summary>
    private void SyncRows()
    {
        foreach (BackgroundDefinition definition in BackgroundsCatalog.All)
        {
            if (!builtIds.Add(definition.id))
                continue;

            var view = new ShopRow(content, definition.id);
            view.SetIcon(BackgroundTextureFactory.GetSprite(definition), Color.white);

            var row = new Row
            {
                definition = definition,
                view = view
            };

            view.Action.onClick.AddListener(() => OnAction(row.definition));

            if (definition.style == BackgroundStyle.Custom)
                row.delete = CreateDeleteButton(view.Root, row);

            rows.Add(row);
        }
    }

    /// <summary>Крестик на превью своей картинки.</summary>
    private Button CreateDeleteButton(Transform parent, Row row)
    {
        Button delete = UiKit.CreateButton("delete", parent, "x", 36f);
        UiKit.SetAnchored((RectTransform)delete.transform,
            new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(44f, -40f), new Vector2(56f, 56f));
        delete.onClick.AddListener(() => DeleteCustom(row));
        return delete;
    }

    private void DeleteCustom(Row row)
    {
        if (row == null || row.definition == null)
            return;

        rows.Remove(row);
        builtIds.Remove(row.definition.id);

        if (row.view.Root != null)
            row.view.Root.gameObject.SetActive(false);

        // контроллер удалит файл и выберет классику, если фон был активным
        controller.DeleteCustom(row.definition);

        if (row.view.Root != null)
            UnityEngine.Object.Destroy(row.view.Root.gameObject);

        Refresh();
    }
}
