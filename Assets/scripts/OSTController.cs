using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour
{
    public AudioClip[] songs; // Array to hold the songs
    private AudioSource audioSource; // AudioSource component
    public float[] difficultyThresholds; // Array to hold difficulty thresholds for each song

    void Start()
    {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
        
        // Check if the AudioSource component exists
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Start the coroutine to play the songs
        StartCoroutine(PlaySongsSequentially());
    }

    IEnumerator PlaySongsSequentially()
    {
        int currentSongIndex = 0;

        while (currentSongIndex < songs.Length)
        {
            // Assign the current song to the AudioSource
            audioSource.clip = songs[currentSongIndex];
            
            // Play the song
            audioSource.Play();

            // Wait until the song finishes or the difficulty threshold is met
            while (audioSource.isPlaying)
            {
                // Check if the difficulty threshold for the next song is met
                if (EnemySpawner.getGlobalDifficultyMultiplier() >= difficultyThresholds[currentSongIndex])
                {
                    // Break the loop to transition to the next song
                    break;
                }
                yield return null; // Wait for the next frame
            }

            // Move to the next song if the difficulty threshold is met
            if (EnemySpawner.getGlobalDifficultyMultiplier() >= difficultyThresholds[currentSongIndex])
            {
                currentSongIndex++;
            }
            else
            {
                // Restart the current song if the difficulty threshold is not met
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length);
            }
        }
    }
}
