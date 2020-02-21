    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
namespace Opti
{
    public class Solver
    {

        private int _nbSeuils;
        private int _nbProjets;
        private int _nbCriteres;
        private int[,] _matricePerf;
        private int[,] _matriceSeuils;

        private double[,] _matriceConcordance = null;
        private double[,] _matriceDiscordance = null;
        private double[,] _matriceCredibilite = null;
        private int[,] _matriceSurclassement = null;
        private Dictionary<int, int> _qualifs = null;
        private List<List<int>> _distiAscendante = null;
        private List<List<int>> _distiDescendante = null;
        private List<List<int>> _distillation = null;

        public int nbSeuils {
            get {
                return _nbSeuils;
            }
        }

        public int nbProjets {
            get {
                return _nbProjets;
            }
        }

        public int nbCriteres {
            get {
                return _nbCriteres;
            }
        }

        public int[,] matricePerformance {
            get {
                return _matricePerf;
            }
        }

        public int[,] matriceSeuils {
            get {
                return _matriceSeuils;
            }
        }

        public double[,] matriceConcordance {
            get {
                if (_matriceConcordance == null) {
                    calculeMatriceConcordance();
                }
                return _matriceConcordance;
            }
        }

        public double[,] matriceDiscordancee {
            get {
                if (_matriceDiscordance == null) {
                    calculeMatriceDiscordance();
                }
                return _matriceDiscordance;
            }
        }

        public double[,] matriceCredibilite {
            get {
                if (_matriceCredibilite == null) {
                    calculeMatriceCredibilite();
                }
                return _matriceCredibilite;
            }
        }

        public int[,] matriceSurclassement {
            get {
                if (_matriceSurclassement == null) {
                    calculeMatriceSurclassement();
                }
                return _matriceSurclassement;
            }
        }

        public Dictionary<int, int> qualifs {
            get {
                if (_qualifs == null) {
                    _qualifs = calculeQualifs(new List<int>());
                }
                return _qualifs;
            }
        }

        public int qualifsMin {
            get {
                int qualifMinMax = 0;
                foreach (KeyValuePair<int, int> item in qualifs) {
                    qualifMinMax = Math.Min(qualifMinMax, item.Value);
                }
                return qualifMinMax;
            }
        }

        public int qualifMax {
            get {
                int qualifMinMax = 0;
                foreach (KeyValuePair<int, int> item in qualifs) {
                    qualifMinMax = Math.Max(qualifMinMax, item.Value);
                }
                return qualifMinMax;
            }
        }

        public List<List<int>> distillationAscendante {
            get {
                if (_distiAscendante == null) {
                    distillation();
                }
                return _distiAscendante;
            }
        }

        public List<List<int>> distillationDescendante {
            get {
                if (_distiDescendante == null) {
                    distillation();
                }
                return _distiDescendante;
            }
        }

        public List<List<int>> disilation {
            get {
                if (_distillation == null) {
                    distillation();
                }
                return _distillation;
            }
        }

        private Solver() {

        }

