using System;
using UnityEngine;

public class GameState : ScriptableObject
{

    protected StateMachine machine;


    /// <summary>стартовало ли состояние</summary>
    [HideInInspector] public bool Began;

    /// <summary>закончилось и состояние</summary>
    [HideInInspector] public bool Ended;

    public virtual void StartState(StateMachine sMachine)
    {
        machine = sMachine;
        Began = true;
        Ended = false;
    }

    public virtual void Update()
    {

    }

    public virtual void Finish()
    {
        Ended = true;
    }

    public void SetMachine(StateMachine m)
    {
        machine = m;
    }

    #region Player Interactions

    /// <summary>палец игрока на позиции</summary>
    public virtual Vector3 OnPlayerTap(Vector3 tapPosition)
    {
        return Vector3.zero;
    }

    /// <summary>игрок опустил палец на экран</summary>
    public virtual void OnPlayerPutFinger()
    {

    }

    /// <summary>игрок убрал палец с экрана</summary>
    public virtual void OnPlayerReleasedFinger()
    {

    }

    /// <summary>игрок использовал бустер</summary>
    public virtual void OnPlayerUsedBooster()
    {

    }


    #endregion

}