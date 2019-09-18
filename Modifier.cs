using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Модификаторы - система управляемых воздействий на игровой процесс (подкрутка)</summary>
[System.Serializable]
public class Modifier
{
    public int modID;
    public ModifierZone Zone;
    public ModifierApplyMoment ApplyMoment;
    public ModifierTargetValue TargetValue;
    public ModifierValueChangerType ChangerType;
    public ModifierSpecialImpactEffect specialImpactEffect;

    public float ImpactValue;
    public bool disposeModifierAfterEffectApplied;
    public bool impactHappened;

    public static bool operator == (Modifier mod1, Modifier mod2)
    {

        if (mod1 is null) return mod2 is null;
        if (mod2 is null) return mod1 is null;

        return mod1.modID == mod2.modID;
    }

    public static bool operator !=(Modifier mod1, Modifier mod2)
    {
        if (mod1 is null) return !(mod2 is null);
        if (mod2 is null) return !(mod1 is null);

        return mod1.modID != mod2.modID;
    }

    public override bool Equals(object obj)
    {
        var mod = obj as Modifier;
        if(mod != null) return modID == mod.modID;

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

#region Modifier special data structs

public enum ModifierApplyMoment
{
    OnStart,
    OnLevelResultsCalculating,
    OnBallImpact,
}

public enum ModifierTargetValue
{
    BallDamage,
    LootAmount,
    BallAmount,
    FirstBallEffect
}

public enum ModifierValueChangerType
{
    simpleSum,
    appliedSum,
    simpleDiff,
    appliedDiff,
    effect
}

public enum ModifierZone
{
    Campaign,
    Cave,
    All
}

public enum ModifierSpecialImpactEffect
{
    ClearLineEffect,
    LightningEffect,
    PoisonSpecialImpactEffect
}

#endregion
