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

            // If an existing DB is present but missing columns from the current model
            // (common during active development), recreate the DB so the model and
            // schema match. We specifically check for the Repayments.PaymentMethod
            // column which previously caused save errors.
            try
            {
                var dbPath = System.IO.Path.Combine(AppContext.BaseDirectory, "MicroLend.db");
                if (System.IO.File.Exists(dbPath))
                {
                    try
                    {
                        var conn = ctx.Database.GetDbConnection();
                        conn.Open();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "PRAGMA table_info('Repayments');";
                        using var rdr = cmd.ExecuteReader();
                        var hasPaymentMethod = false;
                        while (rdr.Read())
                        {
                            var col = rdr[1]?.ToString(); // second column is name
                            if (string.Equals(col, "PaymentMethod", StringComparison.OrdinalIgnoreCase))
                            {
                                hasPaymentMethod = true;
                                break;
                            }
                        }
                        conn.Close();

                        if (!hasPaymentMethod)
                        {
                            // Remove DB so it will be recreated with current model during EnsureCreated
                            try { System.IO.File.Delete(dbPath); } catch { }
                        }
                    }
                    catch
                    {
                        // ignore any introspection errors and let EnsureCreated try to proceed
                        try { ctx.Database.CloseConnection(); } catch { }
                    }
                }
            }
            catch { }

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