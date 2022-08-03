using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class MessagePaginationParams : PaginationParams
    {
        public string Username { get; set; }
        public string Container { get; set; } = "Unread";
    }
}