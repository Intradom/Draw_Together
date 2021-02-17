using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Canvas : MonoBehaviour
{
    [SerializeField] private Texture2D ref_base_canvas = null;
    [SerializeField] private SpriteRenderer ref_SR_self = null;

    [SerializeField] private float PPU = 100f;

    private Texture2D canvas_copy = null;
    private Vector2Int pixel_size = Vector2Int.zero;

    private void Start()
    {
        // Make a copy of the base canvas texture and add it to the sprite renderer
        canvas_copy = Instantiate(ref_base_canvas) as Texture2D; 
        ref_SR_self.sprite = Sprite.Create(canvas_copy, new Rect(0.0f, 0.0f, canvas_copy.width, canvas_copy.height), new Vector2(0.5f, 0.5f), PPU);

        // Calculate the size of a canvas pixel in pixels
        pixel_size = new Vector2Int(canvas_copy.width / Manager_Game.Instance.canvas_pixel_width, canvas_copy.height / Manager_Game.Instance.canvas_pixel_height);
    }

    private void Update()
    {
        Color nc = new Color(Random.value, Random.value, Random.value, Random.value);

        Color[] c = new Color[pixel_size.x * pixel_size.y];
        for (int i = 0; i < pixel_size.x * pixel_size.y; ++i)
        {
            c[i] = nc;
        }

        canvas_copy.SetPixels(0, 0, pixel_size.x, pixel_size.y, c);
        canvas_copy.Apply();
    }
}
