using Cinemachine;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject[] playerPrefab;
    public CinemachineVirtualCamera virtualCamera;

    private void UpdateVirtualCameraTarget(Transform playerTransform)
    {
        if (virtualCamera != null && playerTransform != null)
        {
            virtualCamera.Follow = playerTransform;
            virtualCamera.LookAt = playerTransform;
        }
        else
        {
            Debug.LogWarning("VirtualCamera hoặc PlayerTransform bị null!");
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (playerPrefab == null || virtualCamera == null)
            {
                Debug.LogError("PlayerPrefab or VirtualCamera is not assigned!");
                return;
            }
            int playerIndex = LoginManager.indexPlayer;

            Vector2 randomPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            NetworkObject spawnedPlayer = Runner.Spawn(playerPrefab[playerIndex], randomPosition, Quaternion.identity, player);

            if (spawnedPlayer != null)
            {
                UpdateVirtualCameraTarget(spawnedPlayer.transform);
                SceneLoader.continueLoading = true;
            }
            else
            {
                Debug.LogError("Spawn thất bại!");
            }
        }
    }
}