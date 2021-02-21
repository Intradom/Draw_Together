using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Behavior_Canvas : MonoBehaviour
{
    private struct ColorYUV
    {
        public ColorYUV(Color c)
        {
            // https://stackoverflow.com/questions/5392061/algorithm-to-check-similarity-of-colors
            y = 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
            u = -0.14713f * c.r + -0.28886f * c.g + 0.436f * c.b;
            v = 0.615f * c.r + -0.51499f * c.g + -0.10001f * c.b;
            a = c.a;
        }

        public float y;
        public float u;
        public float v;
        public float a;
    }

    private struct Combined_Color
    {
        public Combined_Color(Color t_self, Color t_p1, Color t_p2)
        {
            self = t_self;
            p1 = t_p1;
            p2 = t_p2;
        }

        public Color self;
        public Color p1;
        public Color p2;
    }

    // References
    [SerializeField] private Texture2D ref_canvas_image = null;
    [SerializeField] private SpriteRenderer ref_SR_self = null;
    [SerializeField] private SpriteRenderer ref_SR_target = null;
    [SerializeField] private Behavior_Well[] script_wells = null;
    [SerializeField] private Controller_Player script_p1 = null;
    [SerializeField] private Controller_Player script_p2 = null;

    // Parameters
    [SerializeField] private Color canvas_starting_color = Color.white;
    [SerializeField] private float target_display_alpha = 1f;
    [SerializeField] private float color_match_threshold = 0f;

    // Member Variables
    private Texture2D canvas_copy = null;
    private Texture2D target_image = null;
    private Vector2Int canvas_size_pixels = Vector2Int.zero;
    private Vector2Int pixel_size = Vector2Int.zero;
    private Vector2 reference_corner = Vector2.zero; // Bottom Left of the canvas
    private float progress = 0f; // 0 is perfect match, 1 is nothing matches
    private int canvas_size_pixels_total = 0;

    // Lower left is (0, 0)
    public void SetPixel(float world_x, float world_y, Color c, int scale)
    {
        Color[] c_array = new Color[pixel_size.x * pixel_size.y * scale * scale];
        for (int i = 0; i < pixel_size.x * pixel_size.y * scale * scale; ++i)
        {
            c_array[i] = c;
        }

        Vector2Int pixel_coord = new Vector2Int((int)((world_x - reference_corner.x) / this.transform.localScale.x), (int)((world_y - reference_corner.y) / this.transform.localScale.y));
        int trunc_scale_half = scale / 2;
        int loc_x = (pixel_coord.x - trunc_scale_half) * pixel_size.x;
        int loc_y = (pixel_coord.y - trunc_scale_half) * pixel_size.y;
        Color previous_color = canvas_copy.GetPixel(loc_x, loc_y);
        canvas_copy.SetPixels(loc_x, loc_y, pixel_size.x * scale, pixel_size.y * scale, c_array);
        canvas_copy.Apply();

        //Debug.Log("T: " + (Color32)target_image.GetPixel(pixel_coord.x, pixel_coord.y));
        //Debug.Log("C: " + (Color32)c);
        //Debug.Log("P: " + (Color32)previous_color);
        AdjustProgress(target_image.GetPixel(pixel_coord.x, pixel_coord.y), c, previous_color, canvas_size_pixels_total);
        //Debug.Log("Progress: " + progress);
        //Color previous_color = progress_tracker[pixel_coord.x][pixel_coord.y];
        //progress_tracker[pixel_coord.x][pixel_coord.y] = c;
    }

    private float ColorDiff(Color c1, Color c2)
    {
        //float diff = (Mathf.Pow(c1.r - c2.r, 2f) + Mathf.Pow(c1.g - c2.g, 2f) + Mathf.Pow(c1.b - c2.b, 2f) + Mathf.Pow(c1.a - c2.a, 2f)) / 4f;

        ColorYUV c_yuv_1 = new ColorYUV(c1);
        ColorYUV c_yuv_2 = new ColorYUV(c2);

        return Mathf.Sqrt(Mathf.Pow(c_yuv_1.y - c_yuv_2.y, 2f) + Mathf.Pow(c_yuv_1.u - c_yuv_2.u, 2f) + Mathf.Pow(c_yuv_1.v - c_yuv_2.v, 2f) + Mathf.Pow(c_yuv_1.a - c_yuv_2.a, 2f));
    }

    private void InitProgress(Color target, Color start, int total_pixels)
    {
        float progress_max = 1f / total_pixels;
        //Debug.Log(ColorDiff(target, start));
        progress += ColorDiff(target, start) * progress_max;
    }

    private void AdjustProgress(Color target, Color current, Color previous, int total_pixels)
    {
        if (current == previous)
        {
            return;
        }

        float progress_max = 1f / total_pixels;
        //Debug.Log("Diff C: " + ColorDiff(target, current));
        //Debug.Log("Diff P: " + ColorDiff(target, previous));
        progress += (ColorDiff(target, current) - ColorDiff(target, previous)) * progress_max;
    }

    private void Start()
    {
        // Make a copy of the base canvas texture and add it to the sprite renderer
        canvas_copy = Instantiate(ref_canvas_image) as Texture2D;
        ref_SR_self.sprite = Sprite.Create(canvas_copy, new Rect(0f, 0f, canvas_copy.width, canvas_copy.height), new Vector2(0.5f, 0.5f), Manager_Main.Instance.canvas_PPU);
        //ref_SR_self.sprite = Sprite.Create(ref_canvas_image, new Rect(0f, 0f, canvas_copy.width, canvas_copy.height), new Vector2(0.5f, 0.5f), Manager_Main.Instance.canvas_PPU);

        // Calculate the size of a canvas pixel in pixels
        canvas_size_pixels = new Vector2Int(Manager_Main.Instance.canvas_pixel_width, Manager_Main.Instance.canvas_pixel_height);
        canvas_size_pixels_total = canvas_size_pixels.x * canvas_size_pixels.y;
        pixel_size = new Vector2Int(canvas_copy.width / canvas_size_pixels.x, canvas_copy.height / canvas_size_pixels.y);

        // Store the reference corner coordinates
        reference_corner = new Vector2(ref_SR_self.bounds.min.x, ref_SR_self.bounds.min.y);

        // Prepare target image
        target_image = Manager_Main.Instance.GetTargetTexture();
        Texture2D tex_target = Instantiate(target_image) as Texture2D;
        //Texture2D tex_target = target_image;
        ref_SR_target.sprite = Sprite.Create(tex_target, new Rect(0f, 0f, tex_target.width, tex_target.height), new Vector2(0.5f, 0.5f), 1f);
        ref_SR_target.color = new Color(1f, 1f, 1f, target_display_alpha);
        ref_SR_target.gameObject.SetActive(false);

        // Track pixel color
        progress = 0f;
        HashSet<Color> canvas_colors = new HashSet<Color>();
        for (int i = 0; i < tex_target.width; ++i)
        {
            for (int j = 0; j < tex_target.height; ++j)
            {
                canvas_colors.Add(tex_target.GetPixel(i, j));
                InitProgress(tex_target.GetPixel(i, j), canvas_starting_color, canvas_size_pixels_total);
            }
        }

        List<Combined_Color> combined_colors = new List<Combined_Color>();
        List<Color> final_colors = new List<Color>();

        //Debug.Log("|||||||||||||||||||||||||||||||||||||");
        Debug.Log(canvas_colors.Count);
        foreach (Color c in canvas_colors)
        {
            Debug.Log((Color32)c);
        }
        foreach (Color primary in canvas_colors)
        {
            foreach (Color secondary in canvas_colors)
            {
                if (primary != secondary)
                {
                    combined_colors.Add(new Combined_Color(Manager_Main.Instance.CombineColors(primary, secondary), primary, secondary));
                }
            }
        }
        foreach (Color c in canvas_colors)
        {
            //Debug.Log("Color: " + (Color32)c);
            bool pass = true;
            foreach (Combined_Color combined in combined_colors)
            {
                if (ColorDiff(combined.self, c) < color_match_threshold && c != combined.p1 && c != combined.p2) // Basically same color
                {
                    //Debug.Log("\t" + (Color32)combined.self + " " + (Color32)c + " " + ColorDiff(combined.self, c));
                    pass = false;
                    break;
                }
            }
            if (pass)
            {
                final_colors.Add(c);
            }
        }

        Debug.Log("~~~~~~~~~~~~~");
        foreach (Color c in final_colors)
        {
            Debug.Log((Color32)c);
        }
        Debug.Log(final_colors.Count);

        // Assign wells
        for (int i = 0; i < final_colors.Count; ++i)
        {
            int wells_length_half = script_wells.Length / 2;
            if (final_colors.Count <= wells_length_half)
            {
                // Mirror wells
                script_wells[i].SetColor(final_colors[i]);
                script_wells[i + wells_length_half].SetColor(final_colors[i]);
            }
            else if (final_colors.Count <= script_wells.Length)
            {
                // Single fill
                script_wells[i].SetColor(final_colors[i]);
            }
            else
            {
                Debug.LogError("ERROR: WELL COLOR OVERFLOW");
            }
        }
        if (script_wells.Length > 0)
        {
            script_p1.SetColor(script_wells[0].GetColor());
            script_p2.SetColor(script_wells[0].GetColor());
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
