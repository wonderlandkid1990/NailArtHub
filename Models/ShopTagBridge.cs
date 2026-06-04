namespace NailArtHub.Models
{
    public class ShopTagBridge
    {
        public int Id { get; set; }

        // Connect Shop
        public int ShopId { get; set; }
        public Shop Shop { get; set; }

        // Connect NailTag
        public int NailTagId { get; set; }
        public NailTag NailTag { get; set; }
    }
}
