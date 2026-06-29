using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] TMP_Text serverNameLbl;
    [SerializeField] TMP_Text gameModeLbl;
    [SerializeField] TMP_Text mapNameLbl;
    [SerializeField] TMP_Text playerCountLbl;
    [SerializeField] Button joinButton;


    public void SetSessionInfo(string serverName, string gameMode, string mapName, int current, int max)
    {
        serverNameLbl.text = serverName;
        gameModeLbl.text = gameMode;
        mapNameLbl.text = mapName;
        playerCountLbl.text = $"{current}/{max}";
    }


    public void SetupJoinButton(string roomName)
    {
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => PhotonManager.Instance.JoinGameReal(roomName));
    }
}