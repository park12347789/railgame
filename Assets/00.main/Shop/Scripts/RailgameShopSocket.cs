using Railgame.Hansol.ShoulderView;
using UnityEngine;

namespace Railgame.Shop
{
    public sealed class RailgameShopSocket : MonoBehaviour, IShoulderInteractable
    {
        [SerializeField] private SectionType compatibleType;
        [SerializeField] private Transform mountAnchor;
        [SerializeField] private RailgamePhysicalShopItem mountedItem;

        public RailgamePhysicalShopItem MountedItem => mountedItem;
        public string InteractionPrompt => mountedItem == null ? "MOUNT ITEM" : "DETACH ITEM";
        public bool CanInteract => true;

        private void Awake()
        {
            if (mountedItem == null)
                return;
            if (!IsCompatible(mountedItem) || mountAnchor == null)
            {
                Debug.LogError($"RAILGAME_SHOP_INITIAL_SOCKET_INVALID socket={name}", this);
                return;
            }
            mountedItem.AttachToSocket(this, mountAnchor);
        }

        public void Initialize(SectionType type, Transform anchor,
            RailgamePhysicalShopItem initialItem = null)
        {
            compatibleType = type;
            mountAnchor = anchor;
            mountedItem = initialItem;
            if (mountedItem != null && IsCompatible(mountedItem) && mountAnchor != null)
                mountedItem.AttachToSocket(this, mountAnchor);
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
            TryMountOrDetach(holder);
        }

        public bool TryMountOrDetach(RailgameCarryHolder holder)
        {
            if (mountAnchor == null)
            {
                Debug.LogError($"RAILGAME_SHOP_SOCKET_ANCHOR_MISSING socket={name}", this);
                return false;
            }
            if (holder == null)
            {
                Debug.LogError($"RAILGAME_SHOP_CARRY_HOLDER_MISSING socket={name}", this);
                return false;
            }

            if (mountedItem != null)
            {
                if (!holder.IsEmpty)
                {
                    Debug.LogError($"RAILGAME_SHOP_DETACH_HANDS_OCCUPIED socket={name}", this);
                    return false;
                }

                RailgamePhysicalShopItem item = mountedItem;
                mountedItem = null;
                if (!holder.TryHoldDetached(item))
                {
                    mountedItem = item;
                    item.AttachToSocket(this, mountAnchor);
                    Debug.LogError($"RAILGAME_SHOP_DETACH_FAILED socket={name}", this);
                    return false;
                }
                return true;
            }

            RailgamePhysicalShopItem held = holder.HeldItem as RailgamePhysicalShopItem;
            if (held == null)
            {
                string reason = holder.HeldItem == null ? "EMPTY_HANDS" : "NON_SHOP_ITEM";
                Debug.LogError($"RAILGAME_SHOP_MOUNT_REJECTED socket={name} reason={reason}", this);
                return false;
            }
            if (!IsCompatible(held))
            {
                Debug.LogError($"RAILGAME_SHOP_INCOMPATIBLE socket={name} item={held.name} expected={compatibleType} actual={held.SectionType}", this);
                return false;
            }

            mountedItem = (RailgamePhysicalShopItem)holder.ReleaseHeld();
            mountedItem.AttachToSocket(this, mountAnchor);
            return true;
        }

        private bool IsCompatible(RailgamePhysicalShopItem item)
        {
            return item != null && item.SectionType == compatibleType;
        }
    }
}
