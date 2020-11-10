using System.Threading.Tasks;
using TreasureData.Models;

namespace TreasureData.Services
{
    public interface IOrderFeedAPI
    {
        Task<VtexOrder> GetOrderInformation(string orderId);
        Task<MerchantSettings> GetMerchantSettings();
    }
}