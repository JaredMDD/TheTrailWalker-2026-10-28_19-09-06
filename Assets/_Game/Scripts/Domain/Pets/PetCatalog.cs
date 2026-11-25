using System;
using System.Collections.Generic;

namespace TheTrailWalker.Domain.Pets
{
    // Milestone 15 (spec 2026-11-24): the six settlement companions of section 13.5. Weights are percentages per supply
    // group (each row sums to 100); every pet costs 1,000 cash, is bought once and never released.
    public enum SupplyGroup { Wood, StoneOre, Fibers, Produce, Seeds, Truffle }

    public sealed class PetDefinition
    {
        public readonly string Id, Name, Specialty, StandingModel, SecondModel;
        public readonly IReadOnlyDictionary<SupplyGroup, int> Weights;
        public readonly long PriceCash;

        public PetDefinition(string id, string name, string specialty, string standing, string second, int wood, int stone, int fibers, int produce, int seeds, int truffle)
        {
            Id = id; Name = name; Specialty = specialty; StandingModel = standing; SecondModel = second; PriceCash = PetCatalog.Price;
            Weights = new Dictionary<SupplyGroup, int> {
                { SupplyGroup.Wood, wood }, { SupplyGroup.StoneOre, stone }, { SupplyGroup.Fibers, fibers },
                { SupplyGroup.Produce, produce }, { SupplyGroup.Seeds, seeds }, { SupplyGroup.Truffle, truffle } };
        }
    }

    public static class PetCatalog
    {
        public const long Price = 1000;
        // Bumped whenever a weight changes, so a saved interval snapshot can tell which table it was rolled with (slice 2).
        public const int WeightsRevision = 1;
        private const string Folder = "Tier02/Companions/Companion";

        public static readonly IReadOnlyList<PetDefinition> All = new[] {
            new PetDefinition("pet.dog", "Dog", "Building supplies", Folder + "DogStanding", Folder + "DogSniffing", 45, 40, 5, 5, 5, 0),
            new PetDefinition("pet.cat", "Cat", "Textile plants and kitchen finds", Folder + "CatStanding", Folder + "CatSitting", 10, 10, 40, 25, 15, 0),
            new PetDefinition("pet.rabbit", "Rabbit", "Fruit, herbs and seeds", Folder + "RabbitAlert", Folder + "RabbitCrouched", 5, 5, 20, 45, 25, 0),
            new PetDefinition("pet.pig", "Pig", "Truffles, from region 10", Folder + "PigStanding", Folder + "PigRooting", 10, 15, 5, 20, 10, 40),
            new PetDefinition("pet.squirrel", "Squirrel", "Seeds and branches", Folder + "SquirrelAlert", Folder + "SquirrelForaging", 25, 10, 15, 20, 30, 0),
            new PetDefinition("pet.tortoise", "Tortoise", "Stone, earth and raw ore", Folder + "TortoiseExtended", Folder + "TortoiseTucked", 10, 60, 15, 10, 5, 0) };

        public static PetDefinition Find(string id)
        {
            foreach (PetDefinition p in All) if (p.Id == id) return p;
            return null;
        }
    }
}
