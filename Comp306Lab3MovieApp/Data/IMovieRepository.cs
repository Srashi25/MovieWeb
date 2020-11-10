using Comp306Lab3MovieApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Data
{
    public interface IMovieRepository
    {
        IQueryable<Movie> Movies { get; }

        void SaveMovie(Movie movie);
        Movie DeleteMovie(int movieID);
        Movie GetMovie(int id);
    }
}
    