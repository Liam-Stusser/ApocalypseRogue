using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private Transform target;
    private Vector3 offset;

    public void Init(Transform targetTransform, Vector3 worldOffset)
    {
        target = targetTransform;
        offset = worldOffset;
    }

    public void SetValue(float current, float max)
    {
        fillImage.fillAmount = Mathf.Clamp01(current / max);
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}

