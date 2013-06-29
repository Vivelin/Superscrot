using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Superscrot
{
    [Serializable]
    public class ConnectionFailedException : Exception
    {
        public ConnectionFailedException() { }
        public ConnectionFailedException(string message) : base(message) { }

        public ConnectionFailedException(string message, string hostname)
            : base(message)
        {
            Hostname = hostname;
        }

        public ConnectionFailedException(string message, Exception inner) : base(message, inner) { }
        public ConnectionFailedException(string message, string hostname, Exception inner)
            : base(message, inner)
        {
            Hostname = hostname;
        }

        protected ConnectionFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public string Hostname { get; private set; }
    }

}
