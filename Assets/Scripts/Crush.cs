using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Crush : MonoBehaviour
{
    public CrushType type;

    public void SetType(CrushType type)
    {
        this.type = type;
        GetComponent<SpriteRenderer>().sprite = type.sprite;
    }

    public CrushType GetType() => type;
}
