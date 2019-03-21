using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alpaca.Data
{
    public class ChannelSetting : Attribute
    {
        public string channel { get; set; } = "ALPACA_DEFAULT_MESSAGE";
    }
}
