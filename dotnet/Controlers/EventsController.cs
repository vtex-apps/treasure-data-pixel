namespace service.Controllers
{
    using TreasureData.Models;
    using TreasureData.Services;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using Vtex.Api.Context;

    public class EventsController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly ITreasureDataAPI _treasureDataAPI;

        public EventsController(IIOServiceContext context, ITreasureDataAPI treasureDataAPI)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._treasureDataAPI = treasureDataAPI ?? throw new ArgumentNullException(nameof(treasureDataAPI));
        }

        public string OnAppsLinked(string account, string workspace)
        {
            return $"OnAppsLinked event detected for {account}/{workspace}";
        }

        public void AllStates(string account, string workspace)
        {
            string bodyAsText = new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync().Result;
            AllStatesNotification allStatesNotification = JsonConvert.DeserializeObject<AllStatesNotification>(bodyAsText);
            bool success = _treasureDataAPI.ProcessNotification(allStatesNotification).Result;
            if (!success)
            {
                _context.Vtex.Logger.Info("Order Broadcast", null, $"Failed to Process Notification {bodyAsText}");
                throw new Exception("Failed to Process Notification");
            }
        }
    }
}
