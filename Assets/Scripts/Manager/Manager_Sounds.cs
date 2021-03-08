using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager_Sounds : MonoBehaviour
{
    public static Manager_Sounds Instance = null;

    [SerializeField] private AudioSource source_track = null;
    [SerializeField] private AudioSource source_sfx = null;

    [SerializeField] private AudioClip track_main = null;
    [SerializeField] private AudioClip sfx_ui_button = null;
    [SerializeField] private AudioClip sfx_error = null;
    [SerializeField] private AudioClip sfx_win = null;
    [SerializeField] private AudioClip sfx_color = null;
    [SerializeField] private AudioClip sfx_color_change = null;
    [SerializeField] private AudioClip sfx_transform = null;

    [SerializeField] private float starting_volume_track = 0.5f;
    [SerializeField] private float starting_volume_sfx = 0.5f;

    public float GetTrackVolume() { return source_track.volume; }
    public float GetSFXVolume() { return source_sfx.volume; }

    public void SetTrackVolume(float v)
    {
        source_track.volume = v;
    }

    public void SetSFXVolume(float v)
    {
        source_sfx.volume = v;
    }

    public void PlaySFXUIButton() { source_sfx.PlayOneShot(sfx_ui_button); }
    public void PlaySFXError() { source_sfx.PlayOneShot(sfx_error); }
    public void PlaySFXWin() { source_sfx.PlayOneShot(sfx_win); }
    public void PlaySFXColor() { source_sfx.PlayOneShot(sfx_color); }
    public void PlaySFXColorChange() { source_sfx.PlayOneShot(sfx_color_change); }
    public void PlaySFXTransform() { source_sfx.PlayOneShot(sfx_transform); }


    public void StopSFX()
    {
        source_sfx.Stop();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this.gameObject);

        source_track.volume = starting_volume_track;
        source_sfx.volume = starting_volume_sfx;
    }

    public void Start()
    {
        source_track.clip = track_main;
        source_track.loop = true;
        source_track.Play();
    }

    private bool ShouldPlaySFX(float last_play = 0f, float clip_length = 0f)
    {
        float e_time = Time.time - last_play;
        if (clip_length != 0 && e_time <= clip_length)
        {
            return false;
        }

        return true;
    }
}
