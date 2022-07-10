using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotManager : MonoBehaviour
{

    [SerializeField] private AllyPostionSlot[] allyPostionSlots;
    public bool slotsVisible = false;

    [SerializeField] Color isFilledColour;
    [SerializeField] Color isEmptyColour;

    // Start is called before the first frame update
    void Start()
    {
        allyPostionSlots = new AllyPostionSlot[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            allyPostionSlots[i] = transform.GetChild(i).GetComponent<AllyPostionSlot>();
        }
        
    }

    public void ShowSlots()
    {
        Debug.Log("Showing slots");
        for (int i = 0; i < allyPostionSlots.Length; i++)
        {
            Slot[] slotsWithinAPostion = allyPostionSlots[i].GetAllSlots();
            for (int j = 0; j < slotsWithinAPostion.Length; j++)
            {
                slotsWithinAPostion[j].image.enabled = true;
                slotsWithinAPostion[j].col.enabled = true;
                if (slotsWithinAPostion[j].occupier == null)
                {
                    //This slot is empty!
                    slotsWithinAPostion[j].image.color = isEmptyColour;
                }
                else
                {
                    //This slot is Occupied!
                    slotsWithinAPostion[j].image.color = isFilledColour;
                }

            }
        }
        slotsVisible = true;
    }

    public void HideSlots()
    {
        for (int i = 0; i < allyPostionSlots.Length; i++)
        {
            Slot[] slotsWithinAPostion = allyPostionSlots[i].GetAllSlots();
            for (int j = 0; j < slotsWithinAPostion.Length; j++)
            {
                slotsWithinAPostion[j].image.enabled = false;
                slotsWithinAPostion[j].col.enabled = false;
            }
        }
        slotsVisible = false;
    }


    public void SwapSlottables(Slottable a, Slottable b)
    {
        ///Swaps two slottables around.
        Slot aSlot = a.slot;
        AllyPostionSlot aPostion = a.postion;
        a.ChangeSlot(b.postion,b.slot);
        b.ChangeSlot(aPostion, aSlot);

    }



}
