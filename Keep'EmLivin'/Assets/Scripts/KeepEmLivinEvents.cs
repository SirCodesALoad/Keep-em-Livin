using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class KeepEmLivinEvents
{
    public static HealEvent healTarget = new HealEvent();


}

public class HealEvent : UnityEvent<HealEventData> { }

public class HealEventData
{
    public GameObject healer;
    public GameObject target;
    public float amount;
    public bool allowOverhealing;

    public HealEventData(GameObject healer, GameObject target, float amount, bool allowOverhealing)
    {
        this.healer = healer;
        this.target = target;
        this.amount = amount;
        this.allowOverhealing = allowOverhealing;
    }
}
