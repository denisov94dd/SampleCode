using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>генератор блоков</summary>
public class BrickManager : MonoBehaviour
{
    #region Props

    public static readonly Vector3 startRow = new Vector3(-3, 3.5f, 0);
    public static readonly Vector3 finishRow = new Vector3(3, 3.5f, 0);
    public static readonly Vector3 playerRow = new Vector3(0, -4.5f, 0);

    /// <summary>таблица правил генерации</summary>
    public LinePatterner generationTable;
    public GameObject[] prefabList;
    public GameObject[] effectsList;

    [HideInInspector] public GameController game;
    public int TurnCount => game.data.turnCount;

    /// <summary>сколько раз ударили мячом</summary>
    [HideInInspector] public int ballHitCounter;
    /// <summary>сколько блоков осталось на поле</summary>
    public int BlockHitCounter { get { return _itemList.Where(item => item.gameObject.activeSelf).Sum(item => item.hitCounter); } }

    private readonly List<ItemController> _itemList = new List<ItemController>(20);
    private readonly List<GameSave.GameItem> _saveList = new List<GameSave.GameItem>(20);
    private readonly List<EffectController> _effectList = new List<EffectController>(30);

    EconomicsRegulator economicsRegulator => MetaGameController.instance.economics;

    #endregion

    #region Генерация по таблице

    /// <summary>генерация линии по данным таблицы</summary>
    public void CreateLineUsingGenerationTable()
    {
        var coinsToCreate = CoinsToCreateOnTableGenerationStep();
        foreach (var coin in coinsToCreate) CreateItem(coin);

        if(game.RequireBossOnLevel)
        {
            int bossTotalHp = game.BossHpChengePerLevel * generationTable.bossesData.deltaHP + generationTable.bossesData.starterHP;
            CreateItem<BossController>(TurnCount, 3, bossTotalHp);
            return;
        }

        foreach (var blockStat in generationTable.blockStats.blocks)
            blockStat.createdOnLine = 0;

        BlockPreset currentBlockPreset = generationTable.blockPresets[0];
        foreach (var preset in generationTable.blockPresets)
        {
            if (preset.startLevel <= TurnCount && preset.endLevel >= TurnCount)
            {
                currentBlockPreset = preset;
                break;
            }
        }

        List<BlockLifeCounter> currentPreset = new List<BlockLifeCounter>();
        float totalPossibility = 0f;
        foreach (var preset in currentBlockPreset.presets) totalPossibility += preset.relativePossibility;

        float randomChoise = Random.Range(0f, totalPossibility);
        float currentResult = 0f;

        foreach (var preset in currentBlockPreset.presets)
        {
            if (randomChoise > currentResult && randomChoise < currentResult + preset.relativePossibility)
            {
                currentPreset = preset.preset.ToList();
                break;
            }
            currentResult += preset.relativePossibility;
        }

        foreach (var presetItem in currentPreset)
        {
            switch (presetItem)
            {
                case BlockLifeCounter.Single:
                    CreateItem(BlockToCreateOnTableGenerationStep(), GameSave.BrickHp.singleHP);
                    break;
                case BlockLifeCounter.Double:
                    CreateItem(BlockToCreateOnTableGenerationStep(), GameSave.BrickHp.doubleHP);
                    break;
                case BlockLifeCounter.Random:
                    CreateItem(BlockToCreateOnTableGenerationStep(), Random.Range(0f, 100f) > 50f ? GameSave.BrickHp.singleHP : GameSave.BrickHp.doubleHP);
                    break;
            }
        }
    }

    void CreateItem(GameSave.ItemType itemToCreate, GameSave.BrickHp baseHP = GameSave.BrickHp.singleHP)
    {
        switch (itemToCreate)
        {
            case GameSave.ItemType.cube: CreateItem<CubeController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.boss: CreateItem<BossController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.onion: CreateItem<OnionController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.corner: CreateItem<CornerController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.armoured: CreateItem<ArmouredController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.dissapearing: CreateItem<DissapearingController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.moving: CreateItem<MovingController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.teleport: CreateItem<TeleportController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.ballCoin: CreateItem<BallCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.starCoin: CreateItem<StarCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.upForceCoin: CreateItem<UpForceCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.diffuseBallCoin: CreateItem<DiffuseCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.dopelgangerCoin: CreateItem<DopelgangerCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.jumpBallCoin: CreateItem<JumpCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.laserCoin: CreateItem<SingleLaserCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.crossLaserCoin: CreateItem<CrossLaserCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.killerCoin: CreateItem<KillerCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.invunerable: CreateItem<InvunerableController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.superBossWall: CreateItem<SuperBossWallController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.superBossShield: CreateItem<SuperBossShieldController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.superBossExplosive: CreateItem<SuperBossExplosiveController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.magicDustCoin: CreateItem<MagicDustCoinCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.removeBallCoin: CreateItem<RemoveBallCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.explosive: CreateItem<ExplosiveController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.blackHoleCoin: CreateItem<BlackHoleCoinController>(itemToCreate, baseHP); break;
            case GameSave.ItemType.reduceBallsCountCoin: CreateItem<ReduceBallsCoinController>(itemToCreate, baseHP); break;
        }
    }

