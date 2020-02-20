using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Multicritères
{
    class Program
    {
        const int nbSeuils = 4;
        static int nbProjets;
        static int nbCriteres;
        static int[,] matricePerf;
        static int[,] matriceSeuils;
        static readonly string rootFolder = @"C:\Users\lucaa\OneDrive\Documents\M2\Multicritères\Multicriteres";
        static readonly string exemple1 = @"C:\Users\lucaa\OneDrive\Documents\M2\Multicritères\Multicriteres\exemple1.txt";
        static readonly string exemple2 = @"C:\Users\lucaa\OneDrive\Documents\M2\Multicritères\Multicriteres\exemple2.txt";
        static readonly string exemple3 = @"C:\Users\lucaa\OneDrive\Documents\M2\Multicritères\Multicriteres\exemple3.txt";

        public static void initialisation(string chemin)
        {
            if (File.Exists(chemin))
            {
                bool arriveA_MatricePerf = false;
                bool arriveA_MatriceSeuils = false;
                int ligneMatrice = 0;
                // Read a text file line by line.
                string[] lines = File.ReadAllLines(chemin);
                foreach (string line in lines)
                {
                    if (line.StartsWith("NbProjets"))
                    {
                        string line2;
                        line2 = line.Remove(0, line.IndexOf(":") + 2);
                        nbProjets = int.Parse(line2);
                        matricePerf = new int[nbProjets, nbCriteres];
                    }
                    if (line.StartsWith("NbCriteres"))
                    {
                        string line2;
                        line2 = line.Remove(0, line.IndexOf(":") + 2);
                        nbCriteres = int.Parse(line2);
                        matriceSeuils = new int[nbSeuils, nbCriteres];
                    }
                    if (arriveA_MatricePerf)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.Contains("MatriceSeuils"))
                        {
                            string[] values = line.Split(" ");
                            int colonneMatrice = 0;
                            foreach (string value in values)
                            {
                                matricePerf[ligneMatrice, colonneMatrice] = int.Parse(value);
                                colonneMatrice++;
                            }
                            ligneMatrice++;
                        }

                    }
                    if (arriveA_MatriceSeuils)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            string[] values = line.Split(" ");
                            int colonneMatrice = 0;
                            foreach (string value in values)
                            {
                                matriceSeuils[ligneMatrice, colonneMatrice] = int.Parse(value);
                                colonneMatrice++;
                            }
                            ligneMatrice++;
                        }
                    }
                    if (line.Contains("MatricePerformance"))
                    {
                        arriveA_MatricePerf = true;
                    }
                    if (line.Contains("MatriceSeuils"))
                    {
                        arriveA_MatricePerf = false;
                        arriveA_MatriceSeuils = true;
                        ligneMatrice = 0;
                    }
                }
            }
        }

        public static double[,] calculeMatriceConcordance(int[,] matricePerf, int[,] matriceSeuils, int nbProjets, int nbCriteres)
        {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < nbCriteres; a++)
            {
                sommeImportances += matriceSeuils[0, a];
            }

            double[,] matriceConcordance = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++)
            {
                for (int j = 0; j < nbProjets; j++)
                {

                    double sommeConcordances = 0.00;
                    double concordance = 0.00;

                    //on somme tous les indices de concordance selon les critères
                    for (int k = 0; k < nbCriteres; k++)
                    {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int indifferenceK = matriceSeuils[1, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (i == j)
                        {
                            concordance = 1;
                        }
                        else if (perfIK >= perfJK - indifferenceK)
                        {
                            concordance = 1;
                        }
                        else if (perfIK <= perfJK - preferenceK)
                        {
                            concordance = 0;
                        }
                        else
                        {
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

        public static double[,] calculeMatriceDiscordance(int[,] matricePerf, int[,] matriceSeuils, int nbProjets, int nbCriteres)
        {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < nbCriteres; a++)
            {
                sommeImportances += matriceSeuils[0, a];
            }

            double[,] matriceDiscordance = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++)
            {
                for (int j = 0; j < nbProjets; j++)
                {

                    double sommeDiscordances = 0.00;
                    double discordance = 0.00;

                    //on somme tous les indices de discordance selon les critères
                    for (int k = 0; k < nbCriteres; k++)
                    {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int vetoK = matriceSeuils[3, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK)
                        {
                            discordance = 0;
                        }
                        else if (perfIK <= perfJK - vetoK)
                        {
                            discordance = 1;
                        }
                        else
                        {
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

        public static double[,] calculeMatriceCredibilite(double[,] matriceConcordance, double[,] matriceDiscordance, int[,] matricePerf, int[,] matriceSeuils, int nbProjets, int nbCriteres)
        {
            double[,] matriceCredibilite = new double[nbProjets, nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < nbProjets; i++)
            {
                for (int j = 0; j < nbProjets; j++)
                {
                    List<double> ensembleJ = new List<double>();
                    double discordance = 0.00;
                    //calcul de l'ensemble des critères discordants
                    for (int k = 0; k < nbCriteres; k++)
                    {

                        int perfIK = matricePerf[i, k];
                        int perfJK = matricePerf[j, k];
                        int vetoK = matriceSeuils[3, k];
                        int preferenceK = matriceSeuils[2, k];
                        int importanceK = matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK)
                        {
                            discordance = 0;
                        }
                        else if (perfIK <= perfJK - vetoK)
                        {
                            discordance = 1;
                        }
                        else
                        {
                            discordance = (double)(perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        if (discordance > matriceConcordance[i, j])
                        {
                            ensembleJ.Add(discordance);
                        }
                    }
                    if (ensembleJ.Count == 0)
                    {
                        matriceCredibilite[i, j] = matriceConcordance[i, j];
                    }
                    else
                    {
                        double cred = 1.0;
                        foreach (double disc in ensembleJ)
                        {
                            cred *= (double)((1 - disc) / (1 - matriceConcordance[i, j]));
                        }
                        cred *= matriceConcordance[i, j];
                        matriceCredibilite[i, j] = cred;
                    }
                }
            }
            return matriceCredibilite;
        }

        public static double calculeMaxMatrice(double[,] matrice)
        {
            double max = 0.00;
            foreach (double valeur in matrice)
            {
                if (valeur == 1) return valeur;
                if (valeur > max) max = valeur;
            }
            return max;
        }

        //matrice surclassement: matrice d'int car 0 ou 1
        public static int[,] calculeMatriceSurclassement(double[,] matriceCredibilite, int nbProjets, double alpha = 0.3, double beta = -0.15)
        {
            double maxCred = calculeMaxMatrice(matriceCredibilite);
            double seuil = alpha + maxCred * beta;
            double niveauCoupe = maxCred - seuil;
            //double diff;
            int[,] matriceSurclassement = new int[nbProjets, nbProjets];
            for (int i = 0; i < nbProjets; i++)
            {
                for (int j = 0; j < nbProjets; j++)
                {
                    //diff = Math.Abs (matriceCredibilite[i, j] - matriceCredibilite [j, i]);
                    if (matriceCredibilite[i, j] >= niveauCoupe)
                    {
                        matriceSurclassement[i, j] = 1;
                    }
                    else
                    {
                        matriceSurclassement[i, j] = 0;
                    }
                }
            }
            return matriceSurclassement;
        }

        public static Dictionary<int, int> calculeQualifs(int[,] matriceSurclassement, int nbProjets, List<int> rangsProjetsDejaClasses)
        {
            Dictionary<int, int> qualifs = new Dictionary<int, int>();
            for (int i = 0; i < nbProjets; i++)
            {
                int sommeLigne = 0, sommeColonne = 0;
                if (!(rangsProjetsDejaClasses.Contains(i)))
                {
                    for (int j = 0; j < nbProjets; j++)
                    {
                        if (!(rangsProjetsDejaClasses.Contains(j)))
                        {
                            sommeLigne += matriceSurclassement[i, j];
                            sommeColonne += matriceSurclassement[j, i];
                        }
                    }
                    qualifs.Add(i, sommeLigne - sommeColonne);
                }
            }
            return qualifs;
        }

        public static int calculeQualifMinMax(Dictionary<int, int> qualifs, Func<int, int, int> function)
        {
            int qualifMinMax = 0;
            foreach (KeyValuePair<int, int> item in qualifs)
            {
                qualifMinMax = function(qualifMinMax, item.Value);
            }
            return qualifMinMax;
        }

        public static void distillationAscDesc(int[,] matriceSurclassement, int nbProjets, List<List<int>> projetsClasses, List<int> rangsA_EviterMode0, List<int> rangsA_EviterMode1, int nbRangsTotalMode1, int nbRangsActuelMode1, int modeDeTravail, Func<int, int, int> function)
        {
            List<int> rangsProjetsA_Classes = new List<int>();
            Dictionary<int, int> qualifs = new Dictionary<int, int>();
            switch (modeDeTravail)
            {
                case 0:
                    qualifs = calculeQualifs(matriceSurclassement, nbProjets, rangsA_EviterMode0);
                    break;
                case 1:
                    qualifs = calculeQualifs(matriceSurclassement, nbProjets, rangsA_EviterMode1);
                    break;
            }
            int qualifMin = calculeQualifMinMax(qualifs, function);

            //classement des projets de + faible qualification dans rangsProjetsA_Classes
            foreach (KeyValuePair<int, int> element in qualifs)
            {
                if (element.Value == qualifMin)
                {
                    rangsProjetsA_Classes.Add(element.Key);
                }
                else
                {
                    if (modeDeTravail == 0)
                    {
                        rangsA_EviterMode1.Add(element.Key);
                    }
                }
            }

            if (rangsProjetsA_Classes.Count > 0)
            {
                //ajout des projets de rangMin à projetsClasses
                //si 1 seul projet est à la qualification min
                if (rangsProjetsA_Classes.Count == 1)
                {
                    //on le classe
                    projetsClasses.Add(rangsProjetsA_Classes);
                    rangsA_EviterMode0.Add(rangsProjetsA_Classes[0]);
                    rangsA_EviterMode1.Add(rangsProjetsA_Classes[0]);
                    if (modeDeTravail == 1)
                    {
                        nbRangsActuelMode1 += rangsProjetsA_Classes.Count;
                        switch (nbRangsActuelMode1 < nbRangsTotalMode1)
                        {
                            case true:
                                {
                                    rangsA_EviterMode1.Add(rangsProjetsA_Classes[0]);
                                }; break;
                            case false:
                                {
                                    rangsA_EviterMode1.Clear();
                                    modeDeTravail = 0;
                                    nbRangsActuelMode1 = 0;
                                    nbRangsTotalMode1 = 0;
                                }; break;
                        }
                        distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }

                //si plusieurs projets ont la qualif min, on cherche à les distiller entre eux
                else
                {
                    //on cherche à savoir si toutes les valeurs de qualification sont les mêmes
                    //auquel cas on ne distille plus les projets
                    Dictionary<int, int> newQualifs = new Dictionary<int, int>();
                    //on ajoute provisoirement les qualifs à rangsAEviterMode0 le temps du calcul des nouvelles qualifs
                    List<int> rangsA_EviterTemp = rangsA_EviterMode1;
                    foreach (int elem in rangsA_EviterMode0)
                    {
                        rangsA_EviterTemp.Add(elem);
                    }
                    switch (modeDeTravail)
                    {
                        case 0: newQualifs = calculeQualifs(matriceSurclassement, nbProjets, rangsA_EviterTemp); break;
                        case 1: newQualifs = calculeQualifs(matriceSurclassement, nbProjets, rangsA_EviterMode1); break;
                    }

                    int firstQualifValue = newQualifs.First().Value;
                    bool valeursToutesEgales = true;
                    foreach (KeyValuePair<int, int> qualif in newQualifs)
                    {
                        if (qualif.Value != firstQualifValue)
                        {
                            valeursToutesEgales = false;
                            break;
                        }
                    }

                    //si tous les projets ont la valeur qualifMin, on ne peut plus les comparer davantage, donc on les classe
                    if (valeursToutesEgales)
                    {
                        //on le classe
                        projetsClasses.Add(rangsProjetsA_Classes);
                        foreach (int elem in rangsProjetsA_Classes)
                        {
                            rangsA_EviterMode0.Add(elem);
                            rangsA_EviterMode1.Add(elem);
                        }
                        if (modeDeTravail == 1)
                        {
                            nbRangsActuelMode1 += rangsProjetsA_Classes.Count;
                            switch (nbRangsActuelMode1 < nbRangsTotalMode1)
                            {
                                case true:
                                    {
                                        foreach (int elem in rangsProjetsA_Classes)
                                        {
                                            rangsA_EviterMode1.Add(elem);
                                        }
                                    }; break;
                                case false:
                                    {
                                        rangsA_EviterMode1.Clear();
                                        modeDeTravail = 0;
                                        nbRangsActuelMode1 = 0;
                                        nbRangsTotalMode1 = 0;
                                    }; break;
                            }
                            distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                        }
                    }
                    else
                    {
                        //résultat de la distillation ascendante de l'ensemble des projets de qualifMin
                        //on calcule les qualifs en évitant les rangs des projets résiduels (soit classés, soit non concernés) 
                        if (modeDeTravail == 0)
                        {
                            nbRangsTotalMode1 = rangsProjetsA_Classes.Count;
                            modeDeTravail = 1;
                        }
                        distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }



                //on continue en ajoutant les projetsResiduels à projetsClasses
                if (modeDeTravail == 0)
                {
                    int somme = 0;
                    foreach (List<int> element in projetsClasses)
                    {
                        foreach (int elem in element)
                        {
                            somme++;
                        }
                    }
                    if (somme < nbProjets)
                    {
                        rangsA_EviterMode1.Clear();
                        distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }
            }
        }

        public static List<List<int>> distillationAscendante(int[,] matriceSurclassement, int nbProjets)
        {
            List<List<int>> projetsClasses = new List<List<int>>();
            List<int> rangsA_EviterMode0 = new List<int>();
            List<int> rangsA_EviterMode1 = new List<int>();
            int nbRangsTotalMode1 = 0, nbRangsActuelMode1 = 0, modeDeTravail = 0;
            distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, Math.Min);
            return projetsClasses;
        }

        public static List<List<int>> distillationDescendante(int[,] matriceSurclassement, int nbProjets)
        {
            List<List<int>> projetsClasses = new List<List<int>>();
            List<int> rangsA_EviterMode0 = new List<int>();
            List<int> rangsA_EviterMode1 = new List<int>();
            int nbRangsTotalMode1 = 0, nbRangsActuelMode1 = 0, modeDeTravail = 0;
            distillationAscDesc(matriceSurclassement, nbProjets, projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, Math.Max);
            return projetsClasses;
        }

        //pour la distillation on ordonne les projets en décroissant selon la différence entre le nombre de projets dominés et le nombre de projets par lequel le projet est dominé
        public static List<List<int>> distillation(int[,] matriceSurclassement, int nbProjets)
        {
            List<List<int>> distiAscendante = distillationAscendante(matriceSurclassement, nbProjets);
            List<List<int>> distiDescendante = distillationDescendante(matriceSurclassement, nbProjets);
            Console.Write("Distillation ascendante: ");
            afficheDistillation(distiAscendante);
            Console.Write("Distillation descendante: ");
            afficheDistillation(distiDescendante);
            List<List<int>> distillation = new List<List<int>>();

            Dictionary<int, int> distiAsc = new Dictionary<int, int>();
            Dictionary<int, int> distiDesc = new Dictionary<int, int>();
            Dictionary<int, int> disti = new Dictionary<int, int>();

            int nbTemp = nbProjets;
            foreach (List<int> projets in distiAscendante)
            {
                nbTemp -= projets.Count;
                foreach (int projet in projets)
                {
                    distiAsc.Add(projet, nbTemp);
                }
            }

            nbTemp = nbProjets;
            foreach (List<int> projets in distiDescendante)
            {
                nbTemp -= projets.Count;
                foreach (int projet in projets)
                {
                    distiDesc.Add(projet, nbTemp);
                }
            }

            foreach (KeyValuePair<int, int> kvp in distiDesc)
            {
                disti.Add(kvp.Key, kvp.Value - distiAsc[kvp.Key]);
            }

            do
            {
                var items = from pair in disti
                            orderby pair.Value descending
                            select pair;
                List<int> listeA_Placer = new List<int>();
                int maxValue = items.First().Value;
                foreach (KeyValuePair<int, int> elem in disti)
                {
                    if (elem.Value == maxValue)
                    {
                        listeA_Placer.Add(elem.Key);
                    }
                }
                foreach (int rang in listeA_Placer)
                {
                    disti.Remove(rang);
                }
                distillation.Add(listeA_Placer);
            } while (disti.Count > 0);

            return distillation;
        }

        public static void afficheMatrice(int[,] matrice, int nbColonnes, int nbCriteres)
        {
            for (int i = 0; i < nbColonnes; i++)
            {
                for (int j = 0; j < nbCriteres; j++)
                {
                    Console.Write(matrice[i, j] + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void afficheMatrice(double[,] matrice, int nbColonnes, int nbCriteres)
        {
            for (int i = 0; i < nbColonnes; i++)
            {
                for (int j = 0; j < nbCriteres; j++)
                {
                    Console.Write(String.Format("{0:0.##}", matrice[i, j]) + "  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void afficheDistillation(List<List<int>> distillation)
        {
            int compteur = 1;
            foreach (List<int> liste in distillation)
            {
                if (liste.Count > 1)
                {
                    int i = 1;
                    foreach (int element in liste)
                    {
                        Console.Write("P" + element);
                        if (i < liste.Count) Console.Write(", ");
                        i++;
                    }
                }
                else
                {
                    Console.Write("P" + liste[0]);
                }
                if (compteur < distillation.Count) Console.Write(" => ");
                compteur++;
            }
            Console.WriteLine();
        }

        public static void executionProgramme(string chemin)
        {
            initialisation(chemin);
            Console.WriteLine("Nb projets: " + nbProjets);
            Console.WriteLine("Nb criteres: " + nbCriteres);
            Console.WriteLine();

            afficheMatrice(matricePerf, nbProjets, nbCriteres);
            afficheMatrice(matriceSeuils, nbSeuils, nbCriteres);

            double[,] matriceConcordance = calculeMatriceConcordance(matricePerf, matriceSeuils, nbProjets, nbCriteres);
            afficheMatrice(matriceConcordance, nbProjets, nbProjets);

            double[,] matriceDiscordance = calculeMatriceDiscordance(matricePerf, matriceSeuils, nbProjets, nbCriteres);
            afficheMatrice(matriceDiscordance, nbProjets, nbProjets);

            double[,] matriceCredibilite = calculeMatriceCredibilite(matriceConcordance, matriceDiscordance, matricePerf, matriceSeuils, nbProjets, nbCriteres);
            afficheMatrice(matriceCredibilite, nbProjets, nbProjets);

            int[,] matriceSurclassement = calculeMatriceSurclassement(matriceCredibilite, nbProjets);
            afficheMatrice(matriceSurclassement, nbProjets, nbProjets);

            List<int> dejaClasses = new List<int>();
            Dictionary<int, int> qualifs = calculeQualifs(matriceSurclassement, nbProjets, dejaClasses);
            foreach (KeyValuePair<int, int> element in qualifs)
            {
                Console.WriteLine("Projet: {0}, valeur de qualif: {1}", element.Key, element.Value);
            }
            Console.WriteLine();

            Console.WriteLine("Qualif min: " + calculeQualifMinMax(qualifs, Math.Min));
            Console.WriteLine("Qualif max: " + calculeQualifMinMax(qualifs, Math.Max));
            Console.WriteLine();

            List<List<int>> disti = distillation(matriceSurclassement, nbProjets);
            Console.Write("Distillation: ");
            afficheDistillation(disti);
        }


        static void Main(string[] args)
        {
            //on peut choisir exemple1, 2 ou 3
            executionProgramme(exemple3);
        }
    }
}