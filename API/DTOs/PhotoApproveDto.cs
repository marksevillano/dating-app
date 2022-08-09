using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class PhotoApproveDto
    {
        public int[] PhotoIds { get; set; }
        public Boolean Predicate { get; set; }
    }
}