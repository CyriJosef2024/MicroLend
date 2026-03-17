namespace MicroLend.UI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            using (var login = new LoginForm())
            {
                var dr = login.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    Application.Run(new Form1());
                }
            }
        }
    }
}