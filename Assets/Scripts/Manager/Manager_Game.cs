using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager_Game : MonoBehaviour
{
    public struct Undo_State
    {
        public Texture2D canvas;
        public float canvas_similarity;

        public Color p1_color;
        public Color p2_color;
        public Vector2 p1_pos;
        public Vector2 p2_pos;
        public bool superform;

        public Color fill1_color;
        public Color fill2_color;
        public bool fill1_on;
        public bool fill2_on;
    }

    // Static instance
    public static Manager_Game Instance = null;

    // References
    public Behavior_Canvas script_canvas = null;
    public Behavior_Fill[] script_fills = null;
    public GameObject ref_camera = null;
    public GameObject ref_p1_form = null;
    public GameObject ref_p2_form = null;
    public GameObject ref_superform = null;
    public Texture2D ref_canvas = null;

    // Tags
    public string tag_canvas = "";
    public string tag_player = "";
    public string tag_wall = "";
    public string tag_well = "";
    public string tag_fill = "";

    // UI
    [SerializeField] private Image ref_ui_image_win = null;
    [SerializeField] private Image ref_ui_image_p1 = null;
    [SerializeField] private Image ref_ui_image_p2 = null;
    [SerializeField] private Slider ref_ui_slider_progress = null;
    [SerializeField] private Slider ref_ui_slider_opacity = null;
    [SerializeField] private Slider ref_ui_slider_track = null;
    [SerializeField] private Slider ref_ui_slider_sfx = null;
    [SerializeField] private Text ref_ui_text_undo_count = null;
    [SerializeField] private Text ref_ui_text_progress = null;
    [SerializeField] private Text ref_ui_text_title = null;

    // Parameters
    //[SerializeField] private string tag_manager_main = "";
    //[SerializeField] private string name_menu_scene = "";
    [SerializeField] private float win_threshold = 1f;
    [SerializeField] private int max_undo_count = 1;

    // Member Variables
    private LinkedList<Undo_State> undo_stack = new LinkedList<Undo_State>();
    private bool won = false;

    public void ShakeCamera()
    {
        ref_camera.GetComponent<Effect_Shake>().Shake();
    }

    public void SetTrackVolume(float v)
    {
        if (Manager_Sounds.Instance) Manager_Sounds.Instance.SetTrackVolume(v);
    }

    public void SetSFXVolume(float v)
    {
        if (Manager_Sounds.Instance) Manager_Sounds.Instance.SetSFXVolume(v);
    }

    public void SetOpacitySlider(float v)
    {
        ref_ui_slider_opacity.value = v;
    }

    public void SaveStates()
    {
        if (undo_stack.Count == max_undo_count && undo_stack.Count > 0)
        {
            // Free previous first before adding new state
            undo_stack.RemoveFirst();
        }

        // Save states for any gameobjects that can change state
        Undo_State state = new Undo_State();

        // Canvas
        state.canvas = Instantiate(ref_canvas) as Texture2D;
        script_canvas.GetState(ref state.canvas, out state.canvas_similarity);

        // Players
        GameObject[] player_gobjs = GameObject.FindGameObjectsWithTag(tag_player);
        foreach (GameObject p in player_gobjs)
        {
            Color color1;
            Color color2;
            Vector2 pos;
            bool is_p1;
            bool superform;
            p.GetComponent<Controller_Base>().GetState(out color1, out color2, out pos, out is_p1, out superform);
            if (superform)
            {
                state.p1_color = color1;
                state.p2_color = color2;
                state.p1_pos = pos; // Assign superform pos to p1
                state.superform = true;
            }
            else
            {
                if (is_p1)
                {
                    state.p1_color = color1;
                    state.p1_pos = pos;
                }
                else
                {
                    state.p2_color = color1;
                    state.p2_pos = pos;
                }
                state.superform = false;
            }
        }

        // Fill Tools
        script_fills[0].GetState(out state.fill1_color, out state.fill1_on);
        script_fills[1].GetState(out state.fill2_color, out state.fill2_on);

        undo_stack.AddLast(state);
        SetUndoCount(undo_stack.Count);
    }

    public void Undo()
    {
        // Nothing in stack to undo
        if (undo_stack.Count <= 0)
        {
            if (Manager_Sounds.Instance) Manager_Sounds.Instance.PlaySFXError();
            return;
        }
        if (Manager_Sounds.Instance) Manager_Sounds.Instance.PlaySFXUIButton();


        // Load states for any gameobjects that can change state
        Undo_State state = undo_stack.Last.Value;
        undo_stack.RemoveLast();

        // Canvas
        script_canvas.SetState(state.canvas, state.canvas_similarity);

        // Players
        bool inst_superform = false;
        GameObject[] player_gobjs = GameObject.FindGameObjectsWithTag(tag_player);
        foreach (GameObject p in player_gobjs)
        {
            bool is_p1;
            bool superform;
            p.GetComponent<Controller_Base>().GetState(out _, out _, out _, out is_p1, out superform);

            if (superform)
            {
                if (state.superform) // Current form is superform and past form was superform
                {
                    p.transform.position = state.p1_pos;
                }
                else // Current form is superform and past form was not superform
                {
                    Destroy(p);
                    GameObject p1 = Instantiate(ref_p1_form, state.p1_pos, Quaternion.identity);
                    GameObject p2 = Instantiate(ref_p2_form, state.p2_pos, Quaternion.identity);
                    p1.GetComponent<Controller_Player>().SetColor(state.p1_color);
                    p2.GetComponent<Controller_Player>().SetColor(state.p2_color);
                }
            }
            else
            {
                if (state.superform) // Current form is not superform and past form was superform
                {
                    Destroy(p);
                    inst_superform = true;
                }
                else // Current form is not superform and past form was not superform
                {
                    if (is_p1)
                    {
                        p.transform.position = state.p1_pos;
                        p.GetComponent<Controller_Player>().SetColor(state.p1_color);
                    }
                    else
                    {
                        p.transform.position = state.p2_pos;
                        p.GetComponent<Controller_Player>().SetColor(state.p2_color);
                    }
                }
            }
        }
        if (inst_superform)
        {
            GameObject superform = Instantiate(ref_superform, state.p1_pos, Quaternion.identity);
            superform.GetComponent<Controller_Superform>().SetColor(state.p1_color, state.p2_color);
        }

        // Fill Tools
        if (state.fill1_on)
        {
            script_fills[0].TurnOn(state.fill1_color);
        }
        else
        {
            script_fills[0].TurnOff();
        }
        if (state.fill2_on)
        {
            script_fills[1].TurnOn(state.fill2_color);
        }
        else
        {
            script_fills[1].TurnOff();
        }

        SetUndoCount(undo_stack.Count);
    }

    public void SetP1Color(Color c)
    {
        ref_ui_image_p1.color = c; 
    }

    public void SetP2Color(Color c)
    {
        ref_ui_image_p2.color = c;
    }

    public void UpdateProgress(float p)
    {
        p = Mathf.Clamp(p, 0f, 1f);
        ref_ui_slider_progress.value = p;
        ref_ui_text_progress.text = ((int)(p * 100f)).ToString() + "%";
        if (p >= win_threshold)
        {
            ref_ui_text_progress.text = "100%"; // Sometimes the math errors make the number not exactly 100% (stuck at 99%)
            if (!won)
            {
                Manager_Sounds.Instance.PlaySFXWin();
                ref_ui_image_win.gameObject.SetActive(true);
            }
            won = true;
        }
    }

    public void ReturnToMenu()
    {

        if (Manager_Sounds.Instance) Manager_Sounds.Instance.PlaySFXUIButton();
        //SceneManager.LoadScene(name_menu_scene);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void CloseMenu()
    {
        if (Manager_Sounds.Instance) Manager_Sounds.Instance.StopSFX();
        ref_ui_image_win.gameObject.SetActive(false);
    }

    private void SetUndoCount(int undo_count)
    {
        ref_ui_text_undo_count.text = "UNDO[" + undo_count.ToString() + "]";
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Manager_Sounds.Instance)
        {
            Manager_Sounds.Instance.StopSFX();
            ref_ui_slider_track.value = Manager_Sounds.Instance.GetTrackVolume();
            ref_ui_slider_sfx.value = Manager_Sounds.Instance.GetSFXVolume();
        }

        won = false;
        ref_ui_image_win.gameObject.SetActive(false);
        SetUndoCount(undo_stack.Count);
        ref_ui_text_title.text = Manager_Main.Instance.GetTargetTitle();
    }
}
