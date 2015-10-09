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
    public delegate void ButtonSelectionEvent(MagicGridButton button, MagicGridControl parentGridControl);

    /// <summary>
    /// Interaction logic for MagicGridButton.xaml
    /// </summary>
    public partial class MagicGridButton : UserControl
    {
        public event ButtonSelectionEvent ButtonSelected;
        public event ButtonSelectionEvent ButtonUnselected;
        
        public bool? AutoUnselectOnTouch { get; set; }
        private int AutoUnselectAfterMilliseconds { get; set; }

        public MagicGridButton()
        {
            InitializeComponent();
            Selectable = true;
            Unselect();
            AutoUnselectAfterMilliseconds = 500;
        }

        DateTime? _lastSelected = null;
        internal MagicGridControl ParentGridControl { get; set; }

        private bool _isSelected { get; set; }
        public bool IsSelected
        {
            get { return _isSelected; }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Select();   
        }

        private void Grid_TouchDown(object sender, TouchEventArgs e)
        {
            //Select();
        }

        private void Select()
        {
            Select(true);
        }

        private void Select(bool fireEvents)
        {
            if (!Selectable) { return; }

            bool parentUnselectOnTouch = false;
            if (ParentGridControl != null)
            {
                parentUnselectOnTouch = ParentGridControl.AutoUnselectOnTouch;
            }

            double lastSelectedMs = 0;
            if (_lastSelected.HasValue)
            {
                lastSelectedMs = DateTime.Now.Subtract(_lastSelected.GetValueOrDefault()).TotalMilliseconds;
            }

            if (IsSelected 
                && AutoUnselectOnTouch.GetValueOrDefault(parentUnselectOnTouch)
                && lastSelectedMs > AutoUnselectAfterMilliseconds)
            {
                Unselect(true);
                return;
            }

            _isSelected = true;
            _lastSelected = DateTime.Now;
            if (ButtonSelected != null && fireEvents == true) { ButtonSelected(this, ParentGridControl); }
            grid.Background = Brushes.DarkGray;
        }

        public void Unselect()
        {
            Unselect(true);
        }

        public void Unselect(bool fireEvents)
        {
            _isSelected = false;
            if (ButtonUnselected != null && fireEvents == true) { ButtonUnselected(this, ParentGridControl); }
            grid.Background = Brushes.WhiteSmoke;
        }

        public string Text
        {
            get { return txtButtonText.Text; }
            set { txtButtonText.Text = value; }
        }

        public bool Selectable { get; set; }
        public bool IsPlaceholderButton { get; set; }
        public dynamic ActionInfo { get; set; }

        public string IndexLabelText
        {
            get { return lblButtonIndex.Content.ToString(); }
            set { lblButtonIndex.Content = value; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Button ");
            if (IsPlaceholderButton)
            {
                sb.Append("Placeholder ");
            }

            sb.Append(this.Text ?? "(no text)");
            return sb.ToString();
        }
    }
}
