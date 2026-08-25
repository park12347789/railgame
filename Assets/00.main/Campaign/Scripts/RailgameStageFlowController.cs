using System;
using Railgame.Map;
using Railgame.Shop;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Railgame.Campaign
{
    public sealed class RailgameStageFlowController : MonoBehaviour
    {
        [SerializeField] private RailgameCampaignSession campaignSession;
        [SerializeField] private RailgameCampaignSeason season;
        [SerializeField] private GameObject stationShopRoot;
        [SerializeField] private RailgameShopCheckout checkout;
        [SerializeField] private string summerSceneName = "Map_Procedural_Summer";

        public RailgameCampaignSeason Season => season;

        public void Initialize(RailgameCampaignSession session, RailgameCampaignSeason stageSeason,
            GameObject shopRoot = null, RailgameShopCheckout shopCheckout = null)
        {
            campaignSession = session;
            season = stageSeason;
            stationShopRoot = shopRoot;
            checkout = shopCheckout;
        }

        private void Awake()
        {
            if (campaignSession == null)
                throw new MissingReferenceException("RailgameStageFlowController requires campaignSession.");
            if (campaignSession.CurrentSeason != season)
                throw new InvalidOperationException($"Stage season mismatch. scene={season} session={campaignSession.CurrentSeason}");

            campaignSession.MarkStageLoaded();
            if (stationShopRoot != null)
                stationShopRoot.SetActive(false);
            if (checkout != null)
                checkout.CheckedOut += DepartToSummer;
        }

        private void OnDestroy()
        {
            if (checkout != null)
                checkout.CheckedOut -= DepartToSummer;
        }

        public void CompleteAtStation()
        {
            campaignSession.CompleteStage();
            if (season == RailgameCampaignSeason.Spring)
            {
                if (stationShopRoot == null || checkout == null)
                    throw new MissingReferenceException("Spring stage requires physical station shop and checkout.");
                stationShopRoot.SetActive(true);
            }
        }

        public void FailAtRailEnd()
        {
            campaignSession.FailStage();
        }

        private void DepartToSummer()
        {
            if (season != RailgameCampaignSeason.Spring)
                throw new InvalidOperationException("Only Spring station shop can depart to Summer.");
            campaignSession.ContinueFromShop();
            ProceduralMapGenerator.SelectVariant(campaignSession.SummerVariantIndex);
            SceneManager.LoadScene(summerSceneName);
        }
    }
}
