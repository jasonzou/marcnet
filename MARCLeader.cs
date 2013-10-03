using System;
using System.Collections.Generic;
using System.Text;

namespace MarcNet
{
    enum marcEncoding { charEncoding_MARC8, charEncoding_Unicode, charEncoding_Unknown };

    public class MARCLeader
    {
        int _recordLength;
        char _recordStatus;
        char _recordType;
        char _charCodingScheme;
        char _indicatorCount;
        char _subfieldCodeLength;
        int _baseAddressOfData;
        char[] _buff = new char[MARCChar.LEADER_LEN];
        string _warn = "";

        public MARCLeader(int recLen, int baseAddress)
        {
            _recordLength = recLen;
            _baseAddressOfData = baseAddress;

            for (int i = 0; i < MARCChar.LEADER_LEN; i++)
            {
                _buff[i] = MARCChar.SPACE;
            }

            String temp = String.Format("{0:00000}", recLen);
            temp.CopyTo(0, _buff, 0, 5);

            temp = String.Format("{0:00000}", baseAddress);
            temp.CopyTo(0, _buff, 12, 5);

            _buff[10] = '2';
            _buff[11] = '2';
            _buff[20] = '4';
            _buff[21] = '5';
            _buff[22] = '0';
            _buff[23] = '0';

        }

        public MARCLeader(char[] leader)
        {

            char[] buff = new char[5];

            Array.Copy(leader, 0, buff, 0, 5);
            _recordLength = getNum(buff);

            Array.Copy(leader, 12, buff, 0, 5);
            _baseAddressOfData = getNum(buff);
            Array.Copy(leader, _buff, MARCChar.LEADER_LEN);
            
            if (_baseAddressOfData >= _recordLength)
            {
                _warn = "Invalid Leader";
            }

        }

        public void update(int length, int baseAddress)
        {
            setRecLength(length);
            setBaseAddressOfData(baseAddress);
        }

        private int getNum(char[] charArray)
        {
            int len = charArray.Length;
            int retNum = 0; ;

            for (int i = 0; i < len; i++)
            {
                int temp = (int)(charArray[i] - '0') * (int)Math.Pow(10, (len - i - 1));
                retNum += temp;
            }
            return retNum;
        }

        public char[] asRaw()
        {
            return _buff;
        }

        public int getRecLength()
        {
            return _recordLength;
        }

        private void setRecLength(int length)
        {
            _recordLength = length;

            String temp = String.Format("{0:00000}", length);
            temp.CopyTo(0, _buff, 0, 5);
        }

        public int getBaseAddressOfData()
        {
            return _baseAddressOfData;
        }

        private void setBaseAddressOfData(int baseAddress)
        {
            _baseAddressOfData = baseAddress;

            String temp = String.Format("{0:00000}", baseAddress);
            temp.CopyTo(0, _buff, 12, 5);
        }

        /*

                bool Parse()
                {
                    if (iBufLength < 24) return false;

                    recordLength = GetNum(szBuf, 5);
                    baseAddressOfData = GetNum(szBuf + 12, 5);
                    return true;
                }


                public marcEncoding GetCharEncoding()
                {
                    if (charCodingScheme == '#')
                    {
                        return charEncoding_MARC8;
                    }
                    else if (charCodingScheme == 'a')
                    {
                        return charEncoding_Unicode;
                    }
                    return charEncoding_Unknown;
                }
                */


    }
}
