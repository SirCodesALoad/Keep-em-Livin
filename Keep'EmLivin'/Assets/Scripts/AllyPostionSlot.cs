using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyPostionSlot : MonoBehaviour
{

    [SerializeField] private Slot[] slots;



    // Start is called before the first frame update
    void Start()
    {
        slots = new Slot[transform.childCount];


        for (int i = 0; i < transform.childCount; i++)
        {
            slots[i] = transform.GetChild(i).GetComponent<Slot>();
        }

    }


    public Slot[] GetAllSlots()
    {
        return slots;
        
    }
    public Slot GetSlotsNearestOccupiedNeighbour(Slot aSlot)
    {
        for (int i = 0; i < slots.Length - 1; i++)
        {
            if (slots[i].occupier != null && slots[i] != aSlot)
            {
                return slots[i];
            }
        }
        return null;

    }

    public Slot GetEmptySlot()
    {

        for (int i = 0; i < slots.Length - 1; i++)
        {
            if (slots[i].occupier == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    public void AddToSlot(Slottable thingToTakeUpSlot)
    {
        ///Adds a slottable object to an empty slot.
        Slot slot = GetEmptySlot();

        if (slot != null) {

            thingToTakeUpSlot.ChangeSlot(this, slot);
        }

    }

    public void AddToSlot(Slottable thingToTakeUpSlot, Slot slot)
    {
        ///Adds a slottable object to specific slot. Note we don't care what's there already.
        thingToTakeUpSlot.ChangeSlot(this, slot);

    }

    public Character GetCharacterInSlot (int slot)
    {
        if (slots[slot] != null && slots[slot].transform.GetChild(0).GetComponent<Character>() != null)
        {
            return slots[slot].transform.GetChild(0).GetComponent<Character>();
        }
        return null;

    }

    public Slottable GetSlottableInSlot(int slot)
    {
        if (slots[slot] != null && slots[slot].transform.GetChild(0).GetComponent<Slottable>() != null)
        {
            return slots[slot].transform.GetChild(0).GetComponent<Slottable>();
        }
        return null;

    }


}
