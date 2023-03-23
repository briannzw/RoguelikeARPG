using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaSound : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip CoinCollectClip;
    public AudioClip StartClip;
    public AudioClip WinClip;
    public AudioClip LoseClip;
    public AudioClip DeadClip;
    public AudioClip[] HurtClips;
    public AudioClip HeavyDamageClips;
    private void Start()
    {
        DungeonGenerator.Instance.OnDungeonComplete += () => audioSource.PlayOneShot(StartClip);
        GameManager.Instance.OnCoinCollect += () => audioSource.PlayOneShot(CoinCollectClip);
        GameManager.Instance.GameTimerEnd += () => audioSource.PlayOneShot(WinClip);
        if(LoseClip != null)
            GameManager.Instance.PlayerLose += () => audioSource.PlayOneShot(LoseClip);
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPlayerDead() => audioSource.PlayOneShot(DeadClip);
    public void OnPlayerHurt() => audioSource.PlayOneShot(HurtClips[Random.Range(0, HurtClips.Length)]);
    public void OnPlayerHeavyDamaged() => audioSource.PlayOneShot(HeavyDamageClips);

    public void PlayCharaSound(AudioClip voiceline)
    {
        audioSource.Stop();
        audioSource.clip = voiceline;
        audioSource.Play();
    }
}
