﻿using System;
using System.Collections.Generic;
using System.Text;

namespace TreasureData.Models
{
    public class MerchantSettings
    {
        public string ApiKey { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string FieldPrefix { get; set; }
        public Dictionary<string, string> CustomFields { get; set; }
    }
}
