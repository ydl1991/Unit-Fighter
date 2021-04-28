using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtil
{

    public static IEnumerator MoveToPosInSec(Transform originTrans, Vector3 target, float seconds)
    {
        Vector3 curPos = originTrans.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / seconds;
            originTrans.position = Vector3.Lerp(curPos, target, t);
            yield return null;
        }

        originTrans.position = target;
    }

    public static IEnumerator RotateToTargetInSec(Transform originTrans, Vector3 rot, float seconds)
    {
        Quaternion curRot = originTrans.rotation;
        Quaternion targetRot = Quaternion.Euler(rot);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / seconds;
            originTrans.rotation = Quaternion.Lerp(curRot, targetRot, t);
            yield return null;
        }

        originTrans.rotation = targetRot;
    }

    public static void Shuffle<T>(List<T> list, XOrShiftRNG rng)
    {
        int n = list.Count;  
        while (n > 1) 
        {  
            n--;  
            int k = rng.RandomIntRange(0, n);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        } 
    }

    public static void Shuffle<T>(T[] list, XOrShiftRNG rng)
    {
        int n = list.Length;  
        while (n > 1) 
        {  
            n--;  
            int k = rng.RandomIntRange(0, n);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        } 
    }
}
