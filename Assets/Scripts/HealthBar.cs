// HealthBar.cs
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Transform target;  
    public Vector3 offset = new Vector3(0, 0.75f, 0);  
    
    private Camera mainCamera;
    private RectTransform rectTransform;
    private Slider slider;
    
    private void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        slider = GetComponent<Slider>();
        
         
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }
    
    private void LateUpdate()
    {
        if (target != null)
        {
           
            Vector3 worldPosition = target.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            rectTransform.position = screenPosition;
            
            
            transform.rotation = Quaternion.identity;
        }
    }
    
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        slider.value = currentHealth / maxHealth;
    }
}