    int PreviousCointGenerationIntervalStartLevel = 0;
    List<GameSave.ItemType> CoinsToCreateOnTableGenerationStep()
    {
        List<GameSave.ItemType> levelCoins = new List<GameSave.ItemType>();


        bool addReduceBallsCoin = economicsRegulator.caveBallReductionData.addReduceBallsForCave &&
            TurnCount >= economicsRegulator.caveBallReductionData.startReduceBallsEachXLevels &&
            TurnCount % economicsRegulator.caveBallReductionData.startReduceBallsEachXLevels <= economicsRegulator.caveBallReductionData.reduceBallsIntervalLengthInLevels;

        if (addReduceBallsCoin) levelCoins.Add(GameSave.ItemType.reduceBallsCountCoin);
        else levelCoins.Add(GameSave.ItemType.ballCoin);

        // ------------- sometimes создаем только монеку-шар
        if (game.CreateOnlyBallCoin) return levelCoins;

        // ------------- выбираем нужный интервал и обновляем статистику для него
        CoinPossibility currentPossibility = generationTable.coinPossibilities[0];
        for (int i = 0; i < generationTable.coinPossibilities.Length; i++)
        {
            //если текущий уровень уже больше начала, но меньше конца - это нужный промежуток
            if (TurnCount >= generationTable.coinPossibilities[i].startLevel && TurnCount <= generationTable.coinPossibilities[i].endLevel)
            {
                currentPossibility = generationTable.coinPossibilities[i];
                if (PreviousCointGenerationIntervalStartLevel != currentPossibility.startLevel)
                {
                    //если уровни не совпадают - нужно обнулить статистику по кол-ву монет
                    PreviousCointGenerationIntervalStartLevel = currentPossibility.startLevel;
                    foreach (var statItem in generationTable.coinStats.coins) statItem.timesCreatedInInterval = 0;
                }
                break;
            }
        }

        // ------------- высчитываем, какие монеты нужно создавать
        List<GameSave.ItemType> itemsToGenerate = new List<GameSave.ItemType>();
        bool skipItem;
        foreach (var item in currentPossibility.coins)
        {
            skipItem = false;
            foreach (var generatedPreviously in generationTable.coinStats.coins)
                if (generatedPreviously.coinType == item.coinType &&
                   generatedPreviously.timesCreatedInInterval >= item.maxAmountInInterval &&
                   item.maxAmountInInterval > 0)
                {
                    skipItem = true;
                    break;
                }
            if (!skipItem) itemsToGenerate.Add(item.coinType);
        }

        // ------------- выбираем монеты для создания и добавляем в стату
        float totalPossibility = 0f;
        foreach (var coinStat in currentPossibility.coins)
            if (itemsToGenerate.Contains(coinStat.coinType))
                totalPossibility += coinStat.relativePossibility;

        float randomChoise = Random.Range(0f, totalPossibility);
        float currentValue = 0f;
        foreach (var item in currentPossibility.coins)
        {
            if (itemsToGenerate.Contains(item.coinType))
            {
                if (randomChoise > currentValue && randomChoise < currentValue + item.relativePossibility)
                {
                    //если выбор лежит в текущем интервале - то это наша монета
                    levelCoins.Add(item.coinType);
                    foreach (var statItem in generationTable.coinStats.coins)
                    {
                        if (statItem.coinType == item.coinType)
                            statItem.timesCreatedInInterval++;
                    }
                    break;
                }
                currentValue += item.relativePossibility;
            }
        }

        return levelCoins;
    }

