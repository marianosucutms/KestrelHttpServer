// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;

namespace Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2
{
    public static class Http2FrameReader
    {
        public static bool ReadFrame(ReadOnlySequence<byte> readableBuffer, Http2Frame frame, uint maxFrameSize, out SequencePosition consumed, out SequencePosition examined)
        {
            consumed = readableBuffer.Start;
            examined = readableBuffer.End;

            if (readableBuffer.Length < Http2Frame.HeaderLength)
            {
                return false;
            }

            var headerSlice = readableBuffer.Slice(0, Http2Frame.HeaderLength);
            headerSlice.CopyTo(frame.Raw);

            if (frame.PayloadLength > maxFrameSize)
            {
                throw new Http2ConnectionErrorException(CoreStrings.FormatHttp2ErrorFrameOverLimit(frame.PayloadLength, maxFrameSize), Http2ErrorCode.FRAME_SIZE_ERROR);
            }

            if (readableBuffer.Length < Http2Frame.HeaderLength + frame.PayloadLength)
            {
                return false;
            }

            readableBuffer.Slice(Http2Frame.HeaderLength, frame.PayloadLength).CopyTo(frame.Payload);
            consumed = examined = readableBuffer.GetPosition(Http2Frame.HeaderLength + frame.PayloadLength);

            return true;
        }
    }
}
