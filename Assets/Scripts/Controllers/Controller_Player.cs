using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Player : Controller_Base
{
    [SerializeField] private GameObject ref_superform = null;

    // Parameters
    [SerializeField] private LayerMask layer_mask_player = 0;
    [SerializeField] private Color starting_color = Color.white;

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

    public void SetColor(Color c)
    {
        current_color = c;

        if (player_number == Player_Number.Player_Two)
        {
            Manager_Game.Instance.SetP2Color(current_color);
        }
        else
        {
            Manager_Game.Instance.SetP1Color(current_color);
        }
    }

    public bool CanTransform()
    {
        // Check if transformation violates collision bounds
        int super_scale = Manager_Main.Instance.super_form_scale;
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y), new Vector2(ref_collider_self.size.x * super_scale - 1, ref_collider_self.size.y * super_scale - 1), 0, layer_mask_obstacles);
        
        return (hit == null);
    }

    public void Transform(Color other_color)
    {
        var inst = Instantiate(ref_superform, transform.position, Quaternion.identity);
        Controller_Superform script_superform = inst.GetComponent<Controller_Superform>();

        if (player_number == Player_Number.Player_Two)
        {
            script_superform.Init(other_color, current_color);
        }
        else 
        {
            script_superform.Init(current_color, other_color);
        }

        Destroy(this.gameObject);
    }

    private bool CanMove(Collider2D obstacle_hit)
    {        
        if (obstacle_hit)
        {
            if (obstacle_hit.tag == tag_well)
            {
                // Change color to well color
                SetColor(obstacle_hit.gameObject.GetComponent<Behavior_Well>().GetColor());

                return false;
            }
            else if (obstacle_hit.tag == tag_wall)
            {
                return false;
            }
        }
        else
        {
            Collider2D player_hit = Physics2D.OverlapBox(new Vector2(transform.position.x + move_dir.x, transform.position.y + move_dir.y), new Vector2(ref_collider_self.size.x - 1, ref_collider_self.size.y - 1), 0, layer_mask_player);

            if (player_hit && player_hit.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()) // Make sure player doesn't collide with itself
            {
                Controller_Player script_player_other = player_hit.gameObject.GetComponent<Controller_Player>();
                if (script_player_other.CanTransform())
                {
                    script_player_other.Transform(current_color);
                    Destroy(this.gameObject);
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void Awake()
    {
        current_color = starting_color;
    }

    private new void Start()
    {
        base.Start();

        Move(Vector2.zero, 1);
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
            if (CanMove(CheckMove(move_dir)))
            {
                Move(move_dir, 1);
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
            if (CanMove(CheckMove(move_dir)))
            {
                Move(move_dir, 1);
            }

            // Put move on CD
            last_move_time = Time.time;
        }
    }
}
