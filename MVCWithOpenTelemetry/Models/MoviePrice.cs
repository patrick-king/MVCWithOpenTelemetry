using System;
using System.Collections.Generic;

namespace MVCWithOpenTelemetry.Models
{
    public partial class MoviePrice
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public DateTime DateEntered { get; set; }
        public int SellerId { get; set; }
        public double Price { get; set; }

        public virtual Movie Movie { get; set; } = null!;
    }
}
