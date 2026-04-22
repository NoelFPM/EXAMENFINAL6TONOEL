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

    [Networked] public int PuntosP1 { get; set; }
    [Networked] public int PuntosP2 { get; set; }
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
        if (killsText != null)
            killsText.text = $"P1: {PuntosP1} | P2: {PuntosP2}";

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
        if (Object.HasStateAuthority && !partidaFinalizada)
        {
            if (shooter.PlayerId == 1) PuntosP1++;
            else PuntosP2++;

            if (PuntosP1 >= 5) FinalizarPartida("JUGADOR 1");
            else if (PuntosP2 >= 5) FinalizarPartida("JUGADOR 2");
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
            PuntosP1 = 0;
            PuntosP2 = 0;
            partidaFinalizada = false;
            cronometroPartida = TickTimer.CreateFromSeconds(Runner, tiempoDePartida);
        }
    }

    private void DeterminarGanadorPorTiempo()
    {
        if (PuntosP1 > PuntosP2) FinalizarPartida("JUGADOR 1");
        else if (PuntosP2 > PuntosP1) FinalizarPartida("JUGADOR 2");
        else FinalizarPartida("EMPATE");
    }
}