using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GameSave
{
    #region Saved Data

    public static GameSave SavedData
    {
        get
        {
            if (savedData == null)
                savedData = Load();

            return savedData;
        }
        set
        {
            savedData = value;
        }
    }

    private static GameSave savedData;

    static string SaveName
    {
        get
        {
            if (MetaGameController.instance == null) return keyName;
            int saveNum = MetaGameController.instance.saveNumber;
#if !UNITY_EDITOR
            saveNum = 0;
#endif
            return saveNum == 0 ? keyName : string.Concat(keyName, saveNum.ToString());
        }
    }

    public const string keyName = "PlayerSave";

    #endregion

    #region Props

    /// <summary>лучший результат за все время</summary>
    public int bestLevel;
    /// <summary>лучший результат за все время</summary>
    public int bestDayLevel;
    /// <summary>текущий день</summary>
    public int day;
    /// <summary>результат за прошлый раз</summary>
    public int deathLevel;
    /// <summary>текущее количество маны в игре</summary>
    public int starCount;
    /// <summary>сколько есть бесплатных бустеров</summary>
    public int deleteBoosterFreeCount;
    /// <summary>сколько есть бесплатных бустеров</summary>
    public int doubleBoosterFreeCount;
    /// <summary>текущий уровень в игре</summary>
    public int turnCount;
    /// <summary>текущее кол-во мячей в игре</summary>
    public int ballCount;
    /// <summary>использовал ли игрок бустеры в игре</summary>
    public bool boostersUsed;
    public GameItem[] itemList;
    public int metaStarsCount;
    public int ticketsCount;

    /// <summary>список открытых драконов</summary>
    public List<DragonSaveView> openedDragons;
    /// <summary>текущий дракон</summary>
    public DragonType selectedDragon;
    public int campaignLevelsCompleted;
    public int lastCollectedStarsWaveNumber;
    public int additionalCampaignLevelsCompleted;
    public bool tutorialCompleted;

    public Achievements achievements;
    public MagicalCaveProgress caveProgress;
    public LocalizationLanguage gameLanguage;
    public bool adsRemoved;

    public List<MetaBuilderQuestSave> completedQuests;
    public List<MetaBuilderFxSave> savedEffects;
    public List<MetaBuilderCollectionSave> savedCollections;
    public List<int> avaliableEffects;
    public List<int> avaliableCollections;
    public List<MetaBuilderQuestsPrizeProgressSave> colletedMetaQuestPrizes;

    public int currentEffectPrizeNumber;
    public int currentCollectionPrizeNumber;
    public int currentFortuneRingRotationNumber;

    public List<ItemType> shownItems;

    /// <summary>было ли показано игроку окно с предложением поставить игре оценку</summary>
    public bool PlayerRatedTheGame;
    public long lastTimeRateWindowShowed;

    public bool playerUnlockedPremiumPet;

    #endregion

    #region Sound And Music

    public bool soundIsOn;
    public bool musicIsOn;

    #endregion

    #region Meta Data

    public long firstLaunchTime;
    public long lastTimeSixHoursCollected;
    public bool bonusLevelTipShowed;
    public long lastTimeDailyBonusCollected;
    public int dailyBonusGiftNumber;
    public bool fortuneRingFreeRotationUsed;
    public int nextSixHoursPrizeAmount;
    public bool caveUIHighlighted;
    public long lastTimeBonusLevelCompleted;
    public int timesAdditionalPlayed;
    public int timesCavePlayed;
    public long lastTimeGatchaFreeSpin;
    public int playerBannerAdsCohort;

    #endregion

    #region Notification

    public List<NotificationInformation> notifications;
    public long lastNotificationTimeAppeared;

    #endregion

    #region Modifiers

    public List<Modifier> AppliedModifiers;

    #endregion

    #region Meta Tutorial

    public bool HavePlayerCompletedMetaTutorial(MetaTutorialType mType)
    {
        return completedMetaTutorials.Contains(mType);
    }

    public void CompleteMetaTutorial(MetaTutorialType mType)
    {
        if (!completedMetaTutorials.Contains(mType))
        {
            completedMetaTutorials.Add(mType);
            Save(this);
        }
    }

    public List<MetaTutorialType> completedMetaTutorials;
    public List<LittleTutorialType> littleTutorialsPassed;

    public enum MetaTutorialType
    {
        BuildingTutorial,
        ChangeAppearanceTutorial,
        CaveFirstTutorial,
        CavePrizeTutorial,
        FortuneRingTutorial,
        PetTutorial,
        EffectsTutorial,
        SixHourBonusTutorial,
        BonusLevelsTutorial,
        DragonWindowTutorial,
        PremiumBuildingTutorial,
        PremiumPetTutorial,
        PremiumPetFirstDialogMapTutorial,
        PremiumPetSecondDialogMapTutorial,
        RateAppWindowFirstTime,
        EggPrizeQuestTutotial,
        Level3ShowQuestsMenuTutorial
    }

    public List<DragonBonusType> usedDragons;

    #endregion

    #region statistics

    /// <summary>сколько потратили маны за всю игру</summary>
    public int starsSpent;
    /// <summary>максимальное число ударов на мяч за всю игру</summary>
    public float maxReboundPerBall;
    /// <summary>сколько раз зачистили все поле</summary>
    public int allClearTimes;

    #endregion

    #region Save-Load

    public static GameSave Load()
    {
        var jsonStr = PlayerPrefs.GetString(SaveName, string.Empty);
        var result = JsonUtility.FromJson<GameSave>(jsonStr);

        if (null == result)
        {
            result = GetEmpty();
        }
        if (result.itemList == null) result.itemList = new GameItem[] { };

        savedData = result;
        return result;
    }

    public static GameSave GetEmpty()
    {
        GameSave save = new GameSave()
        {
            bestLevel = 1,
            day = 0,
            bestDayLevel = 1,
            deathLevel = 1,
            starCount = 0,
            metaStarsCount = 0,
            ticketsCount = 0,
            deleteBoosterFreeCount = 1,
            doubleBoosterFreeCount = 1,
            turnCount = 1,
            ballCount = 1,
            campaignLevelsCompleted = 0,
            additionalCampaignLevelsCompleted = 0,
            boostersUsed = false,
            tutorialCompleted = false,
            completedMetaTutorials = new List<MetaTutorialType>(),
            achievements = new Achievements(),
            caveProgress = new MagicalCaveProgress(),
            openedDragons = new List<DragonSaveView>(),
            completedQuests = new List<MetaBuilderQuestSave>(),
            savedEffects = new List<MetaBuilderFxSave>(),
            savedCollections = new List<MetaBuilderCollectionSave>(),
            avaliableEffects = new List<int>(),
            avaliableCollections = new List<int>(),
            colletedMetaQuestPrizes = new List<MetaBuilderQuestsPrizeProgressSave>(),
            shownItems = new List<ItemType>(),
            littleTutorialsPassed = new List<LittleTutorialType>(),
            AppliedModifiers = new List<Modifier>(),
            musicIsOn = true,
            soundIsOn = true,
            currentEffectPrizeNumber = 0,
            currentCollectionPrizeNumber = 0,
            notifications = new List<NotificationInformation>(),
            firstLaunchTime = DateTime.Now.ToFileTime(),
            playerBannerAdsCohort = 2,
            usedDragons = new List<DragonBonusType>()
        };

        save.gameLanguage = Localizator.GetSystemLanguage();

        if (MetaGameController.instance != null)
            MetaGameController.instance.achievementController.ResetData();
        save.openedDragons.Add(new DragonSaveView(DragonType.DragonOne));
        return save;
    }

    public static void SaveGameOver(GameSave saveData)
    {
        var current = Load();

        current.deathLevel = saveData.deathLevel;
        current.bestLevel = Mathf.Max(saveData.deathLevel, current.bestLevel);
        var currentDay = DateTime.Today.DayOfYear;
        if (currentDay == current.day)
            current.bestDayLevel = Mathf.Max(saveData.deathLevel, current.bestDayLevel);
        else
        {
            current.day = currentDay;
            current.bestDayLevel = saveData.deathLevel;
        }

        current.boostersUsed = false;

        current.deleteBoosterFreeCount = saveData.deleteBoosterFreeCount;
        current.doubleBoosterFreeCount = saveData.doubleBoosterFreeCount;
        current.starsSpent = saveData.starCount;

        current.turnCount = 1;
        current.ballCount = 1;
        current.itemList = new GameItem[] { };
        var jsonStr = JsonUtility.ToJson(current);
        PlayerPrefs.SetString(SaveName, jsonStr);
    }

    public static void SaveMetaGameOver(GameSave saveData)
    {
        var current = Load();

        current.deathLevel = saveData.deathLevel;
        current.bestLevel = Mathf.Max(saveData.deathLevel, current.bestLevel);
        var currentDay = DateTime.Today.DayOfYear;
        if (currentDay == current.day)
            current.bestDayLevel = Mathf.Max(saveData.deathLevel, current.bestDayLevel);
        else
        {
            current.day = currentDay;
            current.bestDayLevel = saveData.deathLevel;
        }

        current.boostersUsed = false;

        current.deleteBoosterFreeCount = saveData.deleteBoosterFreeCount;
        current.doubleBoosterFreeCount = saveData.doubleBoosterFreeCount;
        current.starsSpent = saveData.starCount;

        var jsonStr = JsonUtility.ToJson(current);
        PlayerPrefs.SetString(SaveName, jsonStr);
    }

    public static void Save(GameController game)
    {
        var current = SavedData;

        current.starCount = game.data.starCount;
        current.metaStarsCount = game.data.metaStarsCount;
        current.ticketsCount = game.data.ticketsCount;
        current.turnCount = game.data.turnCount;
        current.ballCount = game.data.ballCount;
        current.itemList = game.cubeManager.Save().ToArray();
        current.selectedDragon = game.data.selectedDragon;
        current.campaignLevelsCompleted = game.data.campaignLevelsCompleted;
        current.lastCollectedStarsWaveNumber = game.data.lastCollectedStarsWaveNumber;
        current.additionalCampaignLevelsCompleted = game.data.additionalCampaignLevelsCompleted;
        current.tutorialCompleted = game.data.tutorialCompleted;
        current.colletedMetaQuestPrizes = game.data.colletedMetaQuestPrizes;
        current.deleteBoosterFreeCount = game.data.deleteBoosterFreeCount;
        current.doubleBoosterFreeCount = game.data.doubleBoosterFreeCount;
        current.starsSpent = game.data.starsSpent;
        current.boostersUsed = game.data.boostersUsed;
        current.maxReboundPerBall = game.data.maxReboundPerBall;
        current.allClearTimes = game.data.allClearTimes;

        current.achievements = game.achievementController.currData;
        current.caveProgress = game.data.caveProgress;
        current.shownItems = game.data.shownItems;

        Save(current);
    }

    public static void Save(GameSave saveData)
    {
        var current = saveData;
        var jsonStr = JsonUtility.ToJson(current);
        savedData = current;
        PlayerPrefs.SetString(SaveName, jsonStr);
    }

    public static void SaveCurrData()
    {
        Save(SavedData);
    }

    #endregion

    #region Json Serialization

    /// <summary>преобразует текущий игровой прогресс в строку и возвращает ее</summary>
    public static string GetJson(GameController game)
    {
        var current = Load();

        current.starCount = game.data.starCount;
        current.metaStarsCount = game.data.metaStarsCount;
        current.ticketsCount = game.data.ticketsCount;
        current.turnCount = game.data.turnCount;
        current.ballCount = game.data.ballCount;
        current.deleteBoosterFreeCount = game.data.deleteBoosterFreeCount;
        current.doubleBoosterFreeCount = game.data.doubleBoosterFreeCount;
        current.itemList = game.cubeManager.Save().ToArray();
        current.campaignLevelsCompleted = game.data.campaignLevelsCompleted;
        current.lastCollectedStarsWaveNumber = game.data.lastCollectedStarsWaveNumber;
        current.additionalCampaignLevelsCompleted = game.data.additionalCampaignLevelsCompleted;
        current.tutorialCompleted = game.data.tutorialCompleted;
        current.achievements = game.data.achievements;
        current.caveProgress = game.data.caveProgress;
        current.shownItems = game.data.shownItems;

        var jsonStr = JsonUtility.ToJson(current);
        return jsonStr;
    }

    /// <summary>возвращает сейв после каста из указанной строки</summary>
    public static GameSave FromJson(string progressJson)
    {
        var result = JsonUtility.FromJson<GameSave>(progressJson);
        if (null == result)
        {
            result = GetEmpty();
            result.achievements = new Achievements();
        }

        if (result.itemList == null) result.itemList = new GameItem[] { };
        return result;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    #endregion

    #region Special Data Structs

    [Serializable]
    public enum ItemType
    {
        cube,
        boss,
        onion,
        corner,
        armoured,
        dissapearing,
        moving,
        teleport,
        ballCoin,
        starCoin,
        upForceCoin,
        diffuseBallCoin,
        dopelgangerCoin,
        jumpBallCoin,
        laserCoin,
        crossLaserCoin,
        killerCoin,
        invunerable,
        superBossWall,
        superBossShield,
        superBossExplosive,
        explosive,
        priest,
        slime,
        millCoin,
        blackHoleCoin,
        prizeBrick,
        magicDustCoin,
        removeBallCoin,
        reduceBallsCountCoin
    }

    public enum BlockOrientation
    {
        left = 3,
        right = 1,
        up = 0,
        down = 2
    }

    public enum BrickHp
    {
        singleHP,
        doubleHP,
        tripleHP,
        quadrupleHP
    }

    [Serializable]
    public struct GameItem
    {
        public ItemType itemType;
        public int level;
        public int index;
        public int shiftDelay;
        public int hitCounter;
        public BlockOrientation orientation;
        public BrickHp baseHP;
    }

    public enum itemBaseType
    {
        brick,
        coin,
        boss,
        superBoss
    }

    public static itemBaseType GetItemType(ItemType type)
    {
        switch (type)
        {
            case ItemType.cube:
            case ItemType.onion:
            case ItemType.corner:
            case ItemType.armoured:
            case ItemType.dissapearing:
            case ItemType.moving:
            case ItemType.teleport:
            case ItemType.explosive:
            case ItemType.priest:
            case ItemType.slime:
                return itemBaseType.brick;
            case ItemType.ballCoin:
            case ItemType.starCoin:
            case ItemType.laserCoin:
            case ItemType.killerCoin:
            case ItemType.upForceCoin:
            case ItemType.diffuseBallCoin:
            case ItemType.crossLaserCoin:
            case ItemType.dopelgangerCoin:
            case ItemType.jumpBallCoin:
            case ItemType.millCoin:
                return itemBaseType.coin;
            case ItemType.boss:
                return itemBaseType.boss;
            case ItemType.superBossWall:
            case ItemType.superBossShield:
            case ItemType.superBossExplosive:
                return itemBaseType.superBoss;
        }
        return itemBaseType.brick;
    }

    [Serializable]
    public struct Achievements
    {
        public int cubeKilledTimes;
        public int armouredKilledTimes;
        public int cornerKilledTimes;
        public int dissaperingKilledTimes;
        public int movingKilledTimes;
        public int onionKilledTimes;
        public int teleportKilledTimes;
        public int ballCoinTimesCollected;
        public int crossLaserCoinTimesCollected;
        public int diffuseCoinTimesCollected;
        public int dopelgangerCoinTimesCollected;
        public int jumpCoinTimesCollected;
        public int killerCoinTimesCollected;
        public int singleLaserCoinTimesCollected;
        public int starCoinTimesCollected;
        public int upForceCoinTimesCollected;
        public int bossKilledTimes;
        public int deleteBoosterTimesUsed;
        public int doubleBoosterTimesUsed;
        public int maxLevel;
        public int maxLevelWithoutBoosters;
        public int maxBalls;
        public int skinsUnlocked;
        public int allClearTimes;
        public int maxMultikill;
        public int killedWithLaser;
        public int topLineKilled;
        public int lastLineKilled;
        public int bricksKilledWith3TimesRebounedBall;
    }

    [Serializable]
    public struct MagicalCaveProgress
    {
        public bool caveOpened;
        public long startTime;
        public int currentDustAmount;
        public int previousDustAmount;
        public int currentMediumPrizeNumber;
        public int currentBigPrizeNumber;
        public bool prize1Collected;
        public bool prize2Collected;
        public bool prize3Collected;
        public int currentBossNumber;
        public int dustThisLevel;
        public int dustTopOnLevel;

        public int currentPrize1Number;
        public int currentPrize2Number;
        public int currentPrize3Number;
        public bool caveTutorialCompleted;

        //analytics
        public float totalPlayTime;
        public float doubleBoostersUsed;
        public float deleteBoostersUsed;
        public int currentLevel;
    }

    #endregion

    #region Cloud Save/Load (in ALPHA)

    public bool IsSimilarForCloudSaving(GameSave other)
    {
        return completedQuests.Count == other.completedQuests.Count && starCount == other.starCount && ticketsCount == other.ticketsCount;
    }

    public bool IsOtherHasBetterProgress(GameSave other)
    {
        return completedQuests.Count < other.completedQuests.Count;
    }

    #endregion
}

[System.Serializable]
public class DragonCrystal
{
    public DragonCrystal()
    {

    }
    public DragonCrystal(CrystalType type, int _count)
    {
        crystalType = type;
        count = _count;
    }

    public CrystalType crystalType;
    public int count;
}

public enum CrystalType
{
    None,
    Tutor,
    Poison,
    Electro,
    Ice,
    Elite
}