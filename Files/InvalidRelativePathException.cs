namespace Microsoft.OpenPublishing.Build.Common.Files
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidRelativePathException : Exception
    {
        public string InvalidRelativePath { get; set; }

        public InvalidRelativePathException(string message, string invalidRelativePath) : base(message)
        {
            InvalidRelativePath = invalidRelativePath;
        }

        public InvalidRelativePathException(string message, string invalidRelativePath, Exception inner) : base(message, inner)
        {
            InvalidRelativePath = invalidRelativePath;
        }

        public InvalidRelativePathException(string message) : base(message)
        {
        }

        public InvalidRelativePathException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidRelativePathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            InvalidRelativePath = info.GetString(nameof(InvalidRelativePath));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(InvalidRelativePath), InvalidRelativePath);
        }
    }
}
