/*

The contents of this file are subject to the Mozilla Public License
Version 1.1 (the "License"); you may not use this file except in
compliance with the License. You may obtain a copy of the License at
http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS"
basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
License for the specific language governing rights and limitations
under the License.

The Original Code is OpenFAST.

The Initial Developer of the Original Code is The LaSalle Technology
Group, LLC.  Portions created by Shariq Muhammad
are Copyright (C) Shariq Muhammad. All Rights Reserved.

Contributor(s): Shariq Muhammad <shariq.muhammad@gmail.com>
                Yuri Astrakhan <FirstName><LastName>@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OpenFAST.Template;
using OpenFAST.Template.Operator;
using OpenFAST.Template.Type;
using Type = OpenFAST.Template.Type.FASTType;

namespace OpenFAST.Utility
{
    internal static class Util
    {
        private static readonly TwinValue NoDiff = new TwinValue(new IntegerValue(0), ByteVectorValue.EmptyBytes);

        public static bool IsBiggerThanInt(long value)
        {
            return (value > Int32.MaxValue) || (value < Int32.MinValue);
        }

        public static ScalarValue GetDifference(byte[] newValue, byte[] priorValue)
        {
            if ((priorValue == null) || (priorValue.Length == 0))
            {
                return new TwinValue(new IntegerValue(0), new ByteVectorValue(newValue));
            }
            if (priorValue.Equals(newValue))
            {
                return NoDiff;
            }
            int appendIndex = 0;
            while ((appendIndex < priorValue.Length) && (appendIndex < newValue.Length)
                   && (newValue[appendIndex] == priorValue[appendIndex]))
                appendIndex++;
            int prependIndex = 1;
            while ((prependIndex <= newValue.Length) && (prependIndex <= priorValue.Length)
                   && (newValue[newValue.Length - prependIndex] == priorValue[priorValue.Length - prependIndex]))
                prependIndex++;
            //        String prepend = newValue.substring(0, value.length() - prependIndex + 1);
            int prependLength = newValue.Length - prependIndex + 1;
            int appendLength = newValue.Length - appendIndex;
            if (prependLength < appendLength)
            {
                return new TwinValue(new IntegerValue(prependIndex - priorValue.Length - 2),
                                     new ByteVectorValue(new ArraySegment<byte>(newValue, 0, prependLength)));
            }
            return new TwinValue(new IntegerValue(priorValue.Length - appendIndex),
                                 new ByteVectorValue(new ArraySegment<byte>(newValue, appendIndex, appendLength)));
        }

        public static byte[] ApplyDifference(ScalarValue baseValue, TwinValue diffValue)
        {
            int subtraction = ((IntegerValue) diffValue.First).Value;
            byte[] baseVal = baseValue.Bytes;
            byte[] diff = diffValue.Second.Bytes;
            if (subtraction < 0)
            {
                subtraction = (-1*subtraction) - 1;
                return ByteUtil.Combine(diff, 0, diff.Length, baseVal, subtraction, baseVal.Length);
            }
            return ByteUtil.Combine(baseVal, 0, baseVal.Length - subtraction, diff, 0, diff.Length);
        }

        //public static ScalarValue GetDifference(StringValue newValue, StringValue priorValue)
        //{
        //    string value = newValue.Value;

        //    if ((priorValue == null) || (priorValue.Value.Length == 0))
        //    {
        //        return new TwinValue(new IntegerValue(0), newValue);
        //    }

        //    if (priorValue.Equals(newValue))
        //    {
        //        return NoDiff;
        //    }

        //    string baseVal = priorValue.Value;
        //    int appendIndex = 0;

        //    while ((appendIndex < baseVal.Length) && (appendIndex < value.Length) &&
        //           (value[appendIndex] == baseVal[appendIndex]))
        //        appendIndex++;

        //    string append = value.Substring(appendIndex);

        //    int prependIndex = 1;

        //    while ((prependIndex <= value.Length) && (prependIndex <= baseVal.Length) &&
        //           (value[value.Length - prependIndex] ==
        //            baseVal[baseVal.Length - prependIndex]))
        //        prependIndex++;

        //    string prepend = value.Substring(0, (value.Length - prependIndex + 1) - (0));

        //    if (prepend.Length < append.Length)
        //    {
        //        return new TwinValue(new IntegerValue(prependIndex - baseVal.Length - 2), new StringValue(prepend));
        //    }

        //    return new TwinValue(new IntegerValue(baseVal.Length - appendIndex), new StringValue(append));
        //}

        //public static StringValue ApplyDifference(StringValue baseValue, TwinValue diffValue)
        //{
        //    int subtraction = ((IntegerValue) diffValue.First).Value;
        //    string baseVal = baseValue.Value;
        //    string diff = ((StringValue) diffValue.Second).Value;

        //    if (subtraction < 0)
        //    {
        //        subtraction = ((-1)*subtraction) - 1;

        //        return new StringValue(diff + baseVal.Substring(subtraction, (baseVal.Length) - (subtraction)));
        //    }

        //    return new StringValue(baseVal.Substring(0, (baseVal.Length - subtraction) - (0)) + diff);
        //}

        public static string CollectionToString(IEnumerable<string> set)
        {
            return CollectionToString(set, ",");
        }

        public static string CollectionToString(IEnumerable<string> set, string sep)
        {
            var str = new StringBuilder();

            str.Append("{");

            foreach (string v in set)
                str.Append(v).Append(sep);

            str.Length = str.Length - sep.Length;
            str.Append("}");

            return str.ToString();
        }

        public static int MillisecondsSinceMidnight(DateTime date)
        {
            Calendar cal = new GregorianCalendar();
            SupportClass.CalendarManager.Manager.SetDateTime(cal, date);
            return SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.HOUR_OF_DAY)*3600000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.MINUTE)*60000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.SECOND)*1000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.MILLISECOND);
        }


        public static DateTime Date(int year, int month, int day)
        {
            Calendar cal = new GregorianCalendar();
            SupportClass.CalendarManager.Manager.Set(cal, year - 1900, month - 1, day);
            return SupportClass.CalendarManager.Manager.GetDateTime(cal);
        }

        public static int DateToInt(DateTime date)
        {
            Calendar cal = new GregorianCalendar();
            SupportClass.CalendarManager.Manager.SetDateTime(cal, date);
            return SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.YEAR)*10000 +
                   (SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.MONTH) + 1)*100 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.DATE);
        }

        public static int TimeToInt(DateTime date)
        {
            Calendar cal = new GregorianCalendar();
            SupportClass.CalendarManager.Manager.SetDateTime(cal, date);
            return SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.HOUR_OF_DAY)*10000000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.MINUTE)*100000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.SECOND)*1000 +
                   SupportClass.CalendarManager.Manager.Get(cal, SupportClass.CalendarManager.MILLISECOND);
        }

        public static int TimestampToInt(DateTime date)
        {
            return DateToInt(date)*1000000000 + TimeToInt(date);
        }

        public static DateTime ToTimestamp(long value)
        {
            Calendar cal = new GregorianCalendar();
            var year = (int) (value/10000000000000L);
            value %= 10000000000000L;
            var month = (int) (value/100000000000L);
            value %= 100000000000L;
            var day = (int) (value/1000000000);
            value %= 1000000000;
            var hour = (int) (value/10000000);
            value %= 10000000;
            var min = (int) (value/100000);
            value %= 100000;
            var sec = (int) (value/1000);
            var ms = (int) (value%1000);
            SupportClass.CalendarManager.Manager.Set(cal, year, month - 1, day, hour, min, sec);
            SupportClass.CalendarManager.Manager.Set(cal, SupportClass.CalendarManager.MILLISECOND, ms);
            return SupportClass.CalendarManager.Manager.GetDateTime(cal);
        }

        public static ComposedScalar ComposedDecimal(QName name, Operator exponentOp, ScalarValue exponentVal,
                                                     Operator mantissaOp, ScalarValue mantissaVal, bool optional)
        {
            var exponentScalar = new Scalar(Global.CreateImplicitName(name), Type.I32, exponentOp, exponentVal, optional);
            var mantissaScalar = new Scalar(Global.CreateImplicitName(name), Type.I64, mantissaOp, mantissaVal, false);
            return new ComposedScalar(name, Type.DECIMAL, new[] {exponentScalar, mantissaScalar}, optional,
                                      new DecimalConverter());
        }

        public static T[] ToArray<T>(ICollection<T> c)
        {
            var array = new T[c.Count];

            int i = 0;
            foreach (T v in c)
                array[i++] = v;

            return array;
        }

        public static Dictionary<TValue, TKey> ReverseDictionary<TKey, TValue>(Dictionary<TKey, TValue> from)
        {
            var dict = new Dictionary<TValue, TKey>();
            foreach (var kv in from)
                dict[kv.Value] = kv.Key;
            return dict;
        }

        [Obsolete(
            "This method allows multiple key values to override one another. Shouldn't we through an exception instead?"
            )]
        public static Dictionary<TKey, Field> ToSafeDictionary<TKey>(Field[] fields, Func<Field, TKey> keySelector)
        {
            var map = new Dictionary<TKey, Field>();

            foreach (Field f in fields)
                map[keySelector(f)] = f;

            return map;
        }

        public static bool ArrayEquals<T>(T[] arr1, T[] arr2)
            where T : class, IEquatable<T>
        {
            if ((arr1 == null) != (arr2 == null)) return false;
            if (arr1 == arr2) return true;
            if (arr1.Length != arr2.Length) return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                T v1 = arr1[i];
                if (ReferenceEquals(v1, null))
                {
                    if (!ReferenceEquals(arr2[i], null))
                        return false;
                }
                else
                {
                    if (!v1.Equals(arr2[i]))
                        return false;
                }
            }

            return true;
        }

        public static bool ArraySegmentEquals(ArraySegment<byte> seg1, ArraySegment<byte> seg2)
        {
            // TODO: unsafe code would produce better results in the byte array comparison, maybe will add it later
            
            if (seg1 == seg2) return true;
            if (seg1.Count != seg2.Count) return false;

            byte[] arr1 = seg1.Array;
            byte[] arr2 = seg2.Array;
            if (arr1 == null || arr2 == null) return false;
            
            var len1 = seg1.Offset + seg1.Count;

            for (int i1 = seg1.Offset, i2 = seg2.Offset; i1 < len1; i1++, i2++)
                if (arr1[i1] != arr2[i2])
                    return false;

            return true;
        }

        public static bool ArrayEquals(byte[] arr1, byte[] arr2)
        {
            //if ((arr1 == null) != (arr2 == null)) return false;
            //if (arr1 == arr2) return true;
            if (arr1.Length != arr2.Length) return false;

            // TODO: unsafe code would produce better results in the byte array comparison, maybe will add it later
            for (int i = 0; i < arr1.Length; i++)
                if (arr1[i] != arr2[i])
                    return false;

            return true;
        }

        public static bool ListEquals<T>(IList<T> arr1, IList<T> arr2)
            where T : class
        {
            if ((arr1 == null) != (arr2 == null)) return false;
            if (arr1 == arr2) return true;
            if (arr1.Count != arr2.Count) return false;

            for (int i = 0; i < arr1.Count; i++)
            {
                T v1 = arr1[i];

                if (ReferenceEquals(v1, null))
                {
                    if (!ReferenceEquals(arr2[i], null))
                        return false;
                }
                else
                {
                    if (!v1.Equals(arr2[i]))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Similar to <see cref="ArrayEquals{T}"/>, except that the actual method being called is <see cref="object.Equals(object)"/>.
        /// </summary>
        public static bool ArrayEqualsSlow<T>(T[] arr1, T[] arr2, int startFrom)
        {
            //if ((arr1 == null) != (arr2 == null)) return false;
            //if (arr1 == arr2) return true;
            if (arr1.Length != arr2.Length) return false;

            for (int i = startFrom; i < arr1.Length; i++)
                if (!Equals(arr1[i], arr2[i]))
                    return false;

            return true;
        }

        public static int GetCollectionHashCode<T>(T[] array)
            where T : class
        {
            if (array == null)
                return 0;

            int result = 1;
            for (int i = 0; i < array.Length; i++)
                result = (result*397) ^ (array[i] == null ? 0 : array[i].GetHashCode());

            return result;
        }

        public static int GetCollectionHashCode<T>(IList<T> array)
            where T : class
        {
            if (array == null)
                return 0;

            int result = 1;
            for (int i = 0; i < array.Count; i++)
                result = (result*397) ^ (array[i] == null ? 0 : array[i].GetHashCode());

            return result;
        }

        public static int GetValTypeCollectionHashCode<T>(T[] array)
            where T : struct
        {
            if(array == null) return 0;
            return GetValTypeCollectionHashCode(array, 0, array.Length);
        }

        public static int GetValTypeCollectionHashCode<T>(ArraySegment<T> array)
            where T : struct
        {
            if(array.Array == null) return 0; // default(ArraySegment)
            return GetValTypeCollectionHashCode(array.Array, array.Offset, array.Count);
        }

        /// Does not perform parameter validation - hence private
        private static int GetValTypeCollectionHashCode<T>(T[] array, int offset, int count)
            where T : struct
        {
            int result = 1;
            var last = offset + count;
            for (int i = offset; i < last; i++)
                result = (result*397) ^ array[i].GetHashCode();

            return result;
        }
    }
}