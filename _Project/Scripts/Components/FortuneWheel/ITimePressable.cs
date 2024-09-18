using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface ITimePressable
{
    public float NeedPressTime { get; }
    public FloatReactiveProperty PressTime { get; }
}
