using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{

    public Slottable occupier;
    public AllyPostionSlot postion;
    public Image image;
    public BoxCollider2D col;


    private void Start()
    {
        postion = transform.parent.GetComponent<AllyPostionSlot>();
        image = GetComponent<Image>();
        image.enabled = false;
        col = GetComponent<BoxCollider2D>();
        col.enabled = false;
    }

    public void SetNewOccupier(Slottable newOccupier)
    {
        occupier = newOccupier;
    }


}
