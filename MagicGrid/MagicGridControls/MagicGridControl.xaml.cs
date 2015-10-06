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
    /// Interaction logic for MagicGridControl.xaml
    /// </summary>
    public partial class MagicGridControl : UserControl
    {
        public event ButtonSelectionEvent ButtonSelected;
        public event ButtonSelectionEvent ButtonUnselected;

        public MagicGridControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public void UnselectAll()
        {
            foreach (var child in wrap.Children)
            {
                MagicGridButton btn = child as MagicGridButton;
                if (btn != null)
                {
                    btn.Unselect();
                }
            }
        }

        public string Title
        {
            get { return panelTitle.Text; }
            set { panelTitle.Text = value; }
        }

        public IEnumerable<MagicGridButton> Buttons
        {
            get
            {
                return wrap.Children.Cast<MagicGridButton>().ToList();
            }
        }

        public void ClearButtons()
        {
            wrap.Children.Clear();
        }

        public MagicGridButton AddButton(string text)
        {
            var btn = new MagicGridButton() { Text = text };

            return AddButton(btn);
        }

        public MagicGridButton AddButton(MagicGridButton button)
        {
            button.ParentGridControl = this;

            button.ButtonSelected += OnChildButtonSelected;
            button.ButtonUnselected += OnChildButtonUnselected;

            wrap.Children.Add(button);
            return button;
        }

        private void OnChildButtonUnselected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            if (ButtonUnselected != null) { ButtonUnselected(button, parentGridControl); }
        }

        private void OnChildButtonSelected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            if (ButtonSelected != null) { ButtonSelected(button, parentGridControl); }
        }


    }
}
