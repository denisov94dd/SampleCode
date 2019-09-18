using System.Collections.Generic;
using UnityEngine;

public static class DragonManager
{
    #region vars

    const int dragonsCount = 9;

    public static List<Dragon> AllDragons
    {
        get
        {
            if (allDragons == null)
            {
                allDragons = new List<Dragon>();
                for (int i = 0; i < dragonsCount; i++)
                    allDragons.Add(Resources.Load("Dragons/" + ((DragonType)i).ToString()) as Dragon);
            }

            return allDragons;
        }
    }

    private static List<Dragon> allDragons;

    #endregion

    #region SetDragonProps

    /// <summary>Открывает нового дракона по типу и сохраняет</summary>
    public static void OpenNewDragon(DragonType newDragonType)
    {
        GameSave.SavedData.openedDragons.Add(new DragonSaveView(newDragonType));
        GameSave.Save(GameSave.SavedData);
    }

    #endregion

    #region GetDragonInfo

    /// <summary>Получить текущего выбранного дракона</summary>
    public static Dragon GetSelectedDragon()
    {
        return AllDragons[(int)GameSave.SavedData.selectedDragon];
    }

    /// <summary>Получить дракона по типу</summary>
    public static Dragon GetDragonByType(DragonType dragonType)
    {
        return AllDragons[(int)dragonType];
    }

    /// <summary>Открыт ли дракон</summary>
    public static bool IsDragonUnlocked(Dragon dragon)
    {
        foreach (DragonSaveView dragonSave in GameSave.SavedData.openedDragons)
            if (dragon.dragonType == dragonSave.dragonType) return true;

        return false;
    }

    public static bool IsDragonUnlocked(DragonType dragonType)
    {
        foreach (DragonSaveView dragonSave in GameSave.SavedData.openedDragons)
            if (dragonType == dragonSave.dragonType) return true;

        return false;
    }

    public static DragonSaveView GetDragonSaveByDragonType(DragonType dragonType)
    {
        foreach (DragonSaveView dragonSave in GameSave.SavedData.openedDragons)
            if (dragonType == dragonSave.dragonType) return dragonSave;

        return null;
    }

    public static void SetDragonName(DragonType dragonType, string dragonName)
    {
        GetDragonSaveByDragonType(dragonType).dragonName = dragonName;
        GameSave.Save(GameSave.SavedData);
    }

    public static string GetDragonName(DragonType dragonType)
    {
        DragonSaveView dSave = GetDragonSaveByDragonType(dragonType);
        if (dSave != null) return GetDragonSaveByDragonType(dragonType).dragonName;

        return string.Empty;
    }

    public static int UnlockedDragonsCount()
    {
        int current = 0;
        foreach (var drag in AllDragons)
            if (IsDragonUnlocked(drag)) current++;

        return current;
    }

    public static GameObject GetBallPrefabByDragonType(DragonType dType)
    {
        return GetDragonByType(dType).ballPrefab;
    }

    #endregion

}
