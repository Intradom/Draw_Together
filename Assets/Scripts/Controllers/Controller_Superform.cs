using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Superform : Controller_Base
{
    [SerializeField] private GameObject ref_pointer = null;
    [SerializeField] private GameObject ref_p1_form = null;
    [SerializeField] private GameObject ref_p2_form = null;

    [SerializeField] private float move_inc = 1f;
    [SerializeField] private float pointer_dist = 1f;
    [SerializeField] private float diagonal_wait_input_seconds = 0.1f; // To differentiate between diagonal movements and separations

    private Color p1_color;
    private Color p2_color;
    private Vector2Int p1_dir;
    private Vector2Int p2_dir;
    private float diagonal_countdown = 0f;
    private bool diagonal_start = false;

    public void Init(Color p1_c, Color p2_c)
    {
        p1_color = p1_c;
        p2_color = p2_c;

        // Big thanks to Meowlo for the collaboration of this line
        current_color = (p1_color + p2_color) / 2;
    }

    private bool CanMove(Collider2D hit)
    {
        if (hit && (hit.tag == tag_well || hit.tag == tag_wall))
        {
            return false;
        }

        return true;
    }

    private new void Start()
    {
        base.Start();
        Move(Vector2.zero, Manager_Game.Instance.super_form_scale);
    }

    private void Update()
    {
        bool p1_down = false;
        bool p2_down = false;
        if (Input.GetButtonDown("P1_Left") || Input.GetButtonDown("P1_Right") || Input.GetButtonDown("P1_Down") || Input.GetButtonDown("P1_Up"))
        {
            p1_dir.x = 0;
            p1_dir.x += Input.GetButton("P1_Left") ? -1 : 0;
            p1_dir.x += Input.GetButton("P1_Right") ? 1 : 0;

            p1_dir.y = 0;
            p1_dir.y += Input.GetButton("P1_Down") ? -1 : 0;
            p1_dir.y += Input.GetButton("P1_Up") ? 1 : 0;
            p1_down = true;
        }
        if (Input.GetButtonUp("P1_Left") && p1_dir.x < 0) p1_dir.x = 0;
        if (Input.GetButtonUp("P1_Right") && p1_dir.x > 0) p1_dir.x = 0;
        if (Input.GetButtonUp("P1_Up") && p1_dir.y > 0) p1_dir.y = 0;
        if (Input.GetButtonUp("P1_Down") && p1_dir.y < 0) p1_dir.y = 0;

        //p2_dir.x = Input.GetButtonDown("P2_Left") ? -1 : (Input.GetButtonDown("P2_Right") ? 1 : p2_dir.x);
        //p2_dir.y = Input.GetButtonDown("P2_Down") ? -1 : (Input.GetButtonDown("P2_Up") ? 1 : p2_dir.y);
        if (Input.GetButtonDown("P2_Left") || Input.GetButtonDown("P2_Right") || Input.GetButtonDown("P2_Down") || Input.GetButtonDown("P2_Up"))
        {
            p2_dir.x = 0;
            p2_dir.x += Input.GetButton("P2_Left") ? -1 : 0;
            p2_dir.x += Input.GetButton("P2_Right") ? 1 : 0;

            p2_dir.y = 0;
            p2_dir.y += Input.GetButton("P2_Down") ? -1 : 0;
            p2_dir.y += Input.GetButton("P2_Up") ? 1 : 0;
            p2_down = true;
        }
        if (Input.GetButtonUp("P2_Left") && p2_dir.x < 0) p2_dir.x = 0;
        if (Input.GetButtonUp("P2_Right") && p2_dir.x > 0) p2_dir.x = 0;
        if (Input.GetButtonUp("P2_Up") && p2_dir.y > 0) p2_dir.y = 0;
        if (Input.GetButtonUp("P2_Down") && p2_dir.y < 0) p2_dir.y = 0;

        float p1_mag = p1_dir.magnitude;
        float p2_mag = p2_dir.magnitude;
        ref_pointer.SetActive(false);
        if ((p1_mag > 0f && p2_mag == 0f) || (p1_mag == 0f && p2_mag > 0f)) // One player points in one direction and another player is not moving, display movement indicator
        {
            Vector2 move_dir = (p1_mag != 0) ? (Vector2)p1_dir * move_inc : (Vector2)p2_dir * move_inc;

            if (CanMove(CheckMove(move_dir)))
            {
                //move_dir.Normalize();
                ref_pointer.SetActive(true);
                ref_pointer.transform.position = this.transform.position + new Vector3(move_dir.x, move_dir.y, 0f) * pointer_dist;
                ref_pointer.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(move_dir.y, move_dir.x) * Mathf.Rad2Deg, Vector3.forward);
            }
        }
        else if (p1_dir == p2_dir && p1_mag > 0f && p2_mag > 0f) // Both players point in same direction, check if move possible and move if possible
        {
            Vector2 move_dir = (Vector2)p1_dir * move_inc;

            if (CanMove(CheckMove(move_dir)))
            {
                Move(move_dir, Manager_Game.Instance.super_form_scale);
            }

            p1_dir = p1_down ? Vector2Int.zero : p1_dir;
            p2_dir = p2_down ? Vector2Int.zero : p2_dir;
            diagonal_start = false;
        }
        else if (p1_mag > 0 && p2_mag > 0) // One player points in one direction and another player points in a different direction, separate superform
        {
            if (!diagonal_start)
            {
                diagonal_start = true;
                diagonal_countdown = Time.time;
            }
        }

        if (diagonal_start && (Time.time - diagonal_countdown) > diagonal_wait_input_seconds)
        {
            var p1 = Instantiate(ref_p1_form, this.transform.position + new Vector3(p1_dir.x, p1_dir.y, 0f), Quaternion.identity);
            var p2 = Instantiate(ref_p2_form, this.transform.position + new Vector3(p2_dir.x, p2_dir.y, 0f), Quaternion.identity);
            p1.GetComponent<Controller_Player>().Init(p1_color);
            p2.GetComponent<Controller_Player>().Init(p2_color);

            Destroy(this.gameObject);
        }
    }
}
