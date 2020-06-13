using entities;
using lib_pgsql.dataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace repositories
{
    public class userRepository
    {
        public profile getUser()
        {
            var myContext = new userEntities();
            var profile_x = myContext.profiles.ToList().Find(x => x.id == 1);
            return profile_x;
        }
    }
}
