using System;
using UnityEngine;

public class ClickerController : Singleton<ClickerController>
{
    
    private PlayerController playerController;

    void Awake()
    {
        playerController = PlayerController.Instance;
    }
    public void addMoney()
    {
        Debug.Log("Added");
        playerController.money += UpgradesController.Instance.ProcessMoney(1);
    }
}
