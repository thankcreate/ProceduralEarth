using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper 
{    
    public static T Looped<T>(this T[] array, int index)
    {
        while (index < 0)
        {
            index += array.Length;
        }
        if (index >= array.Length)
        {
            index %= array.Length;
        }
        return array[index];
    }
}
