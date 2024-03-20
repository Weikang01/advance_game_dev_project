using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardConfig
{
    public static KeyCode JumpKey = KeyCode.Space;
    public static KeyCode LeftKey = KeyCode.A;
    public static KeyCode LeftKey2 = KeyCode.LeftArrow;
    public static KeyCode RightKey = KeyCode.D;
    public static KeyCode RightKey2 = KeyCode.RightArrow;
}

public class GameConfig
{
    public static KeyboardConfig KeyboardConfig = new KeyboardConfig();
}
