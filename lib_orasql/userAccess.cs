using lib_orasql.dataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib_orasql
{
    public class userAccess
    {

        public string getUser()
        {
            try
            { 
            var context = new iristestEntitiesOra();
            var user = context.profiles.ToList().Find(x => x.id == 1);
            return user?.name;
            }
            catch(Exception e)
            {
                return string.Empty;
            }
        }

    }
}
