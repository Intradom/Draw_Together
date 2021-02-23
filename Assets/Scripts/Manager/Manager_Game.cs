using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Manager_Game : MonoBehaviour
{
    // Static instance
    public static Manager_Game Instance = null;

    // References
    [SerializeField] private Image ref_ui_image_win = null;
    [SerializeField] private Image ref_ui_image_p1 = null;
    [SerializeField] private Image ref_ui_image_p2 = null;
    [SerializeField] private Slider ref_ui_slider_progress = null;
    [SerializeField] private Text ref_ui_text_progress = null;

    // Parameters
    //[SerializeField] private string tag_manager_main = "";
    //[SerializeField] private string name_menu_scene = "";
    [SerializeField] private float win_threshold = 1f;

    // Member Variables
    private bool won = false;

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
        ref_ui_text_progress.text = ((int)(p * 100)).ToString() + "%";
        if (p >= win_threshold)
        {
            if (!won)
            {
                ref_ui_image_win.gameObject.SetActive(true);
            }
            won = true;
        }
    }

    public void ReturnToMenu()
    {
        //SceneManager.LoadScene(name_menu_scene);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void CloseMenu()
    {
        ref_ui_image_win.gameObject.SetActive(false);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        won = false;
        ref_ui_image_win.gameObject.SetActive(false);
    }
}
