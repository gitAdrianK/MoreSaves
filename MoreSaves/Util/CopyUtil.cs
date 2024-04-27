using System.IO;
using System.Linq;
using System.Reflection;

namespace MoreSaves.Util
{
    public class CopyUtil
    {
        private static readonly char SEP = Path.DirectorySeparatorChar;

        private static readonly string SAVES = "Saves";
        private static readonly string SAVES_PERMA = "SavesPerma";

        private static readonly string CONTENT_SAVES = $"Content{SEP}{SAVES}{SEP}";
        private static readonly string CONTENT_SAVES_PERMA = $"Content{SEP}{SAVES_PERMA}{SEP}";

        /// <summary>
        /// Copys the combined.sav and attempt_stats.stat files out from the Jump King directory.
        /// </summary>
        /// <param name="folders">The folder they are supposed to be copied into. Where every string is a subfolder.</param>
        public static void CopyOutSaves(params string[] folders)
        {
            string from = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{SEP}";

            string into = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{SEP}";

            string intoFolder = into;
            foreach (string folder in folders)
            {
                intoFolder += $"{folder}{SEP}";
            }

            if (!Directory.Exists(intoFolder))
            {
                Directory.CreateDirectory(intoFolder);
            }
            if (!Directory.Exists($"{intoFolder}{SEP}{SAVES}"))
            {
                Directory.CreateDirectory($"{intoFolder}{SEP}{SAVES}");
            }
            if (!Directory.Exists($"{intoFolder}{SEP}{SAVES_PERMA}"))
            {
                Directory.CreateDirectory($"{intoFolder}{SEP}{SAVES_PERMA}");
            }

            foreach (string filePath in Directory.GetFiles($"{from}{CONTENT_SAVES}"))
            {
                string file = filePath.Split(SEP).Last();
                // As far as I know, the combined and attempt_stats.stat file are enough
                // to copy. But I'll add something of a easily extendable "filter" in
                // case I need to save something else.
                if (file != "combined.sav")
                {
                    continue;
                }
                File.Copy(
                    filePath,
                    $"{intoFolder}{SAVES}{SEP}{file}",
                    true
                );
            }
            foreach (string filePath in Directory.GetFiles($"{from}{CONTENT_SAVES_PERMA}"))
            {
                string file = filePath.Split(SEP).Last();
                // See above comment.
                if (file != "attempt_stats.stat")
                {
                    continue;
                }
                File.Copy(
                    filePath,
                    $"{intoFolder}{SAVES_PERMA}{SEP}{file}",
                    true
                );
            }
        }

        /// <summary>
        /// Copys the combined.sav and attempt_stats.stat files into the Jump King directory.
        /// </summary>
        /// <param name="folders">The directory they are supposed to be copied from. Where every string is a subfolder.</param>
        public static void CopyInSaves(params string[] folders)
        {
            string into = $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{SEP}";

            if (!Directory.Exists($"{into}{CONTENT_SAVES}") || !Directory.Exists($"{into}{CONTENT_SAVES_PERMA}"))
            {
                return;
            }

            string from = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{SEP}";
            string fromFolder = from;
            foreach (string folder in folders)
            {
                fromFolder += $"{folder}{SEP}";
            }

            foreach (string filePath in Directory.GetFiles($"{fromFolder}{SEP}{SAVES}"))
            {
                string file = filePath.Split(SEP).Last();
                File.Copy(
                    filePath,
                    $"{into}{CONTENT_SAVES}{SEP}{file}",
                    true
                );
            }
            foreach (string filePath in Directory.GetFiles($"{fromFolder}{SEP}{SAVES_PERMA}"))
            {
                string file = filePath.Split(SEP).Last();
                File.Copy(
                    filePath,
                    $"{into}{CONTENT_SAVES_PERMA}{SEP}{file}",
                    true
                );
            }
        }
    }
}
