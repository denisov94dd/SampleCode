using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


public class CubeController : ItemController
{
    public GameSave.BrickHp baseHP;
    public bool wasDamagedOnTurn;
    [SerializeField] protected Text countText;
    [SerializeField] public BlockEffect cubeEffectController;
    const float yLocalPosForText = -0.35f;
    const float hitTextLocalScale = 0.3f;

    private void Awake()
    {
        if(!cubeEffectController) cubeEffectController = GetComponentInChildren<BlockEffect>();
    }

    public override void Init(int newLevel, int newIndex, int newHitPoint)
    {
        base.Init(newLevel, newIndex, newHitPoint);
        transform.localScale = Vector3.one;
        baseHP = (GameSave.BrickHp)(newHitPoint / (newLevel != 0 ? newLevel : 1) - 1);
        if (newHitPoint > 0) hitCounter = newHitPoint;

        if (countText)
        {
            SetHPTextPosition();
            countText.text = newHitPoint.ToString();
        }

        if (cubeEffectController)
        {
            cubeEffectController.isPoisoned = false;
            cubeEffectController.ResetEffect();
            cubeEffectController.PlayAppearEffect(this);
            cubeEffectController.OnInit(this);
        }
        manager.game.OnStateEnded += OnStateEnded;
        starterHitCounter = hitCounter;
    }

    public override void Load(BrickManager newManager, GameSave.GameItem saveItem)
    {
        base.Load(newManager, saveItem);
        baseHP = saveItem.baseHP;
        if (countText)
        {
            SetHPTextPosition();
            countText.text = hitCounter.ToString();
        }
        if (cubeEffectController)
        {
            cubeEffectController.isPoisoned = false;
            cubeEffectController.ResetEffect();
            cubeEffectController.PlayAppearEffect(this);
            cubeEffectController.OnInit(this);
        }
        manager.game.OnStateEnded += OnStateEnded;
    }

    public override GameSave.GameItem Save()
    {
        var result = base.Save();
        result.itemType = GameSave.ItemType.cube;
        result.baseHP = baseHP;
        return result;
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        var ball = collision.gameObject.GetComponent<BallController>();
        if (ball == null) return;
        if (!ball.gameObject.activeSelf) return;
        if (ball.fake) return;
        if (ball.ballType == DragonBonusType.SuperFirstBall ||
           ball.ballType == DragonBonusType.Laser ||
           ball.ballType == DragonBonusType.Freezing)
        {
            switch (ball.ballType)
            {
                case DragonBonusType.SuperFirstBall:
                    manager.SplashSuperBallDamage(index, level);
                    break;
                case DragonBonusType.Laser:
                    manager.ExecuteCrossLineDamageEffect(index, level);
                    break;
            }
            ball.ballType = DragonBonusType.None;

            OnGetDamage(ball);
            return;
        }

        //модификаторы на импакт шарика
        if (manager.game.PlayerHasBallImpactModifiers())
            manager.game.ApplyBallImpactModifiersEffect(this);

        OnGetDamage(ball);
    }

    public virtual void PlayGetHitSound()
    {
        SoundManager.instance.OnMonsterHit();
    }

