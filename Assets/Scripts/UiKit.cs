using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Маленькие помощники, чтобы собирать UI из кода.
/// Стиль повторяет сцену: тёмно-серые скруглённые плашки и светлый текст.
/// </summary>
public static class UiKit
{
    public static readonly Color ScreenColor = new Color32(15, 15, 15, 255);
    public static readonly Color CardColor = new Color(0.08f, 0.083f, 0.095f, 0.55f);
    public static readonly Color CardActiveColor = new Color(0.07f, 0.25f, 0.15f, 0.65f);
    public static readonly Color ButtonColor = new Color32(66, 66, 66, 255);
    public static readonly Color ButtonDisabledColor = new Color32(48, 48, 48, 255);
    public static readonly Color ButtonTextColor = new Color(0.96f, 0.97f, 1f, 1f);
    public static readonly Color TitleColor = new Color32(242, 242, 242, 255);
    public static readonly Color MutedColor = new Color32(148, 148, 148, 255);
    public static readonly Color AccentColor = new Color32(70, 160, 105, 255);
    public static readonly Color PriceColor = new Color32(226, 200, 120, 255);

    // оттенки «жидкого стекла»: цвет уходит в тонировку, фон просвечивает через размытие
    public static readonly Color GlassTint = new Color(0.055f, 0.058f, 0.07f, 1f);
    public static readonly Color GlassTintSheet = new Color(0.02f, 0.022f, 0.03f, 1f);
    public static readonly Color GlassTintAccent = new Color(0.10f, 0.36f, 0.22f, 1f);
    public static readonly Color GlassTintDisabled = new Color(0.11f, 0.11f, 0.12f, 1f);

    private const int CornerRadius = 22;
    private static Sprite _roundedSprite;
    private static Sprite _circleSprite;
    private static TMP_FontAsset _font;
    private static bool _fontFailed;
    private static Sprite _glassSprite;
    private static Sprite _glassSpriteSquare;
    private static Material _glassMaterial;
    private static Material _glassSheetMaterial;
    private static Shader _glassShader;
    private static bool _glassMissing;

    /// <summary>Первая буква заглавная — для всех подписей в интерфейсе.</summary>
    public static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (!char.IsLetter(value[0]) || char.IsUpper(value[0]))
            return value;

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    /// <summary>Круг для монетки (белый, тонируется цветом).</summary>
    public static Sprite CircleSprite
    {
        get
        {
            if (_circleSprite == null)
                _circleSprite = BuildCircleSprite(128);
            return _circleSprite;
        }
    }

    /// <summary>Скруглённый спрайт (9-slice), общий для всех плашек.</summary>
    public static Sprite RoundedSprite
    {
        get
        {
            if (_roundedSprite == null)
                _roundedSprite = BuildRoundedSprite(64, CornerRadius);
            return _roundedSprite;
        }
    }

    /// <summary>Маска стекла: R — расстояние до кромки, A — форма.</summary>
    public static Sprite GlassSprite
    {
        get
        {
            if (_glassSprite == null)
                _glassSprite = BuildGlassSprite(64, CornerRadius, CornerRadius, CornerRadius);
            return _glassSprite;
        }
    }

    /// <summary>То же, но с прямыми углами — для полноэкранного листа.</summary>
    public static Sprite GlassSpriteSquare
    {
        get
        {
            if (_glassSpriteSquare == null)
                _glassSpriteSquare = BuildGlassSprite(64, 0, CornerRadius, CornerRadius);
            return _glassSpriteSquare;
        }
    }

    /// <summary>Материал стекла для кнопок и плашек. null, если шейдер не нашёлся.</summary>
    public static Material GlassMaterial => GetGlassMaterial(false);

    /// <summary>Материал для полноэкранного размытого листа.</summary>
    public static Material GlassSheetMaterial => GetGlassMaterial(true);

    /// <summary>Включает на картинке стеклянный эффект. Если шейдера нет — просто тонирует.</summary>
    public static void ApplyGlass(Image image, Color tint, bool sheet = false)
    {
        if (image == null)
            return;

        image.color = tint;

        Material material = GetGlassMaterial(sheet);
        if (material == null)
            return;

        image.sprite = sheet ? GlassSpriteSquare : GlassSprite;
        image.type = Image.Type.Sliced;
        image.material = material;
    }

    /// <summary>
    /// Шрифт интерфейса: Arial из Resources/Fonts/Arial.ttf, собранный в TMP-ассет на старте.
    /// Если файла нет — откатываемся на шрифт из TMP Settings.
    /// </summary>
    public static TMP_FontAsset Font
    {
        get
        {
            if (_font != null)
                return _font;

            if (!_fontFailed)
            {
                UnityEngine.Font source = Resources.Load<UnityEngine.Font>("Fonts/Arial");
                if (source != null)
                    _font = TMP_FontAsset.CreateFontAsset(source);

                if (_font == null)
                    _fontFailed = true;
            }

            if (_font != null)
                return _font;

            _font = TMP_Settings.defaultFontAsset;
            if (_font == null)
                _font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            return _font;
        }
    }

    /// <summary>Переключить на общий шрифт тексты, которые уже расставлены в сцене.</summary>
    public static void ApplyFontToScene()
    {
        TMP_FontAsset font = Font;
        if (font == null)
            return;

        foreach (TMP_Text text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (text.font != font)
                text.font = font;
        }
    }

