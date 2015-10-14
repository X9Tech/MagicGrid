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
        public Brush UnselectedBackground { get; set; }
        public Brush UnselectedForeground { get; set; }
        public Brush SelectedBackground { get; set; }
        public Brush SelectedForeground { get; set; }

        public MagicGridButton()
        {
            InitializeComponent();
            Selectable = true;
            Unselect();
            AutoUnselectAfterMilliseconds = 0;

            UnselectedBackground = Brushes.Black;
            UnselectedForeground = Brushes.White;
            SelectedBackground = Brushes.Orange;
            SelectedForeground = Brushes.Black;
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

        public void Select()
        {
            Select(true);
        }

        public void Select(bool fireEvents)
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
            grid.Background = SelectedBackground;
            this.Foreground = SelectedForeground;
        }

        public void Unselect()
        {
            Unselect(true);
        }

        public void Unselect(bool fireEvents)
        {
            _isSelected = false;
            if (ButtonUnselected != null && fireEvents == true) { ButtonUnselected(this, ParentGridControl); }
            grid.Background = UnselectedBackground;
            this.Foreground = UnselectedForeground;
        }

        public string Text
        {
            get { return txtButtonText.Text; }
            set { txtButtonText.Text = value; }
        }

        public bool Selectable { get; set; }
        public bool IsPlaceholderButton { get; set; }
        public dynamic ActionInfo { get; set; }

        public int? CurrentMagicGridSlotIndex
        {
            get
            {
                var parentPh = this.Parent as MagicGridButtonCanvasPlaceholder;
                if (parentPh == null) { return null; }
                return parentPh.MagicGridSlotIndex;
            }
        }

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
