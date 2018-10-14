using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataTablesServerSide.Utils
{
    public static class StringExtensions
    {
        public static bool IsBlank(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        public static bool IsNotBlank(this string s)
        {
            return !s.IsBlank();
        }
        public static string SplitUppercase(this string s, string splitString = " ")
        {
            return Regex.Replace(s, @"(?<=[A-Za-z])(?=[A-Z][a-z])|(?<=[a-z0-9])(?=[0-9]?[A-Z])", splitString);
        }
    }
}