    GameSave.ItemType BlockToCreateOnTableGenerationStep()
    {
        // ------------- выбираем нужный интервал
        BlockPossibility currentPossibility = generationTable.blockPossibilities[0];
        for (int i = 0; i < generationTable.blockPossibilities.Length; i++)
        {
            if (TurnCount >= generationTable.blockPossibilities[i].startLevel && TurnCount <= generationTable.blockPossibilities[i].endLevel)
            {
                currentPossibility = generationTable.blockPossibilities[i];
                break;
            }
        }

        // ------------- высчитываем, какие блоки нужно создавать
        List<GameSave.ItemType> itemsToGenerate = new List<GameSave.ItemType>();
        bool skipItem;
        foreach (var item in currentPossibility.blocks)
        {
            skipItem = false;
            foreach (var generatedPreviously in generationTable.blockStats.blocks)
            {
                if (generatedPreviously.blockType == item.blockType)
                {
                    int createdSinceLastLevelInterval = 0;

                    if (generatedPreviously.levelsCreated != null && generatedPreviously.levelsCreated.Count > 0)
                        for (int i = generatedPreviously.levelsCreated.Count - 1; i >= 0; i--)
                            if (generatedPreviously.levelsCreated[i] > TurnCount)
                                generatedPreviously.levelsCreated.Remove(generatedPreviously.levelsCreated[i]);

                    foreach (var creation in generatedPreviously.levelsCreated)
                    {
                        if (creation > TurnCount - item.intervalLength && creation < TurnCount)
                            createdSinceLastLevelInterval++;
                    }

                    if (generatedPreviously.createdOnLine >= item.maxAmountOnLine ||
                       (item.intervalLength > 0 && createdSinceLastLevelInterval >= item.maxAmountInInterval)
                      )
                    {
                        skipItem = true;
                        break;
                    }
                }
            }
            if (!skipItem)
                itemsToGenerate.Add(item.blockType);
        }

        // ------------- выбираем блоки для создания и добавляем в стату
        float totalPossibility = 0f;
        foreach (var blockStat in currentPossibility.blocks)
            if (itemsToGenerate.Contains(blockStat.blockType))
                totalPossibility += blockStat.relativePossibility;

        float randomChoise = Random.Range(0f, totalPossibility);
        float currentValue = 0f;
        foreach (var item in currentPossibility.blocks)
        {
            if (itemsToGenerate.Contains(item.blockType))
            {
                if (randomChoise > currentValue && randomChoise < currentValue + item.relativePossibility)
                {
                    foreach (var statItem in generationTable.blockStats.blocks)
                    {
                        if (statItem.blockType == item.blockType)
                        {
                            statItem.createdOnLine++;
                            statItem.levelsCreated.Add(TurnCount);
                            return item.blockType;
                        }
                    }
                }
                currentValue += item.relativePossibility;
            }
        }
        return GameSave.ItemType.cube;
    }

    int CurrentBrickHpReductionValue()
    {
        int result = 0;
        int currentReductionWavesNumber = TurnCount / economicsRegulator.caveBricksHpReductionData.startReductionEachNLevels;

        if (currentReductionWavesNumber < 1) return 0;

        int currentTopBorder = economicsRegulator.caveBricksHpReductionData.numsOfRuductiveWaves;

        //all previous waves
        while(currentReductionWavesNumber - 1 > 0)
        {
            for(int i = 0; i < currentTopBorder; i++) result += economicsRegulator.caveBricksHpReductionData.reductionHpPerStep;

            currentTopBorder -= economicsRegulator.caveBricksHpReductionData.reductionWavesDecreaseStep;
            currentReductionWavesNumber--;
        }

        //current wave
        if ((TurnCount % economicsRegulator.caveBricksHpReductionData.startReductionEachNLevels) <= currentTopBorder)
            for (int i = 0; i < TurnCount % economicsRegulator.caveBricksHpReductionData.startReductionEachNLevels; i++)
                result += economicsRegulator.caveBricksHpReductionData.reductionHpPerStep;

        else result += economicsRegulator.caveBricksHpReductionData.reductionHpPerStep * currentTopBorder;

        return result;
    }

    #endregion

    #region Создание/удаление/сдвиг

    /// <summary>создание заранее заданной линии</summary>
    public void CreateLine(GameSave.GameItem[] itemsToLoad)
    {
        Load(itemsToLoad, 0.07f, 0.1f);
    }

    /// <summary>сдвиг всех блоков вниз, подсчет количества потерянных шаров</summary>
    public int MoveDown()
    {
        return _itemList.Where(item => item.gameObject.activeSelf).Sum(item => item.MoveDown());
    }


