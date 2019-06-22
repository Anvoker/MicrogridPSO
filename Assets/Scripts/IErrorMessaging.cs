using System;

namespace SSM
{
    public interface IErrorMessaging
    {
        Action<string> messageHandler { get; set; }
    }
}