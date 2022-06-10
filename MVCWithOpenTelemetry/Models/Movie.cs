using System;
using System.Collections.Generic;

namespace MVCWithOpenTelemetry.Models
{
    public partial class Movie
    {
        public Movie()
        {
            MoviePrices = new HashSet<MoviePrice>();
        }

        public int Id { get; set; }
        public string? Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string? Genre { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<MoviePrice> MoviePrices { get; set; }
    }
}
