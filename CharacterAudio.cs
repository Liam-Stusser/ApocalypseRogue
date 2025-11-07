using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    public AudioClip walkingClip;
    public AudioClip attackingClip;
    public AudioClip hitClip;
    public AudioClip deathClip;

    public AudioSource audioSource;

    public void PlayStep()
    {
        audioSource.PlayOneShot(walkingClip);
    }

    public void PlayAttack()
    {
        audioSource.PlayOneShot(attackingClip);
    }

    public void PlayHit()
    {
        audioSource.PlayOneShot(hitClip);
    }

    public void PlayDeath()
    {
        audioSource.PlayOneShot(deathClip);
    }
}
