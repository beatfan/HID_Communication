using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HID_SIMPLE.SpecialFunction
{
    public class ClassString
    {
        public static byte[] HexStringToBytes(string HexString)
        {
            try
            {
                List<string> ss = HexString.Split(',').ToList();
                for (int i = 0; i < ss.Count; i++)
                {
                    if (string.IsNullOrEmpty(ss[i]))
                    {
                        ss.RemoveAt(i);
                        i--;
                    }
                }

                byte[] bytes = new byte[ss.Count()];
                for (int i = 0; i < ss.Count; i++)
                {
                    bytes[i] = Convert.ToByte(ss[i], 16);
                }
                return bytes;
            }
            catch
            {
                return null;
            }

        }

    }
}
