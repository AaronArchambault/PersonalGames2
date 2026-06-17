using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public static InputHandler Instance { get; private set; }

    private GameInputActions actions;

    public Vector2 MouseWorldPos { get; private set; }

    public event System.Action OnClickPerformed;
    public event System.Action OnRightClickPerformed;
    public event System.Action OnCancelPerformed;
    public event System.Action<bool> OnSpeedUpChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        actions = new GameInputActions();
    }

    void OnEnable()
    {
        actions.Gameplay.Enable();
        actions.Gameplay.Click.performed        += _ => OnClickPerformed?.Invoke();
        actions.Gameplay.RightClick.performed   += _ => OnRightClickPerformed?.Invoke();
        actions.Gameplay.Cancel.performed       += _ => OnCancelPerformed?.Invoke();
        actions.Gameplay.SpeedUp.performed      += _ => OnSpeedUpChanged?.Invoke(true);
        actions.Gameplay.SpeedUp.canceled       += _ => OnSpeedUpChanged?.Invoke(false);
    }

    void OnDisable() => actions.Gameplay.Disable();

    void Update()
    {
        Vector2 screen = actions.Gameplay.MousePosition.ReadValue<Vector2>();
        MouseWorldPos = Camera.main.ScreenToWorldPoint(screen);
    }
}