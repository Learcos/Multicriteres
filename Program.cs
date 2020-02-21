using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Multicritères {
    class Program {
        static readonly string exemple1 = @"./exemple1.txt";
        static readonly string exemple2 = @"./exemple2.txt";
        static readonly string exemple3 = @"./exemple3.txt";

        public static void afficheMatrice(int[, ] matrice, int nbColonnes, int nbCriteres) {
            for (int i = 0; i < nbColonnes; i++) {
                for (int j = 0; j < nbCriteres; j++) {
                    Console.Write(matrice[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void afficheMatrice(double[, ] matrice, int nbColonnes, int nbCriteres) {
            for (int i = 0; i < nbColonnes; i++) {
                for (int j = 0; j < nbCriteres; j++) {
                    Console.Write(String.Format("{0:0.##}", matrice[i, j]) + "  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void afficheDistillation(List<List<int>> distillation) {
            int compteur = 1;
            foreach (List<int> liste in distillation) {
                if (liste.Count > 1) {
                    int i = 1;
                    foreach (int element in liste) {
                        Console.Write("P" + element);
                        if (i < liste.Count)Console.Write(", ");
                        i++;
                    }
                } else {
                    Console.Write("P" + liste[0]);
                }
                if (compteur < distillation.Count)Console.Write(" => ");
                compteur++;
            }
            Console.WriteLine();
        }

        static void Main(string[] args) {
            //on peut choisir exemple1, 2 ou 3
            //executionProgramme(exemple3);

            Solveur s = new Solveur(exemple3, 4);

            Console.WriteLine("Nb projets: " + s.nbProjets);
            Console.WriteLine("Nb criteres: " + s.nbCriteres);

            afficheMatrice(s.matricePerformance, s.nbProjets, s.nbCriteres);
            afficheMatrice(s.matriceSeuils, s.nbSeuils, s.nbCriteres);

            afficheMatrice(s.matriceConcordance, s.nbProjets, s.nbProjets);
            afficheMatrice(s.matriceDiscordancee, s.nbProjets, s.nbProjets);
            afficheMatrice(s.matriceCredibilite, s.nbProjets, s.nbProjets);
            afficheMatrice(s.matriceSurclassement, s.nbProjets, s.nbProjets);

            foreach (KeyValuePair<int, int> element in s.qualifs) {
                Console.WriteLine("Projet: {0}, valeur de qualif: {1}", element.Key, element.Value);
            }

            Console.WriteLine();

            Console.WriteLine("Qualif min: " + s.qualifsMin);
            Console.WriteLine("Qualif max: " + s.qualifMax);
            Console.WriteLine();

            Console.Write("Distillation ascendante: ");
            afficheDistillation(s.distillationAscendante);

            Console.Write("Distillation descendante: ");
            afficheDistillation(s.distillationDescendante);

            Console.Write("Distillation: ");
            afficheDistillation(s.disilation);

        }
    }
}