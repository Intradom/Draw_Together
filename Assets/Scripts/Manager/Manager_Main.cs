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
    [SerializeField] private Texture2D target_texture = null; // Serialize to allow for easier testing

    public Color CombineColors(Color c1, Color c2)
    {
        // Big thanks to Meowlo for the collaboration of this line
        //return (c1 + c2) / 2;
        return Color.Lerp(c1, c2, 0.5f);
    }

    public Texture2D GetTargetTexture()
    {
        return target_texture;
    }

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
