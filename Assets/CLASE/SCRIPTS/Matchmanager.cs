using Fusion;
using UnityEngine;
using TMPro;
using System.Collections;

public class MatchManager : NetworkBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killsText;

    [Header("Configuración")]
    [SerializeField] private float tiempoDePartida = 180f;

    [Networked, Capacity(10)]
    private NetworkDictionary<PlayerRef, int> PuntajesJugadores { get; }

    [Networked] private TickTimer cronometroPartida { get; set; }
    [Networked] private bool partidaFinalizada { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            cronometroPartida = TickTimer.CreateFromSeconds(Runner, tiempoDePartida);
            partidaFinalizada = false;
        }
        if (winnerText != null) winnerText.gameObject.SetActive(false);
    }

    public override void Render()
    {
        if (killsText != null && Runner.LocalPlayer != default)
        {
            int misKills = PuntajesJugadores.ContainsKey(Runner.LocalPlayer) ? PuntajesJugadores.Get(Runner.LocalPlayer) : 0;
            killsText.text = $"Mis Bajas: {misKills} / 5";
        }

        if (!partidaFinalizada && timerText != null && cronometroPartida.IsRunning)
        {
            float tiempoRestante = cronometroPartida.RemainingTime(Runner) ?? 0;
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && !partidaFinalizada)
        {
            if (cronometroPartida.Expired(Runner))
            {
                DeterminarGanadorPorTiempo();
            }
        }
    }

    public void RegisterKill(PlayerRef shooter)
    {
        if (!Object.HasStateAuthority || partidaFinalizada) return;

        int nuevosPuntos = 1;
        if (PuntajesJugadores.ContainsKey(shooter))
        {
            nuevosPuntos = PuntajesJugadores.Get(shooter) + 1;
            PuntajesJugadores.Set(shooter, nuevosPuntos);
        }
        else
        {
            PuntajesJugadores.Set(shooter, nuevosPuntos);
        }

        if (nuevosPuntos >= 5)
        {
            FinalizarPartida($"JUGADOR {shooter.PlayerId}");
        }
    }

    public void FinalizarPartida(string nombreGanador)
    {
        if (Object.HasStateAuthority)
        {
            partidaFinalizada = true;
            RPC_MostrarVictoria(nombreGanador);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_MostrarVictoria(string ganador)
    {
        StartCoroutine(CicloSiguienteRonda(ganador));
    }

    IEnumerator CicloSiguienteRonda(string ganador)
    {
        if (winnerText != null)
        {
            winnerText.text = (ganador == "EMPATE") ? "¡EMPATE!" : $"¡GANADOR: {ganador}!";
            winnerText.gameObject.SetActive(true);
        }

        for (int i = 10; i > 0; i--)
        {
            if (timerText != null)
                timerText.text = $"Siguiente partida en: {i}s";
            yield return new WaitForSeconds(1f);
        }

        if (winnerText != null) winnerText.gameObject.SetActive(false);

        if (Runner.IsServer)
        {
            PuntajesJugadores.Clear();
            partidaFinalizada = false;
            cronometroPartida = TickTimer.CreateFromSeconds(Runner, tiempoDePartida);
        }
    }

    private void DeterminarGanadorPorTiempo()
    {
        PlayerRef mejorJugador = default;
        int maxKills = -1;
        bool empate = false;

        foreach (var kvp in PuntajesJugadores)
        {
            if (kvp.Value > maxKills)
            {
                maxKills = kvp.Value;
                mejorJugador = kvp.Key;
                empate = false;
            }
            else if (kvp.Value == maxKills)
            {
                empate = true;
            }
        }

        if (maxKills <= 0 || empate)
        {
            FinalizarPartida("EMPATE");
        }
        else
        {
            FinalizarPartida($"JUGADOR {mejorJugador.PlayerId}");
        }
    }
}