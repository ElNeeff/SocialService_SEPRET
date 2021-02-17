using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace SEPRET.CustomClasses
{
    public static class Site
    {
        public static string ToTitleCase(this string text)
        {
            if (text != null)
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
            }
            return null;
        }
    }
}