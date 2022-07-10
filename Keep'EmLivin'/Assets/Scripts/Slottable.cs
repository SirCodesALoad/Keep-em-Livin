using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slottable : MonoBehaviour
{

    public Slot slot;
    public AllyPostionSlot postion;
    public SpriteRenderer sprite;

    private void Start()
    {
        if(slot != null && postion != null)
        {
            slot.SetNewOccupier(this);
            RecenterPostion();
        }
        if(sprite == null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }
    }

    public void RecenterPostion()
    {
        gameObject.transform.parent = slot.transform;
        gameObject.transform.position = slot.transform.position;
        if(sprite != null)
        {
            sprite.sortingLayerID = slot.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingLayerID;
            sprite.sortingOrder = slot.GetComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder;
        }

    }

    public void ChangeSlot(AllyPostionSlot pos, Slot aSlot)
    {
        postion = pos;
        //We need to free the slot up first.
        slot.SetNewOccupier(null);
        slot = aSlot;
        aSlot.SetNewOccupier(this);
        RecenterPostion();

    }


}
