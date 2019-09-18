using UnityEngine;
using System.Collections;

public class BaseCoinController : ItemController
{
    /// <summary>будет ли деактивирован объект монетки сразу при касании</summary>
    [SerializeField] protected bool destroyOnTouch;
    public CoinEffect coinEffectController;
    /// <summary>была ли монетка тронута во время уровня</summary>
    public bool touchedAtLevel;

    public override void Init(int newLevel, int newIndex, int newHitPoint)
    {
        base.Init(newLevel, newIndex, newHitPoint);
        touchedAtLevel = false;
        var pos = transform.localPosition;
        transform.localPosition = pos;
        transform.localScale = Vector3.one;
        manager.game.OnStateEnded += DisableAfterShooting;

        coinEffectController.ResetActivationEffect();
        coinEffectController.PlayAppearEffect(this);
    }

    public override void Load(BrickManager newManager, GameSave.GameItem saveItem)
    {
        base.Load(newManager, saveItem);
        manager.game.OnStateEnded += DisableAfterShooting;

        coinEffectController.ResetActivationEffect();
    }

    public override void OnReachEnd()
    {
        base.OnReachEnd();
        gameObject.SetActive(false);
        coinEffectController.PlayCoinOnReachEndDestroyEffect(this);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsBall(collision))
        {
            touchedAtLevel = true;
            if (destroyOnTouch)
            {
                if (gameObject.activeSelf) coinEffectController.PlayCoinDestroyEffect(this);
                gameObject.SetActive(false);
            }
            else
                coinEffectController.PlayActivationEffect(this);

            if (coinEffectController) coinEffectController.PlayCoinTakeInteractionEffect();
        }
    }

    /// <summary>проверяет, является ли коллайдер мячиком</summary>
    protected bool IsBall(Collider2D coll)
    {
        if (coll == null) return false;
        var ball = coll.gameObject.GetComponent<BallController>();
        if (ball == null) return false;
        if (ball.fake) return false;
        return true;
    }

    private void DisableAfterShooting(GameState state)
    {
        if (state as ShootingState != null && !destroyOnTouch && touchedAtLevel)
        {
            if (gameObject.activeSelf) coinEffectController.PlayCoinDestroyEffect(this);
            gameObject.SetActive(false);
        }
    }

    public override void ManageDestroyOnRestore()
    {
        base.ManageDestroyOnRestore();
        StartCoroutine(SetSmallSizeAndDisable());
    }

    IEnumerator SetSmallSizeAndDisable()
    {
        SpriteTweeners.SpriteScaleCrossFromValueToValue(this, transform, 1, 0, 0.2f);
        yield return new WaitForSeconds(0.21f);
        gameObject.SetActive(false);
        yield break;
    }

}
