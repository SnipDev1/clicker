using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Что получилось после сборки экрана магазина.</summary>
public class ShopScreenContent
{
    public RectTransform List;
    public ScrollRect Scroll;
    public TextMeshProUGUI Balance;
    public Button Close;
}

/// <summary>
/// Собирает общий каркас магазина: затемнение, заголовок, баланс, список со скроллом и кнопку close.
/// Один и тот же каркас у магазина апгрейдов и у магазина фонов, чтобы они выглядели одинаково.
/// </summary>
public static class ShopScreenBuilder
{
    public static ShopScreenContent Build(Transform root, string title, bool clearExistingChildren = false)
    {
        if (clearExistingChildren)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
        }

        // размытый лист «жидкого стекла» на весь экран
        Image backdrop = UiKit.CreateImage("backdrop", root, UiKit.GlassTintSheet, null, raycast: true);
        UiKit.Fill(backdrop.rectTransform);
        UiKit.ApplyGlass(backdrop, UiKit.GlassTintSheet, sheet: true);

        TextMeshProUGUI titleText = UiKit.CreateText("title", root, title, 80f, UiKit.TitleColor,
            TextAlignmentOptions.Center, FontStyles.Bold);
        UiKit.SetTop(titleText.rectTransform, 56f, 120f, 40f, 40f);

        var content = new ShopScreenContent
        {
            Balance = UiKit.CreateText("balance", root, "0$", 46f, UiKit.MutedColor)
        };
        UiKit.SetTop(content.Balance.rectTransform, 176f, 70f, 40f, 40f);

        BuildList(root, out RectTransform list, out ScrollRect scroll);
        content.List = list;
        content.Scroll = scroll;

        Button close = UiKit.CreateButton("close", root, "close", 52f);
        UiKit.SetAnchored((RectTransform)close.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 120f), new Vector2(460f, 150f));
        close.onClick.AddListener(() => root.gameObject.SetActive(false));
        content.Close = close;

        return content;
    }

    private static void BuildList(Transform root, out RectTransform listContent, out ScrollRect scroll)
    {
        RectTransform listRect = UiKit.CreateRect("list", root);
        UiKit.SetStretch(listRect, 40f, 40f, 290f, 230f);

        scroll = listRect.gameObject.AddComponent<ScrollRect>();

        RectTransform viewport = UiKit.CreateRect("viewport", listRect);
        UiKit.Fill(viewport);

        // прозрачная картинка нужна, чтобы список ловил прокрутку
        Image viewportImage = UiKit.CreateImage("viewportMask", viewport, new Color(1f, 1f, 1f, 0f), null, raycast: true);
        UiKit.Fill(viewportImage.rectTransform);
        viewport.gameObject.AddComponent<RectMask2D>();

        listContent = UiKit.CreateRect("content", viewport);
        listContent.anchorMin = new Vector2(0f, 1f);
        listContent.anchorMax = new Vector2(1f, 1f);
        listContent.pivot = new Vector2(0.5f, 1f);
        listContent.anchoredPosition = Vector2.zero;
        listContent.sizeDelta = Vector2.zero;

        var layout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = listContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = listContent;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.12f;
        scroll.scrollSensitivity = 30f;
    }
}
