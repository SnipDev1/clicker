using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Магазин фонов: хранит, что куплено, какой фон выбран, и умеет всё это сохранять.
/// </summary>
public class BackgroundsController : Singleton<BackgroundsController>
{
    [SerializeField] private Image backgroundImage;     // картинка фона на игровом экране
    [SerializeField] private RectTransform gameScreen;  // сюда добавим кнопку входа в магазин

    /// <summary>Что-то изменилось: купили фон или выбрали другой.</summary>
    public event Action StateChanged;

    private readonly HashSet<string> ownedIds = new HashSet<string>();
    private BackgroundShopView shopView;
    private Button entryButton;

    public string CurrentId { get; private set; }
    public BackgroundDefinition Current => BackgroundsCatalog.Get(CurrentId);

    /// <summary>Открыт ли экран магазина фонов.</summary>
    public bool IsShopOpen => shopView != null && shopView.IsVisible;

    protected override void Awake()
    {
        // base.Awake() не вызываем специально: контроллер не должен уезжать в DontDestroyOnLoad
        CustomBackgrounds.ReloadAll();
        LoadState();
        ApplyCurrent();
    }

    private void Start()
    {
        CreateEntryButton();
    }

    private void Update()
    {
        if (shopView != null)
            shopView.Tick();
    }

    public bool IsOwned(string id)
    {
        BackgroundDefinition definition = BackgroundsCatalog.Get(id);
        if (definition != null && definition.ownedByDefault)
            return true;

        return !string.IsNullOrEmpty(id) && ownedIds.Contains(id);
    }

    public void OpenShop()
    {
        if (shopView == null)
        {
            Transform canvas = gameScreen != null ? gameScreen.parent : null;
            if (canvas == null && backgroundImage != null && backgroundImage.canvas != null)
                canvas = backgroundImage.canvas.transform;

            if (canvas == null)
            {
                Debug.LogError("BackgroundsController: не нашёл Canvas для магазина фонов");
                return;
            }

            shopView = new BackgroundShopView(this, canvas);
        }

        shopView.Show();
    }

    public void CloseShop()
    {
        if (shopView != null)
            shopView.Hide();
    }

    /// <summary>Купить фон и сразу его надеть. Возвращает false, если денег не хватило.</summary>
    public bool TryBuy(BackgroundDefinition definition)
    {
        if (definition == null || IsOwned(definition.id))
            return false;

        PlayerController player = PlayerController.Instance;
        if (player == null || !player.SpendMoney(definition.price))
            return false;

        ownedIds.Add(definition.id);
        PlayerPrefs.SetInt(SaveNames.backgroundOwned + definition.id, 1);
        Equip(definition.id);
        return true;
    }

    /// <summary>Надеть уже купленный фон.</summary>
    public bool Select(string id)
    {
        if (!IsOwned(id))
            return false;

        Equip(id);
        return true;
    }

    private void Equip(string id)
    {
        CurrentId = id;
        PlayerPrefs.SetString(SaveNames.backgroundCurrent, id);
        PlayerPrefs.Save();

        ApplyCurrent();
        StateChanged?.Invoke();
    }

    /// <summary>Удалить свою картинку. Если она была выбрана — возвращаемся на классику.</summary>
    public bool DeleteCustom(BackgroundDefinition definition)
    {
        if (definition == null || definition.style != BackgroundStyle.Custom)
            return false;

        BackgroundsCatalog.Remove(definition.id);
        ownedIds.Remove(definition.id);

        if (CurrentId == definition.id)
        {
            CurrentId = BackgroundsCatalog.DefaultId;
            PlayerPrefs.SetString(SaveNames.backgroundCurrent, CurrentId);
            PlayerPrefs.Save();
            ApplyCurrent();
        }

        CustomBackgrounds.Delete(definition);

        StateChanged?.Invoke();
        return true;
    }

    private void ApplyCurrent()
    {
        if (backgroundImage == null)
            return;

        BackgroundDefinition definition = Current ?? BackgroundsCatalog.Default;
        Sprite sprite = definition != null ? BackgroundTextureFactory.GetSprite(definition) : null;

        if (sprite == null)
        {
            // страховка: спрайт мог не пережить перезапуск (Domain Reload выключен)
            definition = BackgroundsCatalog.Default;
            sprite = definition != null ? BackgroundTextureFactory.GetSprite(definition) : null;

            if (sprite != null)
                CurrentId = BackgroundsCatalog.DefaultId;
        }

        if (definition == null || sprite == null)
            return;

        backgroundImage.sprite = sprite;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.preserveAspect = false;
        backgroundImage.color = Color.white;

        // стекло берёт этот фон как источник размытия
        Shader.SetGlobalTexture("_BackgroundTex", sprite.texture);
    }

    private void LoadState()
    {
        ownedIds.Clear();

        foreach (BackgroundDefinition definition in BackgroundsCatalog.All)
        {
            bool owned = definition.ownedByDefault ||
                         PlayerPrefs.GetInt(SaveNames.backgroundOwned + definition.id, 0) == 1;
            if (owned)
                ownedIds.Add(definition.id);
        }

        string saved = PlayerPrefs.GetString(SaveNames.backgroundCurrent, BackgroundsCatalog.DefaultId);
        if (BackgroundsCatalog.Get(saved) == null || !IsOwned(saved))
            saved = BackgroundsCatalog.DefaultId;

        CurrentId = saved;
    }

    private void CreateEntryButton()
    {
        if (entryButton != null || gameScreen == null)
            return;

        entryButton = UiKit.CreateButton("BackgroundsButton", gameScreen, "backgrounds", 72f);
        UiKit.SetAnchored((RectTransform)entryButton.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 320f), new Vector2(605f, 200f));
        entryButton.onClick.AddListener(OpenShop);
    }
}
