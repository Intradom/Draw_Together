using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_Game : MonoBehaviour
{
    // Static instance
    public static Manager_Game Instance = null;

    // Game Defines
    public Color canvas_starting_color = Color.white;
    public int canvas_pixel_width = 32;
    public int canvas_pixel_height = 32;

    // Parameters

    // Member Variables
    private Color[][] tracker;

    public void SetPixel(int x, int y, Color c)
    {
        Color previous_color = tracker[x][y];
        tracker[x][y] = c;
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

    private void Start()
    {
        tracker = new Color[canvas_pixel_width][];

        for (int i = 0; i < canvas_pixel_width; ++i)
        {
            tracker[i] = new Color[canvas_pixel_height];
            for (int j = 0; j < canvas_pixel_height; ++j)
            {
                tracker[i][j] = canvas_starting_color;
            }
        }
    }
}
