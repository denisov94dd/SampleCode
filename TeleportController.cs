using UnityEngine;

public class TeleportController : CubeController
{
    private Animator animator;
    private Vector3 pos;
    private SpiderJumper spiderJumper;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void Init(int newLevel, int newIndex, int newHitPoint)
    {
        base.Init(newLevel, newIndex, newHitPoint);
        manager.game.OnStateEnded += Jump;
        manager.game.OnStateEnded += ChooseNewIndex;
    }

    public override void Load(BrickManager newManager, GameSave.GameItem saveItem)
    {
        base.Load(newManager, saveItem);
        manager.game.OnStateEnded += Jump;
        manager.game.OnStateEnded += ChooseNewIndex;
    }

    public override void OnMoveDownEnded()
    {
        base.OnMoveDownEnded();
    }

    public override GameSave.GameItem Save()
    {
        var result = base.Save();
        result.itemType = GameSave.ItemType.teleport;
        return result;
    }

    private void ChooseNewIndex(GameState state)
    {
        if (state as ShootingState == null)
            return;
        if (manager.GetFreeCellInRow(level).ToArray().Length == 0 || manager.TurnCount - level <= 0)
            return;

        var allVacant = manager.GetFreeCellInRow(level).ToArray();
        var rand = Random.Range(0, allVacant.Length);
        index = allVacant[rand];
    }

    private void Jump(GameState state = null)
    {
        if ((state != null) && (state as ShiftState == null || transform.position.y < -2f))
            return;

        if (manager.GetFreeCellInRow(level).ToArray().Length == 0 || !gameObject.activeSelf || manager.TurnCount - level <= 0)
            return;

        var newPos = BrickManager.startRow;
        newPos.x += index;
        newPos.y -= manager.TurnCount - level;

        if (Mathf.Abs(newPos.x - transform.position.x) < 0.5f)
            return;

        if (manager.TurnCount - level != GameController.blockInRow)
            animator.SetTrigger("Teleport");

        manager.CreateEffect<BlockTeleportation>(transform.position, Quaternion.identity, null);
        spiderJumper = manager.CreateEffect<SpiderJumper>(transform.position, Quaternion.identity, null);
        newPos += manager.transform.position;
        SpiderWebPreEffect webPreEffect = manager.CreateEffect<SpiderWebPreEffect>(newPos, Quaternion.identity, null);
        spiderJumper.StartMovement(newPos, gameObject, webPreEffect);
    }

    private void JumpEvent()
    {
        if (manager.GetFreeCellInRow(level).ToArray().Length == 0 || manager.TurnCount - level <= 0)
            return;
        
        var pos = BrickManager.startRow;
        pos.x += index;
        pos.y -= manager.TurnCount - level;
        transform.localPosition = pos; 
        manager.CreateEffect<BlockTeleportation>(transform.position, Quaternion.identity, null);
    }

    public override void PlayGetHitSound()
    {
        SoundManager.instance.OnSpiderHit();
    }

    public override void OnGetDamage(BallController ball, int damagePercentage = 0, DragonBonusType damageType = DragonBonusType.None)
    {
        base.OnGetDamage(ball, damagePercentage, damageType);
        if (hitCounter <= 0)
        {
            SoundManager.instance.OnSpiderDestroy();
            manager.game.OnStateEnded -= Jump;
            manager.game.OnStateEnded -= ChooseNewIndex;
            manager.game.achievementController.OnBrickDeleted(GameSave.ItemType.teleport, level, ball);
        }
    }

    public override void OnReachEnd()
    {
        manager.game.OnStateEnded -= Jump;
        manager.game.OnStateEnded -= ChooseNewIndex;
        base.OnReachEnd();
    }

    public override void PlayDestroyEffect()
    {
        if (spiderJumper)
            spiderJumper.DisableSelf();
        manager.CreateEffect<TeleportExplosionEffect>(transform.position, Quaternion.identity, null);
    }

    public override void MakeSpecialActionOnStrangeAppear(bool withSpecialPrep = false)
    {
        Jump();
        base.MakeSpecialActionOnStrangeAppear(withSpecialPrep);
    }
}
