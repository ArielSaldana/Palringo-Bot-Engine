/*
 * 
 * By: Ariel Saldana
 * Released under the MIT License
 * https://github.com/arielsaldana
 * http://ahhriel.com
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PalringoBotEngine
{
    internal static class Extensions
    {
        public static int IndexOf(this byte[] array, int value, int startPos = 0)
        {
            for (int i = startPos; i < array.Length; i++)
            {
                if (array[i] == value)
                    return i;
            }
            return -1;
        }

        public static bool IsNumeric(this string input)
        {
            int output;
            return int.TryParse(input, out output);
        }
    }
}
