using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Menu : MonoBehaviour
{
    // Constants
    private readonly List<string> IMAGE_EXTENSIONS = new List<string> { ".PSD", ".TIFF", ".JPG", ".TGA", ".PNG", ".GIF", ".BMP", ".IFF", ".PICT"};

    // References
    [SerializeField] private SpriteRenderer ref_SR_preview = null;
    [SerializeField] private Text ref_ui_text_folder = null;

    // Parameters
    [SerializeField] private string levels_file_path = ""; // Starting from Assets Folder. Folder contains folders and then images like ../<level_file_path>/<folder>/<images>

    private List<string> folder_names = new List<string>();
    private List<List<Texture2D>> textures = new List<List<Texture2D>>();
    private int folder_index = 0;
    private int image_index = 0;

    public void NextFolder() { ChangeIndex(ref folder_index, 1, folder_names.Count, true); }

    public void PreviousFolder() { ChangeIndex(ref folder_index, -1, folder_names.Count, true); }

    public void NextImage() { if (textures.Count > 0) ChangeIndex(ref image_index, 1, textures[folder_index].Count, false); }

    public void PreviousImage() { if (textures.Count > 0) ChangeIndex(ref image_index, -1, textures[folder_index].Count, false); }

    private void ChangeIndex(ref int index, int increment, int max_count, bool folder_change)
    {
        int previous_index = index;

        index += increment;
        index = (index >= max_count) ? 0 : index;
        index = (index < 0) ? (max_count - 1) : index;
        if (index != previous_index)
        {
            image_index = folder_change ? 0 : image_index;
            RefreshDisplay();
        }
    }

    private void RefreshDisplay()
    {
        Texture2D tex = textures[folder_index][image_index];
        if (tex)
        {
            ref_ui_text_folder.text = folder_names[folder_index];
            ref_SR_preview.sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        }
    }

    private void Start()
    {
        string level_directory = Application.dataPath + "/" + levels_file_path;
        //Debug.Log(level_directory);
        if (Directory.Exists(level_directory))
        {
            string[] level_folders_path = Directory.GetDirectories(level_directory);
            foreach (string level_folder_path in level_folders_path)
            {
                //Debug.Log("\t" + level_folder_path);
                if (Directory.Exists(level_folder_path))
                {
                    folder_names.Add(Path.GetFileName(level_folder_path));
                    List<Texture2D> folder_textures = new List<Texture2D>();
                    string[] levels_path = Directory.GetFiles(level_folder_path);
                    foreach (string level_path in levels_path)
                    {
                        // Ignore non-image files
                        if (IMAGE_EXTENSIONS.Contains(Path.GetExtension(level_path).ToUpperInvariant()))
                        {
                            //Debug.Log("\t\t" + level_path);
                            byte[] file_data = File.ReadAllBytes(level_path);
                            Texture2D tex = new Texture2D(1, 1);
                            tex.LoadImage(file_data);
                            tex.filterMode = FilterMode.Point;
                            folder_textures.Add(tex);
                        }
                    }
                    textures.Add(folder_textures);
                }
            }
        }
        Debug.Log("test");
        RefreshDisplay();
    }
}
