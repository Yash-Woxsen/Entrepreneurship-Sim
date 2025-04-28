using UnityEngine;

public class AvatarAnimHandler : MonoBehaviour
{
    public HandleAudioVoiceOver audioVoiceOverHandler;
    public Animator animator;
    
    private void Update()
    {
        if (audioVoiceOverHandler != null)
        {
            bool isPlaying = audioVoiceOverHandler.isAudioPlaying;

            // Set animator bool parameter "IsTalking" based on audio playing state
            animator.SetBool("IsTalking", isPlaying);
        }
    } 
}
