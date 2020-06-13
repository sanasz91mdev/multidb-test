using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multidb_test
{
    class Program
    {
        static void Main(string[] args)
        {
            userModel model = new userModel();
            var person = model.getUser();
            Console.WriteLine(person.name);
            Console.WriteLine(model.getUserName());
            Console.Read();
        }
    }
}
