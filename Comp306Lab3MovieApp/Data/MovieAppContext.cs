using Comp306Lab3MovieApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Data
{
    public class MovieAppContext : DbContext
    {
        public MovieAppContext(DbContextOptions<MovieAppContext> options)
             : base(options)
        {
        }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }
            optionsBuilder.UseSqlServer($"Server=<your db string>;Database=moviewebappdb; User Id= <your usrr name>; Password: <your password>");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
