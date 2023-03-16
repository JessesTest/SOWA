using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PE.BL.Extensions
{
    public static class NameUtility
    {
        public static string[] ParseName(string name)
        {
            string[] parsedName = new string[3];
            string[] splitComma = name.Split(',');

            // If name format is [Last, First Middle]
            if (splitComma.Length > 1)
            {
                parsedName[2] = splitComma[0];
                string[] splitName = splitComma[1].Trim().Split(' ');

                switch (splitName.Length)
                {
                    case 0:
                        break;
                    case 1:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        break;
                    case 2:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        break;
                    default:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        for (int i = 2; i < splitName.Length; i++)
                        {
                            parsedName[1] += splitName[i];
                        }
                        break;
                }
            }
            // If name format is [First Middle Last]
            else if (splitComma.Length == 1)
            {
                string[] splitName = name.Split(' ');

                switch (splitName.Length)
                {
                    case 0:
                        break;
                    case 1:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        parsedName[2] = string.Empty;
                        break;
                    case 2:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        parsedName[2] = splitName[1];
                        break;
                    case 3:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        parsedName[2] = splitName[2];
                        break;
                    default:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        parsedName[2] = splitName[2];
                        for (int i = 3; i < splitName.Length; i++)
                        {
                            parsedName[2] += splitName[i];
                        }
                        break;
                }
            }

            return parsedName;
        }

        public static string FormatName(params string[] strParams)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < strParams.Length; i++)
            {
                if (i != 0 && !string.IsNullOrEmpty(strParams[i]))
                {
                    sb.Append(" ");
                }
                sb.Append(strParams[i]);
            }

            return sb.ToString();
        }
    }
}
