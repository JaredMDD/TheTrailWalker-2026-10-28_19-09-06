using System;
using System.Collections.Generic;
using TheTrailWalker.Domain.Items;
using TheTrailWalker.Domain.Travel;
using UnityEngine;
using UnityEngine.UI;

namespace TheTrailWalker
{
    // Route information (Milestone 8 item 5, November 2, 2026): the departure preview while seated and the remaining leg
    // while walking, both from RouteBudget (the same drain and food model as the game and the route checks). It warns
    // but never locks departure or eats/buys food by itself (section 6.2.4).
    public sealed partial class CollectionPrototype
    {
        private RectTransform routeCard;
        private Text routeText;
        private Image routePlate;
        private float routeRefreshAt;
        private const int RouteServingLimit = 400; // enough for any leg up to the 26-heart ceiling

        private void CreateRouteCard()
        {
            routeCard = Rect("Route card", safe, 0.25f, 0.095f, 0.75f, 0.165f);
            routePlate = routeCard.gameObject.AddComponent<Image>(); routePlate.raycastTarget = false;
            Sprite tab = Resources.Load<Sprite>("UI/Missions/tab");
            if (tab != null) { routePlate.sprite = tab; routePlate.type = Image.Type.Simple; } else routePlate.color = new Color(0.93f, 0.86f, 0.7f, 0.95f);
            routeText = Label(routeCard, "", 0.05f, 0.04f, 0.95f, 0.96f, 20);
            routeText.color = new Color(0.25f, 0.15f, 0.06f, 1f); routeText.raycastTarget = false;
            routeText.resizeTextForBestFit = true; routeText.resizeTextMinSize = 11; routeText.resizeTextMaxSize = 20;
            routeCard.gameObject.SetActive(false);
        }

        // Carried ready food, one entry per serving (smallest first), as the simulator eats it.
        private List<double> CarriedServings()
        {
            var servings = new List<double>();
            foreach (ItemEntry item in ledger.Entries)
            {
                if ((item.Owner != ItemOwner.Backpack && item.Owner != ItemOwner.Pocket && item.Owner != ItemOwner.Hook) || !item.Ready || item.Definition.FoodHearts <= 0) continue;
                for (int i = 0; i < item.Quantity && servings.Count < RouteServingLimit; i++) servings.Add(item.Definition.FoodHearts);
            }
            servings.Sort();
            return servings;
        }
        private long CarriedFoodHearts()
        {
            long hearts = 0;
            foreach (ItemEntry item in ledger.Entries)
                if ((item.Owner == ItemOwner.Backpack || item.Owner == ItemOwner.Pocket || item.Owner == ItemOwner.Hook) && item.Ready && item.Definition.FoodHearts > 0)
                    hearts += (long)item.Definition.FoodHearts * item.Quantity;
            return hearts;
        }

        private void UpdateRouteCard()
        {
            if (routeCard == null || travel == null) return;
            bool covered = inventoryOpen || stamina.Fainted || saveBlocked || TravelInputLocked || active != null
                || (craftPanel != null && craftPanel.activeSelf) || (gearPanel != null && gearPanel.activeSelf) || (missionPanel != null && missionPanel.activeSelf)
                || (mapPanel != null && mapPanel.activeSelf) || (tollPanel != null && tollPanel.activeSelf) || !TrailControlsVisible(InSettlementScene) || BalloonOpen;
            SegmentDefinition leg = travel.CurrentSegment;
            bool show = !covered && leg != null && !leg.RiverApproach;
            routeCard.gameObject.SetActive(show);
            if (!show || Time.unscaledTime < routeRefreshAt) return;
            routeRefreshAt = Time.unscaledTime + 0.5f;
            string destination = leg.SettlementApproach ? "the settlement" : scene.Catalog.Camp(leg.ToCampId)?.Name ?? "the next camp";
            double cost = RouteBudget.Hearts(leg);
            if (phase == TravelPhase.Seated)
            {
                RouteBudget.LegOutcome plan = RouteBudget.Simulate(leg, stamina.Maximum, stamina.Hearts, stamina.PendingFood, CarriedServings(), Math.Max(0.5, movement.SpeedCap));
                long food = CarriedFoodHearts();
                string verdict = plan.Completed
                    ? (plan.ServingsEaten == 0 ? "No food needed" : $"OK: about {plan.ServingsEaten:N0} serving{(plan.ServingsEaten == 1 ? "" : "s")} on the way")
                    : "Not enough food: carry or craft more";
                routeText.text = $"Next: {destination} · {leg.Length:N0} m · {cost:0.#} hearts\nYou {stamina.Hearts:0.#}/{stamina.Maximum:0} hearts · food {food:N0} hearts · {verdict}";
                routeText.color = plan.Completed ? new Color(0.25f, 0.15f, 0.06f, 1f) : new Color(0.62f, 0.12f, 0.05f, 1f);
            }
            else
            {
                // Drain is charged per walked path metre, so the remaining road is measured on the scene's path.
                double remaining = Math.Max(0, path.Length - movement.Distance);
                routeText.text = $"{destination} in {remaining:N0} m · {remaining * RouteBudget.RegionOneDrainPerMetre:0.#} hearts\nYou {stamina.Hearts:0.#}/{stamina.Maximum:0} hearts";
                routeText.color = new Color(0.25f, 0.15f, 0.06f, 1f);
            }
        }
    }
}
