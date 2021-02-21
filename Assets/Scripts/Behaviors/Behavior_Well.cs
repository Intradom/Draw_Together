using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Well : MonoBehaviour
{
    [SerializeField] private SpriteRenderer ref_SR_self = null;

    [SerializeField] private Color well_color = Color.white; // Default color

    public Color GetColor()
    {
        return well_color;
    }

    public void SetColor(Color c)
    {
        well_color = c;
        ref_SR_self.gameObject.SetActive(true);
        ref_SR_self.color = well_color;
    }

    private void Awake()
    {
        ref_SR_self.gameObject.SetActive(false);
    }
}
