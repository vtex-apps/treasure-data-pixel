using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TreasureData.Models
{
    /// <summary>
    /// td_format=pixel
    /// &td_write_key=XXXYYYZZZZ
    /// &td_global_id=td_global_id
    /// &td_ip=td_ip
    /// &td_ua=td_ua
    /// </summary>
    public class TreasureDataHeader
    {
        [JsonProperty("td_format")]
        public string Format { get; set; }

        [JsonProperty("td_global_id")]
        public string GlobalId { get; set; }

        [JsonProperty("td_ip")]
        public string IpAddress { get; set; }

        [JsonProperty("td_ua")]
        public string UserAgent { get; set; }
    }

    public class TreasureDataEvent
    {
        /// email_address(abi_email)
        /// first_name(abi_firstname)
        /// last_name(abi_lastname)
        /// phone_number(abi_phone)
        /// Address1(abi_address1)
        /// Address2(abi_address2)
        /// City(abi_city)
        /// Zip(abi_zip)
        /// country(vtex_country)
        /// region(abi_region)
        /// event_type
        /// value
        // Items.quantity(item_quantity)
        // items.product_ID(item_productid)
        // items.productName(item_productname)
        // Items.sku(item_sku)
        /// order_id

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("email_address")]
        public string Email { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }

    public class TreasureDataItem
    {
        [JsonProperty("item_quantity")]
        public long ItemQuantity { get; set; }

        [JsonProperty("item_productid")]
        public string TtemProductid { get; set; }

        [JsonProperty("item_productname")]
        public string ItemProductname { get; set; }

        [JsonProperty("item_sku")]
        public string ItemSku { get; set; }
    }
}
