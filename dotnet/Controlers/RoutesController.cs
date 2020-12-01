namespace service.Controllers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using TreasureData.Data;
    using TreasureData.Models;
    using TreasureData.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Vtex.Api.Context;

    public class RoutesController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderFeedAPI _orderFeedAPI;
        private readonly ITreasureDataAPI _treasureDataAPI;
        private readonly IIOServiceContext _context;

        public RoutesController(IHttpContextAccessor httpContextAccessor, IOrderFeedAPI orderFeedAPI, ITreasureDataAPI treasureDataAPI, IIOServiceContext context)
        {
            this._httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this._orderFeedAPI = orderFeedAPI ?? throw new ArgumentNullException(nameof(orderFeedAPI));
            this._treasureDataAPI = treasureDataAPI ?? throw new ArgumentNullException(nameof(treasureDataAPI));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> BuildEvent(string orderId, string eventType)
        {
            Console.WriteLine($"[Build Event]");
            VtexOrder vtexOrder = await _orderFeedAPI.GetOrderInformation(orderId);
            TreasureDataEvent treasureDataEvent = await _treasureDataAPI.BuildEvent(vtexOrder, eventType);
            return Json(treasureDataEvent);
        }
    }
}
