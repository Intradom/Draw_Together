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
    [SerializeField] private Image ref_ui_image_p1 = null;
    [SerializeField] private Image ref_ui_image_p2 = null;

    // Parameters
    //[SerializeField] private string tag_manager_main = "";
    [SerializeField] private string name_menu_scene = "";

    // Member Variables

    public void SetP1Color(Color c)
    {
        Debug.Log("run");
        ref_ui_image_p1.color = c; 
    }

    public void SetP2Color(Color c)
    {
        ref_ui_image_p2.color = c;
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(name_menu_scene);
    }

    private void Awake()
    {
        Instance = this;
    }
}
