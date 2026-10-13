using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Lab8_TrianglesWinForms
{
    // Головна форма: генерація, сортування, фільтр, таблиця та результати
    public class MainForm : Form
    {
        private readonly TriangleCollection col;

        private DataGridView dgv;
        private ComboBox cbSort, cbFilter;
        private TextBox tbN;
        private Button btnGen;
        private Label lblResult;
        private ToolStripStatusLabel lblStatus;
        private Font boldFont;

        public MainForm(TriangleCollection col)
        {
            this.col = col;

            Text = "Трикутники";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(960, 600);
            MinimumSize = new Size(900, 500);
            StartPosition = FormStartPosition.CenterScreen;

            // Порядок додавання важливий для Dock: спочатку Fill-елемент
            BuildGrid();
            BuildResultPanel();
            BuildBottomPanel();
            BuildStatusStrip();
            BuildTopPanel();

            cbSort.SelectedIndex = 0;
            cbFilter.SelectedIndex = 0;
            cbSort.SelectedIndexChanged += (s, e) => RefreshGrid();
            cbFilter.SelectedIndexChanged += (s, e) => RefreshGrid();
            AcceptButton = btnGen;              // Enter у полі n = «Згенерувати»

            RefreshGrid();
        }

        // ---------- Побудова інтерфейсу ----------
        private void BuildGrid()
        {
            boldFont = new Font(Font, FontStyle.Bold);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 34,
                RowTemplate = { Height = 30 }
            };

            // шапка таблиці
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkCyan;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.DarkCyan;
            // чергування рядків і виділення
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 248, 248);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 140);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            dgv.Columns.Add(new DataGridViewTextBoxColumn
            { HeaderText = "ID", DataPropertyName = "Id", FillWeight = 7 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            { HeaderText = "Вид", DataPropertyName = "Kind", FillWeight = 22 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            { HeaderText = "Сторони a / b / c", DataPropertyName = "Sides", FillWeight = 31 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Площа",
                DataPropertyName = "Area",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "F2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Периметр",
                DataPropertyName = "Perimeter",
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "F2",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            dgv.CellFormatting += Dgv_CellFormatting;
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) SafeRun(ViewItem); };

            Controls.Add(dgv);
        }

        private void BuildResultPanel()
        {
            lblResult = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(12, 6, 12, 6),
                BackColor = Color.White,
                ForeColor = Color.DarkCyan,
                Font = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(lblResult);
        }

        private void BuildBottomPanel()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                Padding = new Padding(10, 10, 10, 8),
                BackColor = Color.FromArgb(235, 240, 241)
            };
            panel.Controls.Add(MakeButton("Додати", Color.SeaGreen, (s, e) => SafeRun(AddItem)));
            panel.Controls.Add(MakeButton("Редагувати", Color.SteelBlue, (s, e) => SafeRun(EditItem)));
            panel.Controls.Add(MakeButton("Видалити", Color.Firebrick, (s, e) => SafeRun(DeleteItem)));
            panel.Controls.Add(MakeButton("Переглянути", Color.DarkCyan, (s, e) => SafeRun(ViewItem)));
            panel.Controls.Add(MakeButton("Вихід", Color.Gray, (s, e) => Close()));
            Controls.Add(panel);
        }

        private void BuildStatusStrip()
        {
            StatusStrip strip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            strip.Items.Add(lblStatus);
            strip.Items.Add(new ToolStripStatusLabel("Вид:") { ForeColor = Color.Gray });
            strip.Items.Add(new ToolStripStatusLabel("рівносторонній") { ForeColor = Theme.KindColor("Рівносторонній") });
            strip.Items.Add(new ToolStripStatusLabel("прямокутний") { ForeColor = Theme.KindColor("Прямокутний") });
            strip.Items.Add(new ToolStripStatusLabel("рівнобедрений") { ForeColor = Theme.KindColor("Рівнобедрений") });
            Controls.Add(strip);
        }

        private void BuildTopPanel()
        {
            Panel top = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(235, 240, 241)
            };

            // перший рядок: генерація n трикутників
            top.Controls.Add(new Label
            { Text = "Кількість n:", Left = 12, Top = 18, Width = 105, AutoSize = false });

            tbN = new TextBox { Left = 120, Top = 14, Width = 90, Text = "10" };
            top.Controls.Add(tbN);

            btnGen = new Button
            {
                Text = "Згенерувати",
                Left = 222,
                Top = 12,
                Width = 130,
                Height = 32,
                BackColor = Color.DarkCyan,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGen.FlatAppearance.BorderSize = 0;
            btnGen.Click += (s, e) => SafeRun(Generate);
            top.Controls.Add(btnGen);

            // другий рядок: сортування і фільтр
            top.Controls.Add(new Label
            { Text = "Сортувати за:", Left = 12, Top = 63, Width = 105, AutoSize = false });

            cbSort = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 120,
                Top = 59,
                Width = 230
            };
            cbSort.Items.AddRange(new object[]
            {
                "ID",
                "Видом (А–Я)",
                "Площею (за зростанням)",
                "Площею (за спаданням)",
                "Периметром (за зростанням)",
                "Периметром (за спаданням)"
            });
            top.Controls.Add(cbSort);

            top.Controls.Add(new Label
            { Text = "Показати:", Left = 370, Top = 63, Width = 80, AutoSize = false });

            cbFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 452,
                Top = 59,
                Width = 200
            };
            cbFilter.Items.AddRange(new object[]
            {
                "Усі",
                "Рівносторонні",
                "Прямокутні",
                "Рівнобедрені"
            });
            top.Controls.Add(cbFilter);

            Button btnReset = new Button
            {
                Text = "Скинути",
                Left = 664,
                Top = 57,
                Width = 90,
                Height = 32,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) =>
            {
                cbSort.SelectedIndex = 0;
                cbFilter.SelectedIndex = 0;
                RefreshGrid();
            };
            top.Controls.Add(btnReset);

            Controls.Add(top);
        }

        private Button MakeButton(string text, Color back, EventHandler onClick)
        {
            Button b = new Button
            {
                Text = text,
                Width = 150,
                Height = 38,
                Margin = new Padding(0, 0, 8, 0),
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(Font, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        // ---------- Таблиця: сортування, фільтр, оформлення ----------
        private static int Compare(int mode, TTriangle a, TTriangle b)
        {
            int r;
            switch (mode)
            {
                case 1: r = string.Compare(a.Kind, b.Kind, StringComparison.CurrentCultureIgnoreCase); break;
                case 2: r = a.Area.CompareTo(b.Area); break;
                case 3: r = b.Area.CompareTo(a.Area); break;
                case 4: r = a.Perimeter.CompareTo(b.Perimeter); break;
                case 5: r = b.Perimeter.CompareTo(a.Perimeter); break;
                default: r = a.Id.CompareTo(b.Id); break;
            }
            return r != 0 ? r : a.Id.CompareTo(b.Id);
        }

        // Перебудовує таблицю з урахуванням фільтра та сортування, оновлює результати
        private void RefreshGrid(int selectId = -1)
        {
            if (selectId < 0)
            {
                TTriangle cur = SelectedItem();
                if (cur != null) selectId = cur.Id;
            }

            int f = cbFilter.SelectedIndex;
            List<TTriangle> view = col.All
                .Where(t => f <= 0 ||
                            (f == 1 && t is TEquilateral) ||
                            (f == 2 && t is TRight) ||
                            (f == 3 && t is TIsosceles))
                .ToList();

            int mode = cbSort.SelectedIndex;
            view.Sort((a, b) => Compare(mode, a, b));

            dgv.DataSource = view;

            // повертаємо виділення на потрібний трикутник
            foreach (DataGridViewRow row in dgv.Rows)
            {
                TTriangle t = row.DataBoundItem as TTriangle;
                if (t != null && t.Id == selectId)
                {
                    dgv.CurrentCell = row.Cells[0];
                    row.Selected = true;
                    break;
                }
            }

            lblStatus.Text = string.Format("Показано: {0} з {1}", view.Count, col.All.Count);
            lblResult.Text =
                string.Format("Рівносторонніх: {0},  сума площ = {1:F2}\n", col.CountEquilateral, col.SumAreaEquilateral()) +
                string.Format("Прямокутних: {0},  сума площ = {1:F2}\n", col.CountRight, col.SumAreaRight()) +
                string.Format("Рівнобедрених: {0},  сума периметрів = {1:F2}", col.CountIsosceles, col.SumPerimeterIsosceles());
        }

        private void Dgv_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || !(e.Value is string)) return;
            if (dgv.Columns[e.ColumnIndex].DataPropertyName != "Kind") return;

            e.CellStyle.ForeColor = Theme.KindColor((string)e.Value);
            e.CellStyle.Font = boldFont;
        }

        private TTriangle SelectedItem()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.DataBoundItem as TTriangle;
        }

        // ---------- Операції ----------
        private void SafeRun(Action action)
        {
            try
            {
                action();
            }
            catch (InvalidInputException ex)
            {
                MessageBox.Show(this, ex.Message, "Некоректні дані",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Generate()
        {
            int n;
            if (!int.TryParse(tbN.Text.Trim(), out n) || n <= 0 || n > 10000)
            {
                tbN.Focus();
                throw new InvalidInputException("n має бути цілим числом у межах 1..10000.");
            }
            col.Generate(n);
            RefreshGrid();
        }

        private void AddItem()
        {
            using (TriangleForm f = new TriangleForm(col.NextId(), null))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    col.Add(f.Result);
                    RefreshGrid(f.Result.Id);
                }
            }
        }

        private void EditItem()
        {
            TTriangle t = SelectedItem();
            if (t == null) { NothingSelected(); return; }

            using (TriangleForm f = new TriangleForm(t.Id, t))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    col.Replace(t, f.Result);
                    RefreshGrid(f.Result.Id);
                }
            }
        }

        private void DeleteItem()
        {
            TTriangle t = SelectedItem();
            if (t == null) { NothingSelected(); return; }

            DialogResult answer = MessageBox.Show(this,
                "Видалити трикутник #" + t.Id + " (" + t.Kind + ")?", "Підтвердження",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
            {
                col.Remove(t);
                RefreshGrid();
            }
        }

        private void ViewItem()
        {
            TTriangle t = SelectedItem();
            if (t == null) { NothingSelected(); return; }

            using (DetailsForm f = new DetailsForm(t))
            {
                f.ShowDialog(this);
            }
        }

        private void NothingSelected()
        {
            MessageBox.Show(this, "Спочатку оберіть трикутник у таблиці.", "Трикутники",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
