using System;

namespace TheTrailWalker.Domain
{
    [Flags]
    public enum MovementPauseReasons
    {
        None = 0, Inventory = 1, Harvest = 2, Camp = 4, Loading = 8,
        Suspended = 16, Exhausted = 32, Unfocused = 64, Seated = 128
    }

    // Walking intent survives temporary pauses, but never a harvest start or camp arrival.
    public sealed class MovementState
    {
        // Starter cap (no boots) and highest boots cap (rank-four boots), raised October 30, 2026.
        public const float StarterSpeedCap = 1.5f, MaximumSpeedCap = 4.5f; // raised 1.5x (user, October 31, 2026)
        public float Distance { get; private set; }
        public float RequestedSpeed { get; private set; }
        public float SpeedCap { get; private set; } = StarterSpeedCap;
        public float InventoryResumeSpeed { get; private set; }
        public int InventoryDepth { get; private set; }
        public bool PostHarvestStop { get; private set; }
        public MovementPauseReasons Pauses { get; private set; }
        public float ActualSpeed => Pauses == MovementPauseReasons.None && !PostHarvestStop ? RequestedSpeed : 0f;

        public void SetPause(MovementPauseReasons reason, bool enabled)
        {
            if (enabled) Pauses |= reason;
            else Pauses &= ~reason;
        }

        public void SetSpeedCap(float value)
        {
            SpeedCap = Clamp(value, 0f, MaximumSpeedCap);
            RequestedSpeed = Math.Min(RequestedSpeed, SpeedCap);
            InventoryResumeSpeed = Math.Min(InventoryResumeSpeed, SpeedCap);
        }

        public void SpeedGesture(float delta)
        {
            if (Pauses != MovementPauseReasons.None || float.IsNaN(delta)) return;
            if (PostHarvestStop)
            {
                if (delta <= 0f) return;
                PostHarvestStop = false;
            }
            RequestedSpeed = Clamp(RequestedSpeed + delta, 0f, SpeedCap);
        }

        public void OpenInventory()
        {
            if (InventoryDepth++ == 0) InventoryResumeSpeed = RequestedSpeed;
            SetPause(MovementPauseReasons.Inventory, true);
        }

        public void CloseInventory()
        {
            if (InventoryDepth == 0 || --InventoryDepth != 0) return;
            RequestedSpeed = PostHarvestStop ? 0f : Math.Min(InventoryResumeSpeed, SpeedCap);
            SetPause(MovementPauseReasons.Inventory, false);
        }

        public void BeginHarvest()
        {
            RequestedSpeed = InventoryResumeSpeed = 0f;
            PostHarvestStop = true;
            SetPause(MovementPauseReasons.Harvest, true);
        }

        public void EndHarvest() => SetPause(MovementPauseReasons.Harvest, false);

        public void Advance(float seconds, float pathLength)
        {
            if (seconds <= 0f || float.IsNaN(seconds) || pathLength <= 0f) return;
            Distance = Math.Min(pathLength, Distance + ActualSpeed * seconds);
            if (Distance < pathLength) return;
            RequestedSpeed = InventoryResumeSpeed = 0f;
            SetPause(MovementPauseReasons.Camp, true);
        }

        public void Reset()
        {
            Distance = RequestedSpeed = InventoryResumeSpeed = 0f;
            InventoryDepth = 0;
            PostHarvestStop = false;
            // A debug restart must not resume a suspended/unfocused application.
            Pauses &= MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused;
        }

        public void Restore(float distance, float requested, float cap, float inventoryResume, bool inventoryOpen, bool postHarvest, bool camp)
        {
            if (float.IsNaN(distance) || float.IsInfinity(distance) || distance < 0 || float.IsNaN(requested) || float.IsInfinity(requested) || requested < 0 || float.IsNaN(cap) || float.IsInfinity(cap) || cap < 0 || cap > MaximumSpeedCap || requested > cap || float.IsNaN(inventoryResume) || float.IsInfinity(inventoryResume) || inventoryResume < 0 || inventoryResume > cap || (postHarvest && (requested != 0 || inventoryResume != 0)) || (camp && (requested != 0 || inventoryResume != 0)))
                throw new ArgumentException("Invalid saved movement state");
            Distance = distance; RequestedSpeed = requested; SpeedCap = cap; InventoryResumeSpeed = inventoryResume;
            InventoryDepth = inventoryOpen ? 1 : 0; PostHarvestStop = postHarvest;
            Pauses = (Pauses & (MovementPauseReasons.Loading | MovementPauseReasons.Suspended | MovementPauseReasons.Unfocused)) |
                     (inventoryOpen ? MovementPauseReasons.Inventory : 0) | (camp ? MovementPauseReasons.Camp : 0);
        }

        private static float Clamp(float value, float min, float max) => float.IsNaN(value) ? min : Math.Max(min, Math.Min(max, value));
    }

    public enum GestureOwner { Pending, Blocked, Orbit, Speed }

    public static class GestureOwnership
    {
        public static GestureOwner Resolve(bool beganOnAvatar, float dx, float dy, float threshold)
        {
            float x = Math.Abs(dx), y = Math.Abs(dy);
            if (x >= threshold && x >= y * 1.5f) return GestureOwner.Orbit;
            if (y >= threshold && y >= x * 1.5f) return beganOnAvatar ? GestureOwner.Speed : GestureOwner.Blocked;
            return GestureOwner.Pending;
        }
    }
}
