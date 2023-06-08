using UnityEngine;
using Module;
using UnityEngine.InputSystem;
using Module.Detector;
using System.Linq;

public class PlayerAutoTarget : MonoBehaviour
{
    [Header("Referencces")]
    [SerializeField] private float aimArea = 5f;
    [SerializeField] private LayerMask targetMask;

    private PlayerAction playerControls;
    private Transform nearestTarget;

    private void Start()
    {
        playerControls = InputManager.playerAction;
        RegisterInputCallback();
    }

    private void OnEnable()
    {
        RegisterInputCallback();
    }

    private void OnDisable()
    {
        UnregisterInputCallback();
    }

    #region Callbacks
    private void RegisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Attack.performed += DoTarget;
        playerControls.Gameplay.Skill1.performed += DoTarget;
        playerControls.Gameplay.Skill2.performed += DoTarget;
    }

    private void UnregisterInputCallback()
    {
        if (playerControls == null) return;
        playerControls.Gameplay.Attack.performed -= DoTarget;
        playerControls.Gameplay.Skill1.performed -= DoTarget;
        playerControls.Gameplay.Skill2.performed -= DoTarget;
    }

    private void DoTarget(InputAction.CallbackContext context)
    {
        var TargetList = ColliderDetector.Find<Transform>(transform.position, aimArea, targetMask);
        if (TargetList.Count > 0)
        {
            nearestTarget = TargetList.OrderBy(
                obj => (transform.position - obj.transform.position).sqrMagnitude).ToArray()[0];

            Vector3 direction = (nearestTarget.position - transform.position).normalized;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        }
    }
    #endregion
}
