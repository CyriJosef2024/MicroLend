using System.Windows.Forms;

namespace MicroLend.UI
{
    public class LendersForm : Form
    {
        public LendersForm()
        {
            Text = "Lenders";
            Width = 600;
            Height = 400;
            var lbl = new Label { Text = "Lenders management (placeholder)", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            Controls.Add(lbl);
        }
    }
}