using System;
using System.Threading.Tasks;
using TreasureData.Models;

namespace TreasureData.Services
{
    public interface ITreasureDataAPI
    {
        Task<TreasureDataEvent> BuildEvent(VtexOrder vtexOrder, string eventType);
        Task<bool> ProcessNotification(AllStatesNotification hookNotification);
        Task<bool> SendEvent(TreasureDataEvent TreasureDataEvent);
    }
}