using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager_Game : MonoBehaviour
{
    // References

    // Parameters
    [SerializeField] private string tag_manager_main = "";
    [SerializeField] private string name_menu_scene = "";

    // Member Variables

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(name_menu_scene);
    }
}
