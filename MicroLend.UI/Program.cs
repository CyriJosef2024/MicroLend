namespace MicroLend.UI
{
    using MicroLend.DAL;
    using Microsoft.EntityFrameworkCore;

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
    [STAThread]
    static async Task Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        try
        {
            // Ensure database schema is created and seed data is present
            using var ctx = new MicroLendDbContext();
            ctx.Database.EnsureCreated();
            await DataSeeder.SeedAsync(ctx);
        }
        catch (Exception ex)
        {
            try
            {
                // write full exception to temp file for easier inspection
                var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MicroLend_db_error.txt");
                System.IO.File.WriteAllText(tmp, ex.ToString());
                MessageBox.Show("Database initialization error: " + ex.Message + "\nFull details written to: " + tmp, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                MessageBox.Show("Database initialization error: " + ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        Application.Run(new Form1());
    }
    }
}