using Railgame.Hansol.ShoulderView;
using UnityEngine;
using UnityEngine.UI;

namespace Railgame.UI
{
    /// <summary>
    /// Lightweight view model for the always-on gameplay HUD. All references are optional so
    /// individual HUD modules can be introduced without coupling gameplay systems to the canvas.
    /// </summary>
    public sealed class RailgameHudPresenter : MonoBehaviour
    {
        [SerializeField] private Text routeText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text boltText;
        [SerializeField] private Text cargoText;
        [SerializeField] private Text interactionText;
        [SerializeField] private GameObject interactionRoot;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject statusRoot;
        [SerializeField] private ShoulderShopEconomy economy;

        public int DisplayedBolts { get; private set; }

        private void OnEnable()
        {
            if (economy == null)
                economy = FindAnyObjectByType<ShoulderShopEconomy>();
            if (economy != null)
                economy.BoltsChanged += SetBolts;

            SetBolts(economy != null ? economy.Bolts : DisplayedBolts);
            SetStatus(string.Empty);
        }

        private void OnDisable()
        {
            if (economy != null)
                economy.BoltsChanged -= SetBolts;
        }

        private void LateUpdate()
        {
            if (interactionRoot == null || interactionText == null)
                return;
            bool visible = interactionText.gameObject.activeSelf &&
                           !string.IsNullOrWhiteSpace(interactionText.text);
            if (interactionRoot.activeSelf != visible)
                interactionRoot.SetActive(visible);
        }

        public void Initialize(Text route, Text objective, Text bolts, Text cargo, Text interaction,
            Text status, GameObject statusContainer, ShoulderShopEconomy shopEconomy = null)
        {
            if (isActiveAndEnabled && economy != null)
                economy.BoltsChanged -= SetBolts;

            routeText = route;
            objectiveText = objective;
            boltText = bolts;
            cargoText = cargo;
            interactionText = interaction;
            statusText = status;
            statusRoot = statusContainer;
            economy = shopEconomy;

            if (isActiveAndEnabled && economy != null)
                economy.BoltsChanged += SetBolts;
            SetBolts(economy != null ? economy.Bolts : DisplayedBolts);
            SetStatus(string.Empty);
        }

        public void SetRoute(string station, string nextLeg)
        {
            if (routeText != null)
                routeText.text = $"{Safe(station, "FIELD STATION")}\nNEXT  {Safe(nextLeg, "ROUTE CHECK")}";
        }

        public void SetObjective(string objective)
        {
            if (objectiveText != null)
                objectiveText.text = $"OBJECTIVE\n{Safe(objective, "PREPARE THE TRAIN")}";
        }

        public void SetBolts(int value)
        {
            DisplayedBolts = Mathf.Max(0, value);
            if (boltText != null)
                boltText.text = $"BOLTS  {DisplayedBolts:00}";
        }

        public void SetCargo(string item, string hint = null)
        {
            if (cargoText == null)
                return;
            cargoText.text = string.IsNullOrWhiteSpace(item)
                ? "HANDS  EMPTY"
                : $"CARRYING  {item.ToUpperInvariant()}";
            if (!string.IsNullOrWhiteSpace(hint))
                cargoText.text += $"\n{hint.ToUpperInvariant()}";
        }

        public void SetInteraction(string prompt)
        {
            if (interactionText == null)
                return;
            bool visible = !string.IsNullOrWhiteSpace(prompt);
            interactionText.gameObject.SetActive(visible);
            interactionText.text = visible ? prompt.ToUpperInvariant() : string.Empty;
            if (interactionRoot != null)
                interactionRoot.SetActive(visible);
        }

        public void SetStatus(string message)
        {
            bool visible = !string.IsNullOrWhiteSpace(message);
            if (statusRoot != null)
                statusRoot.SetActive(visible);
            if (statusText != null)
                statusText.text = visible ? message.ToUpperInvariant() : string.Empty;
        }

        private static string Safe(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.ToUpperInvariant();
    }
}
