using MicroLend.DAL.Entities;
using MicroLend.DAL.Repositories;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MicroLend.UI
{
    public class BorrowersForm : Form
    {
        private readonly BorrowerRepository _borrowerRepo = new BorrowerRepository();
        private readonly UserRepository _userRepo = new UserRepository();

        private DataGridView dgv;
        private Button btnNew;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnSignup;

        public BorrowersForm()
        {
            Text = "Borrowers";
            Width = 800;
            Height = 520;

            dgv = new DataGridView { Dock = DockStyle.Top, Height = 340, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoGenerateColumns = false };
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Id", DataPropertyName = "Id", Width = 50 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", Width = 220 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Contact", DataPropertyName = "ContactNumber", Width = 150 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Income", DataPropertyName = "MonthlyIncome", Width = 120 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Business", DataPropertyName = "BusinessType", Width = 200 });

            btnNew = new Button { Text = "New", Location = new Point(24, 360), Size = new Size(90, 32) };
            btnEdit = new Button { Text = "Edit", Location = new Point(120, 360), Size = new Size(90, 32) };
            btnDelete = new Button { Text = "Delete", Location = new Point(216, 360), Size = new Size(90, 32) };
            btnSignup = new Button { Text = "Sign up user/borrower", Location = new Point(312, 360), Size = new Size(180, 32) };

            btnNew.Click += async (s, e) => await OnNew();
            btnEdit.Click += async (s, e) => await OnEdit();
            btnDelete.Click += async (s, e) => await OnDelete();
            btnSignup.Click += async (s, e) => await OnSignup();

            Controls.Add(dgv);
            Controls.Add(btnNew);
            Controls.Add(btnEdit);
            Controls.Add(btnDelete);
            Controls.Add(btnSignup);

            Load += async (s, e) => await RefreshGrid();
        }

        private async Task RefreshGrid()
        {
            var list = await _borrowerRepo.GetAllAsync();
            dgv.DataSource = list.Select(b => new
            {
                b.Id,
                b.Name,
                b.ContactNumber,
                MonthlyIncome = b.MonthlyIncome.ToString("C"),
                b.BusinessType
            }).ToList();
        }

        private Borrower? GetSelectedBorrower()
        {
            if (dgv.CurrentRow == null) return null;
            var idProp = dgv.CurrentRow.DataBoundItem.GetType().GetProperty("Id");
            if (idProp == null) return null;
            var id = (int)idProp.GetValue(dgv.CurrentRow.DataBoundItem)!;
            return _borrowerRepo.GetByIdAsync(id).Result;
        }

        private async Task OnNew()
        {
            using var f = new BorrowerEditForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                var b = f.Borrower;
                await _borrowerRepo.AddAsync(b);
                await RefreshGrid();
            }
        }

        private async Task OnEdit()
        {
            var sel = GetSelectedBorrower();
            if (sel == null) { MessageBox.Show("Select a borrower to edit."); return; }
            using var f = new BorrowerEditForm(sel);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                await _borrowerRepo.UpdateAsync(f.Borrower);
                await RefreshGrid();
            }
        }

        private async Task OnDelete()
        {
            var sel = GetSelectedBorrower();
            if (sel == null) { MessageBox.Show("Select a borrower to delete."); return; }
            if (MessageBox.Show($"Delete borrower '{sel.Name}'?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await _borrowerRepo.DeleteAsync(sel.Id);
            await RefreshGrid();
        }

        private async Task OnSignup()
        {
            using var f = new SignupForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                // create user then borrower
                var u = new MicroLend.DAL.Entities.User { Username = f.Username.Trim(), PasswordHash = f.Password, Role = f.Role };
                await _userRepo.AddAsync(u);
                var b = new Borrower { Name = f.FullName, ContactNumber = f.Contact, MonthlyIncome = f.MonthlyIncome, BusinessType = f.BusinessType, UserId = u.Id };
                await _borrowerRepo.AddAsync(b);
                await RefreshGrid();
                MessageBox.Show("User and borrower created.");
            }
        }
    }
}