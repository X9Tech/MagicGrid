using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MagicGridControls
{
    internal class MagicGridButtonCanvasPlaceholder : Border
    {
        internal int MagicGridSlotIndex { get; set; }
        internal int MagicGridRow { get; set; }
        internal int MagicGridColumn { get; set; }
        public bool IsActive { get; set; }
    }
}
