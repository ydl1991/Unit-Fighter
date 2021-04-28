using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil
{
    // convert from system rotation value to inspector-seen rotation value
    public static float WrapAngle(float angle)
    {
        angle %= 360;
        if(angle > 180)
            return angle - 360;

        return angle;
    }
}
