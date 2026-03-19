using System.Windows.Forms;

namespace MicroLend.UI
{
    public class LoansForm : Form
    {
        public LoansForm()
        {
            Text = "Loans";
            Width = 800;
            Height = 500;
            var lbl = new Label { Text = "Loans management (placeholder)", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            Controls.Add(lbl);
            // fetch all loans from context for display
            var ctx = new MicroLend.DAL.MicroLendDbContext();
            var all = ctx.Loans.ToList();
        }
    }
}