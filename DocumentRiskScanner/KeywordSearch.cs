using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentRiskScanner
{
    public class KeywordSearch
    {
        private HashSet<string> _lowRiskKeywords;
        private HashSet<string> _medRiskKeywords;
        private HashSet<string> _highRiskKeywords;

        public KeywordSearch(string dictPath)
        {
            _lowRiskKeywords = new HashSet<string>();
            _medRiskKeywords = new HashSet<string>();
            _highRiskKeywords = new HashSet<string>();

            try
            {
                using (StreamReader reader = new StreamReader(dictPath))
                {
                    while (reader.Peek() != -1)
                    {
                        var nextLine = reader.ReadLine();
                        if (nextLine[0] != '#')
                        {
                            var keywordOnLine = (Risk: int.Parse(nextLine.Split(' ')[0]), Word: nextLine.Split(' ')[1]);

                            switch (keywordOnLine.Risk)
                            {
                                case 0:
                                    _lowRiskKeywords.Add(keywordOnLine.Word.ToLower());
                                    break;
                                case 1:
                                    _medRiskKeywords.Add(keywordOnLine.Word.ToLower());
                                    break;
                                case 2:
                                    _highRiskKeywords.Add(keywordOnLine.Word.ToLower());
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't open dictionary!");
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public (bool success, int riskLevel) Contains(string searchKeyword)
        {
            var result = (false, -1);
            if (_lowRiskKeywords.Contains(searchKeyword.ToLower()))
            {
                result = (true, 0);
            }
            else if (_medRiskKeywords.Contains(searchKeyword.ToLower()))
            {
                result = (true, 1);
            }
            else if (_highRiskKeywords.Contains(searchKeyword.ToLower()))
            {
                result = (true, 2);
            }
            return result;
        }
    }
}
