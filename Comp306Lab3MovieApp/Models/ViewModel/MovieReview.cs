using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Models.ViewModel
{
    public class MovieReview
    {
        public int MovieId { get; set; }
        public Review Review { get; set; }
    }
}
