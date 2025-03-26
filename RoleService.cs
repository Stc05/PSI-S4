using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class RoleService
    {
        public string ChooseRole()
        {
            Console.WriteLine("Choisissez votre rôle :");
            Console.WriteLine("1 - Client");
            Console.WriteLine("2 - Cuisinier");

            string role = Console.ReadLine();

            if (role == "1")
            {
                return "Client";
            }
            else if (role == "2")
            {
                return "Cuisinier";
            }
            else
            {
                Console.WriteLine("Choix invalide.");
                return null;
            }
        }
    }
}
