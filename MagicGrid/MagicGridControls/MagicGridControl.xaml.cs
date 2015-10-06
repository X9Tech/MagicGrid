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

            for (int i = 0; i < 42; i++)
            {
                var b = AddButton(string.Empty, false);
                b.Selectable = false;
                b.IsPlaceholderButton = true;
            }
        }

        public MagicGridButton AddButton(string text)
        {
            return AddButton(text, true);
        }

        internal MagicGridButton AddButton(string text, bool replacePlaceholder)
        {
            var btn = new MagicGridButton() { Text = text };

            return AddButton(btn, replacePlaceholder);
        }

        public MagicGridButton AddButton(MagicGridButton button)
        {
            return AddButton(button, true);
        }

        internal MagicGridButton AddButton(MagicGridButton button, bool replacePlaceholder)
        {
            button.ParentGridControl = this;

            button.ButtonSelected += OnChildButtonSelected;
            button.ButtonUnselected += OnChildButtonUnselected;

            var phb = wrap.Children.Cast<MagicGridButton>().FirstOrDefault(b => b.IsPlaceholderButton);
            if (phb != null && replacePlaceholder == true)
            {
                var index = wrap.Children.IndexOf(phb);
                wrap.Children.Insert(index, button);
                wrap.Children.Remove(phb);
            }
            else
            {
                wrap.Children.Add(button);
            }

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
