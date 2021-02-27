using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller_Base : MonoBehaviour
{
    [SerializeField] protected BoxCollider2D ref_collider_self = null;
    [SerializeField] protected LayerMask layer_mask_obstacles = 0;
    [SerializeField] protected Effect_Shake script_shake = null;
    [SerializeField] protected ParticleSystem particle_splash = null;

    protected Behavior_Canvas script_canvas = null;
    protected Color current_color;

    public abstract void GetState(out Color color1, out Color color2, out Vector2 pos, out bool is_p1, out bool superform);

    // Returns true if can move
    protected bool CanMove(Vector2 move_dir)
    {
        Collider2D hit = Physics2D.OverlapBox(new Vector2(transform.position.x + move_dir.x * script_canvas.gameObject.transform.localScale.x, transform.position.y + move_dir.y * script_canvas.gameObject.transform.localScale.x), new Vector2(ref_collider_self.size.x - 1f, ref_collider_self.size.y - 1f), 0, layer_mask_obstacles);

        return (hit == null);
    }

    protected void Move(Vector2 move_dir, int scale)
    {
        transform.Translate(move_dir * script_canvas.gameObject.transform.localScale);
        particle_splash.Play();

        script_canvas.SetPixel(transform.position.x, transform.position.y, current_color, scale);
    }

    protected void Start()
    {
        script_canvas = Manager_Game.Instance.script_canvas;
    }
}
