using System.Collections;
using UnityEngine;
using TMPro;

public class AbilityUnlockMessage : MonoBehaviour
{
    public TextMeshProUGUI unlockText;
    public float displayDuration = 3f;

    void Start()
    {
        StartCoroutine(ShowUnlockMessage());
    }

    IEnumerator ShowUnlockMessage()
    {
        unlockText.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        unlockText.gameObject.SetActive(false);
    }
}
