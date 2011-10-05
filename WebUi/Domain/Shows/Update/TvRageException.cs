using System;

namespace AlunTv.Test
{
    public class TvRageException : Exception
    {
        public TvRageException(Exception iException) : base(iException.Message, iException)
        {
            
        }
    }
}