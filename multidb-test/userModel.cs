using entities;
using lib_orasql;
using lib_pgsql.dataModel;
using repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multidb_test
{
    public class userModel
    {
        public profile getUser()
        {
            var repo = new userRepository();

            return repo.getUser();
        }

        public string getUserName()
        {
            return new userAccess().getUser();
        }
    }
}
