using System;

namespace Shuttle.Esb
{
    public class InvalidSchemeException : Exception
    {
        public InvalidSchemeException(string supportedScheme, string invalidUri) : base(string.Format(EsbResources.InvalidSchemeException, supportedScheme, invalidUri))
        {
        }
    }
}