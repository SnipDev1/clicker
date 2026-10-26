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
        ulong amount = UpgradesController.Instance.ProcessMoney(1);

        // бонус из ивента с монеткой
        CoinEventController coinEvent = CoinEventController.Instance;
        if (coinEvent != null && coinEvent.IsClickBonusActive)
            amount *= 2;

        playerController.AddMoney(amount);
    }
}
