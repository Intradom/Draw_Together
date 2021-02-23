using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Fill : MonoBehaviour
{
    [SerializeField] private Behavior_Canvas ref_canvas = null;
    [SerializeField] private SpriteRenderer ref_SR_self_light = null;
    [SerializeField] private SpriteRenderer ref_SR_self_color = null;

    private bool on = false;

    public void GetState(out Color color, out bool on)
    {
        color = ref_SR_self_color.color;
        on = this.on;
    }

    public Color GetColor()
    {
        return ref_SR_self_color.color;
    }

    public void Toggle(Color c)
    {
        if (on)
        {
            TurnOff();
        }
        else
        {
            TurnOn(c);
        }
    }

    public void TurnOn(Color c)
    {
        on = true;
        Color new_color = ref_SR_self_light.color;
        new_color.a = 1f; // Make visible
        ref_SR_self_light.color = new_color;

        ref_SR_self_color.color = c;

        ref_canvas.AdjustFillVotes(1); // Should be last line in this function
    }

    public void TurnOff()
    {
        on = false;
        Color new_color = ref_SR_self_light.color;
        new_color.a = 0f; // Makes invisible
        ref_SR_self_light.color = new_color;

        new_color = ref_SR_self_color.color;
        new_color.a = 0f; // Makes invisible
        ref_SR_self_color.color = new_color;

        ref_canvas.AdjustFillVotes(-1); // Should be last line in this function
    }

    private void Start()
    {
        TurnOff();
    }
}
