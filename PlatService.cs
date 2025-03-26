using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rendu2_PSI
{

    public class PlatService
    {
        private string connectionString = "SERVER=127.0.0.1;PORT=3306;" +
                                          "DATABASE=LivInParis;" +
                                          "UID=root;PASSWORD=Granite666!";

        // Afficher les plats disponibles filtrés par nationalité et type
        public void DisplayPlatsFiltre(string nationalite, string typePlat)
        {
            string query = @"
                SELECT p.Id_Plat, p.Nom, p.Type, p.Recette, p.Prix, u.Nom AS CuisinierNom, u.Adresse AS CuisinierAdresse
                FROM Plat p
                JOIN Cuisinier c ON p.Id_Utilisateur = c.Id_Utilisateur
                JOIN Utilisateur u ON c.Id_Utilisateur = u.Id_Utilisateur
                WHERE p.Nationalite = @Nationalite AND p.Type = @Type";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Nationalite", nationalite);
                    cmd.Parameters.AddWithValue("@Type", typePlat);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    Console.WriteLine($"\nListe des {typePlat}s disponibles :");
                    while (reader.Read())
                    {
                        int idPlat = (int)reader["Id_Plat"];
                        string platNom = reader["Nom"].ToString();
                        string recette = reader["Recette"].ToString();
                        decimal prix = (decimal)reader["Prix"];
                        string cuisinierNom = reader["CuisinierNom"].ToString();
                        string cuisinierAdresse = reader["CuisinierAdresse"].ToString();

                        Console.WriteLine($"ID: {idPlat} - Plat: {platNom} - Recette: {recette} - Prix: {prix} €");
                        Console.WriteLine($"Cuisinier: {cuisinierNom} - Adresse: {cuisinierAdresse}");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la récupération des plats : " + ex.Message);
                }
            }
        }

        // Afficher les plats que le cuisinier a ajoutés
        public void DisplayPlatsCuisinier(int idUtilisateur)
        {
            string query = @"
                SELECT p.Id_Plat, p.Nom, p.Type, p.Recette, p.Prix
                FROM Plat p
                WHERE p.Id_Utilisateur = @IdUtilisateur";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@IdUtilisateur", idUtilisateur);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    Console.WriteLine("\nPlats que vous avez ajoutés :");
                    while (reader.Read())
                    {
                        int idPlat = (int)reader["Id_Plat"];
                        string platNom = reader["Nom"].ToString();
                        string recette = reader["Recette"].ToString();
                        decimal prix = (decimal)reader["Prix"];

                        Console.WriteLine($"ID: {idPlat} - Plat: {platNom} - Recette: {recette} - Prix: {prix} €");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la récupération des plats du cuisinier : " + ex.Message);
                }
            }
        }

        // Méthode pour récupérer toutes les nationalités disponibles depuis la base de données
        public List<string> GetAvailableNationalities()
        {
            List<string> nationalities = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Requête pour récupérer toutes les nationalités distinctes
                    string query = "SELECT DISTINCT Nationalite FROM Plat";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    // Ajouter chaque nationalité à la liste
                    while (reader.Read())
                    {
                        string nationality = reader["Nationalite"].ToString();
                        nationalities.Add(nationality);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erreur lors de la récupération des nationalités : " + ex.Message);
                }
            }

            return nationalities;
        }
    }

}
