using UnityEngine;

namespace Railgame.Shop
{
    public sealed class RailgameCarryHolder : MonoBehaviour
    {
        [SerializeField] private Transform carryAnchor;

        public IRailgameCarryable HeldItem { get; private set; }
        public bool IsEmpty => HeldItem == null;

        public void Initialize(Transform anchor)
        {
            carryAnchor = anchor;
        }

        public bool TryHold(IRailgameCarryable item)
        {
            if (carryAnchor == null)
            {
                Debug.LogError("RAILGAME_SHOP_CARRY_ANCHOR_MISSING", this);
                return false;
            }
            if (item == null)
            {
                Debug.LogError("RAILGAME_SHOP_CARRY_ITEM_MISSING", this);
                return false;
            }
            if (HeldItem != null)
            {
                Debug.LogError($"RAILGAME_SHOP_HANDS_OCCUPIED item={HeldItem.CarryObject.name}", this);
                return false;
            }
            if (!item.CanBePickedUp)
            {
                Debug.LogError($"RAILGAME_SHOP_ITEM_UNAVAILABLE item={item.CarryObject.name}", item.CarryObject);
                return false;
            }

            HeldItem = item;
            item.AttachToCarrier(this, carryAnchor);
            return true;
        }

        internal IRailgameCarryable ReleaseHeld()
        {
            IRailgameCarryable item = HeldItem;
            HeldItem = null;
            return item;
        }

        internal bool TryHoldDetached(RailgamePhysicalShopItem item)
        {
            if (HeldItem != null || item == null || carryAnchor == null)
                return false;
            HeldItem = item;
            item.AttachToCarrier(this, carryAnchor);
            return true;
        }
    }
}
