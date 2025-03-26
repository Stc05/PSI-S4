using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{
    public class CuisinierService
    {
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;" +
                                          "DATABASE=LivInParis;" +
                                          "UID=root;PASSWORD=Granite666!";

        public void AddPlat(int idUtilisateur)
        {
            Console.WriteLine("Ajout d'un nouveau plat :");

            Console.Write("Nom du plat : ");
            string nom = Console.ReadLine();

            Console.Write("Type (entrée, plat principal, dessert) : ");
            string type = Console.ReadLine();

            var nationalites = GetExistingNationalities();
            Console.WriteLine("Choisissez une nationalité existante ou entrez une nouvelle :");
            for (int i = 0; i < nationalites.Count; i++)
                Console.WriteLine($"{i + 1}. {nationalites[i]}");
            Console.WriteLine($"{nationalites.Count + 1}. Autre (ajouter une nouvelle)");

            string choixNat = Console.ReadLine();
            string nationalite = (int.TryParse(choixNat, out int idx) && idx > 0 && idx <= nationalites.Count)
                ? nationalites[idx - 1]
                : Console.ReadLine();

            Console.Write("Ingrédients principaux : ");
            string ingredients = Console.ReadLine();

            Console.Write("Régime alimentaire : ");
            string regime = Console.ReadLine();

            Console.Write("Recette : ");
            string recette = Console.ReadLine();

            Console.Write("Prix du plat : ");
            decimal prix = decimal.Parse(Console.ReadLine());

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO Plat (Nom, Type, Date_Fabrication, Date_Péremption, Nationalite,
                        Ingrédients_Principaux, Régime_Alimentaire, Recette, Déclinaison, Id_Utilisateur, Prix)
                        VALUES (@Nom, @Type, @DateFabrication, @DatePeremption, @Nationalite,
                        @Ingredients, @Regime, @Recette, @Declinaison, @IdUtilisateur, @Prix)";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Nom", nom);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@DateFabrication", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DatePeremption", DateTime.Now.AddDays(5));
                    cmd.Parameters.AddWithValue("@Nationalite", nationalite);
                    cmd.Parameters.AddWithValue("@Ingredients", ingredients);
                    cmd.Parameters.AddWithValue("@Regime", regime);
                    cmd.Parameters.AddWithValue("@Recette", recette);
                    cmd.Parameters.AddWithValue("@Declinaison", "Classique");
                    cmd.Parameters.AddWithValue("@IdUtilisateur", idUtilisateur);
                    cmd.Parameters.AddWithValue("@Prix", prix);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ Plat ajouté avec succès !");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("❌ Erreur SQL : " + ex.Message);
                }
            }
        }

        private List<string> GetExistingNationalities()
        {
            List<string> list = new List<string>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT DISTINCT Nationalite FROM Plat", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        list.Add(reader.GetString("Nationalite"));
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur chargement nationalités : " + ex.Message);
                }
            }
            return list;
        }
    }
}
