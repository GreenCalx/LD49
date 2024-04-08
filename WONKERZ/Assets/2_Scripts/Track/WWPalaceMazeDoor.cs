using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WWPalaceMazeDoor : WkzDoor
{
    [Header("WWPalaceMazeDoor")]
    // MazeSpecific
    public WWPalaceMaze.COL_SHAPES From_Shape;
    public WWPalaceMaze.COL_SHAPES To_Shape;

    public int From_Number;
    public int To_Number;

    public bool IsSameNumber() { return From_Number == To_Number; }
    public bool IsSameShape() { return From_Shape == To_Shape; }

    void Start()
    {
        OpenDoor();
    }

}
