using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Canvas : MonoBehaviour
{
    [SerializeField] private Texture2D ref_base_canvas = null;
    [SerializeField] private SpriteRenderer ref_SR_self = null;
    [SerializeField] private SpriteRenderer ref_SR_target = null;

    [SerializeField] private Texture2D target_image = null;
    [SerializeField] private Color canvas_starting_color = Color.white;
    [SerializeField] private float target_display_alpha = 1.0f;

    private Color[][] progress_tracker;
    private Texture2D canvas_copy = null;
    private Vector2Int canvas_size_pixels = Vector2Int.zero;
    private Vector2Int pixel_size = Vector2Int.zero;
    private Vector2 reference_corner = Vector2.zero; // Bottom Left of the canvas

    // Lower left is (0, 0)
    public void SetPixel(float world_x, float world_y, Color c, int scale)
    {
        Color[] c_array = new Color[pixel_size.x * pixel_size.y * scale * scale];
        for (int i = 0; i < pixel_size.x * pixel_size.y * scale * scale; ++i)
        {
            c_array[i] = c;
        }

        Vector2Int pixel_coord = new Vector2Int((int)(world_x - reference_corner.x), (int)(world_y - reference_corner.y));
        int trunc_scale_half = scale / 2;
        canvas_copy.SetPixels((pixel_coord.x - trunc_scale_half) * pixel_size.x, (pixel_coord.y - trunc_scale_half) * pixel_size.y, pixel_size.x * scale, pixel_size.y * scale, c_array);
        canvas_copy.Apply();

        //Color previous_color = progress_tracker[pixel_coord.x][pixel_coord.y];
        //progress_tracker[pixel_coord.x][pixel_coord.y] = c;
    }

    private void Start()
    {
        // Make a copy of the base canvas texture and add it to the sprite renderer
        canvas_copy = Instantiate(ref_base_canvas) as Texture2D; 
        ref_SR_self.sprite = Sprite.Create(canvas_copy, new Rect(0f, 0f, canvas_copy.width, canvas_copy.height), new Vector2(0.5f, 0.5f), Manager_Game.Instance.canvas_PPU);

        // Calculate the size of a canvas pixel in pixels
        canvas_size_pixels = new Vector2Int(Manager_Game.Instance.canvas_pixel_width, Manager_Game.Instance.canvas_pixel_height);
        pixel_size = new Vector2Int(canvas_copy.width / canvas_size_pixels.x, canvas_copy.height / canvas_size_pixels.y);

        // Store the reference corner coordinates
        reference_corner = new Vector2(ref_SR_self.bounds.min.x, ref_SR_self.bounds.min.y);

        // Prepare target image
        Texture2D tex_target = Instantiate(target_image) as Texture2D;
        ref_SR_target.sprite = Sprite.Create(tex_target, new Rect(0f, 0f, tex_target.width, tex_target.height), new Vector2(0.5f, 0.5f), 1);
        ref_SR_target.color = new Color(1f, 1f, 1f, target_display_alpha);
        ref_SR_target.gameObject.SetActive(false);

        // Track pixel color, may be unnecessary
        progress_tracker = new Color[canvas_size_pixels.x][];
        for (int i = 0; i < canvas_size_pixels.x; ++i)
        {
            progress_tracker[i] = new Color[canvas_size_pixels.y];
            for (int j = 0; j < canvas_size_pixels.y; ++j)
            {
                progress_tracker[i][j] = canvas_starting_color;
            }
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Display"))
        {
            ref_SR_target.gameObject.SetActive(true);
        }
        if (Input.GetButtonUp("Display"))
        {
            ref_SR_target.gameObject.SetActive(false);
        }
    }
}
