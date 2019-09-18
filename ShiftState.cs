using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>стейт сдвига вниз</summary>
public class ShiftState : GameState
{
    public static bool LoadItemsOnEndOfTurn;

    /// <summary>крмвая перехода</summary>
    public AnimationCurve shiftingCurve;

    /// <summary>кривая смещения всех блокв сразу</summary>
    public AnimationCurve allBrickShiftngCurve;

    /// <summary>время, за которое линия сдвигается вниз</summary>
    public float shiftTime;

    /// <summary>максимальный разброс по времени для сдвига объектов одной линии</summary>
    public float lineShiftingDeltaTime;

    public override void StartState(StateMachine sMachine)
    {
        base.StartState(sMachine);

        machine.controller.cubeManager.SaveCurrentLevelState();
        machine.controller.player.MoveOnLandPosition(); //игрок движется к точке (если не успел за время игры)

        //нет объектов на сцене - пропускае сдвиг
        if (!machine.controller.cubeManager.GetItemList<ItemController>().ToList().Any())
            Ended = true;
        else
            machine.controller.StartCoroutine(ShiftController()); //начинаем сдвиг
    }

    public override void Finish()
    {
        base.Finish();
        var allItemsInLine = machine.controller.cubeManager.GetRow<ItemController>(machine.controller.data.turnCount - 6, false);
        int totalHp = allItemsInLine.Sum(item => item.hitCounter);
        if (!machine.controller.blockLineEffectChanging)
            machine.controller.ManageLineEffect(totalHp > 0);

        machine.controller.cubeManager.SetBlocksPixelPerfect();
        if (LoadItemsOnEndOfTurn)
        {
            var campaignState = (machine.ActiveState as CampaignMissionState);
            if(campaignState != null)
                campaignState.LoadQuededItems();
            LoadItemsOnEndOfTurn = false;
        }
    }

    public float EasingFunction(float currTime, float totalTime)
    {
        return shiftingCurve.Evaluate(currTime / totalTime) - shiftingCurve.Evaluate((currTime - Time.deltaTime) / totalTime);
    }

    public float EasingFunctionForAllBricks(float currTime, float totalTime)
    {
        return allBrickShiftngCurve.Evaluate(currTime / totalTime) - allBrickShiftngCurve.Evaluate((currTime - Time.deltaTime) / totalTime);
    }

    #region Shift Coroutines

    /// <summary>контролирует сдвиг конкретного элемента игрового поля с указанной задержкой</summary>
    private IEnumerator ItemShiftController(ItemController cube, float cubeShiftingDelay)
    {

        yield return new WaitForSeconds(cubeShiftingDelay);
        var currTime = 0f;
        while (currTime < shiftTime)
        {
            cube.gameObject.transform.localPosition -= Vector3.up * EasingFunction(currTime, shiftTime);
            currTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        cube.SetYPosition();
    }

    /// <summary>контролирует гизонтальный сдвиг движущихся блоков для коррекции позиции</summary>
    private IEnumerator MovingBlocksHorizontalShiftController(ItemController movingBlock, float shiftDelay)
    {
        var currPosX = movingBlock.transform.position.x;
        List<CubeController> itemsInRow = new List<CubeController>();
        itemsInRow = machine.controller.cubeManager.GetRow<CubeController>(movingBlock.level).ToList();
        var idealPosX = movingBlock.index + BrickManager.startRow.x;
        foreach (var item in itemsInRow)
        {
            if (item.index == movingBlock.index && item != movingBlock)
            {
                MovingController m = movingBlock as MovingController;

                int newIndex = machine.controller.cubeManager.GetFreeCellInRow(m.level)[0];
                int minimal = Mathf.Abs(newIndex - m.index);
                foreach(var indexValue in machine.controller.cubeManager.GetFreeCellInRow(m.level))
                {
                    if(Mathf.Abs(indexValue - m.index) < minimal)
                    {
                        minimal = Mathf.Abs(indexValue - m.index);
                        newIndex = indexValue;
                    }
                }
                m.index = newIndex;
                idealPosX = m.index + BrickManager.startRow.x;
                break;
            }
        }

        var deltaPosX = idealPosX - currPosX;
        yield return new WaitForSeconds(shiftDelay);
        var currTime = 0f;
        while (currTime < shiftTime)
        {
            movingBlock.gameObject.transform.localPosition += Vector3.right * (deltaPosX) * Time.deltaTime / shiftTime;
            currTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        var pos = movingBlock.gameObject.transform.localPosition;
        movingBlock.gameObject.transform.localPosition = new Vector3(movingBlock.index + BrickManager.startRow.x, pos.y, pos.z);
        yield break;
    }

    /// <summary>контролирует сдвиг всех элементов игрового поля</summary>
    private IEnumerator ShiftController()
    {
        for (var line = machine.controller.data.turnCount - GameController.blockInRow; line < machine.controller.data.turnCount; line++)
        {
            var allItemsInLine = machine.controller.cubeManager.GetRow<ItemController>(line, true);

            var allMovingInLine = machine.controller.cubeManager.GetItemList<MovingController>().ToList();

            var lineDelay = 0f;

            foreach (var item in allItemsInLine)
            {
                var itemShiftingDelay = Random.Range(0f, lineShiftingDeltaTime);
                if (itemShiftingDelay > lineDelay) lineDelay = itemShiftingDelay;


                machine.controller.StartCoroutine(ItemShiftController(item, itemShiftingDelay));
                if (allMovingInLine.Contains(item))
                    machine.controller.StartCoroutine(MovingBlocksHorizontalShiftController(item, itemShiftingDelay));
            }
            yield return new WaitForSeconds(lineDelay);
        }
        yield return new WaitForSeconds(shiftTime);

        Ended = true;
    }

    #endregion

    #region Rstoration

    bool forcingMOvement = false;
    public void StartStateImmediately()
    {
        forcingMOvement = true;
        StartState(machine);
        forcingMOvement = false;
    }

    #endregion
}
