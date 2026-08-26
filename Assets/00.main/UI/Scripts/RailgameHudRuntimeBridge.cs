using Railgame.Campaign;
using Railgame.Shop;
using UnityEngine;

namespace Railgame.UI
{
    /// <summary>
    /// Small read-only adapter that projects campaign and physical-shop state onto the HUD.
    /// It deliberately owns no gameplay decisions and can be removed without affecting the run.
    /// </summary>
    public sealed class RailgameHudRuntimeBridge : MonoBehaviour
    {
        [SerializeField] private RailgameHudPresenter presenter;
        [SerializeField] private RailgameCampaignSession campaignSession;
        [SerializeField] private RailgameCarryHolder carryHolder;
        [SerializeField] private RailgameShopCheckout checkout;
        [SerializeField, Min(0.05f)] private float refreshInterval = 0.1f;
        [SerializeField, Min(0.1f)] private float statusDuration = 2f;

        private RailgameCampaignState lastState = (RailgameCampaignState)(-1);
        private IRailgameCarryable lastHeldItem;
        private int lastPendingTotal = int.MinValue;
        private float nextRefreshAt;
        private float clearStatusAt;

        private void OnEnable()
        {
            ResolveSceneReferences();
            RefreshNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshAt)
                return;
            nextRefreshAt = Time.unscaledTime + refreshInterval;
            ResolveSceneReferences();
            RefreshNow();
            if (clearStatusAt > 0f && Time.unscaledTime >= clearStatusAt)
            {
                presenter?.SetStatus(string.Empty);
                clearStatusAt = 0f;
            }
        }

        public void Initialize(RailgameHudPresenter hud, RailgameCampaignSession session,
            RailgameCarryHolder holder = null, RailgameShopCheckout shopCheckout = null)
        {
            presenter = hud;
            campaignSession = session;
            carryHolder = holder;
            checkout = shopCheckout;
            lastState = (RailgameCampaignState)(-1);
            lastHeldItem = null;
            lastPendingTotal = int.MinValue;
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (presenter == null || campaignSession == null)
                return;

            RailgameCampaignState state = campaignSession.State;
            presenter.SetRoute(RouteName(state), NextRouteName(state));
            presenter.SetObjective(ObjectiveFor(state));

            IRailgameCarryable held = carryHolder != null ? carryHolder.HeldItem : null;
            if (held != lastHeldItem)
            {
                RefreshCargo(held);
                lastHeldItem = held;
            }

            int pending = checkout != null ? checkout.PendingTotal : -1;
            if (state != lastState)
            {
                ShowStateStatus(state);
                lastState = state;
            }
            else if (state == RailgameCampaignState.StationShop && pending != lastPendingTotal && pending >= 0)
            {
                ShowTemporaryStatus(pending == 0 ? "ROUTE READY" : $"PENDING  {pending:00} BOLTS");
            }
            lastPendingTotal = pending;
        }

        private void ResolveSceneReferences()
        {
            if (presenter == null)
                presenter = GetComponent<RailgameHudPresenter>();
            if (campaignSession == null)
                campaignSession = Resources.Load<RailgameCampaignSession>("RailgameCampaignSession");
            if (carryHolder == null)
                carryHolder = FindAnyObjectByType<RailgameCarryHolder>();
            if (checkout == null)
                checkout = FindAnyObjectByType<RailgameShopCheckout>(FindObjectsInactive.Include);
        }

        private void RefreshCargo(IRailgameCarryable held)
        {
            switch (held)
            {
                case null:
                    presenter.SetCargo(null);
                    break;
                case RailgameBoltPickup bolt:
                    presenter.SetCargo($"BOLT +{bolt.Amount}", "RETURN TO TRAIN");
                    break;
                case RailgamePhysicalShopItem item:
                    string name = item.SectionType switch
                    {
                        SectionType.Engine => "DRIVE MODULE",
                        SectionType.WaterTank => "COOLANT MODULE",
                        SectionType.Cargo => "CARGO MODULE",
                        SectionType.Production => "PRODUCTION MODULE",
                        _ => "UPGRADE MODULE"
                    };
                    presenter.SetCargo(name, item.Owned ? "OWNED" : $"PRICE  {item.Price:00} BOLTS");
                    break;
                default:
                    presenter.SetCargo(held.CarryObject != null ? held.CarryObject.name : "ITEM");
                    break;
            }
        }

        private string ObjectiveFor(RailgameCampaignState state) => state switch
        {
            RailgameCampaignState.SpringPlaying => "REACH PINE STATION",
            RailgameCampaignState.StationShop when carryHolder != null && !carryHolder.IsEmpty =>
                "MOUNT OR RETURN CARRIED ITEM",
            RailgameCampaignState.StationShop when checkout != null && checkout.PendingTotal > 0 =>
                $"DEPART  ·  COST {checkout.PendingTotal:00} BOLTS",
            RailgameCampaignState.StationShop => "MOUNT UPGRADES OR DEPART",
            RailgameCampaignState.SummerPlaying => "REACH FINAL STATION",
            RailgameCampaignState.SpringFailed or RailgameCampaignState.SummerFailed => "RETRY FROM LAST STATION",
            RailgameCampaignState.Results => "JOURNEY COMPLETE",
            _ => "PREPARE THE TRAIN"
        };

        private static string RouteName(RailgameCampaignState state) => state switch
        {
            RailgameCampaignState.StationShop => "PINE STATION · WORKSHOP",
            RailgameCampaignState.LoadingSummer or RailgameCampaignState.SummerPlaying or
                RailgameCampaignState.SummerFailed or RailgameCampaignState.Results => "SUMMER LINE",
            _ => "SPRING LINE"
        };

        private static string NextRouteName(RailgameCampaignState state) => state switch
        {
            RailgameCampaignState.StationShop => "RIVER BEND",
            RailgameCampaignState.LoadingSummer or RailgameCampaignState.SummerPlaying or
                RailgameCampaignState.SummerFailed => "FINAL STATION",
            RailgameCampaignState.Results => "COMPLETE",
            _ => "PINE STATION"
        };

        private void ShowStateStatus(RailgameCampaignState state)
        {
            switch (state)
            {
                case RailgameCampaignState.StationShop:
                    ShowTemporaryStatus("STATION WORKSHOP OPEN");
                    break;
                case RailgameCampaignState.SummerPlaying:
                    ShowTemporaryStatus("ROUTE READY");
                    break;
                case RailgameCampaignState.Results:
                    ShowTemporaryStatus("JOURNEY COMPLETE");
                    break;
            }
        }

        private void ShowTemporaryStatus(string message)
        {
            presenter.SetStatus(message);
            clearStatusAt = Time.unscaledTime + statusDuration;
        }
    }
}
