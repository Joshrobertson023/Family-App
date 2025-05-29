using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccessLibrary
{
    public static class ReferenceParse
    {
        public static List<string> ConvertToReferenceParts(string reference)
        {
            List<string> components = new List<string>();
            string builder = "";

            for (int i = 0; i < reference.Length; i++)
            {
                builder += reference[i];

                if (i < reference.Length - 1)
                {
                    if (reference[i+1] == ' ' || reference[i+1] == ':')
                    {
                        components.Add(builder);
                        builder = "";
                        i++;
                    }
                }
            }
            components.Add(builder);

            return components;
        }

        public static string ConvertToReferenceString(string book, int chapter, List<int> verses)
        {
            string returnString = "";

            returnString += book + " " + chapter.ToString() + ":";

            if (verses.Count > 1)
            {
                for (int i = 0; i < verses.Count; i++)
                {
                    returnString += verses[i].ToString();
                    if (i < verses.Count - 1)
                        returnString += ",";
                }
            }
            else
            {
                returnString += verses[0].ToString();
            }

            return returnString;
        }
    }
}