        public Solver(string path, int nbSeuils) {
            _nbSeuils = nbSeuils;

            if (File.Exists(path)) {
                bool arriveA_MatricePerf = false;
                bool arriveA_MatriceSeuils = false;
                int ligneMatrice = 0;
                // Read a text file line by line.
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines) {
                    if (line.StartsWith("NbProjets")) {
                        string line2;
                        line2 = line.Remove(0, line.IndexOf(":") + 2);
                        _nbProjets = int.Parse(line2);
                        _matricePerf = new int[_nbProjets, _nbCriteres];
                    }
                    if (line.StartsWith("NbCriteres")) {
                        string line2;
                        line2 = line.Remove(0, line.IndexOf(":") + 2);
                        _nbCriteres = int.Parse(line2);
                        _matriceSeuils = new int[_nbSeuils, _nbCriteres];
                    }
                    if (arriveA_MatricePerf) {
                        if (!string.IsNullOrWhiteSpace(line) && !line.Contains("MatriceSeuils")) {
                            string[] values = line.Split(" ");
                            int colonneMatrice = 0;
                            foreach (string value in values) {
                                _matricePerf[ligneMatrice, colonneMatrice] = int.Parse(value);
                                colonneMatrice++;
                            }
                            ligneMatrice++;
                        }
                    }
                    if (arriveA_MatriceSeuils) {
                        if (!string.IsNullOrWhiteSpace(line)) {
                            string[] values = line.Split(" ");
                            int colonneMatrice = 0;
                            foreach (string value in values) {
                                _matriceSeuils[ligneMatrice, colonneMatrice] = int.Parse(value);
                                colonneMatrice++;
                            }
                            ligneMatrice++;
                        }
                    }
                    if (line.Contains("MatricePerformance")) {
                        arriveA_MatricePerf = true;
                    }
                    if (line.Contains("MatriceSeuils")) {
                        arriveA_MatricePerf = false;
                        arriveA_MatriceSeuils = true;
                        ligneMatrice = 0;
                    }
                }
            }
            else {
                throw new Exception("File not found");
            }
        }

        public void reset() {
            _matriceConcordance = null;
            _matriceDiscordance = null;
            _matriceCredibilite = null;
            _matriceSurclassement = null;
            _qualifs = null;
            _distiAscendante = null;
            _distiDescendante = null;
            _distillation = null;
        }

        private void calculeMatriceConcordance() {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < _nbCriteres; a++) {
                sommeImportances += _matriceSeuils[0, a];
            }

