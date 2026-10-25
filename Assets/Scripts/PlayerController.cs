using System;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    private UInt64 _money = 0;
    private GameSceneController gameSceneController;

    public UInt64 money
    {
        get 
        { 
            return _money; 
        }
        private set 
        { 
            Debug.Log(value);
            _money = value;
            gameSceneController.setMoney(_money);
            SavingSystem.Instance.SaveString(SaveNames.balance, _money.ToString());
        }
    }

    void Awake()
    {
        gameSceneController = GameSceneController.Instance;
        money = SavingSystem.Instance.getMoney(); 
        gameSceneController.setMoney(money);
    }
    
    public bool SpendMoney(UInt64 amount)
    {
        if (money - amount > money)
        {
            return false;
        }
        money -= amount;
        return true;
    }
    public bool AddMoney(UInt64 amount)
    {
        money += amount;
        return true;
    }

}
