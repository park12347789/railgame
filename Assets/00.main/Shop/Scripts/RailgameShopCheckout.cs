using System;
using System.Collections.Generic;
using Railgame.Hansol.ShoulderView;
using UnityEngine;
using UnityEngine.Events;

namespace Railgame.Shop
{
    public sealed class RailgameShopCheckout : MonoBehaviour, IShoulderInteractable
    {
        [SerializeField] private ShoulderShopEconomy economy;
        [SerializeField] private RailgameShopSocket[] sockets;
        [SerializeField] private RailgameCarryHolder[] carryHolders;
        [SerializeField] private UnityEvent checkoutSucceeded;

        public event Action CheckedOut;
        public bool CheckoutCompleted { get; private set; }
        public string InteractionPrompt => CanDepart
            ? $"DEPART  COST {PendingTotal} / BOLTS {economy.Bolts}"
            : $"BLOCKED  COST {PendingTotal} / BOLTS {(economy != null ? economy.Bolts : 0)}";
        public bool CanInteract => !CheckoutCompleted;
        public int PendingTotal => TryCollectPending(false, out _, out int total) ? total : -1;
        public bool CanDepart => !CheckoutCompleted && TryCollectPending(false, out _, out int total) &&
                                 !HasHeldUnowned(false) && economy != null && economy.Bolts >= total;

        public void Initialize(ShoulderShopEconomy shopEconomy, RailgameShopSocket[] shopSockets,
            RailgameCarryHolder[] holders)
        {
            economy = shopEconomy;
            sockets = shopSockets;
            carryHolders = holders;
        }

        public void Interact(ShoulderInteractor interactor)
        {
            TryCheckout();
        }

        public bool TryCheckout()
        {
            if (CheckoutCompleted)
            {
                Debug.LogError("RAILGAME_SHOP_CHECKOUT_ALREADY_COMPLETED", this);
                return false;
            }
            if (economy == null)
            {
                Debug.LogError("RAILGAME_SHOP_ECONOMY_MISSING", this);
                return false;
            }
            if (HasHeldUnowned(true))
                return false;
            if (!TryCollectPending(true, out List<RailgamePhysicalShopItem> pending, out int total))
                return false;
            if (economy.Bolts < total)
            {
                Debug.LogError($"RAILGAME_SHOP_BOLTS_INSUFFICIENT have={economy.Bolts} need={total}", this);
                return false;
            }
            if (!economy.TrySpend(total))
            {
                Debug.LogError($"RAILGAME_SHOP_BOLT_SPEND_FAILED cost={total}", this);
                return false;
            }

            foreach (RailgamePhysicalShopItem item in pending)
                item.MarkOwned();
            CheckoutCompleted = true;
            checkoutSucceeded?.Invoke();
            CheckedOut?.Invoke();
            return true;
        }

        private bool HasHeldUnowned(bool logErrors)
        {
            if (carryHolders == null)
            {
                if (logErrors) Debug.LogError("RAILGAME_SHOP_CARRY_HOLDERS_MISSING", this);
                return true;
            }

            foreach (RailgameCarryHolder holder in carryHolders)
            {
                if (holder == null)
                {
                    if (logErrors) Debug.LogError("RAILGAME_SHOP_CARRY_HOLDER_REFERENCE_MISSING", this);
                    return true;
                }
                if (holder.HeldItem is RailgamePhysicalShopItem item && !item.Owned)
                {
                    if (logErrors) Debug.LogError($"RAILGAME_SHOP_UNPAID_ITEM_HELD item={item.name}", holder);
                    return true;
                }
                if (holder.HeldItem != null)
                {
                    if (logErrors) Debug.LogError($"RAILGAME_SHOP_CARRIED_OBJECT_HELD item={holder.HeldItem.CarryObject.name}", holder);
                    return true;
                }
            }
            return false;
        }

        private bool TryCollectPending(bool logErrors, out List<RailgamePhysicalShopItem> pending, out int total)
        {
            pending = new List<RailgamePhysicalShopItem>();
            total = 0;
            if (sockets == null)
            {
                if (logErrors) Debug.LogError("RAILGAME_SHOP_SOCKETS_MISSING", this);
                return false;
            }

            HashSet<RailgamePhysicalShopItem> unique = new();
            foreach (RailgameShopSocket socket in sockets)
            {
                if (socket == null)
                {
                    if (logErrors) Debug.LogError("RAILGAME_SHOP_SOCKET_REFERENCE_MISSING", this);
                    return false;
                }

                RailgamePhysicalShopItem item = socket.MountedItem;
                if (item == null || item.Owned)
                    continue;
                if (!unique.Add(item))
                {
                    if (logErrors) Debug.LogError($"RAILGAME_SHOP_DUPLICATE_ITEM item={item.name}", this);
                    return false;
                }
                if (item.PendingPrice < 0)
                {
                    if (logErrors) Debug.LogError($"RAILGAME_SHOP_INVALID_PRICE item={item.name} price={item.PendingPrice}", item);
                    return false;
                }

                try
                {
                    total = checked(total + item.PendingPrice);
                }
                catch (OverflowException)
                {
                    if (logErrors) Debug.LogError("RAILGAME_SHOP_PRICE_OVERFLOW", this);
                    return false;
                }
                pending.Add(item);
            }
            return true;
        }
    }
}
