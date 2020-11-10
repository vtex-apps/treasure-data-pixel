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
    public class TreasureDataEvent
    {
        [JsonProperty("td_format")]
        public string Format { get; set; }

        [JsonProperty("td_write_key")]
        public string WriteKey { get; set; }

        [JsonProperty("td_global_id")]
        public string GlobalId { get; set; }

        [JsonProperty("td_ip")]
        public string IpAddress { get; set; }

        [JsonProperty("td_ua")]
        public string UserAgent { get; set; }





        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("email")]
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

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }


    }
}
