using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niawa.Utilities
{
    public class InlineSortedListCreator
    {
        public static SortedList<string, string> CreateStrStr(string key, string value)
        {
            SortedList<string, string> list = new SortedList<string, string>();
            list.Add(key, value);
            return list;
        }

    }
}
