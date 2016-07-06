﻿using API;
using Opc.Ua;
using System;
using System.Text;
using Utils;

namespace Connection.OPCUA
{
    public class DataItem : IDataItem
    {
        public Connection           mConnection;
        public uint                 mClientHandle   = 0;
        public int                  mSampling       = 100;
        public NodeId               mNodeId         = null;
        public StatusCode           mLastStatusCode = 0;

        public object               mValue          = 0;
        public object               Value
        {
            get
            {
                if (Access.HasFlag(EAccess.READ) == false)
                {
                    throw new InvalidOperationException("No access. ");
                }

                return mValue;
            }
            set
            {
                if (Access.HasFlag(EAccess.WRITE) == false)
                {
                    throw new InvalidOperationException("No access. ");
                }

                if (ValuesCompare.isNotEqual(mValue, value))
                {
                    object lPrevValue   = mValue;
                    mValue              = value;

                    try
                    {
                        mConnection.writeAttribute(mNodeId, Attributes.Value, value);
                    }
                    catch
                    {
                        mValue = lPrevValue;
                        throw;
                    }

                    if (Access.HasFlag(EAccess.READ))
                    {
                        raiseValueChanged();
                    }
                }
            }
        }
        public event EventHandler   ValueChanged;
        public void                 raiseValueChanged()
        {
            EventHandler lEvent = ValueChanged;
            if (lEvent != null) lEvent(this, EventArgs.Empty);
        }
        public void                 initValue(string aType, bool aArray)
        {
            mValue = StringUtils.getInitValue(aType, aArray);
        }
        public object               InitValue
        {
            get
            {
                return mValue;
            }
        }

        private volatile EAccess    mAccess         = EAccess.NO_ACCESS;
        public void                 setAccess(byte aAccess)
        {
            switch (aAccess)
            {
                case AccessLevels.CurrentRead:          mAccess = EAccess.READ; break;
                case AccessLevels.CurrentWrite:         mAccess = EAccess.WRITE; break;
                case AccessLevels.CurrentReadOrWrite:   mAccess = EAccess.READ_WRITE; break;
                default:                                mAccess = EAccess.NO_ACCESS; break;
            }
        }
        public EAccess              Access
        {
            get
            {
                if (mConnection.Connected && StatusCode.IsGood(mLastStatusCode))
                {
                    return mAccess;
                }
                else
                {
                    return EAccess.NO_ACCESS;
                }
            }
        }
        public void                 onConnectionStateChanged(object aSender, EventArgs aEventArgs)
        {
            raisePropertiesChanged();
        }

        public string               Description
        {
            get
            {
                var lResult = new StringBuilder("'");
                lResult.Append(mNodeId.ToString());
                lResult.Append("', ");
                lResult.Append(StringUtils.ObjectToString(mSampling));
                lResult.Append(" ms");
                return lResult.ToString();
            }
        }

        public event EventHandler   PropertiesChanged;
        public void                 raisePropertiesChanged()
        {
            EventHandler lEvent = PropertiesChanged;
            if (lEvent != null) lEvent(this, EventArgs.Empty);
        }
    }
}