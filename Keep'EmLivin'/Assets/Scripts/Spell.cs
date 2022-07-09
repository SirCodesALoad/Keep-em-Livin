using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;
using UnityEngine.Events;

//Actions as a monobehiavour?
//Create somethiing like Damage DATA to carry all the data.
[CreateAssetMenu]
public class Spell : ScriptableObject
{
    [SerializeField] public new string name = "New Spell";
    [SerializeField] public float castTime = 0f;
    [SerializeField] public float amount = 0f;
    [SerializeField] public int amountOfTargets = 0;
    [SerializeField] public float amountDuring = 0f;
    [SerializeField] public bool allowOverheal = false;
    [SerializeField] public bool interuptable = true;

    //[SerializeField] private PlayerAction action;
    [SerializeField] private UnityEvent<Spell, TargetData> startCastingEvents;
    [SerializeField] private UnityEvent<Spell, TargetData> interuptCastingEvents;
    [SerializeField] private UnityEvent<Spell, TargetData> finishCastingEvents;
    [SerializeField] private UnityEvent<Spell, TargetData> duringCastingEvents;

    public void StartCasting(TargetData data)
    {
        startCastingEvents.Invoke(this, data);
    }
    public void DuringCasting(TargetData data)
    {
        duringCastingEvents.Invoke(this, data);
    }
    public void FinishCasting(TargetData data)
    {
        finishCastingEvents.Invoke(this, data);
    }
    public void InteruptCastingTargetData (TargetData data)
    {
        interuptCastingEvents.Invoke(this, data);
    }
}

public class TargetData
{
    public Character target;
    public Character caster;

    public TargetData(Character Target, Character Caster)
    {
        target = Target;
        caster = Caster;
    }


}

enum SpellState
{
    ready,
    casting,
    cooldown
}

