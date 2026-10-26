using UnityEngine;

/// <summary>
/// Пассивный доход: сначала покупаем саму возможность, потом улучшаем уровень.
/// Доход = уровень² в секунду, цена каждого следующего уровня растёт вдвое.
/// </summary>
public class PassiveIncomeController : Singleton<PassiveIncomeController>
{
    public const ulong UnlockPrice = 200;
    public const int MaxLevel = 30;

    private int _level;
    private double _pending;

    /// <summary>0 — ещё не куплено.</summary>
    public int Level => _level;
    public bool IsUnlocked => _level > 0;
    public bool IsMaxed => _level >= MaxLevel;

    /// <summary>Сколько капает в секунду.</summary>
    public ulong IncomePerSecond => (ulong)_level * (ulong)_level;

    /// <summary>Сколько стоит покупка / следующее улучшение.</summary>
    public ulong NextPrice => IsMaxed ? 0UL : (_level == 0 ? UnlockPrice : UnlockPrice * (1UL << _level));

    protected override void Awake()
    {
        _level = Mathf.Clamp(PlayerPrefs.GetInt(SaveNames.passiveIncomeLevel, 0), 0, MaxLevel);
    }

    private void Update()
    {
        if (_level <= 0 || PlayerController.Instance == null)
            return;

        _pending += IncomePerSecond * Time.deltaTime;
        if (_pending < 1.0)
            return;

        ulong whole = (ulong)_pending;
        _pending -= whole;
        PlayerController.Instance.AddMoney(whole);
    }

    /// <summary>Купить доход или поднять его уровень. false — не хватило денег.</summary>
    public bool TryBuyOrUpgrade()
    {
        if (IsMaxed)
            return false;

        PlayerController player = PlayerController.Instance;
        if (player == null || !player.SpendMoney(NextPrice))
            return false;

        _level++;
        PlayerPrefs.SetInt(SaveNames.passiveIncomeLevel, _level);
        PlayerPrefs.Save();
        return true;
    }
}
