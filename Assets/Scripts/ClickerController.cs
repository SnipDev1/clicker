using System;
using UnityEngine;

public class ClickerController : Singleton<ClickerController>
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
            SavingSystem.Instance.setMoney(_money);
        }
    }

    protected override void Awake()
    {
        gameSceneController = GameSceneController.Instance;

        money = SavingSystem.Instance.getMoney(); 
        gameSceneController.setMoney(money);

        // if(money == null)
        //     money = 0;
    }

    public void addMoney()
    {
        Debug.Log("Added");
        money += UpgradesController.Instance.ProcessMoney(1);
    }
}
