using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;

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
        internal int GridSlotsPerPage { get; private set; }

        DispatcherTimer _resizeTimer = null;
        private bool _queueResize = true;
        private bool _queueReloadButtons = true;

        public bool EnablePaging { get; set; }
        public int Pages { get; set; }
        public int CurrentPage { get; set; }

        private int _phCount = 0;
        public int PlaceholderCount
        {
            get
            {
                return _phCount;
            }
            set
            {
                _phCount = value;
                _queueResize = true;
            }
        }

        public double MinimumButtonWidth { get; set; }
        public double MinimumButtonHeight { get; set; }

        public MagicGridControl()
        {
            InitializeComponent();
            this.DataContext = this;
            ShowButtonIndexes = true;
            _buttons.CollectionChanged += OnButtonsCollectionChanged;
            this.SizeChanged += OnSizeChanged;
            this.LayoutUpdated += OnLayoutUpdated;
            this.Loaded += OnLoaded;

            txtPageText.Text = "Page " + (CurrentPage + 1).ToString();

            MinimumButtonHeight = 80;
            MinimumButtonWidth = 100;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _resizeTimer = new DispatcherTimer();
            _resizeTimer.Interval = TimeSpan.FromMilliseconds(100);
            _resizeTimer.IsEnabled = true;
            _resizeTimer.Tick += OnResizeTimerTick;
        }

        private void OnResizeTimerTick(object sender, EventArgs e)
        {
            if (_queueResize)
            {
                ResizeGrid();
            }

            if (_queueReloadButtons)
            {
                ReloadButtonsCanvas();
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            //_queueResize = true;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _queueResize = true;
        }

        private void OnButtonsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _queueReloadButtons = true;
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

        private void DLog(string msg)
        {
            if (Debugger.IsAttached )
            {
                Debug.WriteLine(msg);
            }
        }

        public void ClearButtons()
        {
            DLog("Clearing buttons");
            this.Dispatcher.Invoke((Action)(() => {
                _buttons.Clear();
                DLog("Cleared buttons, adding " + PlaceholderCount.ToString() + " placeholders");

                for (int i = 0; i < PlaceholderCount; i++)
                {
                    var b = AddButton(string.Empty, false);
                    b.Selectable = false;
                    b.IsPlaceholderButton = true;
                }
                DLog("Placeholders added");

                _queueReloadButtons = true;
            }));
           
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
            DLog("Adding button " + (button.Text ?? "(null title)"));
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

            DLog("Button added, calling reload canvas");
            //_queueReloadButtons = true;
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
            _queueReloadButtons = false;
            DLog("Reloading buttons canvas, buttons count = " + _buttons.Count.ToString());
            int startingPageButtonIndex = GridSlotsPerPage * CurrentPage;
            this.txtPageText.Text = "Page " + (CurrentPage + 1).ToString();

            for (int i = 0; i < _slotPlaceholders.Count; i++)
            {
                if (_slotPlaceholders[i].IsActive == false)
                {
                    continue;
                }

                int btnIndex = i + startingPageButtonIndex;
                if (btnIndex < _buttons.Count)
                {
                    if (_buttons[btnIndex].Parent != null)
                    {
                        MagicGridButtonCanvasPlaceholder existingParent = _buttons[btnIndex].Parent as MagicGridButtonCanvasPlaceholder;
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

                    _slotPlaceholders[i].Child = _buttons[btnIndex];
                    _buttons[btnIndex].Width = double.NaN;
                    _buttons[btnIndex].Height = double.NaN;
                } else
                {
                    _slotPlaceholders[i].Child = null;
                }
                
            }
        }

        private void ResizeGrid()
        {
            _queueResize = false;
            DLog("Resizing grid");
            gridPaging.Visibility = (EnablePaging ? Visibility.Visible : Visibility.Collapsed);

            double canvasWidth = canvasButtons.ActualWidth;
            double canvasHeight = canvasButtons.ActualHeight;
            int numExistingSlots = _slotPlaceholders.Count;

            double minButtonWidth = MinimumButtonWidth;
            if (minButtonWidth < 10) { minButtonWidth = 10; }

            int buttonsPerRow = Convert.ToInt32(System.Math.Floor(canvasWidth / minButtonWidth));
            double buttonWidth = (canvasWidth / Convert.ToDouble(buttonsPerRow));
            double buttonHeight = MinimumButtonHeight;

            if (buttonHeight < 10) { buttonHeight = 10; }

            int rowsPerPage = (Convert.ToInt32(System.Math.Floor(canvasHeight / buttonHeight)));
            GridSlotsPerPage = buttonsPerRow * rowsPerPage;

            double currentTop = 0;
            double currentLeft = 0;
            int row = 0;
            int col = 0;
            int index = 0;
            bool gridFull = false;

            List<MagicGridButtonCanvasPlaceholder> unusedPlaceholders = new List<MagicGridButtonCanvasPlaceholder>();
            unusedPlaceholders.AddRange(_slotPlaceholders);

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

                MagicGridButtonCanvasPlaceholder ph = null;
                ph = _slotPlaceholders.FirstOrDefault(sp => sp.MagicGridSlotIndex == index);

                if (ph == null)
                { 
                    ph = new MagicGridButtonCanvasPlaceholder();
                    ph.MagicGridSlotIndex = index;
                    canvasButtons.Children.Add(ph);
                    _slotPlaceholders.Add(ph);
                } else
                {
                    unusedPlaceholders.Remove(ph);
                }

                ph.Visibility = Visibility.Visible;
                ph.IsActive = true;
                ph.MagicGridRow = row;
                ph.MagicGridColumn = col;
                ph.Width = buttonWidth;
                ph.Height = buttonHeight;
                Canvas.SetTop(ph, currentTop);
                Canvas.SetLeft(ph, currentLeft);

                currentLeft += buttonWidth;
                index += 1;
                col += 1;
            }

            foreach (var p in unusedPlaceholders)
            {
                p.Visibility = Visibility.Collapsed;
                p.IsActive = false;
            }

            if (_slotPlaceholders.Count != numExistingSlots)
            {
                _queueReloadButtons = true;
            }
        }

        public void PageForward()
        {
            CurrentPage += 1;
            _queueReloadButtons = true;
        }

        public void PageBack()
        {
            if (CurrentPage > 0)
            {
                CurrentPage -= 1;
                _queueReloadButtons = true;
            }
        }

        private void btnPageBack_Click(object sender, RoutedEventArgs e)
        {
            PageBack();
        }

        private void btnPageForward_Click(object sender, RoutedEventArgs e)
        {
            PageForward();
        }
    }
}
