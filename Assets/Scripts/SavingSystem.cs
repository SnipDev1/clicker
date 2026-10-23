using System;
using Unity.VisualScripting;
using UnityEngine;

public class SavingSystem : Singleton<SavingSystem>
{
    private string saveMoneyKey = "Balance";

    public UInt64 getMoney()
    {
        if (PlayerPrefs.HasKey(saveMoneyKey))
        {
            string savedString = PlayerPrefs.GetString(saveMoneyKey);
            
            // Безопасно парсим строку обратно в ulong
            if (ulong.TryParse(savedString, out ulong result))
            {
                Debug.Log($"Успешно загружено: {result}");
                return result;
            }
        }
        return 0;
    }

    public string getUpgrade(string upgradeName)
    {
        if (PlayerPrefs.HasKey(upgradeName))
        {
            string savedString = PlayerPrefs.GetString(upgradeName);
            Debug.Log(savedString);
            return savedString;
        
        }
        return "";
    }

    public void setMoney(UInt64 money)
    {
        PlayerPrefs.SetString(saveMoneyKey, money.ToString());
        PlayerPrefs.Save();
        Debug.Log(money.ToString());
    }

    public void setUpgrade(string upgradeName, int coef)
    {
        PlayerPrefs.SetString(upgradeName, coef.ToString());
        PlayerPrefs.Save();

    }
}
