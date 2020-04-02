using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Awake()
    {
        var fill = 0.3f;
        float angle = Mathf.Clamp01(fill);
        angle *= 90f * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        sin /= cos;

        Debug.LogError(cos);
        Debug.LogError(sin);
        Debug.LogError(Mathf.Lerp(0, 1, sin));
    }
}