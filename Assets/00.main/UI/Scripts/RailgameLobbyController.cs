using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Railgame.Campaign;
using Railgame.Map;

namespace Railgame.UI
{
    public sealed class RailgameLobbyController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private RailgameSettingsPanel settingsPanel;
        [SerializeField] private RailgameCampaignSession campaignSession;
        [SerializeField] private string springSceneName = "Map_Procedural_Spring";

        private void Awake()
        {
            RailgameSettingsPanel.ApplySavedSettings();
            if (campaignSession == null)
                Debug.LogError("RAILGAME_CAMPAIGN_SESSION_MISSING lobby", this);
            else
                campaignSession.ResetToLobby();
            startButton?.onClick.AddListener(StartGame);
            settingsButton?.onClick.AddListener(OpenSettings);
            quitButton?.onClick.AddListener(Quit);
            if (settingsPanel != null) settingsPanel.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            startButton?.onClick.RemoveListener(StartGame);
            settingsButton?.onClick.RemoveListener(OpenSettings);
            quitButton?.onClick.RemoveListener(Quit);
        }

        private void StartGame()
        {
            if (campaignSession == null)
            {
                Debug.LogError("RAILGAME_CAMPAIGN_SESSION_MISSING lobby cannot start", this);
                return;
            }

            campaignSession.StartNewRun();
            ProceduralMapGenerator.SelectVariant(campaignSession.SpringVariantIndex);
            SceneManager.LoadScene(springSceneName);
        }
        private void OpenSettings() => settingsPanel?.Open();

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