    /// <summary>создает объект из потомка ItemController</summary>
    private void CreateItem<T>(GameSave.ItemType newType, GameSave.BrickHp baseHp = GameSave.BrickHp.singleHP) where T : ItemController
    {
        var freeCellList = GetFreeCellInRow(TurnCount);
        if (freeCellList.Count <= 0) return; //свободных мест нет!
        var freeColumn = freeCellList[Random.Range(0, freeCellList.Count)];

        var newBlock = _itemList.FirstOrDefault(item => item.GetType() == typeof(T) && !item.gameObject.activeSelf);

        if (newBlock == null)
        {
            foreach (var prefabItem in prefabList)
                if (null != prefabItem.gameObject.GetComponent<T>())
                {
                    newBlock = (Instantiate(prefabItem, transform)).GetComponent<T>();
                    break;
                }

            newBlock.manager = this;
            _itemList.Add(newBlock);
        }

        int decreasedValue = ((int)baseHp + 1) * CurrentBrickHpReductionValue();
        newBlock.Init(TurnCount, freeColumn, TurnCount * ((int)baseHp + 1) - decreasedValue);
    }

    public ItemController CreateItem<T>(int itemLevel, int itemIndex, int itemHP) where T : ItemController
    {
        var newBlock = _itemList.FirstOrDefault(item => item.GetType() == typeof(T) && !item.gameObject.activeSelf);

        if (newBlock == null)
        {
            foreach (var prefabItem in prefabList)
                if (null != prefabItem.gameObject.GetComponent<T>())
                {
                    newBlock = (Instantiate(prefabItem, transform)).GetComponent<T>();
                    break;
                }

            newBlock.manager = this;
            _itemList.Add(newBlock);
        }

        newBlock.Init(itemLevel, itemIndex, itemHP);
        return newBlock;
    }

    public T CreateEffect<T>(Vector3 position, Quaternion rotation, Transform parent) where T : EffectController
    {
        EffectController newEffect = null;
        for (int i = 0; i < _effectList.Count; i++)
        {
            if (_effectList[i].GetType() == typeof(T) && _effectList[i].isFree)
            {
                newEffect = _effectList[i];
                break;
            }
        }

        if (newEffect == null)
        {
            foreach (var prefabEffect in effectsList)
                if (null != prefabEffect.gameObject.GetComponent<T>())
                {
                    newEffect = Instantiate(prefabEffect, position, Quaternion.identity, null).GetComponent<T>();
                    break;
                }
            _effectList.Add(newEffect);
        }
        newEffect.Init(position, rotation, parent, this);
        return (T)newEffect;
    }

    /// <summary>при попадании в блок</summary>
    public void OnHit(BallController ball)
    {
        if (game.Machine.IsInStateOf<ShootingState>() || game.Machine.IsInStateOf<CampaignMissionState>() || game.Machine.IsInStateOf<TutorialState>())
        {
            if (ball != null) ballHitCounter++;
            if (BlockHitCounter == 0) game.OnClear();
        }
    }

    /// <summary>сплеш-урон от супер-мячика</summary>
    public void SplashSuperBallDamage(int index, int level)
    {
        var allBricks = GetItemList<CubeController>();
        List<CubeController> damagedBricks = new List<CubeController>();
        foreach (var brick in allBricks)
        {
            if (brick.index >= index - 1 &&
               brick.index <= index + 1 &&
               brick.level >= level - 1 &&
               brick.level <= level + 1)
            {
                damagedBricks.Add(brick);
                continue;
            }
        }
        foreach (var brick in damagedBricks)
        {
            bool isMainBlock = brick.level == level && brick.index == index;
            brick.OnGetDamage(null, isMainBlock ?
                              MetaGameController.instance.damagePercentForFirstBrick :
                              MetaGameController.instance.damagePercentForSurroundingBricks, DragonBonusType.SuperFirstBall);
            brick.PlayCritEffect(isMainBlock);
        }
    }

    /// <summary>сплеш-урон он шарика при использовании супер-пета</summary>
    public void PlaySuperDragonPetDamageEffect(int index, int level, bool isThisBrickOnly = false)
    {

        var allBricks = GetItemList<CubeController>();
        List<CubeController> damagedBricks = new List<CubeController>();
        foreach (var brick in allBricks)
        {
            if (brick.index >= index - 1 &&
               brick.index <= index + 1 &&
               brick.level <= level + 1 && brick.level >= level - 1)
            {
                damagedBricks.Add(brick);
                continue;
            }
        }
        foreach (var brick in damagedBricks)
        {
            bool ismainBrick = brick.level == level && brick.index == index;
            bool isExplosive = brick.GetComponent<ExplosiveController>() != null;
            bool isBoss = brick.GetComponent<BossController>() != null;

            if (ismainBrick || !isThisBrickOnly)
            {
                brick.OnGetDamage(null,
                                  ismainBrick ?
                                  (isBoss ? 1 : MetaGameController.instance.premiumDragonPetMainBlockDamagePercentage)
                                  :
                                  (isExplosive ?
                                   100 :
                                   (isBoss ?
                                    1 : MetaGameController.instance.premiumDragonSurroudingBlocksDamagePercentage)),
                                  DragonBonusType.SuperFirstBall);
                brick.PlayCritEffect(ismainBrick);
            }
        }
    }

