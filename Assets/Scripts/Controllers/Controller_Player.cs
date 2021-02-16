using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Player : MonoBehaviour
{
    [SerializeField] private BoxCollider2D ref_collider_self = null;

    [SerializeField] private LayerMask mask_collision = 0;

    private enum Player_Number
    {
        Player_One,
        Player_Two
    }

    [SerializeField] private Player_Number player_number = Player_Number.Player_One;

    [SerializeField] private float move_inc = 1f;
    [SerializeField] private float move_cd_seconds = 1f;

    private float last_move_time = 0;

    // Looks for collisions
    private bool CheckMove(float x_inc, float y_inc)
    {
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x + x_inc, transform.position.y + y_inc), new Vector2(ref_collider_self.size.x - 1, ref_collider_self.size.y - 1), 0, mask_collision);
        
        return (hit == null);
    }

    private void Update()
    {
        float e_time = Time.time - last_move_time;
        if (e_time > move_cd_seconds)
        {
            float x_inc = 0f;
            float y_inc = 0f;

            if (player_number == Player_Number.Player_One)
            {
                x_inc = move_inc * (Input.GetButton("P1_Left") ? -1f : (Input.GetButton("P1_Right") ? 1f : 0f));
                y_inc = move_inc * (Input.GetButton("P1_Down") ? -1f : (Input.GetButton("P1_Up") ? 1f : 0f));
            }
            else if (player_number == Player_Number.Player_Two)
            {
                x_inc = move_inc * (Input.GetButton("P2_Left") ? -1f : (Input.GetButton("P2_Right") ? 1f : 0f));
                y_inc = move_inc * (Input.GetButton("P2_Down") ? -1f : (Input.GetButton("P2_Up") ? 1f : 0f));
            }

            if (CheckMove(x_inc, y_inc))
            {
                transform.Translate(new Vector2(x_inc, y_inc));
            }

            // Put move on CD
            last_move_time = Time.time;
        }
    }
}
