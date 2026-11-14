namespace TheTrailWalker.Domain.Travel
{
    // Per-region look (spec 2026-11-08, section 2): fog, sun, ambient, sky and the Fern Hollow valley floor. Data only;
    // the scene builder applies it. Colours are linear RGB triples. The default reproduces the look every region had before.
    public sealed class RegionLook
    {
        public bool Fog;
        public float[] FogColor = { 0.70f, 0.76f, 0.80f };
        public float FogStart = 25f, FogEnd = 90f;
        public float SunIntensity = 1.1f;
        public float[] SunTint = { 1f, 1f, 1f };
        public float[] Ambient = { 0.5f, 0.5f, 0.5f };
        // Camera clear colour behind the terrain; with fog it matches the fog so distant silhouettes fade instead of cutting out.
        public float[] Sky = { 0.44f, 0.64f, 0.70f };
        // Valley floor: flat band either side of the road, rising to ValleyHeight by ValleyRiseEnd metres from it.
        public bool Valley;
        public float ValleyFlat, ValleyHeight, ValleyRiseEnd;
        // Fern and bush scatter weights on the valley slopes (beyond the flat band) and on its floor.
        public float SlopeScatter = 1f, FloorScatter = 1f;
    }

    public static class RegionLooks
    {
        public static RegionLook Default => new RegionLook();
        // The settlement (backdrop proposal, November 9, 2026): Fern Hollow's light with a far haze tinted toward Cloudstep's
        // grey-blue, so region 5's mountains on the horizon sit back.
        public static RegionLook Settlement => new RegionLook { Fog = true, FogColor = new[] { 0.72f, 0.78f, 0.84f }, FogStart = 70f, FogEnd = 340f };

        // The look follows the walked leg's region (a landing belongs to the region it leads into); the settlement approach
        // uses Fern Hollow's palette and light without the valley slopes.
        public static RegionLook For(int regionNumber, bool settlementApproach)
        {
            switch (regionNumber)
            {
                case 3: // Mistwood: linear fog, a softer cool sun and a slightly cooler grey ambient.
                    return new RegionLook { Fog = true, FogColor = new[] { 0.70f, 0.76f, 0.80f }, FogStart = 25f, FogEnd = 90f, SunIntensity = 0.9f,
                        SunTint = new[] { 0.92f, 0.96f, 1.0f }, Ambient = new[] { 0.46f, 0.49f, 0.52f }, Sky = new[] { 0.70f, 0.76f, 0.80f } };
                case 4: // Fern Hollow: a valley floor; ferns and bushes crowd the slopes.
                    return settlementApproach ? Default
                        : new RegionLook { Valley = true, ValleyFlat = 16f, ValleyHeight = 7f, ValleyRiseEnd = 40f, SlopeScatter = 2f, FloorScatter = 0.5f };
                default: return Default;
            }
        }
    }
}
