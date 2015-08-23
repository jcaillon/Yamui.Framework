using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace YamuiFramework.Controls
{
    public partial class YamuiGrid : DataGridView
    {
        DataGridHelper scrollhelper;
        DataGridHelper scrollhelperH;

        public YamuiGrid()
        {
            InitializeComponent();

            StyleGrid();

            Controls.Add(_vertical);
            Controls.Add(_horizontal);

            Controls.SetChildIndex(_vertical, 0);
            Controls.SetChildIndex(_horizontal, 1);

            _horizontal.Visible = false;
            _vertical.Visible = false;

            scrollhelper = new DataGridHelper(_vertical, this);
            scrollhelperH = new DataGridHelper(_horizontal, this, false);
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta > 0 && FirstDisplayedScrollingRowIndex > 0)
            {
                FirstDisplayedScrollingRowIndex--;
            }
            else if (e.Delta < 0)
            {
                //this.FirstDisplayedScrollingRowIndex++;
                FirstDisplayedScrollingRowIndex++;
                if (e.Delta > 0 && FirstDisplayedScrollingRowIndex > 0)
                {
                    FirstDisplayedScrollingRowIndex--;
                }
                else if (e.Delta < 0)
                {
                    FirstDisplayedScrollingRowIndex++;
                }
            }
        }

        private void StyleGrid()
        {
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            EnableHeadersVisualStyles = false;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            BackColor = ThemeManager.FormColor.BackColor();
            BackgroundColor = ThemeManager.FormColor.BackColor();
            GridColor = ThemeManager.FormColor.BackColor();
            ForeColor = ThemeManager.ButtonColors.Normal.ForeColor();
            Font = new Font("Segoe UI", 11f, FontStyle.Regular, GraphicsUnit.Pixel);

            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            AllowUserToResizeRows = false;

            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            ColumnHeadersDefaultCellStyle.BackColor = ThemeManager.AccentColor;
            ColumnHeadersDefaultCellStyle.ForeColor = ThemeManager.ButtonColors.Press.ForeColor();

            RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            RowHeadersDefaultCellStyle.BackColor = ThemeManager.AccentColor;
            RowHeadersDefaultCellStyle.ForeColor = ThemeManager.ButtonColors.Press.ForeColor();

            DefaultCellStyle.BackColor = ThemeManager.FormColor.BackColor();

            DefaultCellStyle.SelectionBackColor = ThemeManager.AccentColor;
            DefaultCellStyle.SelectionForeColor = (ThemeManager.Theme == Themes.Light) ? Color.FromArgb(37, 37, 38) : Color.FromArgb(230, 230, 230);

            DefaultCellStyle.SelectionBackColor = ThemeManager.AccentColor;
            DefaultCellStyle.SelectionForeColor = (ThemeManager.Theme == Themes.Light) ? Color.FromArgb(37, 37, 38) : Color.FromArgb(230, 230, 230);

            RowHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.AccentColor;
            RowHeadersDefaultCellStyle.SelectionForeColor = (ThemeManager.Theme == Themes.Light) ? Color.FromArgb(37, 37, 38) : Color.FromArgb(230, 230, 230);

            ColumnHeadersDefaultCellStyle.SelectionBackColor = ThemeManager.AccentColor;
            ColumnHeadersDefaultCellStyle.SelectionForeColor = (ThemeManager.Theme == Themes.Light) ? Color.FromArgb(37, 37, 38) : Color.FromArgb(230, 230, 230);

            CellMouseEnter += (sender, args) => {
                if (args.RowIndex > 0)
                    Rows[args.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(160, 160, 160);
            };
            CellMouseLeave += (sender, args) => {
                if (args.RowIndex > 0)
                    Rows[args.RowIndex].DefaultCellStyle.BackColor = ThemeManager.FormColor.BackColor();
            };
        }
    }

    public class DataGridHelper
    {
        /// <summary>
        /// The associated scrollbar or scrollbar collector
        /// </summary>
        private YamuiScrollBar _scrollbar;

        /// <summary>
        /// Associated Grid
        /// </summary>
        private DataGridView _grid;

        /// <summary>
        /// if greater zero, scrollbar changes are ignored
        /// </summary>
        private int _ignoreScrollbarChange;

        /// <summary>
        /// 
        /// </summary>
        private bool _ishorizontal;
        private HScrollBar hScrollbar;
        private VScrollBar vScrollbar;

        public DataGridHelper(YamuiScrollBar scrollbar, DataGridView grid, bool vertical = true)
        {
            _scrollbar = scrollbar;
            _scrollbar.UseBarColor = true;
            _grid = grid;
            _ishorizontal = !vertical;

            foreach (var item in _grid.Controls)
            {
                if (item.GetType() == typeof(VScrollBar))
                {
                    vScrollbar = (VScrollBar)item;
                }

                if (item.GetType() == typeof(HScrollBar))
                {
                    hScrollbar = (HScrollBar)item;
                }
            }

            _grid.RowsAdded += _grid_RowsAdded;
            _grid.UserDeletedRow += _grid_UserDeletedRow;
            _grid.Scroll += _grid_Scroll;
            _grid.Resize += _grid_Resize;
            _scrollbar.Scroll += _scrollbar_Scroll;
            _scrollbar.ScrollbarSize = 17;

            UpdateScrollbar();
        }

        void _grid_Scroll(object sender, ScrollEventArgs e)
        {
            UpdateScrollbar();
        }

        void _grid_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            UpdateScrollbar();
        }

        void _grid_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            UpdateScrollbar();
        }

        void _scrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            if (_ignoreScrollbarChange > 0) return;

            if (_ishorizontal)
            {
                hScrollbar.Value = _scrollbar.Value;

                try
                {
                    _grid.HorizontalScrollingOffset = _scrollbar.Value;
                }
                catch { }
            }
            else
            {
                if (_scrollbar.Value >= 0 && _scrollbar.Value < _grid.Rows.Count)
                {
                    _grid.FirstDisplayedScrollingRowIndex = _scrollbar.Value + (_scrollbar.Value == 1 ? -1 : 1) >= _grid.Rows.Count ? _grid.Rows.Count - 1 : _scrollbar.Value + (_scrollbar.Value == 1 ? -1 : 1);
                }  else
                {
                    _grid.FirstDisplayedScrollingRowIndex = _scrollbar.Value -1;
                }
                    //_grid.FirstDisplayedScrollingRowIndex = _scrollbar.Value + (_scrollbar.Value == 1 ? -1 : 1);
            }

            _grid.Invalidate();
        }

        private void BeginIgnoreScrollbarChangeEvents()
        {
            _ignoreScrollbarChange++;
        }

        private void EndIgnoreScrollbarChangeEvents()
        {
            if (_ignoreScrollbarChange > 0)
                _ignoreScrollbarChange--;
        }

        /// <summary>
        /// Updates the scrollbar values
        /// </summary>
        public void UpdateScrollbar()
        {
            try
            {
                BeginIgnoreScrollbarChangeEvents();

                if (_ishorizontal)
                {
                    int visibleCols = VisibleFlexGridCols();
                    _scrollbar.Maximum = hScrollbar.Maximum;
                    _scrollbar.Minimum = hScrollbar.Minimum;
                    _scrollbar.SmallChange = hScrollbar.SmallChange;
                    _scrollbar.LargeChange = hScrollbar.LargeChange;
                    _scrollbar.Location = new Point(0, _grid.Height - _scrollbar.ScrollbarSize);
                    _scrollbar.Width = _grid.Width - (vScrollbar.Visible ? _scrollbar.ScrollbarSize : 0);
                    _scrollbar.BringToFront();
                    _scrollbar.Visible = hScrollbar.Visible;
                    _scrollbar.Value = hScrollbar.Value == 0 ? 1 : hScrollbar.Value;
                }
                else
                {
                    int visibleRows = VisibleFlexGridRows();
                    _scrollbar.Maximum = _grid.RowCount;
                    _scrollbar.Minimum = 1;
                    _scrollbar.SmallChange = 1;
                    _scrollbar.LargeChange = Math.Max(1, visibleRows - 1);
                    _scrollbar.Value = _grid.FirstDisplayedScrollingRowIndex;
                    if (_grid.RowCount > 0 && _grid.Rows[_grid.RowCount - 1].Cells[0].Displayed)
                    {
                        _scrollbar.Value =  _grid.RowCount;
                    }
                    _scrollbar.Location = new Point(_grid.Width - _scrollbar.ScrollbarSize, 0);
                    _scrollbar.Height = _grid.Height - (hScrollbar.Visible ? _scrollbar.ScrollbarSize : 0);
                    _scrollbar.BringToFront();
                    _scrollbar.Visible = vScrollbar.Visible;
                }
            }
            finally
            {
                EndIgnoreScrollbarChangeEvents();
            }
        }

        /// <summary>
        /// Determine the current count of visible rows
        /// </summary>
        /// <returns></returns>
        private int VisibleFlexGridRows()
        {
            return _grid.DisplayedRowCount(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private int VisibleFlexGridCols()
        {
            return _grid.DisplayedColumnCount(true);
        }

        public bool VisibleVerticalScroll()
        {
            bool _return = _grid.DisplayedRowCount(true) < _grid.RowCount + (_grid.RowHeadersVisible ? 1 : 0);

            return _return;
        }

        public bool VisibleHorizontalScroll()
        {
            bool _return = _grid.DisplayedColumnCount(true) < _grid.ColumnCount + (_grid.ColumnHeadersVisible ? 1 : 0);

            return _return;
        }

        #region Events of interest

        void _grid_Resize(object sender, EventArgs e)
        {
            UpdateScrollbar();
        }

        void _grid_AfterDataRefresh(object sender, ListChangedEventArgs e)
        {
            UpdateScrollbar();
        }
        #endregion
    }
}
