using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class CowEntity : MonoBehaviour
{
    public CowData Data          { get; private set; }
    public GridCell CurrentCell  { get; set; }

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public TMP_Text       tierLabel;     // optional small tier number badge
    public TMP_Text       nameLabel;     // optional cow name under sprite

    public void Initialize(CowData data, GridCell cell)
    {
        Data        = data;
        CurrentCell = cell;

        // Apply visuals
        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color  = data.tintColor;

        if (tierLabel) tierLabel.text = data.tier.ToString();
        if (nameLabel) nameLabel.text = data.cowName;

        // Animate in with a little pop scale
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.25f).setEaseOutBack();
    }

    // Call this from DragHandler when a drag starts
    public void OnPickUp()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.15f, 0.1f).setEaseOutQuad();
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    // Call when dropped (not merged)
    public void OnPutDown(GridCell newCell)
    {
        CurrentCell           = newCell;
        transform.position    = newCell.worldPosition;
        LeanTween.scale(gameObject, Vector3.one, 0.1f).setEaseOutQuad();
        GetComponent<SpriteRenderer>().sortingOrder = 1;
    }
}
