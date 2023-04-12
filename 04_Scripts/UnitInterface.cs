using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAction
{
    void Walk();
    void Attack();
    void Dead();
}

public interface IGetter
{
    int GetHealth();
}

public enum STATE
{
    WALK,
    ATTACK,
    DEAD,
}

