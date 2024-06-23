using JumpKing.SaveThread;
using System.IO;

namespace MoreSaves.Patching
{
    public static class XmlWrapper
    {
        private static readonly char SEP;

        static XmlWrapper()
        {
            SEP = Path.DirectorySeparatorChar;
        }

        public static void Serialize(GeneralSettings generalSettings, params string[] folders)
        {
            string path = BuildAndCreatePath(folders) + SEP + ModStrings.SETTINGS;
            XmlSerializerHelper.Serialize(path, generalSettings);
        }

        /// <summary>
        /// Builds a path from given folders, starting from the path to the dll. If the path doesn't exists creates it.
        /// </summary>
        /// <param name="folders">The folders making up the path to the save, starting from the path to the dll</param>
        /// <returns>The path</returns>
        private static string BuildAndCreatePath(params string[] folders)
        {
            string path = ModEntry.dllDirectory;
            foreach (string folder in folders)
            {
                path += folder + SEP;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            return path;
        }
    }
}
