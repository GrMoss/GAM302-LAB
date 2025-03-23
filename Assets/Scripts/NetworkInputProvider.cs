// using Fusion;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class NetworkInputProvider : NetworkBehaviour
// {
//     [SerializeField] private InputActionReference moveActionToUse;

//     private void Awake()
//     {
//         if (moveActionToUse != null && moveActionToUse.action != null)
//         {
//             moveActionToUse.action.Enable();
//         }
//     }

//     private void OnDisable()
//     {
//         if (moveActionToUse != null && moveActionToUse.action != null)
//         {
//             moveActionToUse.action.Disable();
//         }
//     }

//     public override void FixedUpdateNetwork()
//     {
//         if (HasInputAuthority)
//         {
//             NetworkInputData input = new NetworkInputData();
//             input.MoveDirection = moveActionToUse.action.ReadValue<Vector2>();
//             Runner.ProvideInput(input);
//         }
//     }
// }