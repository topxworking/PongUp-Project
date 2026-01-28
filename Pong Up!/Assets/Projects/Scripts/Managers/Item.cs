using UnityEngine;

public enum ItemType
{
    ScorePlus5,
    ScorePlus10,
    ScoreMinus5,
    ScoreMinus10,
    SmallPaddle,
    LargePaddle
}

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public float fallSpeed = 1f;

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < -6f) Destroy(gameObject);
    }
}
