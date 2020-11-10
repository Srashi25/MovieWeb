using Comp306Lab3MovieApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comp306Lab3MovieApp.Data
{
    public interface IUserRepository
    {
        IQueryable<User> Users { get; }
        void SaveUser(User user);
        User GetUserMovies(string email);

    }
}