            _matriceConcordance = new double[_nbProjets, _nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < _nbProjets; i++) {
                for (int j = 0; j < _nbProjets; j++) {

                    double sommeConcordances = 0.00;
                    double concordance = 0.00;

                    //on somme tous les indices de concordance selon les critères
                    for (int k = 0; k < _nbCriteres; k++) {

                        int perfIK = _matricePerf[i, k];
                        int perfJK = _matricePerf[j, k];
                        int indifferenceK = _matriceSeuils[1, k];
                        int preferenceK = _matriceSeuils[2, k];
                        int importanceK = _matriceSeuils[0, k];

                        if (i == j) {
                            concordance = 1;
                        }
                        else if (perfIK >= perfJK - indifferenceK) {
                            concordance = 1;
                        }
                        else if (perfIK <= perfJK - preferenceK) {
                            concordance = 0;
                        }
                        else {
                            concordance = (double)(preferenceK + perfIK - perfJK) / (preferenceK - indifferenceK);
                        }
                        sommeConcordances += importanceK * concordance;
                    }
                    sommeConcordances /= sommeImportances;
                    _matriceConcordance[i, j] = sommeConcordances;
                }
            }
        }

        private void calculeMatriceDiscordance() {
            //on trouve le k
            double sommeImportances = 0.0;
            for (int a = 0; a < _nbCriteres; a++) {
                sommeImportances += _matriceSeuils[0, a];
            }

            _matriceDiscordance = new double[_nbProjets, _nbProjets];
            //on prend tous les projets deux à deux
            for (int i = 0; i < _nbProjets; i++) {
                for (int j = 0; j < _nbProjets; j++) {

                    double sommeDiscordances = 0.00;
                    double discordance = 0.00;

                    //on somme tous les indices de discordance selon les critères
                    for (int k = 0; k < _nbCriteres; k++) {

                        int perfIK = _matricePerf[i, k];
                        int perfJK = _matricePerf[j, k];
                        int vetoK = _matriceSeuils[3, k];
                        int preferenceK = _matriceSeuils[2, k];
                        int importanceK = _matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK) {
                            discordance = 0;
                        }
                        else if (perfIK <= perfJK - vetoK) {
                            discordance = 1;
                        }
                        else {
                            discordance = (double)(perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        sommeDiscordances += importanceK * discordance;
                    }
                    sommeDiscordances /= sommeImportances;
                    _matriceDiscordance[i, j] = sommeDiscordances;
                }
            }
        }

        private void calculeMatriceCredibilite() {
            _matriceCredibilite = new double[_nbProjets, _nbProjets];

            //on prend tous les projets deux à deux
            for (int i = 0; i < _nbProjets; i++) {
                for (int j = 0; j < _nbProjets; j++) {
                    List<double> ensembleJ = new List<double>();
                    double discordance = 0.00;
                    //calcul de l'ensemble des critères discordants
                    for (int k = 0; k < _nbCriteres; k++) {

                        int perfIK = _matricePerf[i, k];
                        int perfJK = _matricePerf[j, k];
                        int vetoK = _matriceSeuils[3, k];
                        int preferenceK = _matriceSeuils[2, k];
                        int importanceK = _matriceSeuils[0, k];

                        if (perfIK >= perfJK - preferenceK) {
                            discordance = 0;
                        }
                        else if (perfIK <= perfJK - vetoK) {
                            discordance = 1;
                        }
                        else {
                            discordance = (double)(perfJK - perfIK - preferenceK) / (vetoK - preferenceK);
                        }
                        if (discordance > matriceConcordance[i, j]) {
                            ensembleJ.Add(discordance);
                        }
                    }
                    if (ensembleJ.Count == 0) {
                        matriceCredibilite[i, j] = matriceConcordance[i, j];
                    }
                    else {
                        double cred = 1.0;
                        foreach (double disc in ensembleJ) {
                            cred *= (double)((1 - disc) / (1 - matriceConcordance[i, j]));
                        }
                        cred *= matriceConcordance[i, j];
                        matriceCredibilite[i, j] = cred;
                    }
                }
            }
        }

        private double calculeMaxMatrice(double[,] matrice) {
            double max = 0.00;
            foreach (double valeur in matrice) {
                if (valeur == 1) return valeur;
                if (valeur > max) max = valeur;
            }
            return max;
        }

        private void calculeMatriceSurclassement(double alpha = 0.3, double beta = -0.15) {
            double maxCred = calculeMaxMatrice(matriceCredibilite);
            double seuil = alpha + maxCred * beta;
            double niveauCoupe = maxCred - seuil;
            //double diff;
            _matriceSurclassement = new int[_nbProjets, _nbProjets];
            for (int i = 0; i < _nbProjets; i++) {
                for (int j = 0; j < _nbProjets; j++) {
                    //diff = Math.Abs (matriceCredibilite[i, j] - matriceCredibilite [j, i]);
                    if (matriceCredibilite[i, j] >= niveauCoupe) {
                        matriceSurclassement[i, j] = 1;
                    }
                    else {
                        matriceSurclassement[i, j] = 0;
                    }
                }
            }
        }

        private Dictionary<int, int> calculeQualifs(List<int> rangsProjetsDejaClasses) {
            Dictionary<int, int> qualifs = new Dictionary<int, int>();
            for (int i = 0; i < _nbProjets; i++) {
                int sommeLigne = 0, sommeColonne = 0;
                if (!(rangsProjetsDejaClasses.Contains(i))) {
                    for (int j = 0; j < _nbProjets; j++) {
                        if (!(rangsProjetsDejaClasses.Contains(j))) {
                            sommeLigne += matriceSurclassement[i, j];
                            sommeColonne += matriceSurclassement[j, i];
                        }
                    }
                    qualifs.Add(i, sommeLigne - sommeColonne);
                }
            }
            return qualifs;
        }

        private int calculeQualifMinMax(Dictionary<int, int> qualifs, Func<int, int, int> function) {
            int qualifMinMax = 0;
            foreach (KeyValuePair<int, int> item in qualifs) {
                qualifMinMax = function(qualifMinMax, item.Value);
            }
            return qualifMinMax;
        }

        private void distillationAscDesc(List<List<int>> projetsClasses, List<int> rangsA_EviterMode0, List<int> rangsA_EviterMode1, int nbRangsTotalMode1, int nbRangsActuelMode1, int modeDeTravail, Func<int, int, int> function) {
            List<int> rangsProjetsA_Classes = new List<int>();
            Dictionary<int, int> qualifications = new Dictionary<int, int>();
            switch (modeDeTravail) {
                case 0:
                    qualifications = calculeQualifs(rangsA_EviterMode0);
                    break;
                case 1:
                    qualifications = calculeQualifs(rangsA_EviterMode1);
                    break;
            }
            int qualifMin = calculeQualifMinMax(qualifications, function);

            //classement des projets de + faible qualification dans rangsProjetsA_Classes
            foreach (KeyValuePair<int, int> element in qualifications) {
                if (element.Value == qualifMin) {
                    rangsProjetsA_Classes.Add(element.Key);
                }
                else {
                    if (modeDeTravail == 0) {
                        rangsA_EviterMode1.Add(element.Key);
                    }
                }
            }

            if (rangsProjetsA_Classes.Count > 0) {
                //ajout des projets de rangMin à projetsClasses
                //si 1 seul projet est à la qualification min
                if (rangsProjetsA_Classes.Count == 1) {
                    //on le classe
                    projetsClasses.Add(rangsProjetsA_Classes);
                    rangsA_EviterMode0.Add(rangsProjetsA_Classes[0]);
                    rangsA_EviterMode1.Add(rangsProjetsA_Classes[0]);
                    if (modeDeTravail == 1) {
                        nbRangsActuelMode1 += rangsProjetsA_Classes.Count;
                        switch (nbRangsActuelMode1 < nbRangsTotalMode1) {
                            case true: {
                                    rangsA_EviterMode1.Add(rangsProjetsA_Classes[0]);
                                };
                                break;
                            case false: {
                                    rangsA_EviterMode1.Clear();
                                    modeDeTravail = 0;
                                    nbRangsActuelMode1 = 0;
                                    nbRangsTotalMode1 = 0;
                                };
                                break;
                        }
                        distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }

                //si plusieurs projets ont la qualif min, on cherche à les distiller entre eux
                else {
                    //on cherche à savoir si toutes les valeurs de qualification sont les mêmes
                    //auquel cas on ne distille plus les projets
                    Dictionary<int, int> newQualifs = new Dictionary<int, int>();
                    //on ajoute provisoirement les qualifs à rangsAEviterMode0 le temps du calcul des nouvelles qualifs
                    List<int> rangsA_EviterTemp = rangsA_EviterMode1;
                    foreach (int elem in rangsA_EviterMode0) {
                        rangsA_EviterTemp.Add(elem);
                    }
                    switch (modeDeTravail) {
                        case 0:
                            newQualifs = calculeQualifs(rangsA_EviterTemp);
                            break;
                        case 1:
                            newQualifs = calculeQualifs(rangsA_EviterMode1);
                            break;
                    }

                    int firstQualifValue = newQualifs.First().Value;
                    bool valeursToutesEgales = true;
                    foreach (KeyValuePair<int, int> qualif in newQualifs) {
                        if (qualif.Value != firstQualifValue) {
                            valeursToutesEgales = false;
                            break;
                        }
                    }

                    //si tous les projets ont la valeur qualifMin, on ne peut plus les comparer davantage, donc on les classe
                    if (valeursToutesEgales) {
                        //on le classe
                        projetsClasses.Add(rangsProjetsA_Classes);
                        foreach (int elem in rangsProjetsA_Classes) {
                            rangsA_EviterMode0.Add(elem);
                            rangsA_EviterMode1.Add(elem);
                        }
                        if (modeDeTravail == 1) {
                            nbRangsActuelMode1 += rangsProjetsA_Classes.Count;
                            switch (nbRangsActuelMode1 < nbRangsTotalMode1) {
                                case true: {
                                        foreach (int elem in rangsProjetsA_Classes) {
                                            rangsA_EviterMode1.Add(elem);
                                        }
                                    };
                                    break;
                                case false: {
                                        rangsA_EviterMode1.Clear();
                                        modeDeTravail = 0;
                                        nbRangsActuelMode1 = 0;
                                        nbRangsTotalMode1 = 0;
                                    };
                                    break;
                            }
                            distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                        }
                    }
                    else {
                        //résultat de la distillation ascendante de l'ensemble des projets de qualifMin
                        //on calcule les qualifs en évitant les rangs des projets résiduels (soit classés, soit non concernés) 
                        if (modeDeTravail == 0) {
                            nbRangsTotalMode1 = rangsProjetsA_Classes.Count;
                            modeDeTravail = 1;
                        }
                        distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }

                //on continue en ajoutant les projetsResiduels à projetsClasses
                if (modeDeTravail == 0) {
                    int somme = 0;
                    foreach (List<int> element in projetsClasses) {
                        foreach (int elem in element) {
                            somme++;
                        }
                    }
                    if (somme < _nbProjets) {
                        rangsA_EviterMode1.Clear();
                        distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, function);
                    }
                }
            }
        }

        private List<List<int>> DistillationAscendante() {
            List<List<int>> projetsClasses = new List<List<int>>();
            List<int> rangsA_EviterMode0 = new List<int>();
            List<int> rangsA_EviterMode1 = new List<int>();
            int nbRangsTotalMode1 = 0, nbRangsActuelMode1 = 0, modeDeTravail = 0;
            distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, Math.Min);
            return projetsClasses;
        }

        private List<List<int>> DistillationDescendante() {
            List<List<int>> projetsClasses = new List<List<int>>();
            List<int> rangsA_EviterMode0 = new List<int>();
            List<int> rangsA_EviterMode1 = new List<int>();
            int nbRangsTotalMode1 = 0, nbRangsActuelMode1 = 0, modeDeTravail = 0;
            distillationAscDesc(projetsClasses, rangsA_EviterMode0, rangsA_EviterMode1, nbRangsTotalMode1, nbRangsActuelMode1, modeDeTravail, Math.Max);
            return projetsClasses;
        }

        private void distillation() {
            _distiAscendante = DistillationAscendante();
            _distiDescendante = DistillationDescendante();
            _distillation = new List<List<int>>();

            Dictionary<int, int> distiAsc = new Dictionary<int, int>();
            Dictionary<int, int> distiDesc = new Dictionary<int, int>();
            Dictionary<int, int> disti = new Dictionary<int, int>();

            int nbTemp = _nbProjets;
            foreach (List<int> projets in _distiAscendante) {
                nbTemp -= projets.Count;
                foreach (int projet in projets) {
                    distiAsc.Add(projet, nbTemp);
                }
            }

            nbTemp = _nbProjets;
            foreach (List<int> projets in _distiDescendante) {
                nbTemp -= projets.Count;
                foreach (int projet in projets) {
                    distiDesc.Add(projet, nbTemp);
                }
            }

            foreach (KeyValuePair<int, int> kvp in distiDesc) {
                disti.Add(kvp.Key, kvp.Value - distiAsc[kvp.Key]);
            }

            do {
                var items = from pair in disti
                            orderby pair.Value descending
                            select pair;
                List<int> listeA_Placer = new List<int>();
                int maxValue = items.First().Value;
                foreach (KeyValuePair<int, int> elem in disti) {
                    if (elem.Value == maxValue) {
                        listeA_Placer.Add(elem.Key);
                    }
                }
                foreach (int rang in listeA_Placer) {
                    disti.Remove(rang);
                }
                _distillation.Add(listeA_Placer);
            } while (disti.Count > 0);

        }

    }
}
