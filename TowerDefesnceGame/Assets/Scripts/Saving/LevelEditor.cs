// Scripts/Saving/LevelEditor.cs

using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class LevelEditor : MonoBehaviour
{
    public Tilemap  tilemap;
    public TileBase pathTile;
    public TileBase groundTile;
    public GameObject editorUI;

    private bool editorActive = false;
    private bool erasing      = false;

    void Start()
    {
        if (editorUI) editorUI.SetActive(false);
    }

    public void ToggleEditor()
    {
        editorActive = !editorActive;
        if (editorUI) editorUI.SetActive(editorActive);
    }

    void Update()
    {
        if (!editorActive) return;

        erasing = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;

        if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
            PaintAt(Camera.main.ScreenToWorldPoint(
                UnityEngine.InputSystem.Mouse.current.position.ReadValue()));
    }

    void PaintAt(Vector3 worldPos)
    {
        worldPos.z = 0;
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        tilemap.SetTile(cell, erasing ? groundTile : pathTile);
    }

    public void SaveLevel(string fileName = "level.json")
    {
        // SavedLevelData replaces the old LevelData name
        // (LevelData is now used by LevelSelectManager)
        var data = new SavedLevelData();
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;
            bool isPath = tilemap.GetTile(pos) == pathTile;
            data.tiles.Add(new TileEntry { x = pos.x, y = pos.y, isPath = isPath });
        }
        string path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"Level saved: {path}");
    }

    public void LoadLevel(string fileName = "level.json")
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path)) { Debug.LogWarning("No level file."); return; }
        var data = JsonUtility.FromJson<SavedLevelData>(File.ReadAllText(path));
        tilemap.ClearAllTiles();
        foreach (var t in data.tiles)
            tilemap.SetTile(new Vector3Int(t.x, t.y, 0), t.isPath ? pathTile : groundTile);
    }
}