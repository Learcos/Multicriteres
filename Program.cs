using System;
using System.Collections.Generic;
using System.Linq;

namespace Multicritères {
    class Program {
        public static int[, ] initialiseMatricePerformance () {
            const int nbCriteres = 5; //à changer: valeur à lire dans le fichier txt
            const int nbProjets = 5; //à changer: valeur à lire dans le fichier txt
            /*
            * int[,] matricePerf = new int[nbCriteres, nbProjets];
            * remplir matrice avec valeurs du fichier
            for(int i = 0; i < nbProjets; i++){
                for(int j = 0; j < nbCriteres; j++){
                    matricePerf[i, j] = valeur lue dans le fichier
                }
            }
            */
            int[, ] matricePerf = new int[nbProjets, nbCriteres] { {-14, 90, 0, 40, 100 }, { 129, 100, 0, 0, 0 }, {-10, 50, 0, 10, 100 }, { 44, 90, 0, 5, 20 }, {-14, 100, 0, 20, 40 }
            };
            return matricePerf;
        }

        public static int[, ] initialiseMatriceSeuils () {
            const int nbCriteres = 5; //à changer: valeur à lire dans le fichier txt
            const int nbSeuils = 4; //importance, indifférence, préférence, veto
            /*
            * int[,] matriceSeuils = new int[nbCriteres, nbProjets];
            * remplir matrice avec valeurs du fichier
            for(int i = 0; i < nbProjets; i++){
                for(int j = 0; j < nbCriteres; j++){
                    matriceSeuil[i, j] = valeur lue dans le fichier;
                }
            }
            */
            int[, ] matriceSeuils = new int[nbSeuils, nbCriteres] { { 1, 1, 1, 1, 1 }, { 25, 16, 0, 12, 10 }, { 50, 24, 1, 24, 20 }, { 100, 60, 2, 48, 90 }
            };
            return matriceSeuils;
        }

        public static List<List<int>> initialiseListeProjets (int nbProjets) {
            List<List<int>> projets = new List<List<int>> ();
            for (int i = 0; i < nbProjets; i++) {
                List<int> rangProjet = new List<int> ();
                rangProjet.Add (i);
                projets.Add (rangProjet);
            }
            return projets;
        }

        public static double[, ] calculeMatriceConcordance (int[, ] matricePerf, int[, ] matriceSeuils, int nbProjets, int nbCriteres) {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < nbCriteres; a++) {
                sommeImportances += matriceSeuils[0, a];
            }

            double[, ] matriceConcordance = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++) {
                for (int j = 0; j < nbProjets; j++) {

                    double sommeConcordances = 0.00;
                    double concordance = 0.00;

                    //on somme tous les indices de concordance selon les critères
                    for (int k = 0; k < nbCriteres; k++) {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int indifferenceK = matriceSeuils[1, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (i == j) {
                            concordance = 1;
                        } else if (perfIK >= perfJK - indifferenceK) {
                            concordance = 1;
                        } else if (perfIK <= perfJK - preferenceK) {
                            concordance = 0;
                        } else {
                            concordance = (double) (preferenceK + perfIK - perfJK) / (preferenceK - indifferenceK);
                        }
                        sommeConcordances += importanceK * concordance;
                    }
                    sommeConcordances /= sommeImportances;
                    matriceConcordance[i, j] = sommeConcordances;
                }
            }
            return matriceConcordance;
        }

        public static double[, ] calculeMatriceDiscordance (int[, ] matricePerf, int[, ] matriceSeuils, int nbProjets, int nbCriteres) {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < nbCriteres; a++) {
                sommeImportances += matriceSeuils[0, a];
            }

