using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// State를 바꾸기 위해 사용
/// </summary>
[Serializable]
public class Transition
{
    public Decision decision;
    public State trueState;
    public State falseState;
}
