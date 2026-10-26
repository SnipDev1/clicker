/// <summary>Ачивка за баланс на счёте.</summary>
public class AchievementDefinition
{
    public string id;
    public string title;
    public string badge;
    public ulong requirement;
}

public static class AchievementsCatalog
{
    public static readonly AchievementDefinition[] All =
    {
        new AchievementDefinition
        {
            id = "k1", title = "first thousand", badge = "1k", requirement = 1000
        },
        new AchievementDefinition
        {
            id = "k5", title = "five thousand", badge = "5k", requirement = 5000
        },
        new AchievementDefinition
        {
            id = "k10", title = "ten thousand", badge = "10k", requirement = 10000
        },
        new AchievementDefinition
        {
            id = "k25", title = "twenty five thousand", badge = "25k", requirement = 25000
        }
    };
}
