using BLREdit.UI.Views;

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

namespace BLREdit.UI.Controls
{
    /// <summary>
    /// Interaction logic for ExtraControl.xaml
    /// </summary>
    public partial class ExtraControl : EquipmentControl
    {
        public ExtraControl()
        {
            InitializeComponent();
            EquipmentControlGrid = ExtraControlGrid;
        }

        private void RandomizeTaunts_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRLoadout loadout)
            {
                loadout.RandomizeTaunts();
            }
        }

        private void RandomizeDepot_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRLoadout loadout)
            {
                loadout.RandomizeDepot();
            }
        }

        private void RandomizeEmblem_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRLoadout loadout)
            {
                loadout.RandomizeEmblem();
            }
        }

        private void RandomizeVoices_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is BLRLoadout loadout)
            {
                loadout.RandomizeVoices();
            }

        }
    }
}
