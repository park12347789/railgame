using Railgame.Hansol.ShoulderView;
using UnityEngine;

namespace Railgame.Shop
{
    public sealed class RailgameBoltDeposit : MonoBehaviour, IShoulderInteractable
    {
        [SerializeField] private ShoulderShopEconomy economy;

        public string InteractionPrompt => "DEPOSIT BOLT";
        public bool CanInteract => economy != null;

        public void Initialize(ShoulderShopEconomy shopEconomy)
        {
            economy = shopEconomy;
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
            TryDeposit(holder);
        }

        public bool TryDeposit(RailgameCarryHolder holder)
        {
            if (economy == null)
            {
                Debug.LogError("RAILGAME_BOLT_ECONOMY_MISSING", this);
                return false;
            }
            if (holder == null)
            {
                Debug.LogError("RAILGAME_BOLT_CARRY_HOLDER_MISSING", this);
                return false;
            }
            if (holder.HeldItem is not RailgameBoltPickup bolt)
            {
                Debug.LogError("RAILGAME_BOLT_NOT_HELD", this);
                return false;
            }

            holder.ReleaseHeld();
            bolt.Deposit(economy);
            return true;
        }
    }
}
