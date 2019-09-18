using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePatterner : ScriptableObject
{
    [Header("Пресеты блоков (таблица 1)")]
    [SerializeField] public BlockPreset[] blockPresets;
    [Header("Появление блоков (таблица 2)")]
    [SerializeField] public BlockPossibility[] blockPossibilities;
    [Header("Появление монет (таблица 3)")]
    [SerializeField] public CoinPossibility[] coinPossibilities;
    [Header("данные боссов для пещеры")]
    [SerializeField] public BossInfo bossesData;
    [Header("данные монеток-кристалов")]
    [SerializeField] public GemCoinAdditionalInfo[] gemsData;
    [Header("количество пыл за монетки")]
    [SerializeField] public MagicDustCoinAdditionalInfo[] dustCoinsData;
    [Header("сколько шаров дает монетка-шар на каждом этапа пещеры")]
    [SerializeField] public BallCoinAdditionalCaveInfo[] ballCoinData;
    [Header("сколько шаров отнимает негативная монетка-шар на каждом этапе пещеры")]
    [SerializeField] public RemoveBallCoinAdditionalCaveInfo[] removeBallCoinData;
    [Header("не ставить")]
    [SerializeField] public CoinIntervalStatistics coinStats;
    [SerializeField] public BlockAppearenceStatistics blockStats;

    #region Helper Methods


    public int GetGemsAmountForLevel(int levelNumber)
    {
        if (gemsData == null || gemsData.Length == 0) return 0;

        for(int i = gemsData.Length - 1; i > 0; i--)
            if (gemsData[i].intervalFloor <= levelNumber)
                return gemsData[i].gemsAmount;

        return 0;
    }

    public int GetMagicDustAmountForDustCoinOnLevel(int levelNumber)
    {
        if (dustCoinsData == null || dustCoinsData.Length == 0) return 0;

        for (int i = dustCoinsData.Length - 1; i > 0; i--)
            if (dustCoinsData[i].lowerLevel <= levelNumber)
                return dustCoinsData[i].dustAmount;

        return 0;
    }

    public int GetBallsIncreaserCountForBallCoinOnLevel(int levelNumber)
    {
        if (ballCoinData == null || ballCoinData.Length == 0) return 1;

        for (int i = ballCoinData.Length - 1; i > 0; i--)
            if (ballCoinData[i].intervalFloor <= levelNumber)
                return ballCoinData[i].addBallAmount;
        return 1;
    }

    public int GetRemoveBallsIncreaserCountForBallCoinOnLevel(int levelNumber)
    {
        if (removeBallCoinData == null || removeBallCoinData.Length == 0) return 1;

        for (int i = removeBallCoinData.Length - 1; i > 0; i--)
            if (removeBallCoinData[i].intervalFloor <= levelNumber)
                return removeBallCoinData[i].removeBallAmount;
        return 1;
    }

    #endregion

}

#region BlockPresets

[System.Serializable]
public class RemoveBallCoinAdditionalCaveInfo
{
    public int intervalFloor;
    public int removeBallAmount;
}

[System.Serializable]
public class BallCoinAdditionalCaveInfo
{
    public int intervalFloor;
    public int addBallAmount;
}

[System.Serializable]
public class MagicDustCoinAdditionalInfo
{
    public int lowerLevel;
    public int dustAmount;
}

[System.Serializable]
public class GemCoinAdditionalInfo
{
    public int intervalFloor;
    public int gemsAmount;
}

[System.Serializable]
public class BossInfo
{
    public int starterHP;
    public int deltaHP;
}

[System.Serializable]
public class BlockPreset
{
    public int startLevel;
    public int endLevel;
    public BlockPresetItem[] presets;
}

[System.Serializable]
public class BlockPresetItem
{
    public float relativePossibility;
    public BlockLifeCounter[] preset;
}

public enum BlockLifeCounter
{
    Single,
    Double,
    Random
}

#endregion

#region Block Possibility

[System.Serializable]
public class BlockPossibility
{
    public int startLevel;
    public int endLevel;
    public BlockPossibilityItem[] blocks;
}

[System.Serializable]
public class BlockPossibilityItem
{
    public GameSave.ItemType blockType;
    public int maxAmountOnLine;
    public float relativePossibility;
    public int maxAmountInInterval;
    public int intervalLength;
}

#endregion

#region Coin Possibility

[System.Serializable]
public class CoinPossibility
{
    public int startLevel;
    public int endLevel;
    public CoinPossibilityItem[] coins;

}

[System.Serializable]
public class CoinPossibilityItem
{
    public GameSave.ItemType coinType;
    public float relativePossibility;
    public int maxAmountInInterval;
}

#endregion

#region Stats

[System.Serializable]
public class CoinIntervalStatistics
{
    public CoinStatisticsItem[] coins;
}

[System.Serializable]
public class CoinStatisticsItem
{
    public GameSave.ItemType coinType;
    public int timesCreatedInInterval;
}

[System.Serializable]
public class BlockAppearenceStatistics
{
    public BlockAppearanceItem[] blocks;
}

[System.Serializable]
public class BlockAppearanceItem
{
    public GameSave.ItemType blockType;
    public int createdOnLine;
    public List<int> levelsCreated;
}

#endregion

