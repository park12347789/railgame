using Railgame.Hansol.ShoulderView;
using UnityEngine;

namespace Railgame.Campaign
{
    public sealed class RailgameCampaignEconomyBridge : MonoBehaviour
    {
        [SerializeField] private RailgameCampaignSession campaignSession;
        [SerializeField] private ShoulderShopEconomy economy;

        public void Initialize(RailgameCampaignSession session, ShoulderShopEconomy shopEconomy)
        {
            campaignSession = session;
            economy = shopEconomy;
        }

        private void Awake()
        {
            if (campaignSession == null || economy == null)
                throw new MissingReferenceException("Campaign economy bridge requires session and economy.");
            economy.BoltsChanged += SaveBolts;
            economy.Initialize(campaignSession.BankedBolts);
        }

        private void OnDestroy()
        {
            if (economy != null)
                economy.BoltsChanged -= SaveBolts;
        }

        private void SaveBolts(int value)
        {
            campaignSession.SetBankedBolts(value);
        }
    }
}
