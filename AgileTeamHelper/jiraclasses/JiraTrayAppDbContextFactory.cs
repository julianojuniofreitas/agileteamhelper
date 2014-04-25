using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace JiraTrayApp
{
    public static class JiraTrayAppDbContextFactory
    {
        private static string _dbPath = null;

        public static JiraTrayAppDbContext Create()
        {
            if (String.IsNullOrEmpty(_dbPath))
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                string jiraTrayAppDataPath = Path.Combine(localAppDataPath, "JiraTrayApp");

                if (!Directory.Exists(jiraTrayAppDataPath))
                {
                    Directory.CreateDirectory(jiraTrayAppDataPath);
                }

                _dbPath = Path.Combine(jiraTrayAppDataPath, "JiraTrayApp.sdf");
            }

            return new JiraTrayAppDbContext("Data Source=" + _dbPath);
        }
    }
}
