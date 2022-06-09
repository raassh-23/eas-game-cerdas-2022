using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioClip clickSFX;
    
    [SerializeField]
    private AudioClip winSFX;
    
    [SerializeField]
    private AudioClip loseSFX;

    [SerializeField]
    private AudioClip resetSFX;

    [SerializeField]
    private AudioClip damagedSFX;

    [SerializeField]
    private AudioClip dropMineSFX;

    [SerializeField]
    private AudioClip shootSFX;

    [SerializeField]
    private AudioClip powerUpSFX;

    [SerializeField]
    private AudioClip checkPointSFX;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic()
    {
        if (audioSource.isPlaying) return;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void PlayClickSFX()
    {
        audioSource.PlayOneShot(clickSFX);
    }

    public void PlayWinSFX()
    {
        audioSource.PlayOneShot(winSFX);
    }

    public void PlayLoseSFX()
    {
        audioSource.PlayOneShot(loseSFX);
    }

    public void PlayResetSFX()
    {
        audioSource.PlayOneShot(resetSFX);
    }

    public void PlayDamagedSFX()
    {
        audioSource.PlayOneShot(damagedSFX);
    }

    public void PlayDropMineSFX()
    {
        audioSource.PlayOneShot(dropMineSFX);
    }

    public void PlayShootSFX()
    {
        audioSource.PlayOneShot(shootSFX);
    }

    public void PlayPowerUpSFX()
    {
        audioSource.PlayOneShot(powerUpSFX);
    }

    public void PlayCheckpointSFX()
    {
        audioSource.PlayOneShot(checkPointSFX);
    }
}