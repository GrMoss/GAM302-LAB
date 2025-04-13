using System;
using Fusion;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnScoreChangedRender))]
    public int PlayerScore { get; set; }

    [Networked, OnChangedRender(nameof(OnGoldChangedRender))]
    public int PlayerGold { get; set; }

    [Networked]
    public string PlayerName { get; set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnGoldChanged;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            PlayerName = LoginManager.PlayerNameStatic;
            // Debug.Log($"[PlayerManager] Spawned player {PlayerName}");
        }
    }

    private void OnScoreChangedRender()
    {
        OnScoreChanged?.Invoke(PlayerScore);
    }

    private void OnGoldChangedRender()
    {
        OnGoldChanged?.Invoke(PlayerGold);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddScore(int score)
    {
        PlayerScore += score;
        FirebaseWebGL.Instance.SaveScore(this);
        // Debug.Log($"[PlayerManager] Added {score} score to {PlayerName}, new score: {PlayerScore}");
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddGold(int gold)
    {
        PlayerGold += gold;
        // Debug.Log($"[PlayerManager] Added {gold} gold to {PlayerName}, new gold: {PlayerGold}");
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReduceGold(int gold)
    {
        if (PlayerGold >= gold)
        {
            PlayerGold -= gold;
            // Debug.Log($"[PlayerManager] Reduced {gold} gold from {PlayerName}, new gold: {PlayerGold}");
        }
        else
        {
            // Debug.LogWarning($"[PlayerManager] Not enough gold to reduce {gold} from {PlayerName}, current gold: {PlayerGold}");
        }
    }

    public int GetPlayerScore()
    {
        return PlayerScore;
    }

    public int GetPlayerGold()
    {
        return PlayerGold;
    }
}