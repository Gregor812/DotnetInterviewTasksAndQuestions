﻿using System;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockWriter<in T> : IDisposable
    {
        void WriteFileBlock(T block, string filename, CancellationToken ct);
    }
}