    /// <summary>наносит урон ядом</summary>
    public void DealPoisonDamage()
    {
        var allBricks = GetItemList<CubeController>();
        foreach (var brick in allBricks)
        {
            if (brick.wasDamagedOnTurn)
            {
                brick.OnGetDamage(null, MetaGameController.instance.poisonEffectDamagePercent, DragonBonusType.Poison);
                brick.wasDamagedOnTurn = false;
                brick.cubeEffectController.PlayPoisonEffect(false, brick);
            }
        }
    }

    public void DealFreezingDamage()
    {
        var allbricks = GetItemList<CubeController>();
        foreach (var brick in allbricks)
        {
            if (brick.hitCounter < MetaGameController.instance.freezingHpLevelToBlow * brick.hpAtTheBeginningOfTheLevel && brick.cubeEffectController.isFreezed)
                brick.BlowIce();
        }
    }

    /// <summary>мяч собрал монетку, добавляющую мяч</summary>
    public void OnBallCoin(int count = 1)
    {
        game.player.AddBall(count);
    }

    public void OnRemoveBallCoin(int count)
    {
        game.player.AddBall(-count);
    }

    /// <summary>мяч собрал монетку, добавляющую ману</summary>
    public void OnStarCoin()
    {

    }

    public bool blockPositionChanger;
    /// <summary>в конце сдвига выравнивает все блоки ровно по клеткам (вдруг что!)</summary>
    public void SetBlocksPixelPerfect()
    {
        if (blockPositionChanger) return;
        foreach (var item in GetItemList<ItemController>())
            item.SetYPosition();
    }
    #endregion

    #region эффекты

    #region Delete Booster Use Peocess

    /// <summary>удаляем N ближайших блоков</summary>
    public void BonusDeleting(int delCount = 5)
    {
        StartCoroutine(BonusDeletingProcess(delCount));
    }

    public System.Action actionOnBoosterProcessEnded;

    System.Collections.IEnumerator BonusDeletingProcess(int delCount)
    {
        var delBlockList = _itemList
            .Where(block => block.gameObject.activeSelf)
            .Where(block => block.hitCounter > 0)
            .Where(block => block as BossController == null) //боссов бустером не уничтожаем
            .OrderBy(block => block.level * 1000 + block.hitCounter)
            .Take(delCount);

        game.player.effectController.PlayLightningReadyEffect();
        yield return new WaitForSeconds(0.6f);

        foreach (var item in delBlockList)
        {
            CubeController cController = item.GetComponent<CubeController>();
            game.OnDamageDealtUpdateLevelProgress(item.hitCounter);
            if (cController != null)
                cController.OnInstantDeleting();
        }

        //если на карте больше не осталось блоков - сдвигаем вниз и генерим новые (конец уровня)
        if (BlockHitCounter == 0)
        {
            if (!game.Machine.IsTutorial)
            {
                if (game.Machine.IsCampaign)
                    ((CampaignMissionState)game.Machine.ActiveState).StartNextLine();
                else
                {
                    game.OnEndTurn();
                    game.Machine.ForceNextState<ShiftState>();
                }
            }
            else game.Machine.ActiveState.OnPlayerUsedBooster();

        }
        game.CheckBlocksNearPlayer();
        actionOnBoosterProcessEnded?.Invoke();
        yield break;
    }

    #endregion

    #region Clear Line Effect

