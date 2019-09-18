using UnityEngine;

public class CrossLaserCoinController : BaseCoinController
{

    public override void Init(int newLevel, int newIndex, int newHitPoint)
    {
        base.Init(newLevel, newIndex, newHitPoint);

        GetComponent<LaserEffects>().InitializeRaysForCross();
    }

    public override void OnMoveDownEnded()
    {
        GetComponent<LaserEffects>().UpdateRaysPositions();
    }

    public override void Load(BrickManager newManager, GameSave.GameItem saveItem)
    {
        base.Load(newManager, saveItem);

        GetComponent<LaserEffects>().InitializeRaysForCross();
    }

    public override GameSave.GameItem Save()
    {
        var result = base.Save();
        result.itemType = GameSave.ItemType.crossLaserCoin;
        return result;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsBall(collision)) return;

        manager.ExecuteCrossLineDamageEffect(index, level);
        GetComponent<LaserEffects>().StartLaserEffectsForCross();
        base.OnTriggerEnter2D(collision);
    }
}
