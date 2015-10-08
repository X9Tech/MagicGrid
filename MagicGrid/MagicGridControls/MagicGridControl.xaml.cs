using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public bool OnlyAllowSingleSelection { get; set; }
        public bool AutoUnselectOnTouch { get; set; }

        public MagicGridControl()
        {
            InitializeComponent();
            this.DataContext = this;
            ShowButtonIndexes = true;
            _buttons.CollectionChanged += OnButtonsCollectionChanged;
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeGrid();
        }

        private void OnButtonsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        // List<MagicGridButton> _buttons = new List<MagicGridButton>();
        ObservableCollection<MagicGridButton> _buttons = new ObservableCollection<MagicGridButton>();
        ObservableCollection<MagicGridButtonCanvasPlaceholder> _slotPlaceholders = new ObservableCollection<MagicGridButtonCanvasPlaceholder>();

        public bool ShowButtonIndexes { get; set; }

        public void UnselectAll()
        {
            foreach (var child in _buttons)
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
                return _buttons.ToArray();
            }
        }

        public void ClearButtons()
        {
            _buttons.Clear();

            for (int i = 0; i < 42; i++)
            {
                var b = AddButton(string.Empty, false);
                b.Selectable = false;
                b.IsPlaceholderButton = true;
            }

            ReloadButtonsCanvas();
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

            string labelIndex = string.Empty;

            var phb = _buttons.FirstOrDefault(b => b.IsPlaceholderButton);
            if (phb != null && replacePlaceholder == true)
            {
                var index = _buttons.IndexOf(phb);
                _buttons.Insert(index, button);
                _buttons.Remove(phb);
            }
            else
            {
                _buttons.Add(button);
            }

            labelIndex = (_buttons.IndexOf(button)+1).ToString();
            
            if (!ShowButtonIndexes)
            {
                labelIndex = string.Empty;
            }

            button.IndexLabelText = labelIndex.ToString();

            ReloadButtonsCanvas();

            return button;
        }

        private void OnChildButtonUnselected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            if (ButtonUnselected != null) { ButtonUnselected(button, parentGridControl); }
        }

        private void OnChildButtonSelected(MagicGridButton button, MagicGridControl parentGridControl)
        {
            if (ButtonSelected != null) { ButtonSelected(button, parentGridControl); }

            if (OnlyAllowSingleSelection)
            {
                foreach (var btn in Buttons)
                {
                    if (btn != button && btn.IsSelected) { btn.Unselect(true); }
                }
            }
        }

        private void ReloadButtonsCanvas()
        {
            for (int i = 0; i < _slotPlaceholders.Count; i++)
            {
                if (i < _buttons.Count)
                {
                    if (_buttons[i].Parent != null)
                    {
                        MagicGridButtonCanvasPlaceholder existingParent = _buttons[i].Parent as MagicGridButtonCanvasPlaceholder;
                        if (existingParent != null)
                        {
                            if (existingParent != _slotPlaceholders[i])
                            {
                                existingParent.Child = null;
                            } else
                            {
                                continue;
                            }
                        }
                    }

                    _slotPlaceholders[i].Child = _buttons[i];
                    _buttons[i].Width = double.NaN;
                    _buttons[i].Height = double.NaN;
                } else
                {
                    _slotPlaceholders[i].Child = null;
                }
                
            }
        }

        private void ResizeGrid()
        {
            double canvasWidth = canvasButtons.ActualWidth;
            double canvasHeight = canvasButtons.ActualHeight;
            int numExistingSlots = _slotPlaceholders.Count;

            double minButtonWidth = 100;
            int buttonsPerRow = Convert.ToInt32(System.Math.Floor(canvasWidth / minButtonWidth));

            double buttonWidth = (canvasWidth / Convert.ToDouble(buttonsPerRow));
            double buttonHeight = 80;

            canvasButtons.Children.Clear();
            _slotPlaceholders.Clear();

            double currentTop = 0;
            double currentLeft = 0;
            int row = 0;
            int col = 0;
            int index = 0;
            bool gridFull = false;

            while (!gridFull)
            {
                if (currentLeft + buttonWidth >= (canvasWidth + 50))
                {
                    // new line
                    currentLeft = 0;
                    col = 0;
                    currentTop += buttonHeight;
                    row += 1;
                }

                if (currentTop + buttonHeight > canvasHeight)
                {
                    gridFull = true;
                    break;
                }

                MagicGridButtonCanvasPlaceholder ph = new MagicGridButtonCanvasPlaceholder();
                ph.MagicGridRow = row;
                ph.MagicGridColumn = col;
                ph.MagicGridSlotIndex = index;

                canvasButtons.Children.Add(ph);
                _slotPlaceholders.Add(ph);
                ph.Width = buttonWidth;
                ph.Height = buttonHeight;
                //ph.Background = Brushes.Yellow;
                //ph.BorderThickness = new Thickness(2);
                //ph.BorderBrush = Brushes.Red;
                Canvas.SetTop(ph, currentTop);
                Canvas.SetLeft(ph, currentLeft);

                currentLeft += buttonWidth;
                index += 1;
                col += 1;
            }

            if (_slotPlaceholders.Count != numExistingSlots)
            {
                ReloadButtonsCanvas();
            }
        }
    }
}
