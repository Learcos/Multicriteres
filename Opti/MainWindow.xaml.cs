using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace Opti
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Solver solveur = null;

        public MainWindow() {
            InitializeComponent();
        }

        public static string afficheDistillation(List<List<int>> distillation) {
            int compteur = 1;
            string res = "";
            foreach (List<int> liste in distillation) {
                if (liste.Count > 1) {
                    int i = 1;
                    foreach (int element in liste) {
                        res += "P" + element.ToString();
                        if (i < liste.Count) {
                            res += ",";
                        }
                        i++;
                    }
                }
                else {
                    res += "P" + liste[0].ToString();
                }
                if (compteur < distillation.Count) {
                    res += " => ";
                }
                compteur++;
            }
            return res;
        }

        private void Ouvrir_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files(*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true) {
                solveur = new Solver(openFileDialog.FileName, 4);
                label_titre.Visibility = Visibility.Visible;
                updateDistilations();
                setPerf();
            }
        }

        private void updateDistilations() {
            label_distilation.Visibility = Visibility.Visible;
            label_distilation.Content = "Distillation descendante : " + afficheDistillation(solveur.disilation);

            label_distilationAscendante.Visibility = Visibility.Visible;
            label_distilationAscendante.Content = "Distillation ascendante : " + afficheDistillation(solveur.distillationAscendante);

            label_distilationDescendante.Visibility = Visibility.Visible;
            label_distilationDescendante.Content = "Distillation : " + afficheDistillation(solveur.distillationDescendante);
        }

        private void setPerf() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<int>(solveur.matricePerformance);
            label_titre.Content = "Matrice de performance";
            dataGrid.IsReadOnly = true;
        }

         private void setSeuils() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<int>(solveur.matriceSeuils);
            label_titre.Content = "Matrice de seuils";
            dataGrid.IsReadOnly = false ;
            solveur.reset();
        }

        private void setConcordance() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<double>(solveur.matriceConcordance);
            label_titre.Content = "Matrice de concordance";
            dataGrid.IsReadOnly = true;
        }

        private void setDiscordance() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<double>(solveur.matriceDiscordancee);
            label_titre.Content = "Matrice de discordance";
            dataGrid.IsReadOnly = true;
        }

        private void setCredibilité() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<double>(solveur.matriceCredibilite);
            label_titre.Content = "Matrice de crédibilité";
            dataGrid.IsReadOnly = true;
        }

        private void setSurclassement() {
            dataGrid.ItemsSource = Helper.GetBindable2DArray<int>(solveur.matriceSurclassement);
            label_titre.Content = "Matrice de surclassement";
            dataGrid.IsReadOnly = true;
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) {
            DataGridTextColumn column = e.Column as DataGridTextColumn;
            Binding binding = column.Binding as Binding;
            binding.Path = new PropertyPath(binding.Path.Path + ".Value");
            if (e.PropertyType.GenericTypeArguments[0] == typeof(double))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "N2";
        }

        private void button_pref_Click(object sender, RoutedEventArgs e) {
            if(solveur != null) {
                setPerf();
            }
        }

        private void button_seuils_Click(object sender, RoutedEventArgs e) {
            if(solveur != null) {
                setSeuils();
            }
        }

        private void button_concordance_Click(object sender, RoutedEventArgs e) {
            setConcordance();
        }

        private void button_discordance_Click(object sender, RoutedEventArgs e) {
            setDiscordance();
        }

        private void button_credibilite_Click(object sender, RoutedEventArgs e) {
            setCredibilité();
        }

        private void button_surclassement_Click(object sender, RoutedEventArgs e) {
            setSurclassement();
        }

        private void dataGrid_CurrentCellChanged(object sender, EventArgs e) {
            solveur.reset();
            updateDistilations();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
}
