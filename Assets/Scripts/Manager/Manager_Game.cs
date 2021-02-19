using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Game : MonoBehaviour
{
    // Static instance
    public static Manager_Game Instance = null;

    // Game Defines
    public int canvas_pixel_width = 32;
    public int canvas_pixel_height = 32;
    public float canvas_PPU = 100f;
    public int super_form_scale = 3;

    // References
    [SerializeField] private Behavior_Canvas script_canvas = null;

    // Parameters

    // Member Variables

    public Behavior_Canvas GetCanvasScript()
    {
        return script_canvas;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}
