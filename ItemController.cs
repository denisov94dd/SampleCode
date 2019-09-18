using UnityEngine;
using System.Collections.Generic;

/// <summary>Абастрактный класс для всех предметов на поле</summary>
public class ItemController : MonoBehaviour
{
    [HideInInspector] public int starterHitCounter;
    [HideInInspector] public int hitCounter;

    [HideInInspector] public int level;
    [HideInInspector] public int index;
    [HideInInspector] public int shiftDelay;
    [HideInInspector] public BrickManager manager;
    [HideInInspector] public GameSave.BlockOrientation orientation;

    public GameSave.ItemType currItemType;

    public virtual void Init(int newLevel, int newIndex, int newHitPoint)
    {
        gameObject.SetActive(true);
        level = newLevel;
        index = newIndex;
        shiftDelay = 0;

        var pos = BrickManager.startRow;
        pos.x += index;
        pos.y -= manager.TurnCount - (newLevel + shiftDelay);
        transform.localPosition = pos;

        manager.game.OnLevelChanged += OnMoveDownEnded;

        manager.game.OnElementAppearedOnScreen(this);
    }

    public virtual void Load(BrickManager newManager, GameSave.GameItem saveItem)
    {
        manager = newManager;
        gameObject.SetActive(true);
        level = saveItem.level;
        index = saveItem.index;
        hitCounter = saveItem.hitCounter;
        starterHitCounter = hitCounter;
        orientation = saveItem.orientation;
        shiftDelay = saveItem.shiftDelay;

        var pos = BrickManager.startRow;
        pos.x += index;
        pos.y -= manager.TurnCount - (saveItem.level + shiftDelay);
        transform.localPosition = pos;

        manager.game.OnLevelChanged += OnMoveDownEnded;

        manager.game.OnElementAppearedOnScreen(this);
    }

    public virtual int MoveDown()
    {
        if (manager.TurnCount - (level + shiftDelay) >= GameController.blockInRow)
        {
            OnReachEnd();
            return hitCounter;
        }

        return 0;
    }

    /// <summary>выставляет правильную позицию для объекта на поле в зависимости от уровня</summary>
    public virtual void SetYPosition()
    {
        var pos = transform.localPosition;
        var y = BrickManager.startRow.y - (manager.TurnCount - (level + shiftDelay));
        pos.y = y;
        transform.localPosition = pos;
    }

    /// <summary>Метод, который вызывается после сдвига всех кубиков, делегат OnLevelChanged</summary>
    public virtual void OnMoveDownEnded()
    {

    }

    public virtual void OnReachEnd()
    {

    }

    public virtual GameSave.GameItem Save()
    {
        return new GameSave.GameItem()
        {
            itemType = GameSave.ItemType.cube,
            level = level,
            index = index,
            shiftDelay = shiftDelay,
            hitCounter = hitCounter,
            orientation = orientation
        };
    }

    /// <summary>возвращает список индексов, на которых находится объект</summary>
    public virtual List<int> GetPositionIndexes()
    {
        var positions = new List<int>(1) { index };
        return positions;
    }

    public virtual void ItemUpdate(float currentTime)
    {

    }

    public virtual void PlayDestroyEffect()
    {

    }

    public virtual void UpdateHitCouterText()
    {

    }

    protected Vector3 scaleVectorOnDisappearWhileRestoreLevel = new Vector3(0.1f, 1.65f, 0.1f);
    public virtual void ManageDestroyOnRestore()
    {

    }

    public virtual void MakeSpecialActionOnStrangeAppear(bool withSpecialPrep = false)
    {

    }

}