    public void ExecuteClearLineEffect(ItemController lineBaseItem)
    {
        int currLine = lineBaseItem.index;
        int middleLineIndex = (GameController.blockInRow + 1) / 2 - 1;

        List<CubeController> damagedBricks = new List<CubeController>();
        int countOfRockets = (DragonManager.GetDragonByType(DragonType.DragonPremium) as PremiumDragon).maxNumberOfRockets;
        int rocketsLaunched = 0;
        for (int i = 0; i < countOfRockets; i++)
        {
            if (rocketsLaunched == countOfRockets) break;
            var allBrickINCo = GetCol<CubeController>(currLine).ToList();
            foreach (var item in allBrickINCo)
            {
                rocketsLaunched++;
                damagedBricks.Add(item);
                if (rocketsLaunched == countOfRockets) break;
            }
            if (currLine == middleLineIndex) break;

            if (lineBaseItem.index > middleLineIndex) currLine--;
            else currLine++;
        }

        var Allitems = GetItemList<CubeController>().ToList();
        foreach (var item in damagedBricks)
            if (Allitems.Contains(item)) Allitems.Remove(item);

        Allitems.Sort((item1, item2) => (Mathf.Abs(item1.index - middleLineIndex) * 1000 - item1.level - (Mathf.Abs(item2.index - middleLineIndex) * 1000 - item2.level)));
        foreach (var item in Allitems)
        {
            if (rocketsLaunched == countOfRockets) break;
            damagedBricks.Add(item);
            rocketsLaunched++;
        }

        int damagePercentage = (DragonManager.GetDragonByType(DragonType.DragonPremium) as PremiumDragon).damagePecent;

        foreach (var item in damagedBricks)
            item.OnGetDamage(null, damagePercentage, DragonBonusType.SuperFirstBall);

    }

    #endregion

    #region Cross DamageEffect

    public void ExecuteCrossLineDamageEffect(int index, int level)
    {
        var cross = GetCross<CubeController>(level, index);
        CreateEffect<ElectricLaser>(GetWorldPosition(level, index), Quaternion.identity, null);
        foreach (var cube in cross) cube.OnGetDamage(null, MetaGameController.instance.damagePercentageForLaserHit, DragonBonusType.Laser);
    }

    #endregion

    #region Ice Blast Effect

    public void ExecuteIceBlastEffect(CubeController cube)
    {
        var allBricks = GetItemList<CubeController>();
        List<CubeController> damagedBricks = new List<CubeController>();
        cube.cubeEffectController.ManageFreezeEffect(false, cube, cube.hitCounter <= 0);
        foreach (var brick in allBricks)
        {
            if (brick.index >= cube.index - 1 &&
               brick.index <= cube.index + 1 &&
               brick.level >= cube.level - 1 &&
               brick.level <= cube.level + 1)
            {
                damagedBricks.Add(brick);
                if (brick == this) damagedBricks.Remove(brick);
                continue;
            }
        }

        foreach (var brick in damagedBricks)
            brick.OnGetDamage(null, MetaGameController.instance.damagePercentForFreezingBlast, DragonBonusType.Freezing);
    }

    #endregion

    #region Delete All Process

    public void DeleteAll()
    {
        var allBlocks = GetItemList<ItemController>().ToList();
        foreach (var block in allBlocks)
            block.gameObject.SetActive(false);
    }

    #endregion

    #endregion

    #region математика над клетками

    /// <summary>предметы в строке и столбце (крест)</summary>
    public IEnumerable<T> GetCross<T>(int row, int col) where T : ItemController
    {
        return GetItemList<T>().Where(item => item.level == row || item.index == col ||
                                      (item as BossController != null && (item.level == row + 1 || item.index == col - 1)));
    }

    /// <summary>предметы в строке</summary>
    public IEnumerable<T> GetRow<T>(int row, bool ignoreLineInterception = false) where T : ItemController
    {
        var itemList = GetItemList<T>().Where(item => item.level + item.shiftDelay == row).ToList<T>();
        if (ignoreLineInterception) return itemList;

        foreach (var item in GetItemList<T>())
            if (item as BossController != null && item.level + item.shiftDelay == row + 1 && !itemList.Contains(item))
                itemList.Add(item);

        return itemList;
        //нельзя просто брать уровни - у некоторых блоков геометрия не на одну клетку в высоту
    }

    /// <summary>предметы в столбце</summary>
    public IEnumerable<T> GetCol<T>(int col) where T : ItemController
    {
        var itemList = GetItemList<T>().Where(item => item.index == col).ToList<T>();

        foreach (var item in GetItemList<T>())
            if (item as BossController != null && item.index == col - 1 && !itemList.Contains(item))
                itemList.Add(item);

        return itemList;
    }

    public IEnumerable<T> GetItemList<T>() where T : ItemController
    {
        return _itemList
            .Where(item => item.gameObject.activeSelf && item is T)
            .Cast<T>();
    }

