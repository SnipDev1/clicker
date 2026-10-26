using System;
using UnityEngine;

public class UpgradesController : Singleton<UpgradesController>
{
    public UInt64 clickIncrease = 0;

    protected override void Awake()
    {
        UpdateUpgrades();
    }
    public void UpdateUpgrades()
    {
        ulong.TryParse(SavingSystem.Instance.getUpgrade(SaveNames.upgrade1), out ulong result);
        clickIncrease = result;
    }

    public UInt64 ProcessMoney(UInt64 money)
    {
        return money + clickIncrease;
    }

}
