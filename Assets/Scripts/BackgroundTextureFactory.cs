using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Рисует картинки фонов в рантайме, чтобы не тащить в проект текстуры.
/// Спрайты кэшируются: каждый фон генерируется один раз.
/// </summary>
public static class BackgroundTextureFactory
{
    // пропорции под вертикальный экран 1080x1920
    private const int Width = 320;
    private const int Height = 568;

    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite GetSprite(BackgroundDefinition definition)
    {
        if (definition == null)
            return null;

        // своя картинка уже готова, генерировать нечего
        if (definition.style == BackgroundStyle.Custom)
            return definition.customSprite;

        if (Cache.TryGetValue(definition.id, out Sprite cached) && cached != null)
            return cached;

        Sprite sprite = Build(definition);
        Cache[definition.id] = sprite;
        return sprite;
    }

    private static Sprite Build(BackgroundDefinition definition)
    {
        var pixels = new Color[Width * Height];

        for (int y = 0; y < Height; y++)
        {
            float t = y / (float)(Height - 1);            // 0 = низ, 1 = верх
            Color baseColor = Color.Lerp(definition.bottomColor, definition.topColor, t);

            for (int x = 0; x < Width; x++)
            {
                float u = x / (float)(Width - 1);
                pixels[y * Width + x] = ApplyStyle(baseColor, definition, u, t);
            }
        }

        if (definition.style == BackgroundStyle.Stars)
            DrawStars(pixels, definition);

        var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
        {
            name = "Background_" + definition.id,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Color ApplyStyle(Color baseColor, BackgroundDefinition definition, float u, float t)
    {
        switch (definition.style)
        {
            case BackgroundStyle.Glow:
            {
                float distance = Vector2.Distance(new Vector2(u, t), definition.accentPosition);
                float strength = Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, definition.accentRadius));
                return Color.Lerp(baseColor, definition.accentColor, Mathf.Pow(strength, 2.2f) * 0.65f);
            }

            case BackgroundStyle.Waves:
            {
                float wave = Mathf.Sin(t * Mathf.PI * 10f + Mathf.Sin(u * Mathf.PI * 2f) * 1.2f);
                return Color.Lerp(baseColor, definition.accentColor, Mathf.Max(0f, wave) * 0.16f);
            }

            default:
                return baseColor;
        }
    }

    private static void DrawStars(Color[] pixels, BackgroundDefinition definition)
    {
        var random = new System.Random(StableSeed(definition.id));
        Color starColor = Color.Lerp(Color.white, definition.accentColor, 0.35f);

        // крупные звёзды с ореолом
        for (int i = 0; i < 14; i++)
        {
            float x = (float)random.NextDouble() * Width;
            float y = (float)random.NextDouble() * Height;
            DrawDot(pixels, x, y, Mathf.Lerp(3f, 6f, (float)random.NextDouble()), 0.10f, starColor);
        }

        // мелкие звёзды
        for (int i = 0; i < 130; i++)
        {
            float x = (float)random.NextDouble() * Width;
            float y = (float)random.NextDouble() * Height;
            float radius = Mathf.Lerp(0.5f, 1.8f, (float)random.NextDouble());
            float brightness = Mathf.Lerp(0.25f, 1f, (float)random.NextDouble());
            DrawDot(pixels, x, y, radius, brightness, starColor);
        }
    }

    private static void DrawDot(Color[] pixels, float centerX, float centerY, float radius, float brightness, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radius - 1f));
        int maxX = Mathf.Min(Width - 1, Mathf.CeilToInt(centerX + radius + 1f));
        int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radius - 1f));
        int maxY = Mathf.Min(Height - 1, Mathf.CeilToInt(centerY + radius + 1f));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(centerX, centerY));
                float alpha = Mathf.Clamp01(1f - distance / (radius + 0.001f)) * brightness;
                if (alpha <= 0f)
                    continue;

                int index = y * Width + x;
                pixels[index] = Color.Lerp(pixels[index], color, alpha);
            }
        }
    }

    /// <summary>Стабильный сид, чтобы звёзды не перескакивали между запусками.</summary>
    private static int StableSeed(string value)
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < value.Length; i++)
                hash = hash * 31 + value[i];
            return hash & 0x7FFFFFFF;
        }
    }
}
