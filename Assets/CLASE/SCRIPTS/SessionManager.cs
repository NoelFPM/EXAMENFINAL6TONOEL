using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class SessionManager : MonoBehaviour
{
    [SerializeField] private GameObject ventanaServidores;
    [SerializeField] private GameObject ventanaCrear;
    [SerializeField] private GameObject ventanaUnir;
    [SerializeField] private TMP_InputField inputNombrePartida;
    [SerializeField] private TMP_InputField inputMaxJugadores;
    [SerializeField] private Toggle toggleEsPrivada;
    [SerializeField] private GameObject mensajeDatosIncompletos;
    [SerializeField] private TMP_InputField inputBuscarNombre;
    [SerializeField] private GameObject mensajeNoEncontrado;
    [SerializeField] private Transform contenedorScroll;
    [SerializeField] private GameObject prefabBotonPartida;
    [SerializeField] private GameObject mensajeNoServidores;

    private List<SessionInfo> listaDeSalasActuales;

    private void Start()
    {
        RegresarAVentanaServidores();
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.InitializeSessionCallback(OnSessionListUpdated);
        }
    }

    public void RegresarAVentanaServidores()
    {
        ventanaServidores.SetActive(true);
        ventanaCrear.SetActive(false);
        ventanaUnir.SetActive(false);
    }

    public void IrAVentanaCrear()
    {
        ventanaCrear.SetActive(true);
        ventanaUnir.SetActive(false);
        if (mensajeDatosIncompletos != null) mensajeDatosIncompletos.SetActive(false);
    }

    public void IrAVentanaUnir()
    {
        ventanaCrear.SetActive(false);
        ventanaUnir.SetActive(true);
        if (mensajeNoEncontrado != null) mensajeNoEncontrado.SetActive(false);
    }

    public void OnSessionListUpdated(List<SessionInfo> sessionList)
    {
        listaDeSalasActuales = sessionList;
        UpdateSessionListOnCanvas();
    }

    public void UpdateSessionListOnCanvas()
    {

        foreach (Transform hijo in contenedorScroll) Destroy(hijo.gameObject);

        if (listaDeSalasActuales == null || listaDeSalasActuales.Count == 0)
        {
            if (mensajeNoServidores) mensajeNoServidores.SetActive(true);
            return;
        }
        if (mensajeNoServidores) mensajeNoServidores.SetActive(false);


        foreach (SessionInfo sala in listaDeSalasActuales)
        {
            if (!sala.IsVisible) continue;

            GameObject nuevoBoton = Instantiate(prefabBotonPartida, contenedorScroll);
            SessionEntry entry = nuevoBoton.GetComponent<SessionEntry>();

            if (entry != null)
            {
                string modo = sala.Properties.ContainsKey("GameMode") ? sala.Properties["GameMode"].ToString() : "N/A";
                string mapa = sala.Properties.ContainsKey("Map") ? sala.Properties["Map"].ToString() : "N/A";

                entry.SetSessionInfo(sala.Name, modo, mapa, sala.PlayerCount, sala.MaxPlayers);
                entry.SetupJoinButton(sala.Name);
            }
        }
    }


    public async void RegistrarYCrearPartida()
    {
        
        if (string.IsNullOrEmpty(inputNombrePartida.text))
        {
            if (mensajeDatosIncompletos != null)
            {
                mensajeDatosIncompletos.GetComponentInChildren<TMP_Text>().text = "Falta el parámetro: Nombre del Server";
                mensajeDatosIncompletos.SetActive(true);
            }
            return;
        }

        if (string.IsNullOrEmpty(inputMaxJugadores.text))
        {
            if (mensajeDatosIncompletos != null)
            {
                mensajeDatosIncompletos.GetComponentInChildren<TMP_Text>().text = "Falta el parámetro: Cantidad de Jugadores";
                mensajeDatosIncompletos.SetActive(true);
            }
            return;
        }

        int maxPlayers = int.Parse(inputMaxJugadores.text);

        if (maxPlayers < 2 || maxPlayers > 10)
        {
            if (mensajeDatosIncompletos != null)
            {
                mensajeDatosIncompletos.GetComponentInChildren<TMP_Text>().text = "El parámetro Jugadores debe ser de 2 a 10";
                mensajeDatosIncompletos.SetActive(true);
            }
            return;
        }

        if (mensajeDatosIncompletos != null)
        {
            mensajeDatosIncompletos.GetComponentInChildren<TMP_Text>().text = "Conectando al servidor...";
            mensajeDatosIncompletos.SetActive(true);
        }

        if (PhotonManager.Instance != null)
        {
            await PhotonManager.Instance.StartCustomGame(inputNombrePartida.text, maxPlayers);
        }
    }

    public void BuscarYUnirsePorTexto()
    {
        if (string.IsNullOrEmpty(inputBuscarNombre.text))
        {
            if (mensajeNoEncontrado != null) mensajeNoEncontrado.SetActive(true);
            return;
        }

        if (mensajeNoEncontrado != null) mensajeNoEncontrado.SetActive(false);
        UnirseAPartidaDirecto(inputBuscarNombre.text);
    }

    private void UnirseAPartidaDirecto(string roomName)
    {
        if (PhotonManager.Instance != null)
        {
            PhotonManager.Instance.JoinGameReal(roomName);
        }
    }
}