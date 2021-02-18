using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Superform : MonoBehaviour
{
    private Color player_one_color;
    private Color player_two_color;

    public void Init(Color p1_color, Color p2_color)
    {
        player_one_color = p1_color;
        player_two_color = p2_color;
    }
}
