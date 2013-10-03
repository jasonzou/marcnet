using System;
using System.Collections.Generic;
using System.Text;

namespace MarcNet
{
    class Field008
    {
        char[] _buff = new char[40];

        // 00-05 Format: yymmdd
        string _dateOfCreation;

        /// <summary>
        /// 06: Type of Publication date
        /// s: single year; date1: 4 digit year, date2: blank
        /// m: multiple years; date1: beginning, date2: ending
        /// r: reprint & original; date1: reprint, date2: original
        /// c: current periodical date1: beginning, date2: "9999"
        /// d: dead periodical date1: beginning, date2: ending
        /// " " 
        /// </summary>
        char _typeOfPulibcationDate;

        /// <summary>
        /// 07 -10: 4 digit year
        /// </summary>
        string _date1;

        /// <summary>
        /// 11-14: 4 digit year
        /// </summary>
        string _date2;

        /// <summary>
        /// 15-17: Country of Publication Code
        /// default: onc (Ontario)
        /// </summary>
        string _countryOfPublicationCode;

        /// <summary>
        /// 35-37: Language of publication code
        /// default: eng (English)
        /// </summary>
        string _languageOfPublicationCode;

        public Field008()
        {
            for (int i = 0; i < 40; i++)
            {
                _buff[i] = ' ';
            }
        }
    }
}
