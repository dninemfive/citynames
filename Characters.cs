using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace citynames;
public static class Characters
{
    public const char NULL = '\0';
    /// <summary>
    /// The ETX (End-of-Text) character in ASCII. Used to mark the end of a word,
    /// which allows randomly-generated words to break in positions which make sense.
    /// </summary>
    public const char STOP = (char)3;
}
