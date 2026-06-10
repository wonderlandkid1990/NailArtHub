using System.Resources;

namespace NailArtHub
{
    public class SharedResource
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("NailArtHub.SharedResource", typeof(SharedResource).Assembly);

        public static string Error_ShopName => _resourceManager.GetString("Error_ShopName");
        public static string Error_OwnerName => _resourceManager.GetString("Error_OwnerName");
        public static string Error_Address => _resourceManager.GetString("Error_Address");
        public static string Error_Location => _resourceManager.GetString("Error_Location");
        public static string Error_TradeNo => _resourceManager.GetString("Error_TradeNo");

    }
}
