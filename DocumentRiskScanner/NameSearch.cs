using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentRiskScanner
{
    public class NameSearch
    {
        private HashSet<string> _names;

        public NameSearch(string dictPath)
        {
            _names = new HashSet<string>();
            try
            {
                using (StreamReader reader = new StreamReader(dictPath))
                {
                    while (reader.Peek() != -1)
                    {
                        var nextLine = reader.ReadLine();
                        if (nextLine[0] != '#')
                        {
                            var nameOnLine = nextLine.Split(' ')[1];

                            // Names with a + can be formatted in multiple ways,
                            // but we're in a rush so I'm going to just pretend it's a hyphen
                            nameOnLine = nameOnLine.Replace('+', '-');
                            _names.Add(nameOnLine);
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

        public bool Contains(string searchName)
        {
            return _names.Contains(searchName);
        }
    }
}
