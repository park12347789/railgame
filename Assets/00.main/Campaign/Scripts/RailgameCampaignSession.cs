using System;
using UnityEngine;

namespace Railgame.Campaign
{
    public enum RailgameCampaignState
    {
        Lobby,
        LoadingSpring,
        SpringPlaying,
        SpringFailed,
        StationShop,
        LoadingSummer,
        SummerPlaying,
        SummerFailed,
        Results
    }

    public enum RailgameCampaignSeason
    {
        Spring,
        Summer
    }

    [CreateAssetMenu(menuName = "Railgame/Campaign Session", fileName = "RailgameCampaignSession")]
    public sealed class RailgameCampaignSession : ScriptableObject
    {
        public const int VariantCount = 5;

        [NonSerialized] private RailgameCampaignState state = RailgameCampaignState.Lobby;
        [NonSerialized] private int springVariantIndex;
        [NonSerialized] private int summerVariantIndex;
        [NonSerialized] private int bankedBolts;

        public RailgameCampaignState State => state;
        public int SpringVariantIndex => springVariantIndex;
        public int SummerVariantIndex => summerVariantIndex;
        public int BankedBolts => bankedBolts;

        public RailgameCampaignSeason CurrentSeason => state switch
        {
            RailgameCampaignState.LoadingSpring or RailgameCampaignState.SpringPlaying or
                RailgameCampaignState.SpringFailed or RailgameCampaignState.StationShop => RailgameCampaignSeason.Spring,
            RailgameCampaignState.LoadingSummer or RailgameCampaignState.SummerPlaying or
                RailgameCampaignState.SummerFailed or RailgameCampaignState.Results => RailgameCampaignSeason.Summer,
            _ => throw new InvalidOperationException("Lobby has no active campaign season.")
        };

        public int CurrentVariantIndex => CurrentSeason == RailgameCampaignSeason.Spring
            ? springVariantIndex
            : summerVariantIndex;

        public void StartNewRun()
        {
            RequireState(nameof(StartNewRun), RailgameCampaignState.Lobby, RailgameCampaignState.Results);
            springVariantIndex = UnityEngine.Random.Range(0, VariantCount);
            summerVariantIndex = UnityEngine.Random.Range(0, VariantCount);
            bankedBolts = 0;
            SetState(RailgameCampaignState.LoadingSpring);
        }

        public void SetBankedBolts(int value)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            bankedBolts = value;
        }

        public void MarkStageLoaded()
        {
            switch (state)
            {
                case RailgameCampaignState.LoadingSpring:
                    SetState(RailgameCampaignState.SpringPlaying);
                    break;
                case RailgameCampaignState.LoadingSummer:
                    SetState(RailgameCampaignState.SummerPlaying);
                    break;
                default:
                    Reject(nameof(MarkStageLoaded));
                    break;
            }
        }

        public void FailStage()
        {
            switch (state)
            {
                case RailgameCampaignState.SpringPlaying:
                    SetState(RailgameCampaignState.SpringFailed);
                    break;
                case RailgameCampaignState.SummerPlaying:
                    SetState(RailgameCampaignState.SummerFailed);
                    break;
                default:
                    Reject(nameof(FailStage));
                    break;
            }
        }

        public void RetryStage()
        {
            switch (state)
            {
                case RailgameCampaignState.SpringFailed:
                    SetState(RailgameCampaignState.LoadingSpring);
                    break;
                case RailgameCampaignState.SummerFailed:
                    SetState(RailgameCampaignState.LoadingSummer);
                    break;
                default:
                    Reject(nameof(RetryStage));
                    break;
            }
        }

        public void CompleteStage()
        {
            switch (state)
            {
                case RailgameCampaignState.SpringPlaying:
                    SetState(RailgameCampaignState.StationShop);
                    break;
                case RailgameCampaignState.SummerPlaying:
                    SetState(RailgameCampaignState.Results);
                    break;
                default:
                    Reject(nameof(CompleteStage));
                    break;
            }
        }

        public void ContinueFromShop()
        {
            RequireState(nameof(ContinueFromShop), RailgameCampaignState.StationShop);
            SetState(RailgameCampaignState.LoadingSummer);
        }

        public void ResetToLobby()
        {
            springVariantIndex = 0;
            summerVariantIndex = 0;
            bankedBolts = 0;
            SetState(RailgameCampaignState.Lobby);
        }

        private void RequireState(string operation, params RailgameCampaignState[] allowedStates)
        {
            foreach (RailgameCampaignState allowedState in allowedStates)
                if (state == allowedState)
                    return;
            Reject(operation);
        }

        private void Reject(string operation)
        {
            string message = $"RAILGAME_CAMPAIGN_INVALID_TRANSITION operation={operation} state={state}";
            Debug.LogError(message, this);
            throw new InvalidOperationException(message);
        }

        private void SetState(RailgameCampaignState next)
        {
            state = next;
            Debug.Log($"RAILGAME_CAMPAIGN_STATE state={state} springVariant={springVariantIndex} summerVariant={summerVariantIndex}", this);
        }
    }
}
