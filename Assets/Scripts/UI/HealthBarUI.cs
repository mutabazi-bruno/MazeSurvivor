using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        Character player = GameObject.FindGameObjectWithTag("Player").GetComponent<Character>();

        // set the bar correctly right at the start, not just after the first hit
        
        slider.maxValue = player.CurrentHealth;
        slider.value = player.CurrentHealth;

        player.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(int current, int max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}