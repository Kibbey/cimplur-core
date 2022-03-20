using System;
using System.IO;

namespace Memento.Libs
{

    public static class LoadDotEnv
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(new[] { '=' }, 2);
                /*
                line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);*/

                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}