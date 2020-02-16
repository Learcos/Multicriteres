using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

        public static double[, ] calculeMatriceConcordance (int nbProjets, int nbCriteres) {
            int[, ] matricePerf = initialiseMatricePerformance ();
            int[, ] matriceSeuils = initialiseMatriceSeuils ();

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
                            concordance = (double)(preferenceK + perfIK - perfJK) / (preferenceK - indifferenceK);
                        }
                        sommeConcordances += importanceK * concordance;
                    }
                    sommeConcordances /= sommeImportances;
                    matriceConcordance[i, j] = sommeConcordances;
                }
            }
            return matriceConcordance;
        }

        public static double[, ] calculeMatriceDiscordance (int nbProjets, int nbCriteres) {
            int[, ] matricePerf = initialiseMatricePerformance ();
            int[, ] matriceSeuils = initialiseMatriceSeuils ();

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
                            discordance = (double)(perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        sommeDiscordances += importanceK * discordance;
                    }
                    sommeDiscordances /= sommeImportances;
                    matriceDiscordance[i, j] = sommeDiscordances;
                }
            }
            return matriceDiscordance;
        }

        public static double[, ] calculeMatriceCredibilite (int nbProjets, int nbCriteres){
            double[, ] matriceConcordance = calculeMatriceConcordance(nbProjets, nbCriteres);
            double[, ] matriceDiscordance = calculeMatriceDiscordance(nbProjets, nbCriteres);

            //pour calculer l'ensemble J pour chaque deux projets
            int[, ] matricePerf = initialiseMatricePerformance ();
            int[, ] matriceSeuils = initialiseMatriceSeuils ();

            double[, ] matriceCredibilite = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++) {
                for (int j = 0; j < nbProjets; j++) {
                    List<double> ensembleJ = new List<double>();
                    double discordance = 0.00;
                    //calcul de l'ensemble des critères discordants
                    for(int k = 0; k < nbCriteres; k++){

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
                            discordance = (double)(perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        if(discordance > matriceConcordance[i,j]){
                            ensembleJ.Add(discordance);
                        }
                    }
                    if(ensembleJ.Count == 0){
                        matriceCredibilite[i,j] = matriceConcordance[i,j];
                    }
                    else{
                        double cred = 1.0;
                        foreach (double disc in ensembleJ)
                        {
                            cred *= (double)((1-disc)/(1-matriceConcordance[i,j]));
                        }
                        cred *= matriceConcordance[i,j];
                        matriceCredibilite[i,j] = cred;
                    }
                }
            }
            return matriceCredibilite;
        }

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
                    Console.Write (String.Format("{0:0.##}", matrice[i, j]) + "  ");
                }
                Console.WriteLine ();
            }
        }

        static void Main (string[] args) {
            int[, ] matricePerf = initialiseMatricePerformance ();
            int[, ] matriceSeuils = initialiseMatriceSeuils ();
            double[, ] matriceConcordance = calculeMatriceConcordance (5, 5);
            double[, ] matriceDiscordance = calculeMatriceDiscordance (5, 5);
            double[, ] matriceCredibilite = calculeMatriceCredibilite(5,5);
            afficheMatrice (matricePerf, 5, 5);
            Console.WriteLine ();
            afficheMatrice (matriceSeuils, 4, 5);
            Console.WriteLine ();
            afficheMatrice (matriceConcordance, 5, 5);
            Console.WriteLine ();
            afficheMatrice(matriceDiscordance, 5, 5);
            Console.WriteLine ();
            afficheMatrice(matriceCredibilite, 5, 5);
        }
    }
}