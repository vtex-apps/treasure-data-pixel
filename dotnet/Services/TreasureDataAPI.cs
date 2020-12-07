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
using System.ComponentModel;

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

        //public async Task<bool> ProcessNotification(AllStatesNotification allStatesNotification)
        //{
        //    bool success = true;
        //    // Use our server - side Track API for the following:
        //    // Placed Order - When an order successfully processes on your system
        //    // Ordered Product - An event for each item in a processed order
        //    // Fulfilled Order - When an order is sent to the customer
        //    // Cancelled Order - When a customer cancels their order
        //    // Refunded Order - When a customer’s order is refunded
        //    string orderId = allStatesNotification.OrderId;
        //    TreasureDataEvent treasureDataEvent;
        //    VtexOrder vtexOrder = await _orderFeedAPI.GetOrderInformation(orderId);
        //    switch (allStatesNotification.Domain)
        //    {
        //        case Constants.Domain.Fulfillment:
        //            switch (allStatesNotification.CurrentState)
        //            {
        //                case Constants.Status.ReadyForHandling:
        //                    treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.PlacedOrder);
        //                    if (treasureDataEvent != null)
        //                    {
        //                        success = await SendEvent(treasureDataEvent);
        //                        // Send each item as a separate event
        //                        foreach (Item item in vtexOrder.Items)
        //                        {
        //                            VtexOrder order = vtexOrder;
        //                            order.Items = new List<Item> { item };
        //                            treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.OrderedProduct);
        //                            success = success && await SendEvent(treasureDataEvent);
        //                        }
        //                    }

        //                    break;
        //                case Constants.Status.Invoiced:
        //                    treasureDataEvent = await BuildEvent(vtexOrder, Constants.Events.FulfilledOrder);
        //                    if (treasureDataEvent != null)
        //                    {
        //                        success = await SendEvent(treasureDataEvent);
        //                    }

        //                    break;
        //                //case Constants.Status.Canceled:
        //                //    break;
        //                default:
        //                    Console.WriteLine($"State {allStatesNotification.CurrentState} not implemeted.");
        //                    _context.Vtex.Logger.Info("ProcessNotification", null, $"State {allStatesNotification.CurrentState} not implemeted.");
        //                    break;
        //            }
        //            break;
        //        case Constants.Domain.Marketplace:
        //            break;
        //        default:
        //            Console.WriteLine($"Domain {allStatesNotification.Domain} not implemeted.");
        //            _context.Vtex.Logger.Info("ProcessNotification", null, $"Domain {allStatesNotification.Domain} not implemeted.");
        //            break;
        //    }

        //    return success;
        //}

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
            VtexOrder vtexOrder = await _orderFeedAPI.GetOrderInformation(orderId);
            switch (allStatesNotification.Domain)
            {
                case Constants.Domain.Fulfillment:
                    switch (allStatesNotification.CurrentState)
                    {
                        case Constants.Status.ReadyForHandling:
                            success = await this.BuildAndSendEvent(vtexOrder, Constants.Events.PlacedOrder);
                            break;
                        case Constants.Status.Invoiced:
                            success = await this.BuildAndSendEvent(vtexOrder, Constants.Events.FulfilledOrder);
                            break;
                        case Constants.Status.Canceled:
                            success = await this.BuildAndSendEvent(vtexOrder, Constants.Events.CanceledOrder);
                            break;
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
        public async Task<bool> SendEvent(object treasureDataEventRecord)
        {
            bool success = false;
            string jsonSerializedEvent = JsonConvert.SerializeObject(treasureDataEventRecord);
            _context.Vtex.Logger.Info("SendEvent", null, $"{jsonSerializedEvent}");
            Console.WriteLine($"Event = {jsonSerializedEvent}");
            MerchantSettings merchantSettings = await _orderFeedAPI.GetMerchantSettings();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{Constants.TreasureDataAPIEndpoint}/{Constants.TreasureDataPostback}/{merchantSettings.Database}/{merchantSettings.Table}?{Constants.TD_KEY_HEADER}={merchantSettings.ApiKey}"),
                Content = new StringContent(jsonSerializedEvent, Encoding.UTF8, Constants.APPLICATION_JSON)
            };

            // td_write_key
            //request.Headers.Add(Constants.TD_KEY_HEADER, merchantSettings.ApiKey);
            request.Headers.Add(Constants.HTTP_FORWARDED_HEADER, this._httpContextAccessor.HttpContext.Request.Headers[Constants.FORWARDED_HEADER].ToString());
            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
                //request.Headers.Add(Constants.VtexIdCookie, authToken);
            }

            var client = _clientFactory.CreateClient();
            // We want to error so that the Event Broasdcast will re-play
            //try
            //{
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[-] SendEvent Response {response.StatusCode} Content = '{responseContent}' |{request.RequestUri}| [-]");
                _context.Vtex.Logger.Info("SendEvent", null, $"[{response.StatusCode}] '{responseContent}' {request.RequestUri}");

                success = response.IsSuccessStatusCode;
            //}
            //catch(Exception ex)
            //{
            //    _context.Vtex.Logger.Error("SendEvent", null, $"Error sending '{jsonSerializedEvent}' tp '{request.RequestUri}'", ex);
            //}

            return success;
        }

        public async Task<bool> BuildAndSendEvent(VtexOrder vtexOrder, string eventType)
        {
            bool sent = true;
            if (vtexOrder != null)
            {
                MerchantSettings merchantSettings = await _orderFeedAPI.GetMerchantSettings();

                TreasureDataHeader treasureDataHeader = new TreasureDataHeader
                {
                    // td_format=pixel
                    // &td_write_key=XXXYYYZZZZ
                    // &td_global_id=td_global_id
                    // &td_ip=td_ip
                    // &td_ua=td_ua
                    Format = "pixel",
                    GlobalId = "td_global_id",
                    IpAddress = "td_ip",
                    UserAgent = "ua"
                };

                TreasureDataEvent treasureDataEvent = new TreasureDataEvent
                {
                    OrderId = vtexOrder.OrderId,
                    Country = vtexOrder.ShippingData.Address.Country,
                    EventType = eventType,
                    Value = ToDollar(vtexOrder.Value)
                };

                TreasureDataEventPrefix treasureDataEventPrefix = new TreasureDataEventPrefix
                {
                    Address1 = vtexOrder.ShippingData.Address.Street,
                    Address2 = vtexOrder.ShippingData.Address.Complement,
                    City = vtexOrder.ShippingData.Address.City,
                    Email = vtexOrder.ClientProfileData.Email,
                    FirstName = vtexOrder.ClientProfileData.FirstName,
                    LastName = vtexOrder.ClientProfileData.LastName,
                    PhoneNumber = vtexOrder.ClientProfileData.Phone,
                    Zip = vtexOrder.ShippingData.Address.PostalCode,
                    ConsentKey = null   // TODO: ready consent status from orderform
                };

                Dictionary<string, string> tdRecordBase = new Dictionary<string, string>();
                Dictionary<string, string> tdHeader = ToDictionary(treasureDataHeader);
                Dictionary<string, string> tdEvent = ToDictionary(treasureDataEvent);
                Dictionary<string, string> tdEventPrefix = AddPrefix(ToDictionary(treasureDataEventPrefix), merchantSettings.FieldPrefix);
                
                tdRecordBase = AddToDictionary(tdRecordBase, tdHeader);
                tdRecordBase = AddToDictionary(tdRecordBase, tdEvent);
                tdRecordBase = AddToDictionary(tdRecordBase, tdEventPrefix);
                if (merchantSettings.CustomFields != null)
                {
                    Dictionary<string, string> customFields = new Dictionary<string, string>();
                    foreach(CustomField setting in merchantSettings.CustomFields)
                    {
                        customFields.Add(setting.ColumnName, setting.ColumnValue);
                    }

                    tdRecordBase = AddToDictionary(tdRecordBase, customFields);
                }

                foreach (Item item in vtexOrder.Items)
                {
                    TreasureDataItem treasureDataItem = new TreasureDataItem
                    {
                        ItemProductname = item.Name,
                        ItemQuantity = item.Quantity,
                        ItemSku = item.SellerSku,
                        TtemProductid = item.ProductId
                    };

                    Dictionary<string, string> tdItem = ToDictionary(treasureDataItem);
                    Dictionary<string, string> treasureDataEventRecord = AddToDictionary(tdRecordBase, tdItem);

                    sent &= await this.SendEvent(treasureDataEventRecord);
                }
            }
            else
            {
                Console.WriteLine($"Could not load order {vtexOrder.OrderId}");
                _context.Vtex.Logger.Info("BuildEvent", null, $"Could not load order {vtexOrder.OrderId}");
            }

            return sent;
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

        public static Dictionary<string, string> ToDictionary(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return dictionary;
        }

        public static Dictionary<string, string> AddPrefix(Dictionary<string, string> obj, string prefix)
        {
            if(!string.IsNullOrEmpty(prefix))
            {
                Dictionary<string, string> newObj = new Dictionary<string, string>();
                foreach (var kvp in obj)
                {
                    newObj.Add($"{prefix}{kvp.Key}", kvp.Value);
                }

                obj = newObj;
            }
            
            return obj;
        }

        public static Dictionary<string, string> AddToDictionary(Dictionary<string, string> addTo, Dictionary<string, string> addFrom)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();
            if (addTo != null)
            {
                foreach (var kvp in addTo)
                {
                    retDict.Add(kvp.Key, kvp.Value);
                }
            }

            if (addFrom != null)
            {
                foreach (var kvp in addFrom)
                {
                    retDict.Add(kvp.Key, kvp.Value);
                }
            }

            return retDict;
        }
    }
}
