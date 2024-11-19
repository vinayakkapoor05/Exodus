// Background.cs
using UnityEngine;

public class InfiniteBackgroundScroller : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float backgroundHeight;

    private Transform[] backgrounds;
    private int topIndex;
    private int bottomIndex;
    private float thresholdY;  

    private void Start()
    {
        if (playerTransform == null)
        {
            return;
        }

        backgrounds = new Transform[3];
        backgrounds[1] = transform;

        GameObject topClone = Instantiate(gameObject, transform.parent);
        topClone.transform.position = transform.position + Vector3.up * backgroundHeight;
        Destroy(topClone.GetComponent<InfiniteBackgroundScroller>());
        backgrounds[2] = topClone.transform;

        GameObject bottomClone = Instantiate(gameObject, transform.parent);
        bottomClone.transform.position = transform.position + Vector3.down * backgroundHeight;
        Destroy(bottomClone.GetComponent<InfiniteBackgroundScroller>());
        backgrounds[0] = bottomClone.transform;

        topIndex = 2;
        bottomIndex = 0;

        thresholdY = playerTransform.position.y;
    }

    private void Update()
    {
        float playerY = playerTransform.position.y;

        if (playerY > backgrounds[topIndex].position.y - backgroundHeight / 2)
        {
            MoveBottomToTop();
        }
        else if (playerY < backgrounds[bottomIndex].position.y + backgroundHeight / 2)
        {
            MoveTopToBottom();
        }
    }

    private void MoveBottomToTop()
    {
        backgrounds[bottomIndex].position = backgrounds[topIndex].position + Vector3.up * backgroundHeight;

        topIndex = bottomIndex;
        bottomIndex = (bottomIndex + 1) % 3;
    }

    private void MoveTopToBottom()
    {
        backgrounds[topIndex].position = backgrounds[bottomIndex].position + Vector3.down * backgroundHeight;

        bottomIndex = topIndex;
        topIndex = (topIndex - 1 + 3) % 3;
    }
}
