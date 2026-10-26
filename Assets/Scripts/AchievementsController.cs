using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ачивки за баланс: как только на счёте побывало N денег, ачивка открывается навсегда.
/// </summary>
public class AchievementsController : Singleton<AchievementsController>
{
    [SerializeField] private RectTransform gameScreen;

    /// <summary>Открылась новая ачивка.</summary>
    public event Action StateChanged;

    private readonly List<bool> unlocked = new List<bool>();
    private AchievementView view;
    private Button entryButton;
    private PlayerController player;
    private ulong maxSeen;
    private int unlockedCount;

    public int TotalCount => AchievementsCatalog.All.Length;
    public int UnlockedCount => unlockedCount;

    /// <summary>Открыт ли экран ачивок.</summary>
    public bool IsOpen => view != null && view.IsVisible;

    protected override void Awake()
    {
        for (int i = 0; i < AchievementsCatalog.All.Length; i++)
        {
            string key = SaveNames.achievement + AchievementsCatalog.All[i].id;
            unlocked.Add(PlayerPrefs.GetInt(key, 0) == 1);
        }

        unlockedCount = CountUnlocked();
    }

    private void Start()
    {
        player = PlayerController.Instance;

        if (player != null && player.money > maxSeen)
            maxSeen = player.money;

        CreateEntryButton();
    }

    private void Update()
    {
        if (player == null)
            return;

        if (player.money > maxSeen)
            maxSeen = player.money;

        if (unlockedCount < TotalCount)
            TryUnlock();
    }

    public bool IsUnlocked(int index)
    {
        return index >= 0 && index < unlocked.Count && unlocked[index];
    }

    public void Open()
    {
        if (view == null)
        {
            Transform canvas = gameScreen != null ? gameScreen.parent : null;
            if (canvas == null)
            {
                Debug.LogError("AchievementsController: не нашёл Canvas");
                return;
            }

            view = new AchievementView(this, canvas);
        }

        view.Show();
    }

    private void TryUnlock()
    {
        bool changed = false;

        for (int i = 0; i < AchievementsCatalog.All.Length; i++)
        {
            if (unlocked[i])
                continue;

            AchievementDefinition definition = AchievementsCatalog.All[i];
            if (maxSeen < definition.requirement)
                continue;

            unlocked[i] = true;
            PlayerPrefs.SetInt(SaveNames.achievement + definition.id, 1);
            changed = true;
        }

        if (!changed)
            return;

        PlayerPrefs.Save();
        unlockedCount = CountUnlocked();
        StateChanged?.Invoke();
    }

    private int CountUnlocked()
    {
        int count = 0;
        for (int i = 0; i < unlocked.Count; i++)
        {
            if (unlocked[i])
                count++;
        }
        return count;
    }

    private void CreateEntryButton()
    {
        if (entryButton != null || gameScreen == null)
            return;

        entryButton = UiKit.CreateButton("AchievementsButton", gameScreen, "achievements", 72f);
        UiKit.SetAnchored((RectTransform)entryButton.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 540f), new Vector2(605f, 200f));
        entryButton.onClick.AddListener(Open);
    }
}
