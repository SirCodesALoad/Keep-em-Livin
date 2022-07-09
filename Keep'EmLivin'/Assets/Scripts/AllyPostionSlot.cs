using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyPostionSlot : MonoBehaviour
{

    public GameObject[] slots;



    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[transform.childCount];

        if (transform.childCount >= 2)
        {
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                slots[i] = transform.GetChild(i).GetComponent<GameObject>();
            }
        }

    }

    public Character GetCharacterInSlot (int slot)
    {
        if (slots[slot] != null && slots[slot].transform.GetChild(0).GetComponent<Character>() != null)
        {
            return slots[slot].transform.GetChild(0).GetComponent<Character>();
        }
        return null;

    }
    

}
