using MagicGridControls;
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

namespace GridSandboxApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                layout.Grids.ToList().ForEach(g => g.UnselectAll());
            }
        }

        private MagicGridControl _grid1 = null;
        private MagicGridControl _grid2 = null;
        private MagicGridControl _grid3 = null;
        private MagicGridControl _grid4 = null;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var grids = layout.Grids.ToList();
            _grid1 = grids[0];
            _grid2 = grids[1];
            _grid3 = grids[2];
            _grid4 = grids[3];

            grids.ForEach(g => g.ClearButtons());

            _grid1.Title = "Fixtures";
            for (int i = 1; i < 20; i++)
            {
                _grid1.AddButton("Fixture " + i.ToString());
            }

            _grid1.ButtonSelected += OnFixtureButtonSelected;

            foreach (var f in System.IO.Directory.GetFiles("C:\\SFX", "*.wav"))
            {
                var fInfo = new System.IO.FileInfo(f);
                var sfxBtn = _grid3.AddButton(fInfo.Name);
                sfxBtn.ActionInfo = fInfo;
                sfxBtn.ButtonSelected += OnSfxButtonSelected;
            }
        }

        private void OnSfxButtonSelected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            System.IO.FileInfo fInfo = button.ActionInfo;
            System.Media.SoundPlayer sp = new System.Media.SoundPlayer(fInfo.FullName);
            sp.Play();
            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                System.Threading.Thread.Sleep(1000);
                this.Dispatcher.Invoke(new Action(() => { button.Unselect(); }));
            });
        }

        private void OnFixtureButtonSelected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            _grid1.Buttons.Where(b => b != button).ToList().ForEach(b => b.Unselect());
            _grid2.ClearButtons();
            //_grid3.ClearButtons();
            //_grid4.ClearButtons();

            _grid2.Title = "Intensity";
            
            for (int i = 0; i <= 100; i=i+10)
            {
                _grid2.AddButton(i.ToString() + "%");
            }

            
        }
    }
}
