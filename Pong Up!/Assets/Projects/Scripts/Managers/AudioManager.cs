using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("UI References")]
    public Image bottonImage;
    public Sprite muteSprite;
    public Sprite unmuteSprite;

    private bool isMuted = false;

    void Start()
    {
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;

        ApplyMute();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    private void ApplyMute()
    {
        AudioListener.volume = isMuted ? 0f : 1f;

        if (bottonImage != null && muteSprite != null && unmuteSprite != null)
        {
            bottonImage.sprite = isMuted ? muteSprite : unmuteSprite;
        }
    }
}