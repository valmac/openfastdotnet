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
using OpenFAST.Template.Type.Codec;
using OpenFAST.Utility;

namespace OpenFAST.Template.Type
{
    [Serializable]
    public abstract class FASTType
    {
        private static readonly Dictionary<string, FASTType> TypeNameMap = new Dictionary<string, FASTType>();

        public static readonly FASTType U8 = new UnsignedIntegerType(8, 256);
        public static readonly FASTType U16 = new UnsignedIntegerType(16, 65536);
        public static readonly FASTType U32 = new UnsignedIntegerType(32, 4294967295L);
        public static readonly FASTType U64 = new UnsignedIntegerType(64, Int64.MaxValue);
        public static readonly FASTType I8 = new SignedIntegerType(8, SByte.MinValue, SByte.MaxValue);
        public static readonly FASTType I16 = new SignedIntegerType(16, Int16.MinValue, Int16.MaxValue);
        public static readonly FASTType I32 = new SignedIntegerType(32, Int32.MinValue, Int32.MaxValue);
        public static readonly FASTType I64 = new SignedIntegerType(64, Int64.MinValue, Int64.MaxValue);
        internal sealed class FASTTypeString : StringType
        {
            public FASTTypeString(string typeName, TypeCodec codec, TypeCodec nullableCodec)
                : base(typeName, codec, nullableCodec)
            {
            }

            public override ScalarValue GetValue(byte[] bytes)
            {
                return new StringValue(System.Text.Encoding.ASCII.GetString(bytes));
            }
        }
        public static readonly FASTType STRING = new FASTTypeString("string", TypeCodec.ASCII, TypeCodec.NULLABLE_ASCII);
        public static readonly FASTType ASCII = new FASTTypeString("ascii", TypeCodec.ASCII, TypeCodec.NULLABLE_ASCII);
        internal sealed class FASTUTFTypeString : StringType
        {
            public FASTUTFTypeString(string typeName, TypeCodec codec, TypeCodec nullableCodec)
                : base(typeName, codec, nullableCodec)
            {
            }

            public override ScalarValue GetValue(byte[] bytes)
            {
                return new StringValue(System.Text.Encoding.UTF8.GetString(bytes));
            }
        }
        public static readonly FASTType UNICODE = new FASTUTFTypeString("unicode", TypeCodec.UNICODE, TypeCodec.NULLABLE_UNICODE);
        public static readonly FASTType BYTE_VECTOR = new ByteVectorType();
        public static readonly FASTType DECIMAL = new DecimalType();

        private static FASTType[] _staticAllTypes;
        public static readonly FASTType[] INTEGER_TYPES = new[] {U8, U16, U32, U64, I8, I16, I32, I64};

        private readonly string _name;

        static FASTType()
        {
            //STRING = new StringType("string", TypeCodec.ASCII, TypeCodec.NULLABLE_ASCII);
            //ASCII = new StringType("ascii", TypeCodec.ASCII, TypeCodec.NULLABLE_ASCII);
            //UNICODE = new StringType("unicode", TypeCodec.UNICODE, TypeCodec.NULLABLE_UNICODE);
        }

        protected FASTType(string typeName)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");
            _name = typeName;
            TypeNameMap[typeName] = this;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public abstract ScalarValue DefaultValue { get; }

        public static Dictionary<string, FASTType> RegisteredTypeMap
        {
            get { return TypeNameMap; }
        }

        public static FASTType GetType(string typeName)
        {
            FASTType value;
            if (TypeNameMap.TryGetValue(typeName, out value))
                return value;

            throw new ArgumentOutOfRangeException(
                "typename", typeName,
                "The type does not exist.  Existing types are " + Util.CollectionToString(TypeNameMap.Keys));
        }

        public override string ToString()
        {
            return _name;
        }

        public virtual string Serialize(ScalarValue value)
        {
            return value.ToString();
        }

        public abstract TypeCodec GetCodec(Operator.Operator op, bool optional);
        public abstract ScalarValue GetValue(string value);
        public abstract bool IsValueOf(ScalarValue priorValue);

        public virtual void ValidateValue(ScalarValue value)
        {
        }

        public static FASTType[] AllTypes()
        {
            return _staticAllTypes ??
                   (_staticAllTypes = new[]
                                          {
                                              U8, U16, U32, U64, I8, I16, I32, I64, STRING, ASCII,
                                              UNICODE, BYTE_VECTOR, DECIMAL
                                          });
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            
            var other = obj as FASTType;
            if (ReferenceEquals(null, other)) return false;
            return Equals(other._name, _name);
        }

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        #endregion

        public virtual ScalarValue GetValue(byte[] bytes)
        {
            throw new UnsupportedOperationException();
        }
    }
}