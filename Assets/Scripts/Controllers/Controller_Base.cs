using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller_Base : MonoBehaviour
{
    [SerializeField] protected BoxCollider2D ref_collider_self = null;

    // Tags
    [SerializeField] protected string tag_player = "";
    [SerializeField] protected string tag_wall = "";
    [SerializeField] protected string tag_well = "";
    [SerializeField] protected string tag_canvas = "";

    [SerializeField] protected LayerMask layer_mask_obstacles = 0;

    protected Behavior_Canvas script_canvas = null;
    protected Color current_color;

    // Looks for collisions
    protected Collider2D CheckMove(Vector2 move_dir)
    {
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x + move_dir.x * script_canvas.gameObject.transform.localScale.x, transform.position.y + move_dir.y * script_canvas.gameObject.transform.localScale.x), new Vector2(ref_collider_self.size.x - 1f, ref_collider_self.size.y - 1f), 0, layer_mask_obstacles);

        return hit;
    }

    protected void Move(Vector2 move_dir, int scale)
    {
        transform.Translate(move_dir * script_canvas.gameObject.transform.localScale);

        script_canvas.SetPixel(transform.position.x, transform.position.y, current_color, scale);
    }

    protected void Start()
    {
        script_canvas = GameObject.FindGameObjectWithTag(tag_canvas).GetComponent<Behavior_Canvas>();
    }
}
