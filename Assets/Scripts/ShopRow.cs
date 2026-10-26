using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Одна строка магазина: превью слева, название с подписью и кнопка справа.
/// Одна и та же строка используется и в магазине апгрейдов, и в магазине фонов.
/// </summary>
public class ShopRow
{
    public const float Height = 200f;

    public readonly RectTransform Root;
    public readonly Image Card;
    public readonly Image Icon;
    public readonly TextMeshProUGUI IconLabel;
    public readonly TextMeshProUGUI Title;
    public readonly TextMeshProUGUI Subtitle;
    public readonly Button Action;
    public readonly Image ActionBackground;
    public readonly TextMeshProUGUI ActionLabel;

    public ShopRow(Transform parent, string id)
    {
        Root = UiKit.CreateRect("row_" + id, parent);

        var layout = Root.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = Height;
        layout.minHeight = Height;

        Card = UiKit.CreatePanel("card", Root, UiKit.CardColor);
        UiKit.Fill(Card.rectTransform);

        // превью: картинка фона или цветная плитка с символом
        Icon = UiKit.CreatePanel("icon", Root, UiKit.CardColor);
        UiKit.SetAnchored(Icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(85f, 0f), new Vector2(120f, 150f));

        IconLabel = UiKit.CreateText("iconLabel", Icon.transform, string.Empty, 52f, UiKit.TitleColor,
            TextAlignmentOptions.Center, FontStyles.Bold);
        UiKit.Fill(IconLabel.rectTransform);

        Title = UiKit.CreateText("title", Root, string.Empty, 44f, UiKit.TitleColor,
            TextAlignmentOptions.Left, FontStyles.Bold);
        UiKit.SetAnchored(Title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(175f, 34f), new Vector2(500f, 62f));

        Subtitle = UiKit.CreateText("subtitle", Root, string.Empty, 34f, UiKit.MutedColor, TextAlignmentOptions.Left);
        UiKit.SetAnchored(Subtitle.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(175f, -36f), new Vector2(500f, 52f));

        Action = UiKit.CreateButton("action", Root, "buy", 34f);
        UiKit.SetAnchored((RectTransform)Action.transform, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(-124f, 0f), new Vector2(228f, 116f));

        ActionBackground = Action.targetGraphic as Image;
        ActionLabel = Action.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    /// <summary>Превью фона (sprite != null) или цветная плитка с подписью.</summary>
    public void SetIcon(Sprite sprite, Color color, string label = "")
    {
        if (sprite != null)
        {
            Icon.sprite = sprite;
            Icon.type = Image.Type.Simple;
            Icon.preserveAspect = true;
        }
        else
        {
            Icon.sprite = UiKit.RoundedSprite;
            Icon.type = Image.Type.Sliced;
            Icon.preserveAspect = false;
        }

        Icon.color = color;
        IconLabel.SetText(label);
    }

    public void SetTitle(string title, string subtitle)
    {
        Title.SetText(UiKit.Capitalize(title));
        Subtitle.SetText(UiKit.Capitalize(subtitle));
    }

    public void SetCardColor(Color color)
    {
        Card.color = color;
    }

    public void SetAction(string label, bool interactable, Color background)
    {
        ActionLabel.SetText(UiKit.Capitalize(label));
        Action.interactable = interactable;
        ActionBackground.color = background;
    }

    /// <summary>Статус вместо кнопки: не жмётся и не приглушается (получено / закрыто).</summary>
    public void SetStatus(string label, Color background)
    {
        ActionLabel.SetText(UiKit.Capitalize(label));
        ActionBackground.color = background;
        Action.interactable = false;

        ColorBlock colors = Action.colors;
        colors.disabledColor = Color.white;
        Action.colors = colors;
    }
}
