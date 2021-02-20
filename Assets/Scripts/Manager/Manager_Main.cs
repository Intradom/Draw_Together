using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Main : MonoBehaviour
{    
    // Static instance
    public static Manager_Main Instance = null;

    // Game Defines
    public int canvas_pixel_width = 32;
    public int canvas_pixel_height = 32;
    public float canvas_PPU = 100f;
    public int super_form_scale = 3;

    // Member Variable
    private Texture2D target_texture = null;

    public void SetTargetTexture(Texture2D tex)
    {
        target_texture = tex;
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
