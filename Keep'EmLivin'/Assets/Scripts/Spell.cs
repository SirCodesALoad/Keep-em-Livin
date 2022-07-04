using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

//Actions as a monobehiavour?
//Create somethiing like Damage DATA to carry all the data.

public class Spell : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private float castTime;
    [SerializeField] private float timeSpentCasting;
    [SerializeField] private PlayerAction action;
    //private SpellState state = SpellState.ready;

    public delegate void SpellStateEventHandler(Spell sp, Character target);
    public delegate void UpdateEventHandler(Character target, float deltaTime);

    public virtual event SpellStateEventHandler StartCasting; //We start casting the spell.
    public virtual event SpellStateEventHandler Interupt; //We're interrupted casting the spell.
    public virtual event SpellStateEventHandler FinishCasting; //We finish casting the spell.
    public virtual event UpdateEventHandler DuringCasting; //We do something during the casting of the spell.


    public static void StartCasting_SingleTargetHeal(Spell sp, Character target)
    {

    }

}

enum SpellState
{
    ready,
    casting,
    cooldown
}

