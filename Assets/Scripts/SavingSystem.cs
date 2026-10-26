using System;
using UnityEngine;

public class SavingSystem : Singleton<SavingSystem>
{
    void Awake()
    {
        //Reset();
    }
    public void Reset()
    {
        SaveString(SaveNames.upgrade1, "0");
        SaveString(SaveNames.balance, "0");
        SaveString(SaveNames.upgrade1Price, "0");
        ResetBackgrounds();
        PlayerPrefs.DeleteKey(SaveNames.passiveIncomeLevel);
        PlayerPrefs.Save();
    }

    private void ResetBackgrounds()
    {
        PlayerPrefs.DeleteKey(SaveNames.backgroundCurrent);

        foreach (BackgroundDefinition definition in BackgroundsCatalog.All)
            PlayerPrefs.DeleteKey(SaveNames.backgroundOwned + definition.id);

        PlayerPrefs.Save();
    }
    public UInt64 getMoney()
    {
        if (PlayerPrefs.HasKey(SaveNames.balance))
        {
            string savedString = PlayerPrefs.GetString(SaveNames.balance);
            
            // Безопасно парсим строку обратно в ulong
            if (ulong.TryParse(savedString, out ulong result))
            {
                return result;
            }
        }
        return 0;
    }

    public string getUpgrade(string upgradeName)
    {
        if (PlayerPrefs.HasKey(upgradeName))
        {
            return PlayerPrefs.GetString(upgradeName);
        }
        return "";
    }

    public void SaveString(string stringName, string value)
    {
        PlayerPrefs.SetString(stringName, value);
        PlayerPrefs.Save();
    }
}
