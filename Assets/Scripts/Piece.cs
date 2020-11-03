using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public enum FaceColor
    {
        NONE,
        RED,
        GREEN,
        BLUE,
        ORANGE,
        YELLOW,
        WHITE,
    };

    [SerializeField] public List<FaceColor> FaceColors;
}
