namespace Rendu2_PSI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AuthService authService = new AuthService();
            CuisinierService cuisinierService = new CuisinierService();
            PlatService platService = new PlatService();
            CommandeService commandeService = new CommandeService();
            AdminService adminService = new AdminService();

            Console.WriteLine("Bienvenue dans LivInParis!");
            Console.WriteLine("Voulez-vous vous inscrire (1) ou vous connecter (2) ?");
            string choix = Console.ReadLine();

            if (choix == "1")
                Inscription(authService, cuisinierService, platService, commandeService);
            else if (choix == "2")
                Connexion(authService, cuisinierService, platService, commandeService, adminService);
            else
                Console.WriteLine("Choix invalide.");
        }

        static void Inscription(AuthService authService, CuisinierService cuisinierService, PlatService platService, CommandeService commandeService)
        {
            Console.WriteLine("Entrez votre nom :");
            string nom = Console.ReadLine();

            Console.WriteLine("Entrez votre prénom :");
            string prénom = Console.ReadLine();

            Console.WriteLine("Entrez votre email :");
            string email = Console.ReadLine();

            Console.WriteLine("Entrez votre mot de passe :");
            string motDePasse = Console.ReadLine();

            Console.WriteLine("Entrez votre adresse :");
            string adresse = Console.ReadLine();

            Console.WriteLine("Entrez la station de métro la plus proche :");
            string stationProche = Console.ReadLine();

            Console.WriteLine("Souhaitez-vous être cuisinier ? (oui/non)");
            bool estCuisinier = Console.ReadLine().ToLower() == "oui";

            string specialite = null;
            if (estCuisinier)
            {
                var specialites = platService.GetAvailableNationalities();
                Console.WriteLine("Choisissez une spécialité existante ou entrez une nouvelle :");
                for (int i = 0; i < specialites.Count; i++)
                    Console.WriteLine((i + 1) + ". " + specialites[i]);

                Console.WriteLine((specialites.Count + 1) + ". Autre (ajouter une nouvelle)");
                string choixSpecialite = Console.ReadLine();

                int index = int.Parse(choixSpecialite);
                if (index > 0 && index <= specialites.Count)
                    specialite = specialites[index - 1];
                else
                {
                    Console.WriteLine("Entrez une nouvelle spécialité :");
                    specialite = Console.ReadLine();
                }
            }

            bool success = authService.Register(nom, prénom, email, motDePasse, adresse, stationProche, estCuisinier, specialite);

            if (success)
            {
                Console.WriteLine("Inscription réussie !");
                if (estCuisinier)
                    ShowPostRegistrationMenu(cuisinierService, platService, commandeService);
                else
                    ShowClientMenu(commandeService, platService);
            }
            else
                Console.WriteLine("Inscription échouée.");
        }

        static void Connexion(AuthService authService, CuisinierService cuisinierService, PlatService platService, CommandeService commandeService, AdminService adminService)
        {
            Console.WriteLine("Entrez votre email :");
            string email = Console.ReadLine();

            Console.WriteLine("Entrez votre mot de passe :");
            string motDePasse = Console.ReadLine();

            bool success = authService.Login(email, motDePasse);

            if (!success)
            {
                Console.WriteLine("Échec de la connexion.");
                return;
            }

            Console.WriteLine("Connexion réussie !");

            bool isAdmin = AuthService.CurrentUserRole == "Admin";
            bool isCuisinier = authService.IsCuisinier(email);

            if (isAdmin)
            {
                ShowAdminMenu(adminService);
                return;
            }

            if (isCuisinier)
            {
                Console.WriteLine("Souhaitez-vous accéder à :");
                Console.WriteLine("1. Espace Client");
                Console.WriteLine("2. Espace Cuisinier");

                string choix = Console.ReadLine();
                if (choix == "1")
                    ShowClientMenu(commandeService, platService);
                else if (choix == "2")
                    ShowCuisinierMenu(cuisinierService, platService);
                else
                    Console.WriteLine("Choix invalide.");
            }
            else
            {
                ShowClientMenu(commandeService, platService);
            }
        }


        static void ShowPostRegistrationMenu(CuisinierService cuisinierService, PlatService platService, CommandeService commandeService)
        {
            Console.WriteLine("Que souhaitez-vous faire ?");
            Console.WriteLine("1. Accéder à mon espace client");
            Console.WriteLine("2. Accéder à mon espace cuisinier");
            string choix = Console.ReadLine();

            if (choix == "1") ShowClientMenu(commandeService, platService);
            else if (choix == "2") ShowCuisinierMenu(cuisinierService, platService);
            else Console.WriteLine("Choix invalide.");
        }

        static void ShowAdminMenu(AdminService adminService)
        {
            while (true)
            {
                Console.WriteLine("\n=== ESPACE ADMINISTRATEUR ===");
                Console.WriteLine("1. Supprimer un utilisateur (client ou cuisinier)");
                Console.WriteLine("2. Modifier les informations d’un utilisateur");
                Console.WriteLine("3. Afficher les clients par ordre alphabétique");
                Console.WriteLine("4. Afficher les clients par rue");
                Console.WriteLine("5. Afficher les clients par montant cumulé des achats");
                Console.WriteLine("6. Nombre de livraisons par cuisinier");
                Console.WriteLine("7. Commandes dans une période donnée");
                Console.WriteLine("8. Moyenne des prix des commandes");
                Console.WriteLine("9. Liste des commandes d’un client par période/nationalité");
                Console.WriteLine("0. Quitter l’espace admin");

                Console.Write("Votre choix : ");
                string choix = Console.ReadLine();

                switch (choix)
                {
                    case "1":
                        adminService.DeleteUser();
                        break;
                    case "2":
                        adminService.ModifyUser();
                        break;
                    case "3":
                        adminService.DisplayClientsAlphabetical();
                        break;
                    case "4":
                        adminService.DisplayClientsByStreet();
                        break;
                    case "5":
                        adminService.DisplayClientsByTotalSpent();
                        break;
                    case "6":
                        adminService.ShowDeliveriesPerCuisinier();
                        break;
                    case "7":
                        adminService.ShowOrdersInPeriod();
                        break;
                    case "8":
                        adminService.ShowAverageOrderPrice();
                        break;
                    case "9":
                        adminService.ShowClientOrdersFiltered();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }


        static void ShowClientMenu(CommandeService commandeService, PlatService platService)
        {
            while (true)
            {
                Console.WriteLine("\nEspace Client");
                Console.WriteLine("1. Filtrer les plats disponibles");
                Console.WriteLine("2. Passer une commande");
                Console.WriteLine("3. Se déconnecter");
                string choix = Console.ReadLine();

                if (choix == "1")
                {
                    var nationalites = platService.GetAvailableNationalities();
                    Console.WriteLine("Choisissez une nationalité :");
                    for (int i = 0; i < nationalites.Count; i++)
                        Console.WriteLine((i + 1) + ". " + nationalites[i]);

                    string choixNat = Console.ReadLine();
                    int idx = int.Parse(choixNat);
                    string nationalite = nationalites[idx - 1];

                    Console.WriteLine("Entrez le type de plat (entrée, plat principal, dessert) :");
                    string type = Console.ReadLine();

                    platService.DisplayPlatsFiltre(nationalite, type);
                }
                else if (choix == "2")
                {
                    var nationalites = platService.GetAvailableNationalities();
                    Console.WriteLine("Choisissez une nationalité :");
                    for (int i = 0; i < nationalites.Count; i++)
                        Console.WriteLine((i + 1) + ". " + nationalites[i]);

                    string choixNat = Console.ReadLine();
                    int idx = int.Parse(choixNat);
                    string nationalite = nationalites[idx - 1];

                    Console.WriteLine("Entrez le type de plat (entrée, plat principal, dessert) :");
                    string type = Console.ReadLine();

                    platService.DisplayPlatsFiltre(nationalite, type);
                    Console.WriteLine("Entrez le nombre de plats différents à commander :");
                    int n = int.Parse(Console.ReadLine());

                    int[] ids = new int[n];
                    int[] qtes = new int[n];

                    for (int i = 0; i < n; i++)
                    {
                        Console.WriteLine("Entrez l'ID du plat " + (i + 1) + " :");
                        ids[i] = int.Parse(Console.ReadLine());

                        Console.WriteLine("Quantité :");
                        qtes[i] = int.Parse(Console.ReadLine());
                    }

                    commandeService.PassOrder(ids, qtes);
                }
                else if (choix == "3")
                    break;
                else
                    Console.WriteLine("Choix invalide.");
            }
        }

        static void ShowCuisinierMenu(CuisinierService cuisinierService, PlatService platService)
        {
            int idUtilisateur = AuthService.CurrentUserId;
            while (true)
            {
                Console.WriteLine("\nEspace Cuisinier");
                Console.WriteLine("1. Voir les plats disponibles");
                Console.WriteLine("2. Ajouter un plat");
                Console.WriteLine("3. Se déconnecter");
                string choix = Console.ReadLine();

                if (choix == "1")
                    platService.DisplayPlatsCuisinier(idUtilisateur);
                else if (choix == "2")
                    cuisinierService.AddPlat(idUtilisateur);
                else if (choix == "3")
                    break;
                else
                    Console.WriteLine("Choix invalide.");
            }
        }
    }
}
}