    /// <summary>получить номера свободных клеток на линии</summary>
    public List<int> GetFreeCellInRow(int row)
    {
        var vacantPlaces = new List<int>(GameController.blockInRow);
        for (var i = 0; i < GameController.blockInRow; i++) vacantPlaces.Add(i);

        foreach (var index in GetRow<ItemController>(row)
            .SelectMany(item => item.GetPositionIndexes()))
            //.Where(index => vacantPlaces.Contains(index)))
            vacantPlaces.Remove(index);

        return vacantPlaces;
    }

    /// <summary>позиция с нижнего левого угла</summary>
    public Vector3 GetWorldPosition(int row, int col)
    {
        return startRow + Vector3.down * 5 + new Vector3(row, col);
    }

    #endregion

    #region Сериализация/Десериализация

    public List<GameSave.GameItem> Save()
    {
        _saveList.Clear();

        foreach (var item in _itemList.Where(item => item.level <= TurnCount && item.gameObject.activeSelf))
            _saveList.Add(item.Save());

        return _saveList;
    }

    public void Load(GameSave.GameItem[] itemList, float itemIndexLoadDelta, float itemLevelLoadDelta, bool forceLoad = false)
    {
        StartCoroutine(SmoothLoader(itemList, itemIndexLoadDelta, itemLevelLoadDelta, forceLoad));
    }

    System.Collections.IEnumerator SmoothLoader(GameSave.GameItem[] itemList, float itemIndexLoadDelta, float itemLevelLoadDelta, bool forceLoad = false)
    {
        var Sorted = itemList.ToList();
        Sorted.Sort((x, y) => x.level * 1000 - y.level * 1000 + x.index - y.index);
        var prevItemLvl = 0;
        foreach (var saveItem in Sorted.Where(item => forceLoad ? item.level > int.MinValue : item.level <= TurnCount))
        {
            if (prevItemLvl != 0 && saveItem.level != prevItemLvl) yield return new WaitForSeconds(itemLevelLoadDelta);

            prevItemLvl = saveItem.level;
            switch (saveItem.itemType)
            {
                case GameSave.ItemType.cube: LoadItem<CubeController>(saveItem); break;
                case GameSave.ItemType.onion: LoadItem<OnionController>(saveItem); break;
                case GameSave.ItemType.corner: LoadItem<CornerController>(saveItem); break;
                case GameSave.ItemType.armoured: LoadItem<ArmouredController>(saveItem); break;
                case GameSave.ItemType.dissapearing: LoadItem<DissapearingController>(saveItem); break;
                case GameSave.ItemType.moving: LoadItem<MovingController>(saveItem); break;
                case GameSave.ItemType.teleport: LoadItem<TeleportController>(saveItem); break;
                case GameSave.ItemType.ballCoin: LoadItem<BallCoinController>(saveItem); break;
                case GameSave.ItemType.starCoin: LoadItem<StarCoinController>(saveItem); break;
                case GameSave.ItemType.killerCoin: LoadItem<KillerCoinController>(saveItem); break;
                case GameSave.ItemType.upForceCoin: LoadItem<UpForceCoinController>(saveItem); break;
                case GameSave.ItemType.diffuseBallCoin: LoadItem<DiffuseCoinController>(saveItem); break;
                case GameSave.ItemType.jumpBallCoin: LoadItem<JumpCoinController>(saveItem); break;
                case GameSave.ItemType.laserCoin: LoadItem<SingleLaserCoinController>(saveItem); break;
                case GameSave.ItemType.crossLaserCoin: LoadItem<CrossLaserCoinController>(saveItem); break;
                case GameSave.ItemType.dopelgangerCoin: LoadItem<DopelgangerCoinController>(saveItem); break;
                case GameSave.ItemType.invunerable: LoadItem<InvunerableController>(saveItem); break;
                case GameSave.ItemType.boss: LoadItem<BossController>(saveItem); break;
                case GameSave.ItemType.superBossWall: LoadItem<SuperBossWallController>(saveItem); break;
                case GameSave.ItemType.superBossShield: LoadItem<SuperBossShieldController>(saveItem); break;
                case GameSave.ItemType.superBossExplosive: LoadItem<SuperBossExplosiveController>(saveItem); break;
                case GameSave.ItemType.explosive: LoadItem<ExplosiveController>(saveItem); break;
                case GameSave.ItemType.priest: LoadItem<PriestController>(saveItem); break;
                case GameSave.ItemType.slime: LoadItem<SlimeController>(saveItem); break;
                case GameSave.ItemType.millCoin: LoadItem<MillCoinController>(saveItem); break;
                case GameSave.ItemType.blackHoleCoin: LoadItem<BlackHoleCoinController>(saveItem); break;
                case GameSave.ItemType.magicDustCoin: LoadItem<MagicDustCoinCoinController>(saveItem); break;
                case GameSave.ItemType.removeBallCoin: LoadItem<RemoveBallCoinController>(saveItem); break;
                case GameSave.ItemType.reduceBallsCountCoin: LoadItem<ReduceBallsCoinController>(saveItem); break;
                default: Debug.LogWarningFormat("can't load {0}", saveItem.itemType); break;
            }

            yield return new WaitForSeconds(itemIndexLoadDelta);
        }
        if (game.Machine.IsInStateOf<LoadingState>()) game.Machine.ActiveState.Ended = true;
    }

