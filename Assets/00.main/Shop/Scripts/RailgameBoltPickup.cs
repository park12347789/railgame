using Railgame.Hansol.ShoulderView;
using UnityEngine;

namespace Railgame.Shop
{
    public sealed class RailgameBoltPickup : MonoBehaviour, IShoulderInteractable, IRailgameCarryable
    {
        [SerializeField, Min(1)] private int amount = 1;

        private RailgameCarryHolder carrier;

        public int Amount => amount;
        public bool IsBanked { get; private set; }
        public GameObject CarryObject => gameObject;
        public bool IsHeld => carrier != null;
        public bool CanBePickedUp => !IsHeld && !IsBanked;
        public string InteractionPrompt => $"PICK UP BOLT (+{amount})";
        public bool CanInteract => CanBePickedUp;

        public void Initialize(int boltAmount)
        {
            if (boltAmount <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(boltAmount));
            amount = boltAmount;
        }

        public void Interact(ShoulderInteractor interactor)
        {
            if (interactor == null)
            {
                Debug.LogError("RAILGAME_BOLT_INTERACTOR_MISSING", this);
                return;
            }

            RailgameCarryHolder holder = interactor.GetComponent<RailgameCarryHolder>();
            if (holder == null)
            {
                Debug.LogError("RAILGAME_BOLT_CARRY_HOLDER_MISSING", interactor);
                return;
            }
            holder.TryHold(this);
        }

        public void AttachToCarrier(RailgameCarryHolder holder, Transform anchor)
        {
            carrier = holder;
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        internal void Deposit(ShoulderShopEconomy economy)
        {
            if (economy == null)
                throw new MissingReferenceException("Bolt deposit requires ShoulderShopEconomy.");
            economy.AddBolts(amount);
            carrier = null;
            IsBanked = true;
            gameObject.SetActive(false);
        }
    }
}
