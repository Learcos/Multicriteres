using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Opti
{
    /// <summary>
    /// Logique d'interaction pour FenetreA_Propos.xaml
    /// </summary>
    public partial class FenetreA_Propos : Window
    {
        public FenetreA_Propos() {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
