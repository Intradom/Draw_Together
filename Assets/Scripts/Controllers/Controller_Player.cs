using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Player : MonoBehaviour
{
    [SerializeField] private BoxCollider2D ref_collider_self = null;
    [SerializeField] private GameObject ref_superform = null;
    [SerializeField] private Behavior_Canvas script_canvas = null;
    

    // Tags
    [SerializeField] private string tag_player = "";
    [SerializeField] private string tag_wall = "";
    [SerializeField] private string tag_well = "";

    // Parameters
    [SerializeField] private LayerMask mask_collision = 0;
    [SerializeField] private Color starting_color = Color.white;

    private Color current_color;

    private enum Player_Number
    {
        Player_One,
        Player_Two
    }

    [SerializeField] private Player_Number player_number = Player_Number.Player_One;

    [SerializeField] private float move_inc = 1f;
    [SerializeField] private float move_cd_seconds = 1f;
    [SerializeField] private float held_thresh_seconds = 0.5f;

    private Vector2 move_dir = Vector2.zero;
    private float last_move_time = 0f;
    private float held_time = 0f;

    // Looks for collisions
    private bool CanMove(Vector2 move_dir)
    {
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x + move_dir.x, transform.position.y + move_dir.y), new Vector2(ref_collider_self.size.x - 1, ref_collider_self.size.y - 1), 0);
        
        if (hit)
        {
            if (hit.tag == tag_player)
            {
                if (hit.gameObject.GetComponent<Controller_Player>().CanTransform())
                {
                    hit.gameObject.GetComponent<Controller_Player>().Transform(current_color);
                    Destroy(this.gameObject);
                }
                else
                {
                    return false;
                }
            }
            else if (hit.tag == tag_well)
            {
                // Change color to well color
                current_color = hit.gameObject.GetComponent<Behavior_Well>().GetColor();

                return false;
            }
            else if (hit.tag == tag_wall)
            {
                return false;
            }
        }

        return true;
    }

    private void Move(Vector2 move_dir)
    {
        transform.Translate(move_dir);

        script_canvas.SetPixel(transform.position.x, transform.position.y, current_color);
    }

    private bool CanTransform()
    {
        // Check if transformation violates collision bounds
        int super_scale = Manager_Game.Instance.super_form_scale;
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y), new Vector2(ref_collider_self.size.x * super_scale - 1, ref_collider_self.size.y * super_scale - 1), 0, mask_collision);
        
        return (hit == null);
    }

    private void Transform(Color other_color)
    {
        var inst = Instantiate(ref_superform, transform.position, Quaternion.identity);
        Controller_Superform script_superform = inst.GetComponent<Controller_Superform>();

        if (player_number == Player_Number.Player_One)
        {
            script_superform.Init(current_color, other_color);
        }
        else 
        {
            script_superform.Init(other_color, current_color);
        }

        Destroy(this.gameObject);
    }

    private void Start()
    {
        current_color = starting_color;
    }

    private void Update()
    {
        string player_prefix = (player_number == Player_Number.Player_Two) ? "P2" : "P1";
        float x_dir = 0f;
        float y_dir = 0f;

        if (Input.GetButtonUp(player_prefix + "_Left") || Input.GetButtonUp(player_prefix + "_Right"))
        {
            move_dir.x = 0f;
            held_time = 0f;
        }
        if (Input.GetButtonUp(player_prefix + "_Down") || Input.GetButtonUp(player_prefix + "_Up"))
        {
            move_dir.y = 0f;
            held_time = 0f;
        }

        x_dir = move_inc * (Input.GetButtonDown(player_prefix + "_Left") ? -1f : (Input.GetButtonDown(player_prefix + "_Right") ? 1f : 0f));
        y_dir = move_inc * (Input.GetButtonDown(player_prefix + "_Down") ? -1f : (Input.GetButtonDown(player_prefix + "_Up") ? 1f : 0f));

        if (Mathf.Abs(x_dir) > 0f || Mathf.Abs(y_dir) > 0f) // At least one of the buttons was just pressed
        {
            move_dir = new Vector2(Mathf.Clamp(x_dir + move_dir.x, -1f, 1f), Mathf.Clamp(y_dir + move_dir.y, -1f, 1f));
            if (CanMove(move_dir))
            {
                Move(move_dir);
            }
            held_time = 0f;
        }

        if (move_dir.magnitude > 0f)
        {
            held_time += Time.deltaTime;
        }
        float e_time = Time.time - last_move_time;
        if (held_time > held_thresh_seconds && e_time > move_cd_seconds)
        {
            if (CanMove(move_dir))
            {
                Move(move_dir);
            }

            // Put move on CD
            last_move_time = Time.time;
        }
    }
}