    public static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent != null ? parent.gameObject.layer : 5;
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    /// <summary>Пустой прямоугольник-картинка (без спрайта — просто заливка цветом).</summary>
    public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null, bool raycast = false)
    {
        var rect = CreateRect(name, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Simple;
        image.raycastTarget = raycast;
        return image;
    }

    /// <summary>Плашка со скруглёнными углами.</summary>
    public static Image CreatePanel(string name, Transform parent, Color color, bool raycast = false)
    {
        var image = CreateImage(name, parent, color, RoundedSprite, raycast);
        image.type = Image.Type.Sliced;
        return image;
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string content, float maxFontSize, Color color,
        TextAlignmentOptions alignment = TextAlignmentOptions.Center, FontStyles fontStyle = FontStyles.Normal)
    {
        var rect = CreateRect(name, parent);
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = Font;
        text.text = Capitalize(content);
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = fontStyle;
        text.enableAutoSizing = true;
        text.fontSizeMin = 8f;
        text.fontSizeMax = maxFontSize;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
        return text;
    }

    public static Button CreateButton(string name, Transform parent, string label, float maxFontSize = 44f)
    {
        var background = CreatePanel(name, parent, ButtonColor, raycast: true);
        var button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;

        var text = CreateText("label", background.transform, label, maxFontSize, ButtonTextColor);
        Fill(text.rectTransform);

        ApplyButtonStyle(button);
        return button;
    }

    /// <summary>Приводит к общему виду кнопку, сделанную руками в сцене.</summary>
    public static void ApplyButtonStyle(Button button)
    {
        if (button == null)
            return;

        if (button.targetGraphic is Image image)
            ApplyGlass(image, GlassTint);

        TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
            label.color = ButtonTextColor;

        button.transition = Selectable.Transition.ColorTint;

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.62f, 0.62f, 0.62f, 0.7f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    /// <summary>Растянуть на весь родитель.</summary>
    public static void Fill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    /// <summary>Растянуть по ширине с отступами по краям.</summary>
    public static void SetStretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    /// <summary>Полоса во всю ширину, прижатая к верху: top — отступ сверху, height — высота.</summary>
    public static void SetTop(RectTransform rect, float top, float height, float left = 0f, float right = 0f)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, -top - height);
        rect.offsetMax = new Vector2(-right, -top);
    }

    /// <summary>Поставить элемент в точку с якорем (anchor в долях родителя).</summary>
    public static void SetAnchored(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static Material GetGlassMaterial(bool sheet)
    {
        if (sheet ? _glassSheetMaterial != null : _glassMaterial != null)
            return sheet ? _glassSheetMaterial : _glassMaterial;

        if (_glassMissing)
            return null;

        if (_glassShader == null)
        {
            // шейдер лежит в Resources, чтобы попасть в билд (Shader.Find его не находит там)
            _glassShader = Resources.Load<Shader>("LiquidGlass");
            if (_glassShader == null)
                _glassShader = Shader.Find("UI/LiquidGlass");

            if (_glassShader == null)
            {
                _glassMissing = true;
                Debug.LogWarning("UiKit: шейдер LiquidGlass не найден, стекло отключено");
                return null;
            }
        }

        var material = new Material(_glassShader)
        {
            name = sheet ? "LiquidGlassSheet" : "LiquidGlass",
            hideFlags = HideFlags.DontSave
        };

        material.SetFloat("_BlurSize", sheet ? 0.008f : 0.006f);
        material.SetFloat("_Refraction", sheet ? 0f : 0.011f);
        material.SetFloat("_TintStrength", sheet ? 0.65f : 0.45f);
        material.SetFloat("_Opacity", sheet ? 0.95f : 0.93f);
        material.SetFloat("_RimStrength", sheet ? 0f : 1.25f);
        material.SetFloat("_RimWidth", 0.4f);
        material.SetFloat("_Shade", sheet ? 0.05f : 0.10f);
        material.SetColor("_RimColor", Color.white);

        if (sheet)
            _glassSheetMaterial = material;
        else
            _glassMaterial = material;

        return material;
    }

    /// <summary>Маска стекла: R — расстояние до кромки (0..1), A — форма с антиалиасингом.</summary>
    private static Sprite BuildGlassSprite(int size, int radius, int band, int border)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "UiKit_GlassSdf",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        var pixels = new Color32[size * size];
        float half = size * 0.5f;
        float inner = half - radius;
        float safeBand = Mathf.Max(1f, band);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f - half;
                float py = y + 0.5f - half;

                float qx = Mathf.Abs(px) - inner;
                float qy = Mathf.Abs(py) - inner;

                float outsideX = Mathf.Max(qx, 0f);
                float outsideY = Mathf.Max(qy, 0f);
                float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
                float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);

                float signedDistance = outside + inside - radius;   // меньше нуля — внутри
                float coverage = Mathf.Clamp01(0.5f - signedDistance);
                float edge = Mathf.Clamp01(-signedDistance / safeBand);

                byte value = (byte)Mathf.RoundToInt(edge * 255f);
                pixels[y * size + x] = new Color32(value, value, value, (byte)Mathf.RoundToInt(coverage * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    private static Sprite BuildCircleSprite(int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "UiKit_Circle",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        var pixels = new Color32[size * size];
        float center = size * 0.5f;
        float radius = center - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                float coverage = Mathf.Clamp01(radius - distance + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(coverage * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite BuildRoundedSprite(int size, int radius)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "UiKit_Rounded",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = RoundedCoverage(x + 0.5f, y + 0.5f, size, radius);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        // border = радиус, чтобы углы не растягивались при 9-slice
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
            SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private static float RoundedCoverage(float px, float py, float size, float radius)
    {
        // расстояние до центра ближайшего скругления
        float centerX = Mathf.Clamp(px, radius, size - radius);
        float centerY = Mathf.Clamp(py, radius, size - radius);
        float distance = Vector2.Distance(new Vector2(px, py), new Vector2(centerX, centerY));
        return Mathf.Clamp01(radius - distance + 0.5f);
    }
}
