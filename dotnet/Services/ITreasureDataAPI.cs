using System;
using System.Threading.Tasks;
using TreasureData.Models;

namespace TreasureData.Services
{
    public interface ITreasureDataAPI
    {
        Task<bool> ProcessNotification(AllStatesNotification hookNotification);
        Task<bool> SendEvent(object TreasureDataEvent);
        Task<bool> BuildAndSendEvent(VtexOrder vtexOrder, string eventType);
    }
}