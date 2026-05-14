using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float duration = 0.1f; // Ne kadar sürede hedefe ulaşsın?

    void Update() // LateUpdate yerine Update deniyoruz (Interpolate açıkken)
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // DOMove'un eski işlemleri iptal etmesi (Complete) takılmayı önler
        transform.DOMove(targetPosition, duration).SetEase(Ease.Linear).SetUpdate(UpdateType.Normal);
    }
}