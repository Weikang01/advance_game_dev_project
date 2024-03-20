using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EventListener: UnitySingleton<EventListener>
{
    private bool m_is_space_down = false;

    void Update()
    {
        // Update the key states each frame
        CaptureKeyStates();
    }

    void CaptureKeyStates()
    {
        // get current pressed keys
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_is_space_down = true;
        }
    }

    public bool IsSpaceDown() {
        bool r = m_is_space_down;
        m_is_space_down = false;
        CaptureKeyStates();
        return r;
    }
}
