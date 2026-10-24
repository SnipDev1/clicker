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
        set 
        { 
            if (value < 0)
                return;
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

}
