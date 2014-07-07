using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFTP_Client
{
    class Utils
    {
        public static byte[] partByteArray(byte[] input, int begin, int end)
        {

            byte[] ausschnitt = new byte[end - begin];

            for (int i = 0; i < end - begin; i++)
            {
                ausschnitt[i] = input[begin + i];
            }
            return ausschnitt;
        }

        public static byte[] concatByteArrays(byte[] first, byte[] second)
        {
            byte[] newbarray = new byte[first.Length + second.Length];

            for (int i = 0; i < first.Length; i++)
            {
                newbarray[i] = first[i];
            }

            for (int i = 0; i < second.Length; i++)
            {
                newbarray[i + first.Length] = second[i];
            }

            return newbarray;

        }

    }
}
