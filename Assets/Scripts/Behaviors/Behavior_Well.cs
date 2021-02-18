using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Well : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ref_SR_self = null;

    [SerializeField] private Color well_color = Color.white;

    public Color GetColor()
    {
        return well_color;
    }

    private void Start()
    {
        ref_SR_self.color = well_color;
    }
}
