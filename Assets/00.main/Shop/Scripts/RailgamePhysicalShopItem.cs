using Railgame.Hansol.ShoulderView;
using UnityEngine;

namespace Railgame.Shop
{
    public sealed class RailgamePhysicalShopItem : MonoBehaviour, IShoulderInteractable, IRailgameCarryable
    {
        [SerializeField] private SectionType sectionType;
        [SerializeField, Min(0)] private int price;
        [SerializeField] private bool owned;

        private RailgameCarryHolder carrier;
        private int mountedPriceSnapshot;

        public SectionType SectionType => sectionType;
        public int Price => price;
        public bool Owned => owned;
        public GameObject CarryObject => gameObject;
        public bool IsHeld => carrier != null;
        public bool CanBePickedUp => !IsHeld && MountedSocket == null;
        public RailgameShopSocket MountedSocket { get; private set; }
        public int PendingPrice => MountedSocket != null && !owned ? mountedPriceSnapshot : 0;
        public string InteractionPrompt => owned ? "PICK UP OWNED ITEM" : $"PICK UP ({price} BOLTS)";
        public bool CanInteract => !IsHeld && MountedSocket == null;

        public void Initialize(SectionType type, int itemPrice, bool isOwned = false)
        {
            sectionType = type;
            price = itemPrice;
            owned = isOwned;
        }

        public void Interact(ShoulderInteractor interactor)
        {
            if (interactor == null)
            {
                Debug.LogError("RAILGAME_SHOP_INTERACTOR_MISSING", this);
                return;
            }

            RailgameCarryHolder holder = interactor.GetComponent<RailgameCarryHolder>();
            if (holder == null)
            {
                Debug.LogError("RAILGAME_SHOP_CARRY_HOLDER_MISSING", interactor);
                return;
            }
            holder.TryHold(this);
        }

        public void AttachToCarrier(RailgameCarryHolder holder, Transform anchor)
        {
            carrier = holder;
            MountedSocket = null;
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        internal void AttachToSocket(RailgameShopSocket socket, Transform anchor)
        {
            carrier = null;
            MountedSocket = socket;
            if (!owned)
                mountedPriceSnapshot = price;
            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        internal void MarkOwned()
        {
            owned = true;
        }
    }
}
