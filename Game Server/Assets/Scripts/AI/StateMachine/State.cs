using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    public string stateName { get; protected set; }
    public virtual void UpdateState() {}
    public virtual void EnterState() {}
    public virtual void ExitState() {}
    public virtual bool StateDone() { return false; }
}