            double[, ] matriceDiscordance = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++) {
                for (int j = 0; j < nbProjets; j++) {

                    double sommeDiscordances = 0.00;
                    double discordance = 0.00;

                    //on somme tous les indices de discordance selon les critères
                    for (int k = 0; k < nbCriteres; k++) {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int vetoK = matriceSeuils[3, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK) {
                            discordance = 0;
                        } else if (perfIK <= perfJK - vetoK) {
                            discordance = 1;
                        } else {
                            discordance = (double) (perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        sommeDiscordances += importanceK * discordance;
                    }
                    sommeDiscordances /= sommeImportances;
                    matriceDiscordance[i, j] = sommeDiscordances;
                }
            }
            return matriceDiscordance;
        }

        public static double[, ] calculeMatriceCredibilite (double[, ] matriceConcordance, double[, ] matriceDiscordance, int[, ] matricePerf, int[, ] matriceSeuils, int nbProjets, int nbCriteres) {
            double[, ] matriceCredibilite = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++) {
                for (int j = 0; j < nbProjets; j++) {
                    List<double> ensembleJ = new List<double> ();
                    double discordance = 0.00;
                    //calcul de l'ensemble des critères discordants
                    for (int k = 0; k < nbCriteres; k++) {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int vetoK = matriceSeuils[3, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK) {
                            discordance = 0;
                        } else if (perfIK <= perfJK - vetoK) {
                            discordance = 1;
                        } else {
                            discordance = (double) (perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        if (discordance > matriceConcordance[i, j]) {
                            ensembleJ.Add (discordance);
                        }
                    }
                    if (ensembleJ.Count == 0) {
                        matriceCredibilite[i, j] = matriceConcordance[i, j];
                    } else {
                        double cred = 1.0;
                        foreach (double disc in ensembleJ) {
                            cred *= (double) ((1 - disc) / (1 - matriceConcordance[i, j]));
                        }
                        cred *= matriceConcordance[i, j];
                        matriceCredibilite[i, j] = cred;
                    }
                }
            }
            return matriceCredibilite;
        }

        public static double calculeMaxMatrice (double[, ] matrice) {
            double max = 0.00;
            foreach (double valeur in matrice) {
                if (valeur == 1) return valeur;
                if (valeur > max) max = valeur;
            }
            return max;
        }

        //matrice surclassement: matrice d'int car 0 ou 1
        public static int[, ] calculeMatriceSurclassement (double[, ] matriceCredibilite, int nbProjets, double alpha = 0.3, double beta = -0.15) {
            double maxCred = calculeMaxMatrice (matriceCredibilite);
            double seuil = alpha + maxCred * beta;
            double niveauCoupe = maxCred - seuil;
            //double diff;
            int[, ] matriceSurclassement = new int[nbProjets, nbProjets];
            for (int i = 0; i < nbProjets; i++) {
                for (int j = 0; j < nbProjets; j++) {
                    //diff = Math.Abs (matriceCredibilite[i, j] - matriceCredibilite [j, i]);
                    if (matriceCredibilite[i, j] >= niveauCoupe) {
                        matriceSurclassement[i, j] = 1;
                    } else {
                        matriceSurclassement[i, j] = 0;
                    }
                }
            }
            return matriceSurclassement;
        }

        public static Dictionary<int, int> calculeQualifs (int[, ] matriceSurclassement, int nbProjets, List<int> rangsProjetsDejaClasses) {
            Dictionary<int, int> qualifs = new Dictionary<int, int> ();
            for (int i = 0; i < nbProjets; i++) {
                int sommeLigne = 0, sommeColonne = 0;
                if (!(rangsProjetsDejaClasses.Contains (i))) {
                    for (int j = 0; j < nbProjets; j++) {
                        if (!(rangsProjetsDejaClasses.Contains (j))) {
                            sommeLigne += matriceSurclassement[i, j];
                            sommeColonne += matriceSurclassement[j, i];
                        }
                    }
                    qualifs.Add (i, sommeLigne - sommeColonne);
                }
            }
            return qualifs;
        }

        public static int calculeQualifMinMax (Dictionary<int, int> qualifs, Func<int, int, int> function) {
            int qualifMinMax = 0;
            foreach (KeyValuePair<int, int> item in qualifs) {
                qualifMinMax = function (qualifMinMax, item.Value);
            }
            return qualifMinMax;
        }

        /*public static List<List<int>> distillationAscendante (List<List<int>> projets, Dictionary<int, int> qualifs, int[, ] matriceSurclassement, int nbProjets) {
            List<List<int>> projetsClasses = new List<List<int>> ();
            List<int> rangsProjetsClasses = new List<int> ();
            List<List<int>> projetsResiduels = new List<List<int>> ();
            List<int> rangsProjetsResiduels = new List<int> ();
            Dictionary<int, int> newQualifs = new Dictionary<int, int> ();

            int qualifMin = calculeQualifMinMax (qualifs, Math.Min);

            //classement des projets de + faible qualification dans rangsProjetsClasses
            foreach (KeyValuePair<int, int> element in qualifs) {
                if (element.Value == qualifMin) {
                    rangsProjetsClasses.Add (element.Key);
                } else {
                    rangsProjetsResiduels.Add (element.Key);
                }
            }
            if (rangsProjetsClasses.Count > 0) {
                //si 1 seul projet est à la qualification min
                if (rangsProjetsClasses.Count == 1) {
                    //on le classe
                    projetsClasses.Add (rangsProjetsClasses);
                    foreach (int item in rangsProjetsResiduels) {
                        List<int> listTemp = new List<int> ();
                        listTemp.Add (item);
                        projetsResiduels.Add (listTemp);
                    }
                    //on recalcule les qualifications en évitant les projets déjà classés
                    newQualifs = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsClasses);
                    //on fait la même chose pour les projets residuels
                    do {
                        distillationAscendante (projetsResiduels, newQualifs, matriceSurclassement, nbProjets);
                    } while (rangsProjetsClasses.Count < nbProjets);
                }
                //si plusieurs projets ont la qualif min, on cherche à les distiller entre eux
                else {
                    //on recalcule les qualifications pour l'ensemble des projetsClasses
                    //càd en évitant les projetsResiduels
                    Dictionary<int, int> newQualifs2 = new Dictionary<int, int> ();
                    Dictionary<int, int> newQualifs3 = new Dictionary<int, int> ();
                    newQualifs2 = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsResiduels);
                    //on cherche à savoir si toutes les valeurs de qualification sont les mêmes
                    //auquel cas on ne distille plus les projets
                    int firstQualifValue = newQualifs2.First ().Value;
                    bool valeursToutesEgales = true;
                    foreach (KeyValuePair<int, int> element in newQualifs2) {
                        if (element.Value != firstQualifValue) {
                            valeursToutesEgales = false;
                            break;
                        }
                    }
                    if (valeursToutesEgales) {
                        projetsClasses.Add (rangsProjetsClasses);
                        foreach (int item in rangsProjetsResiduels) {
                            List<int> listTemp = new List<int> ();
                            listTemp.Add (item);
                            projetsResiduels.Add (listTemp);
                        }
                        //on recalcule les qualifications en évitant les projets déjà classés
                        newQualifs3 = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsClasses);
                        //on fait la même chose pour les projets residuels
                        do {
                            distillationAscendante (projetsResiduels, newQualifs3, matriceSurclassement, nbProjets);
                        } while (!valeursToutesEgales && rangsProjetsClasses.Count > 1);
                    } else {
                        do {
                            distillationAscendante (projetsClasses, newQualifs2, matriceSurclassement, nbProjets);
                        } while (!valeursToutesEgales && rangsProjetsClasses.Count > 1);
                    }
                }
            }
            //projets <- projets - projetsClasses
            projets = projetsClasses;
        }*/

        public static int[, ] supprimeElementsMatrice (int[, ] matrice, int matriceSize, int rank) {
            int[, ] newMatrice = new int[matriceSize - 1, matriceSize - 1];
            int ligne = 0;
            int colonne = 0;

            for (int i = 0; i < matriceSize; i++) {
                if (i != rank) {
                    if (i < rank) ligne = i;
                    else ligne = i - 1;
                    for (int j = 0; j < matriceSize; j++) {
                        if (j != rank) {
                            if (j < rank) colonne = j;
                            else colonne = j - 1;
                            newMatrice[ligne, colonne] = matrice[i, j];
                        }
                    }
                }
            }
            return newMatrice;
        }

        public static double[, ] supprimeElementsMatrice (double[, ] matrice, int matriceSize, List<int> rangsA_Delete) {
            double[, ] newMatrice = new double[matriceSize, matriceSize];
            //tri en ordre croissant puis reverse = ordre décroissant
            rangsA_Delete.Sort ();
            rangsA_Delete.Reverse ();
            foreach (int rank in rangsA_Delete) {
                for (int i = 0; i < matriceSize; i++) {
                    if (i != rank) {
                        for (int j = 0; j < matriceSize; j++) {
                            if (j != rank) {
                                newMatrice[i, j] = matrice[i, j];
                            }
                        }
                    }
                }
            }
            return newMatrice;
        }

        /*public static List<List<int>> distillationAscendante2 (Dictionary<int, int> qualifs, int[, ] matriceSurclassement, int nbProjets) {
                List<List<int>> projetsClasses = new List<List<int>> ();
                List<int> rangsProjetsClasses = new List<int> ();
                List<List<int>> projetsResiduels = new List<List<int>> ();
                List<int> rangsProjetsResiduels = new List<int> ();

                Dictionary<int, int> newQualifs = new Dictionary<int, int> ();
                int qualifMin = calculeQualifMinMax (qualifs, Math.Min);

                //classement des projets de + faible qualification dans rangsProjetsClasses
                foreach (KeyValuePair<int, int> element in qualifs) {
                    if (element.Value == qualifMin) {
                        rangsProjetsClasses.Add (element.Key);
                    } else {
                        rangsProjetsResiduels.Add (element.Key);
                    }
                }
                if (rangsProjetsClasses.Count > 0) {
                    //si 1 seul projet est à la qualification min
                    if (rangsProjetsClasses.Count == 1) {
                        //on le classe
                        projetsClasses.Add (rangsProjetsClasses);
                        foreach (int item in rangsProjetsResiduels) {
                            List<int> listTemp = new List<int> ();
                            listTemp.Add (item);
                            projetsResiduels.Add (listTemp);
                        }
                        //on recalcule les qualifications en évitant les projets déjà classés
                        newQualifs = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsClasses);
                        //on fait la même chose pour les projets residuels
                        do {
                            distillationAscendante (projetsResiduels, newQualifs, matriceSurclassement, nbProjets);
                        } while (rangsProjetsClasses.Count < nbProjets);
                    }
                    //si plusieurs projets ont la qualif min, on cherche à les distiller entre eux
                    else {
                        //on recalcule les qualifications pour l'ensemble des projetsClasses
                        //càd en évitant les projetsResiduels
                        Dictionary<int, int> newQualifs2 = new Dictionary<int, int> ();
                        Dictionary<int, int> newQualifs3 = new Dictionary<int, int> ();
                        newQualifs2 = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsResiduels);
                        //on cherche à savoir si toutes les valeurs de qualification sont les mêmes
                        //auquel cas on ne distille plus les projets
                        int firstQualifValue = newQualifs2.First ().Value;
                        bool valeursToutesEgales = true;
                        foreach (KeyValuePair<int, int> element in newQualifs2) {
                            if (element.Value != firstQualifValue) {
                                valeursToutesEgales = false;
                                break;
                            }
                        }
                        if (valeursToutesEgales) {
                            projetsClasses.Add (rangsProjetsClasses);
                            foreach (int item in rangsProjetsResiduels) {
                                List<int> listTemp = new List<int> ();
                                listTemp.Add (item);
                                projetsResiduels.Add (listTemp);
                            }
                            //on recalcule les qualifications en évitant les projets déjà classés
                            newQualifs3 = calculeQualifs (matriceSurclassement, nbProjets, rangsProjetsClasses);
                            //on fait la même chose pour les projets residuels
                            do {
                                distillationAscendante (projetsResiduels, newQualifs3, matriceSurclassement, nbProjets);
                            } while (!valeursToutesEgales && rangsProjetsClasses.Count > 1);
                        } else {
                            do {
                                distillationAscendante (projetsClasses, newQualifs2, matriceSurclassement, nbProjets);
                            } while (!valeursToutesEgales && rangsProjetsClasses.Count > 1);
                        }
                    }
                }
                //projets <- projets - projetsClasses
                projets = projetsClasses;
            }*/

        public static void afficheMatrice (int[, ] matrice, int nbColonnes, int nbCriteres) {
            for (int i = 0; i < nbColonnes; i++) {
                for (int j = 0; j < nbCriteres; j++) {
                    Console.Write (matrice[i, j] + " ");
                }
                Console.WriteLine ();
            }
        }

        public static void afficheMatrice (double[, ] matrice, int nbColonnes, int nbCriteres) {
            for (int i = 0; i < nbColonnes; i++) {
                for (int j = 0; j < nbCriteres; j++) {
                    Console.Write (String.Format ("{0:0.##}", matrice[i, j]) + "  ");
                }
                Console.WriteLine ();
            }
        }

        static void Main (string[] args) {
            int nbProjets = 5, nbCriteres = 5, nbSeuils = 4;

            int[, ] matricePerf = initialiseMatricePerformance ();
            afficheMatrice (matricePerf, nbProjets, nbCriteres);
            Console.WriteLine ();

            int[, ] matriceSeuils = initialiseMatriceSeuils ();
            afficheMatrice (matriceSeuils, nbSeuils, nbCriteres);
            Console.WriteLine ();

            double[, ] matriceConcordance = calculeMatriceConcordance (matricePerf, matriceSeuils, nbProjets, nbCriteres);
            afficheMatrice (matriceConcordance, nbProjets, nbProjets);
            Console.WriteLine ();

            double[, ] matriceDiscordance = calculeMatriceDiscordance (matricePerf, matriceSeuils, nbProjets, nbCriteres);
            afficheMatrice (matriceDiscordance, nbProjets, nbProjets);
            Console.WriteLine ();

            double[, ] matriceCredibilite = calculeMatriceCredibilite (matriceConcordance, matriceDiscordance, matricePerf, matriceSeuils, nbProjets, nbProjets);
            afficheMatrice (matriceCredibilite, nbProjets, nbProjets);
            Console.WriteLine ();

            int[, ] matriceSurclassement = calculeMatriceSurclassement (matriceCredibilite, nbProjets);
            afficheMatrice (matriceSurclassement, nbProjets, nbProjets);
            Console.WriteLine ();

            List<int> dejaClasses = new List<int> ();
            Dictionary<int, int> qualifs = calculeQualifs (matriceSurclassement, nbProjets, dejaClasses);
            foreach (KeyValuePair<int, int> element in qualifs) {
                Console.WriteLine ("Projet: {0}, valeur de qualif: {1}", element.Key, element.Value);
            }
            Console.WriteLine ();
            Console.WriteLine ("Qualif min: " + calculeQualifMinMax (qualifs, Math.Min));
            Console.WriteLine ("Qualif max: " + calculeQualifMinMax (qualifs, Math.Max));

            /*List<List<int>> projets = initialiseListeProjets (nbProjets);
            distillationAscendante (projets, qualifs, matriceSurclassement, nbProjets);
            foreach (List<int> liste in projets) {
                if (liste.Count > 1) {
                    foreach (int element in liste) {
                        Console.Write ("P" + element + ", ");
                    }
                } else {
                    Console.Write ("P" + liste[0]);
                }
                Console.Write (" => ");
            }*/
            Console.WriteLine ();
            int[, ] newMatriceSurclassement = supprimeElementsMatrice (matriceSurclassement, nbProjets, 1);
            afficheMatrice (newMatriceSurclassement, nbProjets - 1, nbProjets - 1);
            Console.WriteLine ();
        }
    }
}