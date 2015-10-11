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
    public delegate void MagicGridPageChangedEvent(MagicGridControl magicGrid, int currentPageIndex);
    /// <summary>
    /// Interaction logic for MagicGridControl.xaml
    /// </summary>
    public partial class MagicGridControl : UserControl
    {
        public event ButtonSelectionEvent ButtonSelected;
        public event ButtonSelectionEvent ButtonUnselected;
        public event MagicGridPageChangedEvent PageChanged;

        public bool OnlyAllowSingleSelection { get; set; }
        public bool AutoUnselectOnTouch { get; set; }
        internal int GridSlotsPerPage { get; private set; }

        DispatcherTimer _resizeTimer = null;
        private bool _queueResize = true;

        public bool EnablePaging { get; set; }
        public int Pages { get; set; }
        public int CurrentPageSlotIndexStart { get; private set; }
        public int CurrentPageSlotIndexEnd { get; private set; }
        public int CurrentPage { get; set; }

        private int _phCount = 0;
        private bool _firePageChangeOnNextRecalculate = true;
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
            CurrentPage = 0;
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
                RecalculateGrid();
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
                _slotPlaceholders.ToList().ForEach(sh => sh.Child = null);
            }));
           
        }

        public MagicGridButton AddButton(string text)
        {
            return AddButton(text, null);
        }

        public MagicGridButton AddButton(string text, int? gridSlotIndex)
        {
            var btn = new MagicGridButton() { Text = text };

            return AddButton(btn, gridSlotIndex);
        }

        public MagicGridButton AddButton(MagicGridButton button)
        {
            return AddButton(button, null);
        }

        internal MagicGridButton AddButton(MagicGridButton button, int? gridSlotIndex)
        {
            if (_slotPlaceholders.Count == 0)
            {
                RecalculateGrid();
            }

            DLog("Adding button " + (button.Text ?? "(null title)"));
            button.ParentGridControl = this;

            button.ButtonSelected += OnChildButtonSelected;
            button.ButtonUnselected += OnChildButtonUnselected;

            string labelIndex = string.Empty;

            MagicGridButtonCanvasPlaceholder targetSlot = null;

            int highestIndex = 0;
            foreach (var slot in _slotPlaceholders.OrderBy(sp => sp.MagicGridSlotIndex))
            {
                if (slot.MagicGridSlotIndex > highestIndex)
                {
                    highestIndex = slot.MagicGridSlotIndex;
                }

                if (slot.Child == null)
                {
                    if (gridSlotIndex == null)
                    {
                        gridSlotIndex = slot.MagicGridSlotIndex;
                        targetSlot = slot;
                        break;
                    }
                }

                if (gridSlotIndex != null)
                {
                    if (slot.MagicGridSlotIndex == gridSlotIndex)
                    {
                        targetSlot = slot;
                    }
                }
            }
            
            if (targetSlot == null)
            {
                MagicGridButtonCanvasPlaceholder newPh = new MagicGridButtonCanvasPlaceholder() { MagicGridSlotIndex = highestIndex + 1 };
                _slotPlaceholders.Add(newPh);
                targetSlot = newPh;
                _queueResize = true;
            }

            targetSlot.Child = button;
            button.Width = double.NaN;
            button.Height = double.NaN;

            labelIndex = (targetSlot.MagicGridSlotIndex + 1).ToString();
            
            if (!ShowButtonIndexes)
            {
                labelIndex = string.Empty;
            }

            button.IndexLabelText = labelIndex.ToString();

            DLog("Button added");

            _buttons.Add(button);
            //_queueReloadButtons = true;
            //ReloadButtonsCanvas();
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

        private void RecalculateGrid()
        {
            _queueResize = false;
            DLog("Resizing grid");

            if (EnablePaging)
            {
                gridPaging.Visibility = Visibility.Visible;
                txtPageText.Text = "Page " + (CurrentPage+1).ToString();
            } else
            {
                gridPaging.Visibility = Visibility.Collapsed;
            }

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
            int index = CurrentPage * GridSlotsPerPage;
            bool gridFull = false;
            int startingSlotIndex = index;

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
                    _slotPlaceholders.Add(ph);
                    _firePageChangeOnNextRecalculate = true;
                } else
                {
                    unusedPlaceholders.Remove(ph);
                }

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
                p.IsActive = false;
            }

            foreach (var p in _slotPlaceholders)
            {
                if (p.IsActive)
                {
                    if (!canvasButtons.Children.Contains(p))
                    {
                        canvasButtons.Children.Add(p);
                    }
                } else
                {
                    if (canvasButtons.Children.Contains(p))
                    {
                        canvasButtons.Children.Remove(p);
                    }
                }
            }

            CurrentPageSlotIndexStart = startingSlotIndex;
            CurrentPageSlotIndexEnd = index;

            if (_firePageChangeOnNextRecalculate)
            {
                _firePageChangeOnNextRecalculate = false;
                if (PageChanged != null)
                {
                    PageChanged(this, CurrentPage);
                }
            }
        }

        public void PageForward()
        {
            CurrentPage += 1;
            _firePageChangeOnNextRecalculate = true;
            RecalculateGrid();
        }

        public void PageBack()
        {
            if (CurrentPage > 0)
            {
                CurrentPage -= 1;
                _firePageChangeOnNextRecalculate = true;
                RecalculateGrid();
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
