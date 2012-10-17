﻿/*
 This program is free software; you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation; either version 2 of the License, or
 (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program; if not, write to the Free Software
 Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Linq;
using System.Text;
using ThalesSim.Core.Cryptography;
using ThalesSim.Core.Cryptography.LMK;

namespace ThalesSim.Core.Utility
{
    public static class Extensions
    {
        #region Hex/binary/byte

        public static bool IsHex(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var chars = text.ToUpper().StripKeyScheme().ToCharArray();

            return !text.Where((t, i) => !char.IsDigit(chars[i]) && (chars[i] < 'A' || chars[i] > 'F')).Any();
        }

        public static bool IsBinary(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            text = text.Replace("1", "").Replace("0", "");
            return (text.Length == 0);
        }

        public static string GetHexString(this byte[] bytes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.GetLength(0); i++)
            {
                sb.AppendFormat("{0:X2}", bytes[i]);
            }

            return sb.ToString();
        }

        public static string GetBinary(this string text)
        {
            if (!text.IsHex())
            {
                throw new InvalidOperationException("Text must be hexadecimal");
            }

            var sb = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                sb.Append(Convert.ToString(Convert.ToInt32(text.Substring(i, 1), 16), 2).PadLeft(4, '0'));
            }
            return sb.ToString();
        }

        public static string FromBinary(this string text)
        {
            if (text.Length % 4 != 0)
            {
                throw new InvalidOperationException("Text length must be divisible by 4");
            }

            if (!text.IsBinary())
            {
                throw new InvalidOperationException(string.Format("String {0} is not binary", text));
            }

            var sb = new StringBuilder();
            for (var i = 0; i < text.Length; i += 4)
            {
                sb.Append(Convert.ToByte(text.Substring(i, 4), 2).ToString("X1"));
            }
            return sb.ToString();
        }

        public static byte[] GetBytes(this string text, Encoding encoding)
        {
            return encoding.GetBytes(text);
        }

        public static byte[] GetBytes(this string text)
        {
            return GetBytes(text,
                            Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage));
        }

        public static string GetString(this byte[] bytes)
        {
            return
                Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage).GetString(
                    bytes);
        }

        public static byte[] GetHexBytes(this string text)
        {
            if (text.Length % 2 != 0)
            {
                throw new InvalidOperationException("Text length must be even");
            }

            if (!text.IsHex())
            {
                throw new InvalidOperationException("Text must be hexadecimal");
            }

            var bytes = new byte[text.Length / 2];
            var index = 0;
            for (var i = 0; i < text.Length; i += 2)
            {
                bytes[index] = Convert.ToByte(text.Substring(i, 2), 16);
                index++;
            }

            return bytes;
        }

        public static string XorHex(this string text, string other)
        {
            text = text.StripKeyScheme();
            other = other.StripKeyScheme();

            if (text.Length != other.Length)
            {
                throw new InvalidOperationException(string.Format("String {0} is different length than {1}", text, other));
            }

            var bText = text.GetHexBytes();
            var bOther = other.GetHexBytes();

            for (var i = 0; i < bText.GetLength(0); i++)
            {
                bText[i] = (byte)(bText[i] ^ bOther[i]);
            }

            return bText.GetHexString();
        }

        #endregion

        #region Key scheme

        public static string GetKeySchemeChar (this KeyScheme scheme)
        {
            switch (scheme)
            {
                case KeyScheme.DoubleLengthKeyAnsi:
                    return "X";
                case KeyScheme.SingleLengthKey:
                    return "Z";
                case KeyScheme.DoubleLengthKeyVariant:
                    return "U";
                case KeyScheme.TripleLengthKeyAnsi:
                    return "Y";
                case KeyScheme.TripleLengthKeyVariant:
                    return "T";
                default:
                    throw new InvalidCastException("Invalid key scheme");
            }
        }

        public static KeyScheme GetKeyScheme (this string text)
        {
            text = text.ToUpper();
            switch (text)
            {
                case "X":
                    return KeyScheme.DoubleLengthKeyAnsi;
                case "Z":
                    return KeyScheme.SingleLengthKey;
                case "U":
                    return KeyScheme.DoubleLengthKeyVariant;
                case "Y":
                    return KeyScheme.TripleLengthKeyAnsi;
                case "T":
                    return KeyScheme.TripleLengthKeyVariant;
                default:
                    throw new InvalidCastException(string.Format("Invalid key scheme {0}", text));
            }
        }

        public static bool StartsWithKeyScheme (this string text)
        {
            text = text.ToUpper();
            return text.StartsWith(GetKeySchemeChar(KeyScheme.DoubleLengthKeyAnsi)) ||
                   text.StartsWith(GetKeySchemeChar(KeyScheme.DoubleLengthKeyVariant)) ||
                   text.StartsWith(GetKeySchemeChar(KeyScheme.SingleLengthKey)) ||
                   text.StartsWith(GetKeySchemeChar(KeyScheme.TripleLengthKeyAnsi)) ||
                   text.StartsWith(GetKeySchemeChar(KeyScheme.TripleLengthKeyVariant));
        }

        public static string StripKeyScheme (this string text)
        {
            return StartsWithKeyScheme(text) ? text.Substring(1) : text;
        }

        #endregion

        #region Parity

        public static bool IsParityOk (this string text, Parity parity)
        {
            if (parity == Parity.None)
            {
                return true;
            }

            text = text.StripKeyScheme();

            if (!text.IsHex())
            {
                throw new InvalidOperationException("Text must be hexadecimal");
            }

            var bytes = text.GetHexBytes();
            if (bytes.Any(b => !b.IsParityOk(parity)))
            {
                return false;
            }

            return true;
        }

        public static bool IsParityOk(this byte b, Parity parity)
        {
            if (parity == Parity.None)
            {
                return true;
            }

            var ones = Convert.ToString(b, 2).Replace("0", "");
            return (ones.Length % 2 != 0 || parity != Parity.Odd) && (ones.Length % 2 != 1 || parity != Parity.Even);
        }

        public static string MakeParity (this string text, Parity parity)
        {
            if (parity == Parity.None)
            {
                return text;
            }

            var schemeChar = string.Empty;
            if (text.StartsWithKeyScheme())
            {
                schemeChar = text.Substring(0, 1);
            }

            var bytes = text.GetHexBytes();

            for (var i = 0; i < bytes.Length; i++)
            {
                if (!bytes[i].IsParityOk(parity))
                {
                    bytes[i] = (byte) (bytes[i] ^ 0x01);
                }
            }

            return schemeChar + bytes.GetHexString();
        }

        #endregion

        #region LMK

        public static LmkPair GetLmkPair (this string text)
        {
            switch (text.ToUpper())
            {
                case "00":
                    return LmkPair.Pair04_05;
                case "01":
                    return LmkPair.Pair06_07;
                case "02":
                    return LmkPair.Pair14_15;
                case "03":
                    return LmkPair.Pair16_17;
                case "04":
                    return LmkPair.Pair18_19;
                case "05":
                    return LmkPair.Pair20_21;
                case "06":
                    return LmkPair.Pair22_23;
                case "07":
                    return LmkPair.Pair24_25;
                case "08":
                    return LmkPair.Pair26_27;
                case "09":
                    return LmkPair.Pair28_29;
                case "0A":
                    return LmkPair.Pair30_31;
                case "0B":
                    return LmkPair.Pair32_33;
                case "10":
                    return LmkPair.Pair04_05;
                case "42":
                    return LmkPair.Pair14_15;
                default:
                    throw new InvalidCastException(string.Format("Cannot parse {0} as an LMK pair", text));
            }
        }

        #endregion

        #region File/directory

        public static string AppendTrailingSeparator (this string text)
        {
            var sep = "\\";
            if (!text.Contains("\\"))
            {
                sep = "/";
            }

            return !text.EndsWith(sep) ? text + sep : text;
        }

        #endregion
    }
}