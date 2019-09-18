using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public GameController controller;

    /// <summary>состояния, которые загружены</summary>
    public List<GameState> States;
    private int currentStateNumber;

    /// <summary>текущее состояние</summary>
    public GameState ActiveState;

    /// <summary>стейт, который должен быть ПРИНУДИТЕЛЬНО включен после текущего (обычно null)</summary>
    private GameState forcedNexState;

    public bool IsCampaign { get { return ActiveState as CampaignMissionState != null; } }

    public bool IsTutorial { get { return ActiveState as TutorialState != null; } }

    public bool IsInStateOf<T>() where T : GameState
    {
        return ActiveState as T != null;
    }

    public StateMachine(GameController gController)
    {
        controller = gController;

        //ставим номер в -1 для загрузки уровня
        currentStateNumber = -1;
        preloadedStatesEnded = true;
    }

    public void Init(List<GameState> states)
    {
        preloadedStatesEnded = true;
        if (states.Count == 0)
            States = null;
        else
        {
            States = states;
            foreach (var st in States)
            {
                st.Began = false;
                st.Ended = false;
            }
            currentStateNumber = 0;
            preloadedStatesEnded = false;
        }
    }

    /// <summary>обновляется из GameController (там свой порядок действий)</summary>
    public void Update()
    {
        //если нет текущего стейта - грузим новый
        if (ActiveState == null)
        {
            ActiveState = GetNextState();
        }
        else
        {
            //если текущий стейт еще не начался - стартуем его
            if (!ActiveState.Began)
            {
                ActiveState.StartState(this);
                controller.OnStateStarted?.Invoke(ActiveState);
            }
            //если текущий стейт уже закончился - заканчиваем его и берем следующий
            else if (ActiveState.Ended)
            {
                ActiveState.Finish();
                controller.OnStateEnded?.Invoke(ActiveState);
                ActiveState = GetNextState();

            }
            //обновляем текущий стейт
            else
            {
                ActiveState.Update();
            }
        }
    }
    bool preloadedStatesEnded;
    /// <summary>получить следующее состояние</summary>
    private GameState GetNextState()
    {
        if(forcedNexState != null)
        {
            var result = forcedNexState;
            result.Began = false;
            result.Ended = false;
            forcedNexState = null;
            return result;
        }
        else if (States == null || States.Count <= currentStateNumber)
        {
            
            if (preloadedStatesEnded == false)
            {
                preloadedStatesEnded = true;
                currentStateNumber = -1;
            }
            //шагов больше не осталось, берем стандартные
            return GetStateFromStandartSequence();
        }
        else if(preloadedStatesEnded == false)
        {
            if(currentStateNumber < States.Count)
                return States[currentStateNumber++];
        }
        return GetStateFromStandartSequence();
    }

    // стандартная последовательность (если нет шагов туториала, подсказок, специальных действий и тд) такая:
    // (грузим уроверь, если есть) => генерируем линию -> прицеливаемся -> стреляем и считаем результат -> сдвигаем линию вниз -> повторяем

    /// <summary>сколько шагов в стандартной последовательности</summary>
    private const int standartSequnceStatesCount = 4;

    /// <summary>возвращает следующий стейт для стандартной последовательности</summary>
    private GameState GetStateFromStandartSequence()
    {
        GameState newActivestate = null;

        //-1 = загрузка игры и отображение предыдущео результата
        if (currentStateNumber == -1)
        {
            newActivestate = GetStandartState<LoadingState>();
            newActivestate.Began = false;
            newActivestate.Ended = false;
        }

        if (ActiveState)
        {
            if (ActiveState.GetType() == typeof(AimingState))
            {
                //после прицеливания -> стрельба
                newActivestate = GetStandartState<ShootingState>();
            }
            else if (ActiveState.GetType() == typeof(ShootingState))
            {
                //после стрельбы -> сдвиг
                newActivestate = GetStandartState<ShiftState>();
            }
            else if (ActiveState.GetType() == typeof(ShiftState))
            {
                //после сдвига -> генерация
                newActivestate = GetStandartState<LineCreatingstate>();
            }
            else if(ActiveState.GetType() == typeof(LoadingState))
            {
                //после загрузки -> прицел
                newActivestate = GetStandartState<AimingState>();
            }
            else if(ActiveState.GetType() == typeof(LineCreatingstate))
            {
                //после создания линии -> сохранение результата
                newActivestate = GetStandartState<ResultCalculatingState>();
            }
            else if(ActiveState.GetType() == typeof(ResultCalculatingState))
            {
                //после подсчета результата - стейт прицеливания
                newActivestate = GetStandartState<AimingState>();
            }
        }

        if (!newActivestate) return null;

        currentStateNumber++;

        newActivestate.Began = false;
        newActivestate.Ended = false;
        return newActivestate;
    }

    /// <summary>возврящает стандартный стейт указанного типа</summary>
    public GameState GetStandartState<T>()
    {
        foreach (var state in controller.StandartStates)
            if (state is T) return state;

        return null;
    }

    /// <summary>принудительно закрывает текущий стейт и начинает новый</summary>
    public void ForceNextState()
    {
        if(ActiveState)
            ActiveState.Ended = true;
    }

    /// <summary>принудительно закрывает текущий стейт и начинает новый</summary>
    public void  ForceNextState<T>()
    {
        forcedNexState = GetStandartState<T>();
        ForceNextState();
    }

    #region Additional Player Interaction

    /// <summary>передает стейту положение тапа, если игрок удерживает</summary>
    public void OnPlayerTouchScreen(Vector3 position)
    {
        if (ActiveState) ActiveState.OnPlayerTap(position);
    }

    /// <summary>передает стейту информацию, что палец был поднят,
    ///  должно вызываться при поднятии пальца с экрана</summary>
    public void OnPlayerReleaseFinger()
    {
        if (ActiveState) ActiveState.OnPlayerReleasedFinger();
    }

    /// <summary>передает стейту информацию об использовании бустера</summary>
    public void OnPlayerActivatedBooster()
    {
        if (ActiveState) ActiveState.OnPlayerUsedBooster();
    }

    #endregion
}
