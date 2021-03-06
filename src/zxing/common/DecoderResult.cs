/*
* Copyright 2007 ZXing authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;

namespace ZXing.Common
{
   /// <summary>
   /// Encapsulates the result of decoding a matrix of bits. This typically
   /// applies to 2D barcode formats. For now it contains the raw bytes obtained,
   /// as well as a String interpretation of those bytes, if applicable.
   /// </summary>
   public sealed class DecoderResult
   {
      public byte[] RawBytes { get; private set; }

      public String Text { get; private set; }

      public ArrayList ByteSegments { get; private set; }

      public String ECLevel { get; private set; }

      public DecoderResult(byte[] rawBytes, String text, ArrayList byteSegments, String ecLevel)
      {
         if (rawBytes == null && text == null)
         {
            throw new ArgumentException();
         }
         RawBytes = rawBytes;
         Text = text;
         ByteSegments = byteSegments;
         ECLevel = ecLevel;
      }
   }
}