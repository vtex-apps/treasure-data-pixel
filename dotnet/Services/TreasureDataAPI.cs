using TreasureData.Data;
using TreasureData.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vtex.Api.Context;

namespace TreasureData.Services
{
    public class TreasureDataAPI : ITreasureDataAPI
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderFeedAPI _orderFeedAPI;
        private readonly IIOServiceContext _context;

        public TreasureDataAPI(IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IOrderFeedAPI orderFeedAPI, IIOServiceContext context)
        {
          this._httpContextAccessor = httpContextAccessor ??
                                      throw new ArgumentNullException(nameof(httpContextAccessor));

          this._clientFactory = clientFactory ??
                                throw new ArgumentNullException(nameof(clientFactory));

          this._orderFeedAPI = orderFeedAPI ??
                                throw new ArgumentNullException(nameof(orderFeedAPI));

          this._context = context ??
                                throw new ArgumentNullException(nameof(context));
        }

        public async Task<bool> ProcessNotification(AllStatesNotification allStatesNotification)
        {
            bool success = true;
            // Use our server - side Track API for the following:
            // Placed Order - When an order successfully processes on your system
            // Ordered Product - An event for each item in a processed order
            // Fulfilled Order - When an order is sent to the customer
            // Cancelled Order - When a customer cancels their order
            // Refunded Order - When a customer’s order is refunded
            string orderId = allStatesNotification.OrderId;
            TreasureDataEvent treasureDataEvent;
            VtexOrder vtexOrder = await _orderFeedAPI.GetOrderInformation(orderId);
            switch (allStatesNotification.Domain)
            {
                case Constants.Domain.Fulfillment:
                    switch (allStatesNotification.CurrentState)
                    {
                        case Constants.Status.ReadyForHandling:
                            treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.PlacedOrder);
                            if (treasureDataEvent != null)
                            {
                                success = await SendEvent(treasureDataEvent);
                                // Send each item as a separate event
                                foreach (Item item in vtexOrder.Items)
                                {
                                    VtexOrder order = vtexOrder;
                                    order.Items = new List<Item> { item };
                                    treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.OrderedProduct);
                                    success = success && await SendEvent(treasureDataEvent);
                                }
                            }

                            break;
                        case Constants.Status.Invoiced:
                            treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.FulfilledOrder);
                            if (treasureDataEvent != null)
                            {
                                success = await SendEvent(treasureDataEvent);
                            }

                            break;
                        //case Constants.Status.Canceled:
                        //    break;
                        default:
                            Console.WriteLine($"State {allStatesNotification.CurrentState} not implemeted.");
                            _context.Vtex.Logger.Info("ProcessNotification", null, $"State {allStatesNotification.CurrentState} not implemeted.");
                            break;
                    }
                    break;
                case Constants.Domain.Marketplace:
                    break;
                default:
                    Console.WriteLine($"Domain {allStatesNotification.Domain} not implemeted.");
                    _context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {allStatesNotification.Domain} not implemeted.");
                    break;
            }

            return success;
        }

        /// <summary>
        /// POST /postback/v3/event/{database}/{table}
        /// </summary>
        /// <param name="treasureDataEvent"></param>
        /// <returns></returns>
        public async Task<bool> SendEvent(TreasureDataEvent treasureDataEvent)
        {
            bool success = false;
            string jsonSerializedEvent = JsonConvert.SerializeObject(treasureDataEvent);
            Console.WriteLine($"Event = {jsonSerializedEvent}");
            MerchantSettings merchantSettings = await _orderFeedAPI.GetMerchantSettings();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Constants.TreasureDataAPIEndpoint}/{Constants.TreasureDataPostback}/{merchantSettings.Database}/{merchantSettings.Table}"),
                Content = new StringContent(jsonSerializedEvent, Encoding.UTF8, Constants.APPLICATION_JSON)
            };

            request.Headers.Add(Constants.HTTP_FORWARDED_HEADER, this._httpContextAccessor.HttpContext.Request.Headers[Constants.FORWARDED_HEADER].ToString());
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
                //request.Headers.Add(Constants.VtexIdCookie, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"[-] SendEvent Response {response.StatusCode} Content = '{responseContent}' [-]");
            _context.Vtex.Logger.Info("SendEvent", null, $"[{response.StatusCode}] {responseContent}");

            success = response.IsSuccessStatusCode;

            return success;
        }

        public async Task<TreasureDataEvent> BuildEvent(VtexOrder vtexOrder, string eventType)
        {
            TreasureDataEvent treasureDataEvent = null;
            if (vtexOrder != null)
            {
                MerchantSettings merchantSettings = await _orderFeedAPI.GetMerchantSettings();
                treasureDataEvent = new TreasureDataEvent
                {
                    // td_format=pixel
                    // &td_write_key=XXXYYYZZZZ
                    // &td_global_id=td_global_id
                    // &td_ip=td_ip
                    // &td_ua=td_ua
                    Format = "pixel",
                    WriteKey = merchantSettings.ApiKey,
                    GlobalId = "td_global_id",
                    IpAddress = "td_ip",
                    UserAgent = "ua",
                    Address1 = vtexOrder.ShippingData.Address.Street,
                    Address2 = vtexOrder.ShippingData.Address.Complement,
                    City = vtexOrder.ShippingData.Address.City,
                    Country = vtexOrder.ShippingData.Address.Country,
                    Email = vtexOrder.ClientProfileData.Email,
                    EventType = eventType,
                    FirstName = vtexOrder.ClientProfileData.FirstName,
                    LastName = vtexOrder.ClientProfileData.LastName,
                    PhoneNumber = vtexOrder.ClientProfileData.Phone,
                    Region = vtexOrder.ShippingData.Address.Neighborhood,
                    Value = vtexOrder.Value,
                    Zip = vtexOrder.ShippingData.Address.PostalCode
                };

                _context.Vtex.Logger.Info("BuildEvent", null, JsonConvert.SerializeObject(treasureDataEvent));
            }
            else
            {
                Console.WriteLine($"Could not load order {vtexOrder.OrderId}");
                _context.Vtex.Logger.Info("BuildEvent", null, $"Could not load order {vtexOrder.OrderId}");
            }

            return treasureDataEvent;
        }

        public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }

        public decimal ToDollar(int asPennies)
        {
            return (decimal)asPennies / 100;
        }
    }
}
