using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentRiskScanner
{
    public class Risk
    {
        public Match Certainty { get; set; }
        public string Term { get; set; }
    }

    public class FileReport
    {
        public string Path { get; set; }
        public int TotalRiskLevel { get; set; }
        public List<Risk> Risks { get; set; }
    }

    public class Program
    {
        private static string _path;
        private static bool _isRecursive;
        private static string _whitelist;
        private static string[] _documentTypes;
        private static string[] _infoTypes;
        private static int _sensitivity;

        private static KeywordSearch _keywordSearch;
        private static NameSearch _nameSearch;

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var argsList = args.ToList();
                _path = argsList.Contains("--path") ? argsList[argsList.IndexOf("--path") + 1] : AppDomain.CurrentDomain.BaseDirectory;
                _isRecursive = argsList.Contains("--recursive");
                _whitelist = argsList.Contains("--whitelist") ? argsList[argsList.IndexOf("--whitelist") + 1] : null;
                _documentTypes = argsList.Contains("--document-types") ? argsList[argsList.IndexOf("--document-types") + 1].Split(',') : null;
                _infoTypes = argsList.Contains("--info-types") ? argsList[argsList.IndexOf("--info-types") + 1].Split(',') : new []{"keyword","email","name","phone","card"};
                _sensitivity = argsList.Contains("--sensitivity")
                    ? int.Parse(argsList[argsList.IndexOf("--sensitivity") + 1])
                    : 5;
            }

            if (_infoTypes.Contains("keyword"))
            {
                _keywordSearch = new KeywordSearch("keyword_dict.txt");
            }

            if (_infoTypes.Contains("name"))
            {
                _nameSearch = new NameSearch("nam_dict.txt");
            }

            List<FileReport> reports = ScanFilesInDirectory(_path);

            // okay, what do we do with the reports?
        }

        public static List<FileReport> ScanFilesInDirectory(string path)
        {
            List<FileReport> reports = new List<FileReport>();
            if (_isRecursive)
            {
                var subDirs = Directory.GetDirectories(path);
                foreach (var subDir in subDirs)
                {
                    reports.AddRange(ScanFilesInDirectory(subDir));
                }
            }

            var filePaths = Directory.GetFiles(path).Where(f => _documentTypes.Contains(f.Split('.').Last()));
            foreach (var filePath in filePaths)
            {
                reports.Add(ScanFile(filePath));
            }
            return reports;
        }

        public static FileReport ScanFile(string filePath)
        {
            // hmm, parsing
        }
    }
}
