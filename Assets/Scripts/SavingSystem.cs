using System;
using UnityEngine;

public class SavingSystem : Singleton<SavingSystem>
{

    public UInt64 getMoney()
    {
        if (PlayerPrefs.HasKey(SaveNames.balance))
        {
            string savedString = PlayerPrefs.GetString(SaveNames.balance);
            
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
        PlayerPrefs.SetString(SaveNames.balance, money.ToString());
        PlayerPrefs.Save();
        Debug.Log(money.ToString());
    }

    public void SaveString(string stringName, string value)
    {
        PlayerPrefs.SetString(stringName, value);
        PlayerPrefs.Save();
    }
}
