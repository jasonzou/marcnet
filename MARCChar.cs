using System;
using System.Collections.Generic;
using System.Text;

namespace MarcNet
{
    public class MARCChar
    {
        public const Char SPACE = '\x20';
        public const Char SUBFIELD_INDICATOR = '\x1F';
        public const Char END_OF_FIELD = '\x1E';
        public const Char END_OF_RECORD = '\x1D';
        public const int DIRECTORY_ENTRY_LEN = 12;
        public const int LEADER_LEN = 24;
    }
}
