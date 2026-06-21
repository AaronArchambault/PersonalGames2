using UnityEngine;

public class TowerPlacer : MonoBehaviour
{
    public static TowerPlacer Instance { get; private set; }

    private GameObject selectedPrefab;
    private int        selectedCost;
    private GameObject previewObj;
    private TowerPlacementPreview preview;
    private bool isPlacing = false;
    public bool IsPlacing => isPlacing;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

   /* void OnEnable()
    {
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
    }*/

            // Scripts/Towers/TowerPlacer.cs — replace OnEnable and OnDisable

void Start()
{
    if (InputHandler.Instance == null)
    {
        Debug.LogError("TowerPlacer: InputHandler.Instance is null! Make sure InputHandler is on the Managers GameObject and its Awake runs first.");
        return;
    }
    SubscribeInput();
}

void SubscribeInput()
{
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


    public void BeginPlacement(GameObject prefab, int cost)
    {
        Cancel(); // Clear previous
        selectedPrefab = prefab;
        selectedCost   = cost;
        isPlacing      = true;

        previewObj = Instantiate(prefab);
        // Disable all logic on preview
        foreach (var c in previewObj.GetComponents<Tower>())  c.enabled = false;
        foreach (var c in previewObj.GetComponents<Collider2D>()) c.enabled = false;

        // Ensure preview component exists
        preview = previewObj.GetComponent<TowerPlacementPreview>();
        if (!preview) preview = previewObj.AddComponent<TowerPlacementPreview>();

        TowerUpgradeUI.Instance?.Hide();
    }

    void Update()
    {
        if (!isPlacing || previewObj == null) return;

        Vector3 snapped = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        previewObj.transform.position = snapped;

        bool valid = IsValidPlacement(snapped);
        preview.SetValid(valid);
    }

    void OnClick()
    {
        if (isPlacing)
        {
            TryPlace();
            return;
        }

        // Click existing tower to select
        Vector2 mouse = InputHandler.Instance.MouseWorldPos;
        var hit = Physics2D.OverlapPoint(mouse);
        if (hit == null) { TowerUpgradeUI.Instance?.Hide(); return; }

        var tower = hit.GetComponent<Tower>();
        if (tower == null) { TowerUpgradeUI.Instance?.Hide(); return; }

        tower.SetSelected(true);
        TowerUpgradeUI.Instance?.Show(tower);
    }

    void OnRightClick() => Cancel();

    void TryPlace()
    {
        Vector3 pos = SnapToGrid(InputHandler.Instance.MouseWorldPos);
        if (!IsValidPlacement(pos)) return;
        if (!GameManager.Instance.SpendGold(selectedCost)) return;

        // In TryPlace(), replace SpendGold line:
int actualCost = Mathf.RoundToInt(selectedCost * LevelThemeManager.TowerCostMult);
if (!GameManager.Instance.SpendGold(actualCost)) return;

        Instantiate(selectedPrefab, pos, Quaternion.identity);

        // Place sound
        AudioManager.Instance?.Play("place");

        // Particle burst at placement
        ObjectPool.Instance.Spawn("PlaceEffect", pos, Quaternion.identity);

        // Keep placing (hold shift style — single place per click)
        Cancel();
    }

    bool IsValidPlacement(Vector3 pos)
    {
        // Not on path tile
        Collider2D pathHit = Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Path"));
        if (pathHit != null) return false;

        // Not on existing tower
        Collider2D towerHit = Physics2D.OverlapCircle(pos, 0.4f, LayerMask.GetMask("Tower"));
        if (towerHit != null) return false;

        return true;
    }

    void Cancel()
    {
        isPlacing = false;
        if (previewObj) Destroy(previewObj);
        previewObj = null;
        selectedPrefab = null;
    }

    void OnSpeedUp(bool active)
    {
        Time.timeScale = active ? 2f : 1f;
        UIManager.Instance?.UpdateSpeedIndicator(active);
    }

    Vector3 SnapToGrid(Vector2 pos) =>
        new Vector3(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f, 0f);
}