    private void LoadItem<T>(GameSave.GameItem saveItem) where T : ItemController
    {
        foreach (var prefabItem in prefabList)
            if (null != prefabItem.gameObject.GetComponent<T>())
            {
                foreach (var item in GetRow<ItemController>(saveItem.level + saveItem.shiftDelay))
                {
                    if (item.index == saveItem.index && !item.gameObject.activeInHierarchy)
                    {
                        item.PlayDestroyEffect();
                        item.gameObject.SetActive(false);
                    }
                }

                var newItem = ((GameObject)Instantiate(prefabItem, transform)).GetComponent<T>();
                _itemList.Add(newItem);
                newItem.Load(this, saveItem);
                return;
            }

        Debug.LogWarningFormat("Can't find prefab for {0}", typeof(T));
    }

    #endregion

    #region MonoBehaviour

    private void Update()
    {
        int i = 0;
        float currentTime = Time.time;
        for (i = 0; i < _effectList.Count; i++)
            if (currentTime > _effectList[i].finishTime && !_effectList[i].isFree)
                _effectList[i].DisableSelf();

        for (i = 0; i < _itemList.Count; i++)
            _itemList[i].ItemUpdate(currentTime);
    }

    #endregion

    #region Сохранение игрового процесса

    public List<List<ItemController>> saveDatas;
    public List<ItemController> lastSavedLevelState { get { return saveDatas[saveDatas.Count - 1]; } }

    public void SaveCurrentLevelState()
    {
        if (saveDatas == null) saveDatas = new List<List<ItemController>>();

        saveDatas.Add(GetItemList<ItemController>().ToList());
    }

    public bool CanRestoreState(int turnsBefore)
    {
        if (saveDatas == null || saveDatas.Count < turnsBefore) return false;
        return true;
    }

    public void RestoreState(int topBorderCounter)
    {
        var allBricks = GetItemList<ItemController>().ToList();
        var currentStateSave = new List<GameSave.GameItem>();
        int minLevel = int.MaxValue;
        foreach (var item in allBricks)
            if (item.level < minLevel)
                minLevel = item.level;

        for (int i = 0; i < allBricks.Count; i++)
        {
            if (allBricks[i].level <= minLevel + topBorderCounter)
            {
                var itemSave = allBricks[i].Save();
                itemSave.hitCounter = allBricks[i].starterHitCounter;
                currentStateSave.Add(itemSave);
            }
        }
    }

    public List<ItemController> GetState(int turnsBefore)
    {
        int indexer = saveDatas.Count;
        if (indexer < turnsBefore) return null;

        var result = saveDatas[saveDatas.Count - turnsBefore];
        List<ItemController> deletedbricksResult = new List<ItemController>();
        foreach (var item in result) if (item.level <= TurnCount) deletedbricksResult.Add(item);

        return deletedbricksResult;
    }

    public GameSave.GameItem[] ItemsFromFieldInSaveFormat(int turnsBefore)
    {
        var itemsOnField = GetState(turnsBefore);

        List<GameSave.GameItem> res = new List<GameSave.GameItem>();

        foreach (var item in itemsOnField)
        {
            var saved = item.Save();
            saved.hitCounter = item.starterHitCounter;
            res.Add(saved);
        }
        return res.ToArray();
    }

    #endregion

    #region Дополнительные таски

    int currentTasksNum = 0;
    public int CurrentTasksNumber
    {
        get { return currentTasksNum; }
        set
        {
            currentTasksNum = value;
            if(currentTasksNum == 0)
            {
                onAllTasksEnded?.Invoke();
                onAllTasksEnded = null;
            }
        }
    }

    System.Action onAllTasksEnded;

    public void OnTasksEndedAddTask(System.Action someAction)
    {
        onAllTasksEnded += someAction;
    }

    #endregion
}