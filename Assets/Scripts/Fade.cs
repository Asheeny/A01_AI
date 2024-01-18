using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Fade : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 5f;
    
    private Image fadeImage;
    private float alpha;

    public bool fadeOut = true;

    private void Awake()
    {
        fadeImage = GetComponent<Image>();    
    }

    void Update()
    {
        alpha = Mathf.Lerp(fadeImage.color.a, fadeOut ? 1 : 0, fadeSpeed * Time.deltaTime);

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);

        if (Mathf.Approximately(fadeImage.color.a, 0))
            gameObject.SetActive(false);
    }
}
