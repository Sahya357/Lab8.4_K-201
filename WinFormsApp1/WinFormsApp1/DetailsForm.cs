using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab8_TrianglesWinForms
{
    // Окреме вікно для виведення повної інформації про трикутник
    public class DetailsForm : Form
    {
        public DetailsForm(TTriangle t)
        {
            Text = "Інформація про трикутник";
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(430, 360);

            Controls.Add(new Label
            {
                Text = t.Kind + " трикутник",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Theme.KindColor(t.Kind),
                Left = 20,
                Top = 15,
                Width = 390,
                Height = 40,
                AutoSize = false,
                AutoEllipsis = true
            });

            AddInfo("Ідентифікатор", "#" + t.Id, Color.DimGray, 70);
            AddInfo("Сторона a", t.A.ToString("F2"), Color.Black, 105);
            AddInfo("Сторона b", t.B.ToString("F2"), Color.Black, 140);
            AddInfo("Сторона c", t.C.ToString("F2"), Color.Black, 175);
            AddInfo("Площа", t.CalcArea().ToString("F2"), Color.SeaGreen, 210);
            AddInfo("Периметр", t.CalcPerimeter().ToString("F2"), Color.DarkCyan, 245);

            Button btnClose = new Button
            {
                Text = "Закрити",
                Left = 298,
                Top = 305,
                Width = 112,
                Height = 36,
                BackColor = Color.DarkCyan,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnClose.FlatAppearance.BorderSize = 0;
            Controls.Add(btnClose);
            CancelButton = btnClose;
            AcceptButton = btnClose;
        }

        private void AddInfo(string caption, string value, Color color, int y)
        {
            Controls.Add(new Label
            {
                Text = caption + ":",
                ForeColor = Color.Gray,
                Left = 20,
                Top = y,
                Width = 170,
                Height = 26,
                AutoSize = false
            });
            Controls.Add(new Label
            {
                Text = value,
                ForeColor = color,
                Font = new Font(Font, FontStyle.Bold),
                Left = 195,
                Top = y,
                Width = 215,
                Height = 26,
                AutoSize = false,
                AutoEllipsis = true
            });
        }
    }
}
