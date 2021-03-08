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
    [SerializeField] private SpriteRenderer ref_SR_self = null;
    [SerializeField] private SpriteRenderer ref_SR_target = null;
    [SerializeField] private Behavior_Well[] script_wells = null;
    [SerializeField] private Controller_Player script_p1 = null; // Exists at beginning
    [SerializeField] private Controller_Player script_p2 = null; // Exists at beginning

    // Parameters
    [SerializeField] private Color canvas_starting_color = Color.white;
    [SerializeField] private float target_display_alpha = 1f;
    [SerializeField] private float color_match_threshold = 0f;
    [SerializeField] private int fill_vote_threshold = 2;
    [SerializeField] private int similarity_decimals = 4;

    // Member Variables
    private Texture2D canvas_current = null;
    private Texture2D canvas_target = null;
    private Vector2Int canvas_size_pixels = Vector2Int.zero;
    private Vector2Int pixel_size = Vector2Int.zero;
    private Vector2 reference_corner = Vector2.zero; // Bottom Left of the canvas
    private float initial_similarity = 0f;
    private float similarity = 0f; // Set similarity relative to beginning state
    private int canvas_size_pixels_total = 0;
    private int fill_votes = 0;

    public void GetState(ref Texture2D canvas, out float similarity)
    {
        Graphics.CopyTexture(canvas_current, canvas);
        similarity = this.similarity;
    }

    public void SetState(Texture2D canvas, float similarity)
    {
        Graphics.CopyTexture(canvas, canvas_current);
        this.similarity = similarity;
        Manager_Game.Instance.UpdateProgress((initial_similarity - similarity) / initial_similarity);
    }

    public void AdjustFillVotes(int vote)
    {
        fill_votes += vote;
        fill_votes = Mathf.Clamp(fill_votes, 0, fill_votes);

        if (fill_votes >= fill_vote_threshold)
        {
            Color fill_color = Manager_Main.Instance.CombineColors(Manager_Game.Instance.script_fills[0].GetColor(), Manager_Game.Instance.script_fills[1].GetColor());
            SetPixel(transform.position.x, transform.position.y, fill_color, Manager_Main.Instance.canvas_pixel_height);

            // Reset all fills
            foreach (Behavior_Fill bf in Manager_Game.Instance.script_fills)
            {
                bf.TurnOff(); // Resets the votes this way
            }
        }
    }

    public void SetPreviewOpacity(float a)
    {
        ref_SR_target.color = new Color(ref_SR_target.color.r, ref_SR_target.color.b, ref_SR_target.color.g, a);
    }

    // Lower left is (0, 0)
    // Only sets square blocks of pixels (can be modified to set rectangles)
    public void SetPixel(float world_x, float world_y, Color c, int scale)
    {
        Color[] c_array = new Color[pixel_size.x * pixel_size.y * scale * scale];
        for (int i = 0; i < pixel_size.x * pixel_size.y * scale * scale; ++i)
        {
            c_array[i] = c;
        }

        // Pixel coordinates of the lower left pixel of the block
        Vector2Int pixel_coord = new Vector2Int((int)((world_x - reference_corner.x) / this.transform.localScale.x) - scale / 2, (int)((world_y - reference_corner.y) / this.transform.localScale.y) - scale / 2);
        
        int loc_x = pixel_coord.x * pixel_size.x;
        int loc_y = pixel_coord.y * pixel_size.y;

        // Calculate similarity adjustment before applying changes
        for (int i = 0; i < scale; ++i)
        {
            for (int j = 0; j < scale; ++j)
            {
                AdjustSimilarity(canvas_target.GetPixel(pixel_coord.x + i, pixel_coord.y + j), c, canvas_current.GetPixel((pixel_coord.x + i) * pixel_size.x, (pixel_coord.y + j) * pixel_size.y), canvas_size_pixels_total);
            }
        }

        canvas_current.SetPixels(loc_x, loc_y, pixel_size.x * scale, pixel_size.y * scale, c_array);
        canvas_current.Apply();
        if (Manager_Sounds.Instance) Manager_Sounds.Instance.PlaySFXColor();

        //Debug.Log("T: " + (Color32)canvas_target.GetPixel(pixel_coord.x, pixel_coord.y));
        //Debug.Log("C: " + (Color32)c);
        //Debug.Log("P: " + (Color32)previous_color);
        //Debug.Log("similarity: " + similarity);
    }

    private float DecimalRound(float val)
    {
        float round_factor = Mathf.Pow(10f, (float)(similarity_decimals));
        return (Mathf.Round(val * round_factor) / round_factor);
    }

    private float ColorDiff(Color c1, Color c2)
    {
        //float diff = (Mathf.Pow(c1.r - c2.r, 2f) + Mathf.Pow(c1.g - c2.g, 2f) + Mathf.Pow(c1.b - c2.b, 2f) + Mathf.Pow(c1.a - c2.a, 2f)) / 4f;

        ColorYUV c_yuv_1 = new ColorYUV(c1);
        ColorYUV c_yuv_2 = new ColorYUV(c2);

        float diff = Mathf.Sqrt(Mathf.Pow(c_yuv_1.y - c_yuv_2.y, 2f) + Mathf.Pow(c_yuv_1.u - c_yuv_2.u, 2f) + Mathf.Pow(c_yuv_1.v - c_yuv_2.v, 2f) + Mathf.Pow(c_yuv_1.a - c_yuv_2.a, 2f));
        diff = DecimalRound(diff);
        Debug.Log("Diff: " + diff);
        return (diff < color_match_threshold) ? 0f : diff;
    }

    private void InitSimilarity(Color target, Color start, int total_pixels)
    {
        float similarity_max = 1f / total_pixels;
        //Debug.Log("IS: " + DecimalRound(ColorDiff(target, start) * similarity_max));
        similarity += DecimalRound(ColorDiff(target, start) * similarity_max);
        Manager_Game.Instance.UpdateProgress(0f);
    }

    // Adjusts similarity for one pixel
    private void AdjustSimilarity(Color target, Color current, Color previous, int total_pixels)
    {
        if (current == previous)
        {
            return;
        }

        float similarity_max = 1f / total_pixels;
        similarity += DecimalRound((ColorDiff(target, current) - ColorDiff(target, previous)) * similarity_max);
        float progress_update = (initial_similarity - similarity) / initial_similarity;
        Debug.Log("Progress: " + progress_update);
        Manager_Game.Instance.UpdateProgress(progress_update);
    }

    private void Start()
    {
        // Init variables
        fill_votes = 0;

        // Make a copy of the base canvas texture and add it to the sprite renderer
        canvas_current = Instantiate(Manager_Game.Instance.ref_canvas) as Texture2D;
        ref_SR_self.sprite = Sprite.Create(canvas_current, new Rect(0f, 0f, canvas_current.width, canvas_current.height), new Vector2(0.5f, 0.5f), Manager_Main.Instance.canvas_PPU);
        //ref_SR_self.sprite = Sprite.Create(ref_canvas_image, new Rect(0f, 0f, canvas_current.width, canvas_current.height), new Vector2(0.5f, 0.5f), Manager_Main.Instance.canvas_PPU);

        // Calculate the size of a canvas pixel in pixels
        canvas_size_pixels = new Vector2Int(Manager_Main.Instance.canvas_pixel_width, Manager_Main.Instance.canvas_pixel_height);
        canvas_size_pixels_total = canvas_size_pixels.x * canvas_size_pixels.y;
        pixel_size = new Vector2Int(canvas_current.width / canvas_size_pixels.x, canvas_current.height / canvas_size_pixels.y);

        // Store the reference corner coordinates
        reference_corner = new Vector2(ref_SR_self.bounds.min.x, ref_SR_self.bounds.min.y);

        // Prepare target image
        canvas_target = Manager_Main.Instance.GetTargetTexture();
        Texture2D tex_target = Instantiate(canvas_target) as Texture2D;
        //Texture2D tex_target = canvas_target;
        ref_SR_target.sprite = Sprite.Create(tex_target, new Rect(0f, 0f, tex_target.width, tex_target.height), new Vector2(0.5f, 0.5f), 1f);
        ref_SR_target.color = new Color(1f, 1f, 1f, target_display_alpha);
        ref_SR_target.gameObject.SetActive(false);
        Manager_Game.Instance.SetOpacitySlider(target_display_alpha);

        // Track pixel color
        similarity = 0f;
        HashSet<Color> canvas_colors = new HashSet<Color>();
        for (int i = 0; i < tex_target.width; ++i)
        {
            for (int j = 0; j < tex_target.height; ++j)
            {
                canvas_colors.Add(tex_target.GetPixel(i, j));
                InitSimilarity(tex_target.GetPixel(i, j), canvas_starting_color, canvas_size_pixels_total);
            }
        }
        initial_similarity = similarity;
        Debug.Log("Initial Sim: " + initial_similarity);

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
                if (ColorDiff(combined.self, c) == 0f && c != combined.p1 && c != combined.p2) // Basically same color
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
