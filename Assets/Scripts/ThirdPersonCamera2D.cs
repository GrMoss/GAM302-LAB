// using UnityEngine;

// public class ThirdPersonCamera2D : MonoBehaviour
// {
//     public Transform Target; 
//     public Vector3 Offset = new Vector3(0, 2, -10);
//     public float SmoothSpeed = 5f; 

//     void LateUpdate()
//     {
//         if (Target == null)
//         {
//             return;
//         }

//         Vector3 desiredPosition = Target.position + Offset;
        
//         transform.position = Vector3.Lerp(transform.position, desiredPosition, SmoothSpeed * Time.deltaTime);
//     }
// }