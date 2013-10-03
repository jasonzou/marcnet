using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MarcNet
{
    public class MARCTag
    {
        /// <summary>
        /// This class is for MARC tags.
        /// </summary>
        
        private char[] buff = new char[3] { '0', '0', '0' };
        public string _warn;

        public MARCTag(string str)
        {
            bool test;

            test = Regex.IsMatch(str, "^[0-9A-Z]{3}$");
            test |= Regex.IsMatch(str, "^[0-9a-z]{3}$");
            if (test)
            {
                str.CopyTo(0, buff, 0, 3);
            }
            else
            {
                _warn = "Invalid Tagno";
            }
        }

        public string tagStr
        {
            get
            {
                string temp;
                temp = new String(buff);
                return temp;
            }

        }

        public bool isControl()
        {
            if (_warn != null)
            {
                return false;
            }
            else
            {
                if (buff[0] == '0' && buff[1] == '0')
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool isData()
        {
            if (buff[0] == '0' || buff[1] == '0')
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //void clear()
        //{
        //}

        //public MARCTag operator =(MARCTag ref mt)
        //{

        //}

        //MARCTag operator=(const string & str);
        //MARCTag operator=(const char * szStr);
        //bool operator<(const MARCTag & mt) const;
        //bool operator==(const MARCTag & mt) const;
    }

}
