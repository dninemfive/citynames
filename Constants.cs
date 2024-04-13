namespace citynames;
public static class Characters
{
    public const char START = (char)2;
    /// <summary>
    /// The ETX (End-of-Text) character in ASCII. Used to mark the end of a word, which allows
    /// randomly-generated words to break in positions which make sense.
    /// </summary>
    public const char STOP = (char)3;
}
public static class Defaults
{
    public const int CONTEXT_LENGTH = 2;
}