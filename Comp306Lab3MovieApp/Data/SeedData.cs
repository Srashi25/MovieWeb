using Comp306Lab3MovieApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MovieAppContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<MovieAppContext>>()))
            {
                // Look for any movies.
                if (context.Movies.Any())
                {
                    return;   // DB has been seeded
                }

                context.Movies.AddRange(
                    new Movie
                    {
                        MovieName = "When Harry Met Sally",
                        ReleaseDate = DateTime.Parse("1989-2-12"),
                        Genre = Genre.COMEDY,
                        Rating = 3,
                       Description = "this is the description",
                        ImageUrl = "https://upload.wikimedia.org/wikipedia/en/3/38/The_SpongeBob_Movie_Sponge_on_the_Run.jpg"
                    },

                    new Movie
                    {
                        MovieName = "Ghostbusters ",
                        ReleaseDate = DateTime.Parse("1984-3-13"),
                        Genre = Genre.HORROR,
                        Rating = 4,
                        Description = "This is really scary kids movie",
                        ImageUrl = "https://upload.wikimedia.org/wikipedia/en/2/2f/Ghostbusters_%281984%29_theatrical_poster.png"
                    },

                    new Movie
                    {
                        MovieName = "Ghostbusters 2",
                        ReleaseDate = DateTime.Parse("1986-2-23"),
                        Genre = Genre.HORROR,
                        Rating = 3,
                        Description = "This is a horror movie",
                        ImageUrl = "https://upload.wikimedia.org/wikipedia/en/0/01/Ghostbusters_ii_poster.jpg"
                    },

                    new Movie
                    {
                        MovieName = "Rio Bravo",
                        ReleaseDate = DateTime.Parse("1959-4-15"),
                        Genre = Genre.THRILLER,
                        Rating = 4,
                        Description = "This is athriller movie",
                        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/51I9qHhm1AL._AC_.jpg"
                    }
                ); ;
                context.SaveChanges();
                // Look for any movies.
                if (context.Users.Any())
                {
                    return;   // DB has been seeded
                }

                context.Users.AddRange(
                    new User
                    {
                        Email = "sadia@gmail.com",
                      Password="password",
                      ConfirmPassword="password"
                    },

                    new User
                    {
                        Email = "charles@gmail.com",
                        Password = "123456",
                        ConfirmPassword = "123456"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
