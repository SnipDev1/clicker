using System.Collections.Generic;
using UnityEngine;

/// <summary>Как рисуется фон.</summary>
public enum BackgroundStyle
{
    Gradient,   // плавный градиент сверху вниз
    Glow,       // градиент + мягкое световое пятно
    Stars,      // градиент + звёзды
    Waves,      // градиент + волны
    Custom      // своя картинка игрока
}

/// <summary>Описание одного фона из магазина.</summary>
public class BackgroundDefinition
{
    public string id;
    public string displayName;
    public ulong price;
    public BackgroundStyle style;

    public Color topColor = new Color(0.23f, 0.23f, 0.23f, 1f);
    public Color bottomColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    public Color accentColor = Color.white;

    /// <summary>Где светится пятно (для Glow). Координаты 0..1 от левого нижнего угла.</summary>
    public Vector2 accentPosition = new Vector2(0.5f, 0.5f);
    public float accentRadius = 0.6f;

    /// <summary>Готовая картинка игрока (для стиля Custom).</summary>
    public Sprite customSprite;

    /// <summary>Путь к файлу своей картинки (для стиля Custom).</summary>
    public string customPath;

    /// <summary>Фон открыт сразу, покупать не нужно.</summary>
    public bool ownedByDefault;
}

/// <summary>Список всех фонов. Порядок = порядок в магазине.</summary>
public static class BackgroundsCatalog
{
    public const string DefaultId = "classic";

    public static readonly List<BackgroundDefinition> All = new List<BackgroundDefinition>
    {
        new BackgroundDefinition
        {
            id = "classic", displayName = "classic", price = 0, style = BackgroundStyle.Gradient,
            topColor = Hex("#3B3B3B"), bottomColor = Hex("#141414"), ownedByDefault = true
        },
        new BackgroundDefinition
        {
            id = "midnight", displayName = "midnight", price = 100, style = BackgroundStyle.Stars,
            topColor = Hex("#16224A"), bottomColor = Hex("#05060F"), accentColor = Hex("#7FA6FF")
        },
        new BackgroundDefinition
        {
            id = "sunset", displayName = "sunset", price = 350, style = BackgroundStyle.Glow,
            topColor = Hex("#2B1055"), bottomColor = Hex("#FA8C40"), accentColor = Hex("#FFD36E"),
            accentPosition = new Vector2(0.5f, 0.18f), accentRadius = 0.75f
        },
        new BackgroundDefinition
        {
            id = "ocean", displayName = "ocean", price = 800, style = BackgroundStyle.Waves,
            topColor = Hex("#0E4C6B"), bottomColor = Hex("#04121C"), accentColor = Hex("#2FD3E0")
        },
        new BackgroundDefinition
        {
            id = "forest", displayName = "forest", price = 1500, style = BackgroundStyle.Glow,
            topColor = Hex("#1B4D2E"), bottomColor = Hex("#05130B"), accentColor = Hex("#9BE86B"),
            accentPosition = new Vector2(0.25f, 0.72f), accentRadius = 0.7f
        },
        new BackgroundDefinition
        {
            id = "lavender", displayName = "lavender", price = 3000, style = BackgroundStyle.Glow,
            topColor = Hex("#4B3B8F"), bottomColor = Hex("#17102B"), accentColor = Hex("#D6B8FF"),
            accentPosition = new Vector2(0.5f, 0.7f), accentRadius = 0.8f
        },
        new BackgroundDefinition
        {
            id = "neon", displayName = "neon", price = 6000, style = BackgroundStyle.Glow,
            topColor = Hex("#2B0038"), bottomColor = Hex("#06010F"), accentColor = Hex("#FF2E88"),
            accentPosition = new Vector2(0.5f, 0.3f), accentRadius = 0.55f
        },
        new BackgroundDefinition
        {
            id = "cosmos", displayName = "cosmos", price = 12000, style = BackgroundStyle.Stars,
            topColor = Hex("#1B0F3A"), bottomColor = Hex("#02010A"), accentColor = Hex("#8B5CF6")
        },
        new BackgroundDefinition
        {
            id = "gold", displayName = "gold", price = 25000, style = BackgroundStyle.Glow,
            topColor = Hex("#4A3410"), bottomColor = Hex("#0B0703"), accentColor = Hex("#FFC94D"),
            accentPosition = new Vector2(0.5f, 0.85f), accentRadius = 0.7f
        }
    };

    public static BackgroundDefinition Get(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < All.Count; i++)
        {
            if (All[i].id == id)
                return All[i];
        }

        return null;
    }

    public static BackgroundDefinition Default => Get(DefaultId);

    /// <summary>Добавить свой фон (картинку игрока). Дубликаты по id игнорируются.</summary>
    public static void Register(BackgroundDefinition definition)
    {
        if (definition == null || Get(definition.id) != null)
            return;

        All.Add(definition);
    }

    public static void Remove(string id)
    {
        for (int i = 0; i < All.Count; i++)
        {
            if (All[i].id != id)
                continue;

            All.RemoveAt(i);
            return;
        }
    }

    /// <summary>Убрать все фоны игрока (нужно перед перечитыванием папки).</summary>
    public static void RemoveCustoms()
    {
        for (int i = All.Count - 1; i >= 0; i--)
        {
            if (All[i].style == BackgroundStyle.Custom)
                All.RemoveAt(i);
        }
    }

    private static Color Hex(string value)
    {
        return ColorUtility.TryParseHtmlString(value, out Color color) ? color : Color.magenta;
    }
}
