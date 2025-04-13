using Cinemachine;
using Fusion;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject[] playerPrefab;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private GameObject chatManagerPrefab;

    private void Awake()
    {
        if (ChatManager.Instance == null && Runner != null && Runner.IsServer)
        {
            Runner.Spawn(chatManagerPrefab, Vector3.zero, Quaternion.identity);
        }
    }

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
            if (playerPrefab == null || playerPrefab.Length == 0 || virtualCamera == null)
            {
                Debug.LogError("PlayerPrefab hoặc VirtualCamera chưa được gán!");
                return;
            }

            int playerIndex = LoginManager.indexPlayer;
            Vector2 randomPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            NetworkObject spawnedPlayer = Runner.Spawn(playerPrefab[playerIndex], randomPosition, Quaternion.identity, player);

            if (spawnedPlayer != null)
            {
                UpdateVirtualCameraTarget(spawnedPlayer.transform);
                SceneLoader.continueLoading = true;

                // Đặt tên người chơi từ LoginManager
                string playerName = LoginManager.PlayerNameStatic ?? "Player_" + player.PlayerId.ToString();
                spawnedPlayer.gameObject.name = playerName;
            }
            else
            {
                Debug.LogError("Spawn thất bại!");
            }
        }
    }
}