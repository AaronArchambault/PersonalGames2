
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
 
public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }
 
    private GameObject         selectedPrefab;
    private int                selectedCost;
    private GameObject         previewObj;
    private TowerPlacementPreview preview;
    private bool               isPlacing = false;
 
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
 
    // ── Placement ─────────────────────────────────────────────
 
    public void BeginPlacement(GameObject prefab, int cost)
    {
        Cancel();
        selectedPrefab = prefab;
        selectedCost   = cost;
        isPlacing      = true;
 
        previewObj = Instantiate(prefab);
        foreach (var c in previewObj.GetComponents<Tower>())
            c.enabled = false;
        foreach (var c in previewObj.GetComponents<Collider2D>())
            c.enabled = false;
 
        preview = previewObj.GetComponent<TowerPlacementPreview>();
        if (!preview) preview = previewObj.AddComponent<TowerPlacementPreview>();
 
        TowerUpgradeUI.Instance?.Hide();
    }
 
    void Update()
    {
        if (!isPlacing || previewObj == null) return;
 
        Vector3 snapped = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        previewObj.transform.position = snapped;
        preview.SetValid(IsValidPlacement(snapped));
    }
 
    // ── Input Handlers ────────────────────────────────────────
 
    void OnClick()
    {
        // Case 1: placing a tower
        if (isPlacing) { TryPlace(); return; }
 
        // Case 2: click hit UI — let the UI button handle it
        if (IsPointerOverUI()) return;
 
        // Case 3: check if a tower was clicked in world space
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
 
        // Case 4: clicked empty space — close upgrade panel
        TowerUpgradeUI.Instance?.Hide();
    }
 
    void OnRightClick() => Cancel();
 
    void TryPlace()
    {
        Vector3 pos = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        if (!IsValidPlacement(pos)) return;
 
        // Apply level theme cost multiplier
        int actualCost = Mathf.RoundToInt(selectedCost * LevelThemeManager.TowerCostMult);
        if (!GameManager.Instance.SpendGold(actualCost)) return;
 
        Instantiate(selectedPrefab, pos, Quaternion.identity);
        AudioManager.Instance?.Play("place_meow");
        ObjectPool.Instance.Spawn("PlaceEffect", pos, Quaternion.identity);
        Cancel();
    }
 
    bool IsValidPlacement(Vector3 pos)
    {
        if (Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Path"))   != null) return false;
        if (Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Tower"))  != null) return false;
        return true;
    }
 
    void Cancel()
    {
        isPlacing = false;
        if (previewObj) Destroy(previewObj);
        previewObj     = null;
        selectedPrefab = null;
    }
 
    void OnSpeedUp(bool active)
    {
        Time.timeScale = active ? 2f : 1f;
        UIManager.Instance?.UpdateSpeedIndicator(active);
    }
 
    // ── Helpers ───────────────────────────────────────────────
 
    bool IsPointerOverUI()
    {
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
 
    Vector3 SnapToGrid(Vector2 pos) =>
        new Vector3(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f, 0f);
}