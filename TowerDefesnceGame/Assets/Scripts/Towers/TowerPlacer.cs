
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
 
public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }
 
    private GameObject          selectedPrefab;
    private int                 selectedCost;
    private GameObject          previewObj;
    private TowerPlacementPreview preview;
    private bool                isPlacing = false;
 
    // Undo support
    private GameObject lastPlacedTower;
    private int        lastPlacedCost;
 
    public bool IsPlacing => isPlacing;
 
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
 
    void Start()
    {
        if (InputHandler.Instance == null)
        {
            Debug.LogError("TowerPlacer: InputHandler.Instance is null!");
            return;
        }
        InputHandler.Instance.OnClickPerformed      += OnClick;
        InputHandler.Instance.OnRightClickPerformed += OnRightClick;
        InputHandler.Instance.OnCancelPerformed     += Cancel;
        InputHandler.Instance.OnSpeedUpChanged      += OnSpeedUp;
    }
 
    void OnDisable()
    {
        if (InputHandler.Instance == null) return;
        InputHandler.Instance.OnClickPerformed      -= OnClick;
        InputHandler.Instance.OnRightClickPerformed -= OnRightClick;
        InputHandler.Instance.OnCancelPerformed     -= Cancel;
        InputHandler.Instance.OnSpeedUpChanged      -= OnSpeedUp;
    }
 
    void Update()
    {
        if (!isPlacing || previewObj == null) return;
        Vector3 snapped = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        previewObj.transform.position = snapped;
        preview.SetValid(IsValidPlacement(snapped));
 
        // Z key to undo
        if (UnityEngine.InputSystem.Keyboard.current.zKey.wasPressedThisFrame)
            UndoLastPlace();
    }
 
    // ── Placement ─────────────────────────────────────────────
 
    public void BeginPlacement(GameObject prefab, int cost)
    {
        Cancel();
        selectedPrefab = prefab;
        selectedCost   = cost;
        isPlacing      = true;
 
        previewObj = Instantiate(prefab);
        foreach (var c in previewObj.GetComponents<Tower>())    c.enabled = false;
        foreach (var c in previewObj.GetComponents<Collider2D>()) c.enabled = false;
 
        preview = previewObj.GetComponent<TowerPlacementPreview>();
        if (!preview) preview = previewObj.AddComponent<TowerPlacementPreview>();
 
        TowerUpgradeUI.Instance?.Hide();
    }
 
    void OnClick()
    {
        if (isPlacing) { TryPlace(); return; }
        if (IsPointerOverUI()) return;
 
        Vector2 mouse = InputHandler.Instance.MouseWorldPos;
        Collider2D hit = Physics2D.OverlapPoint(mouse, LayerMask.GetMask("Tower"));
 
        if (hit != null)
        {
            var tower = hit.GetComponent<Tower>();
            if (tower != null)
            {
                tower.SetSelected(true);
                TowerUpgradeUI.Instance?.Show(tower);
                return;
            }
        }
        TowerUpgradeUI.Instance?.Hide();
    }
 
    void OnRightClick() => Cancel();
 
    void TryPlace()
    {
        Vector3 pos = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        if (!IsValidPlacement(pos)) return;
 
        int actualCost = Mathf.RoundToInt(selectedCost * LevelThemeManager.TowerCostMult);
        if (!GameManager.Instance.SpendGold(actualCost)) return;
 
        var placed = Instantiate(selectedPrefab, pos, Quaternion.identity);
 
        // Store for undo
        lastPlacedTower = placed;
        lastPlacedCost  = actualCost;
 
        // Effects
        AudioManager.Instance?.Play("place_meow");
        ObjectPool.Instance.Spawn("PlaceEffect", pos, Quaternion.identity);
 
        // Scale pop on the placed tower
        StartCoroutine(PlacePop(placed.transform));
 
        // Notify nearby towers of new synergy
        var placedTower = placed.GetComponent<Tower>();
        if (placedTower != null)
        {
            placedTower.RecalculateSynergy();
            // Also update neighbours
            var nearby = Physics2D.OverlapCircleAll(pos, 2f, LayerMask.GetMask("Tower"));
            foreach (var col in nearby)
                col.GetComponent<Tower>()?.RecalculateSynergy();
        }
 
        Cancel();
    }
 
    System.Collections.IEnumerator PlacePop(Transform t)
    {
        if (t == null) yield break;
        Vector3 orig = t.localScale;
        float elapsed = 0f;
        while (elapsed < 0.12f)
        {
            t.localScale = Vector3.Lerp(orig * 0.5f, orig * 1.2f, elapsed / 0.12f);
            elapsed += Time.deltaTime; yield return null;
        }
        elapsed = 0f;
        while (elapsed < 0.08f)
        {
            t.localScale = Vector3.Lerp(orig * 1.2f, orig, elapsed / 0.08f);
            elapsed += Time.deltaTime; yield return null;
        }
        t.localScale = orig;
    }
 
    public void UndoLastPlace()
    {
        if (lastPlacedTower == null) return;
        GameManager.Instance.EarnGold(lastPlacedCost);
        Destroy(lastPlacedTower);
        lastPlacedTower = null;
 
        FloatingTextPool.Instance?.Spawn(
            Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 10f)),
            "Placement undone", Color.white);
    }
 
    bool IsValidPlacement(Vector3 pos)
    {
        if (Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Path"))  != null) return false;
        if (Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Tower")) != null) return false;
        return true;
    }
 
    void Cancel()
    {
        isPlacing = false;
        if (previewObj) Destroy(previewObj);
        previewObj = selectedPrefab = null;
    }
 
    void OnSpeedUp(bool active)
    {
        Time.timeScale = active ? 2f : 1f;
        UIManager.Instance?.UpdateSpeedIndicator(active);
    }
 
    bool IsPointerOverUI()
    {
        var ed = new PointerEventData(EventSystem.current)
            { position = UnityEngine.InputSystem.Mouse.current.position.ReadValue() };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ed, results);
        return results.Count > 0;
    }
 
    Vector3 SnapToGrid(Vector2 pos) =>
        new Vector3(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f, 0f);
}