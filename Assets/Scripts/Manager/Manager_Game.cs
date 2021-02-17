using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Game : MonoBehaviour
{
    // Game Defines
    public int canvas_pixel_width = 32;
    public int canvas_pixel_height = 32;

    public static Manager_Game Instance = null;

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
