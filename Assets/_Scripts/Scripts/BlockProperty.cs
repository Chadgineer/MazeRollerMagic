using UnityEngine;

public enum BlockType
{
    StandardBlock,
    NonStackableObject,
    SpawnPlatform,    // Yeni: Karakterin doğacağı başlangıç noktası
    FinishPlatform    // Yeni: Bölümün bittiği platform
}

public class BlockProperty : MonoBehaviour
{
    public BlockType blockType = BlockType.StandardBlock;

    [Tooltip("Eğer objenin pivotu tam altındaysa/tabanındaysa bunu TRUE yap. Küpler gibi tam merkezdeyse FALSE bırak.")]
    public bool pivotAtBottom = false;
}