using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Lab8_TrianglesWinForms
{
    // Окреме вікно для додавання (existing == null) та редагування трикутника
    public class TriangleForm : Form
    {
        private readonly int id;
        private ComboBox cbKind;
        private TextBox tbP1, tbP2;
        private Label lbP1, lbP2;
        private Button btnOk, btnCancel;

        // Результат (новий або змінений трикутник) після натискання «Зберегти»
        public TTriangle Result { get; private set; }

        public TriangleForm(int id, TTriangle existing)
        {
            this.id = id;

            Text = existing == null ? "Додавання трикутника" : "Редагування трикутника (ID " + id + ")";
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(410, 220);

            cbKind = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cbKind.Items.AddRange(new object[] { "Рівносторонній", "Прямокутний", "Рівнобедрений" });
            tbP1 = new TextBox();
            tbP2 = new TextBox();

            AddRow("Вид", cbKind, 20, out Label dummy);
            AddRow("Параметр 1", tbP1, 62, out lbP1);
            AddRow("Параметр 2", tbP2, 104, out lbP2);

            btnOk = MakeButton("Зберегти", Color.SeaGreen, 160, 165);
            btnOk.Click += BtnOk_Click;
            btnCancel = MakeButton("Скасувати", Color.Gray, 282, 165);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            cbKind.SelectedIndexChanged += (s, e) => UpdateCaptions();
            cbKind.SelectedIndex = 0;

            if (existing != null)
            {
                if (existing is TEquilateral)
                {
                    cbKind.SelectedIndex = 0;
                    tbP1.Text = Fmt(existing.A);
                }
                else if (existing is TRight)
                {
                    cbKind.SelectedIndex = 1;
                    tbP1.Text = Fmt(existing.A);
                    tbP2.Text = Fmt(existing.B);
                }
                else if (existing is TIsosceles)
                {
                    cbKind.SelectedIndex = 2;
                    tbP1.Text = Fmt(existing.C);   // основа
                    tbP2.Text = Fmt(existing.A);   // бічна сторона
                }
            }
        }

        private static string Fmt(double v)
        {
            return v.ToString("0.##", CultureInfo.InvariantCulture);
        }

        // Підписи полів залежать від вибраного виду трикутника
        private void UpdateCaptions()
        {
            switch (cbKind.SelectedIndex)
            {
                case 0:
                    lbP1.Text = "Сторона (a)";
                    lbP2.Text = "—";
                    tbP2.Enabled = false;
                    break;
                case 1:
                    lbP1.Text = "Катет 1";
                    lbP2.Text = "Катет 2";
                    tbP2.Enabled = true;
                    break;
                default:
                    lbP1.Text = "Основа";
                    lbP2.Text = "Бічна сторона";
                    tbP2.Enabled = true;
                    break;
            }
        }

        private void AddRow(string caption, Control c, int y, out Label label)
        {
            label = new Label
            {
                Text = caption,
                Left = 20,
                Top = y + 4,
                Width = 150,
                Height = 24,
                AutoSize = false
            };
            Controls.Add(label);
            c.Left = 180;
            c.Top = y;
            c.Width = 210;
            Controls.Add(c);
        }

        private Button MakeButton(string text, Color back, int left, int top)
        {
            Button b = new Button
            {
                Text = text,
                Left = left,
                Top = top,
                Width = 112,
                Height = 36,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static double ParseDouble(string s, string field)
        {
            double v;
            if (!double.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float,
                                 CultureInfo.InvariantCulture, out v))
                throw new InvalidInputException("Поле «" + field + "» має бути числом.");
            return v;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Control bad = tbP1;      // поле, у якому сталася помилка
            try
            {
                TTriangle t;
                bad = tbP1;
                double p1 = ParseDouble(tbP1.Text, lbP1.Text);
                switch (cbKind.SelectedIndex)
                {
                    case 0:
                        t = new TEquilateral(p1);
                        break;
                    case 1:
                        bad = tbP2;
                        t = new TRight(p1, ParseDouble(tbP2.Text, lbP2.Text));
                        break;
                    default:
                        bad = tbP2;
                        t = new TIsosceles(p1, ParseDouble(tbP2.Text, lbP2.Text));
                        break;
                }
                t.Id = id;

                Result = t;
                DialogResult = DialogResult.OK;     // закриває вікно
            }
            catch (InvalidInputException ex)
            {
                MessageBox.Show(this, ex.Message, "Некоректні дані",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                bad.Focus();
            }
        }
    }
}
