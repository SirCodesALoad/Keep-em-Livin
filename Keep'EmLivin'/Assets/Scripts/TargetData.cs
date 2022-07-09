using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetData
{

    private Character[] targetData;

    public TargetData(Character[] data)
    {
        targetData = data;

    }


    public Character GetCharacterAt(int i)
    {
        if(targetData[i] != null)
        {
            return targetData[i];
        }

        return null;
    }


}
