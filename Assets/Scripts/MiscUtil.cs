﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscUtil
{
    // Send FSM event to all FSMs that is in the same game object with the input fsm / monobehavior
    public static void MySendEventToAll(this Behaviour mb, string name)
    {
        if (!mb)
            return;

        var go = mb.gameObject;

        MySendEventToAll(go, name);
    }

    // Send FSM event to all FSMs that is in the game object
    public static void MySendEventToAll(this GameObject go, string name)
    {
        if (!go)
            return;

        foreach (var i in go.GetComponents<PlayMakerFSM>())
        {
            i.SendEvent(name);
        }
    }

}
