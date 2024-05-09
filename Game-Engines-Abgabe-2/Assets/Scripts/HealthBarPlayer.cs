using UnityEngine;
using UnityEngine.UI;

public class HealthBarPlayer : MonoBehaviour
{
    public Image fillImage;
    public Slider slider;
    public Gradient gradient;
    private PlayerCombat playerScript;
    private GameObject playerObject;
    private int maxHealth;
    
    // Start is called before the first frame update
    void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerScript = playerObject.GetComponent<PlayerCombat>();
        maxHealth = playerScript.healthpoints;
        slider.maxValue = maxHealth;
        slider.value = playerScript.healthpoints;
        fillImage.color = gradient.Evaluate(1f);
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerScript.healthpoints;
        fillImage.color = gradient.Evaluate(slider.normalizedValue);
    }
}
