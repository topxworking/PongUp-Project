using UnityEngine;

public enum ItemType
{
    Score5,
    Score10,
    LargePaddle
}

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public float fallSpeed = 3f;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < -6f) Destroy(gameObject);
    }
}
