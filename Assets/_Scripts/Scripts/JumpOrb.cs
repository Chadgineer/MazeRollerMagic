using UnityEngine;
using DG.Tweening;

public class JumpOrb : MonoBehaviour
{
    private Collider orbCollider;
    private Vector3 originalScale;
    [SerializeField] private float animationDuration = 0.3f;

    void Awake()
    {
        orbCollider = GetComponent<Collider>();
        originalScale = transform.localScale;
    }

    // Oyuncu küreye dokunduğunda çağrılacak fonksiyon
    public void CollectOrb()
    {
        // Çift tetiklenmeyi engellemek için collider'ı anında kapat
        if (orbCollider != null) orbCollider.enabled = false;

        // Küçülerek yok olma animasyonu
        transform.DOKill();
        transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => gameObject.SetActive(false));
    }

    // Oyuncu düştüğünde küreyi eski haline getirecek fonksiyon
    public void ResetOrb()
    {
        // Eğer zaten aktif ve görünürse işlem yapma
        if (gameObject.activeSelf && orbCollider != null && orbCollider.enabled) return;

        transform.DOKill();
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        
        if (orbCollider != null) orbCollider.enabled = true;

        // Pürüzsüzce büyüterek geri getir
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBack);
    }
}