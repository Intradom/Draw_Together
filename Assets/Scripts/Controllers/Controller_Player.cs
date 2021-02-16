using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Player : MonoBehaviour
{
    [SerializeField] private Transform ref_transform_self = null;

    private enum Player_Number
    {
        Player_One,
        Player_Two
    }

    [SerializeField] private Player_Number pn = Player_Number.Player_One;

    [SerializeField] private float move_inc = 1f;
    [SerializeField] private float move_cd_seconds = 1f;

    private float last_move_time = 0;

    private void Update()
    {
        float e_time = Time.time - last_move_time;
        if (e_time > move_cd_seconds)
        {
            float x_inc = 0f;
            float y_inc = 0f;

            if (pn == Player_Number.Player_One)
            {
                x_inc = move_inc * (Input.GetButton("P1_Left") ? -1f : (Input.GetButton("P1_Right") ? 1f : 0f));
                y_inc = move_inc * (Input.GetButton("P1_Down") ? -1f : (Input.GetButton("P1_Up") ? 1f : 0f));
            }
            else if (pn == Player_Number.Player_Two)
            {
                x_inc = move_inc * (Input.GetButton("P2_Left") ? -1f : (Input.GetButton("P2_Right") ? 1f : 0f));
                y_inc = move_inc * (Input.GetButton("P2_Down") ? -1f : (Input.GetButton("P2_Up") ? 1f : 0f));
            }

            ref_transform_self.Translate(new Vector2(x_inc, y_inc));

            // Put move on CD
            last_move_time = Time.time;
        }
    }
}
