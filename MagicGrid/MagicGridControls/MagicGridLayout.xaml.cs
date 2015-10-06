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

namespace MagicGridControls
{
    /// <summary>
    /// Interaction logic for MagicGridLayout.xaml
    /// </summary>
    public partial class MagicGridLayout : UserControl
    {
        public MagicGridLayout()
        {
            InitializeComponent();
        }

        public IEnumerable<MagicGridControl> Grids
        {
            get
            {
                return this.gridContainer.Children.Cast<MagicGridControl>().ToList();
            }
        }

        public void UnselectAll()
        {
            this.Grids.ToList().ForEach(g => g.UnselectAll());
        }
    }
}
