using Fusion;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnHealthChanged))]
    public byte NetworkedHealth { get; set; }

    [Networked] private bool NeedsRespawn { get; set; }

    [SerializeField] private byte maxHealth = 100;
    [SerializeField] private Image healthBar;

    private SimpleKCC _kcc;

    
    private Transform[] _puntosDeMuerte;
    private Transform _lobbyP1;
    private Transform _lobbyP2;

    public override void Spawned()
    {
        _kcc = GetComponent<SimpleKCC>();

        if (Object.HasStateAuthority)
        {
            NetworkedHealth = maxHealth;

            _lobbyP1 = GameObject.Find("LobbyP1")?.transform;
            _lobbyP2 = GameObject.Find("LobbyP2")?.transform;

            GameObject contenedor = GameObject.Find("ContenedorSpawns");
            if (contenedor != null) _puntosDeMuerte = contenedor.GetComponentsInChildren<Transform>();

            MoverALobbyInicial();

            
            GetComponent<SonidosPro>().RPC_SpawnSonido();
        }
    }

    private void MoverALobbyInicial()
    {
        if (Object.InputAuthority.PlayerId == 1 && _lobbyP1 != null)
            _kcc.SetPosition(_lobbyP1.position);
        else if (_lobbyP2 != null)
            _kcc.SetPosition(_lobbyP2.position);
    }

    void OnHealthChanged()
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)NetworkedHealth / maxHealth;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !NeedsRespawn) return;

        NeedsRespawn = false;

       
        if (_puntosDeMuerte != null && _puntosDeMuerte.Length > 1)
        {
            int index = Random.Range(1, _puntosDeMuerte.Length);
            _kcc.SetPosition(_puntosDeMuerte[index].position);
        }
        else
        {
            _kcc.SetPosition(new Vector3(0, 2, 0));
        }

        NetworkedHealth = maxHealth;

       
        GetComponent<SonidosPro>().RPC_SpawnSonido();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(byte damage, PlayerRef shooter)
    {
        if (NetworkedHealth <= 0) return;

        NetworkedHealth = (byte)Mathf.Max(0, NetworkedHealth - damage);

        if (NetworkedHealth <= 0)
        {
            MatchManager matchManager = FindFirstObjectByType<MatchManager>();
            if (matchManager != null)
            {
                matchManager.RegisterKill(shooter);
            }

            NeedsRespawn = true;
        }
    }
}