using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class GmailSettings
    {
        public string Host { get; set; } = "";

        public int Port { get; set; }

        public string Email { get; set; } = "";

        public string Password { get; set; } = "";

        public string DisplayName { get; set; } = "";
    }
}