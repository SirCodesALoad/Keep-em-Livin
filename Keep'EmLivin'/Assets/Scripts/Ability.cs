using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{

    public new string name;
    public float cooldownTime;
    public float activeTime; //The current time the ability has been active.
    public float maxDuration;
    public float castTime;
    public AbilityState state = AbilityState.ready;

    public virtual void ActivateAbility() {}
    public virtual void BeginCooldown() {}
    public virtual void AbilityEnd() { }



}

public class OvertimeAbility : Ability
{

    public float tick; //how much time represents a tick? Once every second, every two seconds?


    public virtual void AbilityTick() { }




}

public enum AbilityState
{
    ready,
    active,
    cooldown
}