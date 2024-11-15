using UnityEngine;
using System.Threading.Tasks;

public static class DirectAudioHelper
{
    public static async Task PlayAudioClip(AudioClip clip, float volume = 1)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null");
            return;
        }

        AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume);

        await Task.Delay((int)(clip.length * 1000));
    }
}