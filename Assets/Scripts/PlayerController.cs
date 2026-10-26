using System;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    // пассивный доход капает постоянно, поэтому баланс пишем на диск не чаще раза в несколько секунд
    private const float SaveInterval = 5f;

    private UInt64 _money = 0;
    private GameSceneController gameSceneController;
    private bool _balanceDirty;
    private float _saveTimer;

    public UInt64 money
    {
        get
        {
            return _money;
        }
        private set
        {
            if (_money == value)
                return;

            _money = value;
            gameSceneController.setMoney(_money);
            _balanceDirty = true;
        }
    }

    void Awake()
    {
        gameSceneController = GameSceneController.Instance;
        money = SavingSystem.Instance.getMoney();
        gameSceneController.setMoney(money);
    }

    void Update()
    {
        if (!_balanceDirty)
            return;

        _saveTimer -= Time.unscaledDeltaTime;
        if (_saveTimer > 0f)
            return;

        SaveBalance();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused)
            SaveBalance();
    }

    void OnApplicationQuit()
    {
        SaveBalance();
    }

    private void SaveBalance()
    {
        if (!_balanceDirty)
            return;

        _balanceDirty = false;
        _saveTimer = SaveInterval;
        SavingSystem.Instance.SaveString(SaveNames.balance, _money.ToString());
    }

    public bool SpendMoney(UInt64 amount)
    {
        if (amount > money)
        {
            return false;
        }
        money -= amount;
        SaveBalance();
        return true;
    }
    public bool AddMoney(UInt64 amount)
    {
        if (amount == 0)
            return false;

        money += amount;
        return true;
    }

}
