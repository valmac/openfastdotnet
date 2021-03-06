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
using System.IO;

namespace OpenFAST.Template
{
    public abstract class Field
    {
        private readonly bool _isOptional;
        private readonly QName _name;
        private Dictionary<QName, string> _attributes;
        private string _id;
        private QName _key;
        private MessageTemplate _messageTemplate;
        private Context _context;

        protected Field(QName name, bool isOptional)
            : this(name, name, isOptional, null)
        {
        }

        protected Field(QName name, QName key, bool isOptional)
            : this(name, key, isOptional, null)
        {
        }

        protected Field(string name, string key, bool isOptional, string id)
            : this(new QName(name), new QName(key), isOptional, id)
        {
        }

        private Field(QName name, QName key, bool isOptional, string id)
        {
            _name = name;
            _key = key;
            _isOptional = isOptional;
            _id = id;
        }

        #region Cloning

        protected Field(Field other)
            : this(other._name, other._key, other._isOptional, other._id)
        {
            // _messageTemplate & _context are now null
            if (other._attributes != null)
                _attributes = new Dictionary<QName, string>(other._attributes);
        }

        #endregion

        public string Name
        {
            get { return _name.Name; }
        }

        public QName QName
        {
            get { return _name; }
        }

        public bool IsOptional
        {
            get { return _isOptional; }
        }

        public QName Key
        {
            get { return _key; }
            set
            {
                ThrowOnReadonly();
                _key = value;
            }
        }

        public string Id
        {
            get { return _id ?? ""; }
            set
            {
                ThrowOnReadonly();
                _id = value;
            }
        }

        public abstract Type ValueType { get; }
        public abstract string TypeName { get; }

        public MessageTemplate MessageTemplate
        {
            get { return _messageTemplate; }
        }

        public Context Context
        {
            get { return _context; }
        }

        public abstract bool UsesPresenceMapBit { get; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

#warning _attributes is a dictionary that does not support equality - used both here & in GetHashCode()

            var other = obj as Field;
            if (ReferenceEquals(null, other)) return false;
            return Equals(other._name, _name) && other._isOptional.Equals(_isOptional) &&
                   Equals(other._attributes, _attributes) && Equals(other.Id, Id) && Equals(other._key, _key);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_name != null ? _name.GetHashCode() : 0);
                result = (result*397) ^ _isOptional.GetHashCode();
                result = (result*397) ^ (_attributes != null ? _attributes.GetHashCode() : 0);
                result = (result*397) ^ (_id != null ? _id.GetHashCode() : 0);
                result = (result*397) ^ (_key != null ? _key.GetHashCode() : 0);
                return result;
            }
        }

        #endregion

        public abstract Field Clone();

        internal void AttachToTemplate(MessageTemplate value)
        {
            if (_messageTemplate != null) // && !ReferenceEquals(_messageTemplate, value))
                throw new InvalidOperationException("This field is already a part of the template " + _messageTemplate.Name);
            _messageTemplate = value;
        }

        internal void AttachToContext(Context value)
        {
            if (_context != null) // && !ReferenceEquals(_context, value))
                throw new InvalidOperationException("This field is already a part of a context");
            if (_messageTemplate == null)
                throw new InvalidOperationException("This field is not part of any template");
            _context = value;
        }

        protected void ThrowOnReadonly()
        {
            if (_context != null)
                throw new InvalidOperationException("This object cannot be edited because it is part of a context");
        }

        public bool IsIdNull()
        {
            return _id == null;
        }

        public virtual bool HasAttribute(QName attributeName)
        {
            return _attributes != null && _attributes.ContainsKey(attributeName);
        }

        public virtual bool TryGetAttribute(QName qname, out string value)
        {
            if (_attributes != null)
                return _attributes.TryGetValue(qname, out value);

            value = null;
            return false;
        }

        public void AddAttribute(QName qname, string value)
        {
            ThrowOnReadonly();
            if (_attributes == null)
                _attributes = new Dictionary<QName, string>();
            _attributes[qname] = value;
        }

        protected bool IsPresent(BitVectorReader presenceMapReader)
        {
            return (!UsesPresenceMapBit) || presenceMapReader.Read();
        }

        public abstract byte[] Encode(IFieldValue value, Group encodeTemplate, Context context,
                                      BitVectorBuilder presenceMapBuilder);

        public abstract IFieldValue Decode(Stream inStream, Group decodeTemplate, Context context,
                                           BitVectorReader presenceMapReader);

        public abstract bool IsPresenceMapBitSet(byte[] encoding, IFieldValue fieldValue);

        public abstract IFieldValue CreateValue(string value);
    }
}