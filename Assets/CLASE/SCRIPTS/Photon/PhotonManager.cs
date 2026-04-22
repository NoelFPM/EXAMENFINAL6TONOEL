using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private GameObject menuCanvas;

    private void Awake()
    {
        SetupRunner();
    }

    private void SetupRunner()
    {
        _runner = GetComponent<NetworkRunner>();

        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();

        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
    }

    public void CreateGame() => _ = StartGame(GameMode.Host);
    public void TryJoinGame() => _ = StartGame(GameMode.Client);

    private async Task StartGame(GameMode mode)
    {
        try
        {
            if (_runner == null)
                SetupRunner();

            if (_runner.IsRunning)
                return;

            if (menuCanvas != null)
                menuCanvas.SetActive(false);

            var sceneManager = GetComponent<NetworkSceneManagerDefault>();

            if (sceneManager == null)
                sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "DeathmatchPartida",
                SceneManager = sceneManager,
                Scene = SceneRef.FromIndex(1),
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new InputInfo();

        if (InputManager.Instance != null)
        {
            myInput.playerPos = InputManager.Instance.GetMoveInput();
            myInput.lookDirection = InputManager.Instance.GetMouseDelta();
            myInput.isMoving = InputManager.Instance.IsMoveInputPressed();
            myInput.isRunInputPressed = InputManager.Instance.WasRunInputPressed();
            myInput.isMovingBackwards = InputManager.Instance.IsMovingBackwards();
            myInput.isMovingOnXAxis = InputManager.Instance.IsMovingOnXAxis();
            myInput.isShooting = InputManager.Instance.IsShootInputPressed();
            myInput.isReloading = InputManager.Instance.IsReloadInputPressed();
        }

        input.Set(myInput);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        var l1 = GameObject.Find("LobbyP1");
        var l2 = GameObject.Find("LobbyP2");

        Vector3 pos = Vector3.up * 2;

        if (player == runner.LocalPlayer)
        {
            if (l1 != null) pos = l1.transform.position;
        }
        else
        {
            if (l2 != null) pos = l2.transform.position;
        }

        runner.Spawn(playerPrefab, pos, Quaternion.identity, player);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("Connection failed");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
}