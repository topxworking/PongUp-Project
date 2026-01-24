using TMPro;
using UnityEngine;

public class GameVersion : MonoBehaviour
{
    private TextMeshProUGUI gameVersion;

    private void Start()
    {
        gameVersion = GetComponent<TextMeshProUGUI>();
        if (gameVersion != null)
        {
            gameVersion.text = "Version " + Application.version;
        }
    }
}
