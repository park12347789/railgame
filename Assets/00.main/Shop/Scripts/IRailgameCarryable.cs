using UnityEngine;

namespace Railgame.Shop
{
    public interface IRailgameCarryable
    {
        GameObject CarryObject { get; }
        bool IsHeld { get; }
        bool CanBePickedUp { get; }
        void AttachToCarrier(RailgameCarryHolder holder, Transform anchor);
    }
}
