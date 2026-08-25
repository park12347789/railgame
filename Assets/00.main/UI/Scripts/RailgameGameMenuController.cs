using Railgame.Campaign;
using Railgame.Map;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Railgame.UI
{
    public sealed class RailgameGameMenuController : MonoBehaviour
    {
        private enum MenuState
        {
            Playing,
            Paused,
            Settings
        }

        [SerializeField] private GameObject pausePanel;
        [SerializeField] private RailgameSettingsPanel settingsPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private RailgameCampaignSession campaignSession;
        [SerializeField] private string lobbySceneName = "Railgame_Lobby";

        private MenuState state;

        private void Awake()
        {
            RailgameSettingsPanel.ApplySavedSettings();
            resumeButton?.onClick.AddListener(Resume);
            settingsButton?.onClick.AddListener(OpenSettings);
            restartButton?.onClick.AddListener(Restart);
            lobbyButton?.onClick.AddListener(ReturnToLobby);
            quitButton?.onClick.AddListener(Quit);
            if (settingsPanel != null) settingsPanel.Closed += ReturnFromSettings;
            SetState(MenuState.Playing);
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            switch (state)
            {
                case MenuState.Playing: SetState(MenuState.Paused); break;
                case MenuState.Paused: Resume(); break;
                case MenuState.Settings: settingsPanel?.CancelAndClose(); break;
            }
        }

        private void OnDestroy()
        {
            if (settingsPanel != null) settingsPanel.Closed -= ReturnFromSettings;
            Time.timeScale = 1f;
        }

        public void Resume() => SetState(MenuState.Playing);

        private void OpenSettings()
        {
            state = MenuState.Settings;
            if (pausePanel != null) pausePanel.SetActive(false);
            settingsPanel?.Open();
        }

        private void ReturnFromSettings() => SetState(MenuState.Paused);

        private void SetState(MenuState next)
        {
            state = next;
            if (pausePanel != null) pausePanel.SetActive(next == MenuState.Paused);
            if (settingsPanel != null && next != MenuState.Settings) settingsPanel.gameObject.SetActive(false);

            bool frozen = next != MenuState.Playing;
            Time.timeScale = frozen ? 0f : 1f;
            Cursor.visible = frozen;
            Cursor.lockState = frozen ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void Restart()
        {
            if (campaignSession == null)
            {
                Debug.LogError("RAILGAME_CAMPAIGN_SESSION_MISSING restart rejected", this);
                return;
            }

            if (campaignSession.State is RailgameCampaignState.SpringFailed or RailgameCampaignState.SummerFailed)
                campaignSession.RetryStage();
            else if (campaignSession.State is RailgameCampaignState.SpringPlaying or RailgameCampaignState.SummerPlaying)
            {
                campaignSession.FailStage();
                campaignSession.RetryStage();
            }
            else
            {
                Debug.LogError($"RAILGAME_RESTART_INVALID state={campaignSession.State}", this);
                return;
            }

            Time.timeScale = 1f;
            ProceduralMapGenerator.SelectVariant(campaignSession.CurrentVariantIndex);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToLobby()
        {
            if (campaignSession == null)
            {
                Debug.LogError("RAILGAME_CAMPAIGN_SESSION_MISSING lobby return rejected", this);
                return;
            }

            campaignSession.ResetToLobby();
            Time.timeScale = 1f;
            SceneManager.LoadScene(lobbySceneName);
        }

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
