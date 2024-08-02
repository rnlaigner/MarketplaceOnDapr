﻿namespace Common.Infra;

public interface ILogging : IDisposable
{
    void Clear() { }

    void Append(object item) { }
}

