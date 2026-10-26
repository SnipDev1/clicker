using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ивент с монеткой: изредка через экран пролетает монета.
/// Успел нажать — 30 секунд двойной доход за клик. Не успел — минус четверть баланса.
/// </summary>
public class CoinEventController : Singleton<CoinEventController>
{
    private const float FirstDelay = 8f;
    private const float MinDelay = 18f;
    private const float MaxDelay = 35f;
    private const float TravelTime = 5f;
    private const float BonusDuration = 30f;

    private static readonly Color BonusColor = new Color32(255, 214, 92, 255);
    private static readonly Color PenaltyColor = new Color32(255, 104, 96, 255);

    [SerializeField] private RectTransform gameScreen;

    private RectTransform canvas;
    private RectTransform coinRoot;
    private TextMeshProUGUI statusLabel;
    private string statusText;

    private bool coinActive;
    private float coinStartTime;
    private float coinBaseY;
    private float spawnTimer;
    private float bonusLeft;
    private float messageLeft;

    /// <summary>Активен ли бонус «двойной клик».</summary>
    public bool IsClickBonusActive => bonusLeft > 0f;

    protected override void Awake()
    {
        spawnTimer = FirstDelay;
    }

    private void Start()
    {
        if (gameScreen == null)
        {
            Debug.LogError("CoinEventController: не задан gameScreen");
            return;
        }

        canvas = gameScreen.parent as RectTransform;
        if (canvas == null)
            return;

        BuildCoin();
        BuildStatusLabel();
    }

    private void Update()
    {
        if (canvas == null)
            return;

        UpdateStatus();

        if (coinActive)
        {
            // открыли магазин — монетку убираем без штрафа
            if (AnyScreenOpen())
            {
                HideCoin();
                spawnTimer = Random.Range(FirstDelay, MinDelay);
                return;
            }

            AnimateCoin();
            return;
        }

        if (AnyScreenOpen())
            return;

        spawnTimer -= Time.unscaledDeltaTime;
        if (spawnTimer <= 0f)
            SpawnCoin();
    }

    private void BuildCoin()
    {
        coinRoot = UiKit.CreateRect("CoinEvent", canvas);
        UiKit.SetAnchored(coinRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(190f, 190f));

        // монетка должна быть над игровым экраном, но под панелями магазинов
        coinRoot.SetSiblingIndex(Mathf.Min(1, canvas.childCount - 1));

        Image glow = UiKit.CreateImage("glow", coinRoot, new Color(1f, 0.78f, 0.22f, 0.22f), UiKit.CircleSprite);
        UiKit.SetAnchored(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(260f, 260f));

        Image body = UiKit.CreateImage("body", coinRoot, new Color32(242, 194, 48, 255), UiKit.CircleSprite, true);
        UiKit.Fill(body.rectTransform);

        Image face = UiKit.CreateImage("face", coinRoot, new Color32(255, 217, 90, 255), UiKit.CircleSprite);
        UiKit.SetAnchored(face.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
            new Vector2(150f, 150f));

        TextMeshProUGUI sign = UiKit.CreateText("sign", coinRoot, "$", 108f, new Color32(107, 74, 0, 255),
            TextAlignmentOptions.Center, FontStyles.Bold);
        UiKit.Fill(sign.rectTransform);

        var button = body.gameObject.AddComponent<Button>();
        button.targetGraphic = body;
        button.transition = Selectable.Transition.ColorTint;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.84f, 0.84f, 0.84f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.05f;
        button.colors = colors;
        button.onClick.AddListener(OnCoinClicked);

        coinRoot.gameObject.SetActive(false);
    }

    private void BuildStatusLabel()
    {
        statusLabel = UiKit.CreateText("ClickBonus", gameScreen, string.Empty, 58f, BonusColor,
            TextAlignmentOptions.Center, FontStyles.Bold);
        UiKit.SetAnchored(statusLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -390f), new Vector2(760f, 90f));
        statusLabel.gameObject.SetActive(false);
    }

    private void AnimateCoin()
    {
        float progress = (Time.unscaledTime - coinStartTime) / TravelTime;
        if (progress >= 1f)
        {
            MissCoin();
            return;
        }

        float halfWidth = canvas.rect.width * 0.5f + 220f;
        float bob = Mathf.Sin(progress * Mathf.PI * 3f) * 70f;
        coinRoot.anchoredPosition = new Vector2(Mathf.Lerp(-halfWidth, halfWidth, progress), coinBaseY + bob);

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.06f;
        coinRoot.localScale = new Vector3(pulse, pulse, 1f);
    }

    private void SpawnCoin()
    {
        coinBaseY = Random.Range(-220f, 330f);
        coinStartTime = Time.unscaledTime;
        coinActive = true;

        coinRoot.anchoredPosition = new Vector2(-(canvas.rect.width * 0.5f + 220f), coinBaseY);
        coinRoot.localScale = Vector3.one;
        coinRoot.gameObject.SetActive(true);
    }

    private void OnCoinClicked()
    {
        if (!coinActive)
            return;

        HideCoin();
        bonusLeft = BonusDuration;
        messageLeft = 0f;
        spawnTimer = Random.Range(MinDelay, MaxDelay);
    }

    private void MissCoin()
    {
        HideCoin();
        spawnTimer = Random.Range(MinDelay, MaxDelay);

        PlayerController player = PlayerController.Instance;
        if (player == null)
            return;

        ulong penalty = player.money / 4;      // ровно 25% от баланса
        if (penalty == 0)
            return;

        player.SpendMoney(penalty);
        messageLeft = 2.5f;
    }

    private void HideCoin()
    {
        coinActive = false;

        if (coinRoot != null)
            coinRoot.gameObject.SetActive(false);
    }

    private void UpdateStatus()
    {
        if (bonusLeft > 0f)
            bonusLeft = Mathf.Max(0f, bonusLeft - Time.unscaledDeltaTime);

        if (messageLeft > 0f)
            messageLeft = Mathf.Max(0f, messageLeft - Time.unscaledDeltaTime);

        string text = null;
        Color color = BonusColor;

        if (bonusLeft > 0f)
        {
            text = $"2x clicks · {Mathf.CeilToInt(bonusLeft)}s";
        }
        else if (messageLeft > 0f)
        {
            text = "-25% balance";
            color = PenaltyColor;
        }

        if (text == null)
        {
            if (statusText == null)
                return;

            statusText = null;
            statusLabel.gameObject.SetActive(false);
            return;
        }

        if (text == statusText)
            return;

        statusText = text;
        statusLabel.SetText(text);
        statusLabel.color = color;
        statusLabel.gameObject.SetActive(true);
    }

    /// <summary>Пока открыт любой магазин, ивент не запускаем.</summary>
    private bool AnyScreenOpen()
    {
        ShopController shop = ShopController.Instance;
        if (shop != null && shop.IsOpen)
            return true;

        BackgroundsController backgrounds = BackgroundsController.Instance;
        if (backgrounds != null && backgrounds.IsShopOpen)
            return true;

        AchievementsController achievements = AchievementsController.Instance;
        if (achievements != null && achievements.IsOpen)
            return true;

        return false;
    }
}
