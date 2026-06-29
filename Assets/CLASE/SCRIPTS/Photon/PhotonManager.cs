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

    [SerializeField] private int randomServerNameMaxLenght = 6;

    public static PhotonManager Instance;
    private System.Action<List<SessionInfo>> _onSessionListUpdatedCallback;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;

        SetupRunner();
    }

    private void Start()
    {
        ConnectToPhotonLobby();
    }

    private void SetupRunner()
    {
        _runner = GetComponent<NetworkRunner>();

        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();

        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
    }


    public async Task StartCustomGame(string serverName, int playerCount)
    {
        if (_runner == null) SetupRunner();

        if (_runner.SessionInfo == null && !_runner.IsRunning)
        {
            await _runner.JoinSessionLobby(SessionLobby.ClientServer);
            await Task.Delay(1000);
        }

        if (menuCanvas != null) menuCanvas.SetActive(false);

        var sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = serverName,
            PlayerCount = playerCount,
            SceneManager = sceneManager,
            Scene = SceneRef.FromIndex(1),
            IsVisible = true
        });
    }

    public async void ConnectToPhotonLobby()
    {
        if (_runner == null) SetupRunner();
        if (_runner.IsRunning) return;
        await _runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public void InitializeSessionCallback(System.Action<List<SessionInfo>> callback)
    {
        _onSessionListUpdatedCallback = callback;
    }

    public void StartRandomGameButton()
    {
        _ = StartRandomGame();
    }

    private async Task StartRandomGame()
    {
        try
        {
            if (_runner == null) SetupRunner();

            if (_runner.SessionInfo == null && !_runner.IsRunning)
            {
                await _runner.JoinSessionLobby(SessionLobby.ClientServer);
                await Task.Delay(1000);
            }

            if (menuCanvas != null) menuCanvas.SetActive(false);

            var sceneManager = GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            string nombreRandom = RandomServerName();
            Debug.Log("Nombre generado: " + nombreRandom);

            await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = nombreRandom,
                CustomLobbyName = "CUSTOM LOBBY #" + nombreRandom,
                SceneManager = sceneManager,
                Scene = SceneRef.FromIndex(1),
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public string RandomServerName()
    {
        string characters = "ABCDEFGHIJKLMNÑOPQRSTUVWXYZabcdefghijklmnñopqrstuvwxyz0123456789";
        string resultado = "";
        if (randomServerNameMaxLenght < 4) randomServerNameMaxLenght = 4;
        for (int i = 0; i < randomServerNameMaxLenght; i++)
        {
            int indiceAlAzar = UnityEngine.Random.Range(0, characters.Length);
            resultado += characters[indiceAlAzar];
        }
        return resultado;
    }

    public void CreateGameReal(string roomName, int maxPlayers, bool esPrivada) => _ = StartGameReal(GameMode.Host, roomName, maxPlayers, esPrivada);

    public void JoinGameReal(string roomName)
    {
        _ = StartGameReal(GameMode.Client, roomName, 8, false);
    }

    private async Task StartGameReal(GameMode mode, string roomName, int maxPlayers, bool esPrivada)
    {
        try
        {
            if (_runner.IsRunning) await _runner.Shutdown();
            SetupRunner();

            if (menuCanvas != null) menuCanvas.SetActive(false);

            var sceneManager = GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

            await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = roomName,
                PlayerCount = maxPlayers,
                SceneManager = sceneManager,
                Scene = SceneRef.FromIndex(1),
                IsVisible = !esPrivada
            });
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        _onSessionListUpdatedCallback?.Invoke(sessionList);
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

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
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