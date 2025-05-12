namespace NailBot.Core.Exceptions
{
    internal class NullTaskException : Exception
    {
        public NullTaskException(string message)
            : base(message)
        {
        }
    }
}
