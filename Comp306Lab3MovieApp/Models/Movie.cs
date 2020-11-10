using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        [Required(ErrorMessage = "Genre is required")]
        public Genre Genre { get; set; }

        [Required(ErrorMessage = "Movie Name is required")]
        public string MovieName { get; set; }
        public User User { get; set; }
        public string Description { get; set; }
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        public double Rating { get; set; } = 0.0;
        public string FilePath { get; set; }
        public string ImageUrl { get; set; }
        public string Substring(string desc)
        {
            string substring = desc.Substring(0, 60);
            return substring;
        }


    }
}