    public virtual void OnGetDamage(BallController ball, int damagePercentage = 0, DragonBonusType damageType = DragonBonusType.None)
    {
        PlayGetHitSound();
        int damage = 0;
        int logDamage;
        if (damagePercentage > 0)
        {
            damage = (int)(damagePercentage / 100f * (float)(baseHP + 1) * level);

            int totalHPBefore = hitCounter;
            logDamage = damage;
            Color effectColor = Color.white;
            switch (damageType)
            {
                case DragonBonusType.Poison: effectColor = Color.green; break;
                case DragonBonusType.Laser: effectColor = Color.yellow; break;
                case DragonBonusType.SuperFirstBall: effectColor = Color.red; break;
                case DragonBonusType.Freezing: effectColor = new Color(0.5f, 0.952f, 1f); break;
            }

            if (damagePercentage >= 100)
            {
                logDamage = totalHPBefore;
                hitCounter = 0;
            }
            manager.CreateEffect<DamageLogEffect>(transform.position, Quaternion.identity, null).LogDamage(-logDamage, effectColor);
        }
        else
        {
            hitCounter--;
            logDamage = 1;
        }

        if (manager.game.currentDragonBonus == DragonBonusType.Poison && ball != null)
        {
            wasDamagedOnTurn = true;
            cubeEffectController.PlayPoisonEffect(wasDamagedOnTurn, this);
        }

        if (manager.game.currentDragonBonus == DragonBonusType.Freezing && hitCounter > 0 && ball != null)
            cubeEffectController.ManageFreezeEffect(hpAtTheBeginningOfTheLevel * MetaGameController.instance.freezingHpLevelToBlow > hitCounter, this);


        if (hitCounter == 0 && manager.game.currentDragonBonus == DragonBonusType.Freezing)
            BlowIce();

        if (hitCounter <= 0)
        {
            if (ball == null && damageType == DragonBonusType.Laser) manager.game.achievementController.OnBrickKilledByLaserCoin();

            if (!GetType().IsSubclassOf(typeof(CubeController))) SoundManager.instance.OnBrickDestroy();
            PlayDestroyEffect();
            if (GetType() == typeof(CubeController)) manager.game.achievementController.OnBrickDeleted(GameSave.ItemType.cube, level, ball);
            manager.game.CheckBlocksNearPlayer();
            gameObject.SetActive(false);
            manager.game.OnBrickKilled(this);
        }
        else
        {
            countText.text = hitCounter.ToString();
            cubeEffectController.PlayImpactEffect(this);
        }

        manager.game.OnDamageDealtUpdateLevelProgress(logDamage > 0 ? logDamage : 1);
        manager.OnHit(ball);
    }

    public virtual void SetHPTextPosition()
    {
        if (!countText.isActiveAndEnabled) return;

        countText.transform.localScale = Vector3.one * hitTextLocalScale;
        countText.transform.parent.position = transform.position + new Vector3(0f, yLocalPosForText, -0.1f);
    }

    public virtual void OnInstantDeleting()
    {
        cubeEffectController.PlayBonusDeleteEffect(manager);
        PlayDestroyEffect();
        gameObject.SetActive(false);
    }

    public override void PlayDestroyEffect()
    {
        cubeEffectController.OnCubeDestroy();
        manager.CreateEffect<ExplosionEffect>(transform.position, Quaternion.identity, null).PlayEffect(baseHP != GameSave.BrickHp.singleHP, cubeEffectController.isAlt);
    }

    //cube update method
    public override void ItemUpdate(float currentTime)
    {
        if (cubeEffectController) cubeEffectController.CubeEffectUpdate(currentTime);
    }

    public override void OnReachEnd()
    {
        base.OnReachEnd();
        manager.StartCoroutine(gameObject.AddComponent<BlockJumpOnEnd>().JumpProcessRoutine());
    }
    public virtual void PlayCritEffect(bool isMainBlock)
    {
        manager.CreateEffect<CriticalHitEffect>(transform.position, Quaternion.identity, null).PlayCritEffect(isMainBlock);
    }

    public virtual void PlayPremiumPetBombEffect()
    {
        manager.CreateEffect<PremBombExplosion>(transform.position, Quaternion.identity, null);
    }

    public virtual void GetHeal(int healPercentage)
    {
        int hpToAdd = (int)(((float)healPercentage / 100f) * starterHitCounter);
        hitCounter += hpToAdd;
        if (hitCounter > starterHitCounter) hitCounter = starterHitCounter;
        countText.text = hitCounter.ToString();
    }

    public int hpAtTheBeginningOfTheLevel { get; private set; }

    public virtual void OnStateEnded(GameState state)
    {
        hpAtTheBeginningOfTheLevel = hitCounter;
    }

    public void BlowIce()
    {
        manager.ExecuteIceBlastEffect(this);
    }

    public override void UpdateHitCouterText()
    {
        countText.text = hitCounter.ToString();
    }

    public override void ManageDestroyOnRestore()
    {
        base.ManageDestroyOnRestore();

        manager.StartCoroutine(SetSmallSizeAndDisable());
    }

    IEnumerator SetSmallSizeAndDisable()
    {
        manager.CreateEffect<BlockAppear>(transform.position, Quaternion.identity, null);
        SpriteTweeners.SpriteScaleCrossFromValueToValue(this, transform, 1, 0, 0.25f);
        yield return new WaitForSeconds(0.36f);
        gameObject.SetActive(false);
        yield break;
    }

    public void PlayFearEffect()
    {
        cubeEffectController.PlayImpactEffect(this);
    }

}
