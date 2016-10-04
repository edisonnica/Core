/*
Copyright Quantler BV, based on original code copyright Tradelink.org.
This file is released under the GNU Lesser General Public License v3. http://www.gnu.org/copyleft/lgpl.html

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/

using Quantler.Interfaces;

namespace Quantler.Tracker
{
    /// <summary>
    /// Tracks and contains multiple securities, can be used as a single reference point for all securities used
    /// </summary>
    public class SecurityTracker : GenericTrackerImpl<ISecurity>, ISecurityTracker
    {
        #region Public Constructors

        /// <summary>
        /// Initialize a new securitytracker object
        /// </summary>
        public SecurityTracker(DataSource defaultsource)
        {
            DefaultSource = defaultsource;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Default exchange for this tracker
        /// </summary>
        public DataSource DefaultSource { get; private set; }

        #endregion Public Properties

        #region Public Indexers

        /// <summary>
        /// Get the security object based on the symbol name
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public ISecurity this[string symbol]
        {
            get
            {
                int idx = getindex(symbol + DefaultSource.ToString());
                if (idx < 0)
                {
                    var security = GetNewSecurity(SecurityType.Forex, symbol, DefaultSource);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get the security object based on the symbol name and its security type
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public ISecurity this[string symbol, SecurityType type]
        {
            get
            {
                int idx = getindex(symbol + DefaultSource.ToString());
                if (idx < 0)
                {
                    var security = GetNewSecurity(type, symbol, DefaultSource);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get the security object based on the symbol and the associated source
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public ISecurity this[string symbol, DataSource source]
        {
            get
            {
                int idx = getindex(symbol + source.ToString());
                if (idx < 0)
                {
                    var security = GetNewSecurity(SecurityType.Forex, symbol, source);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get security by symbol, source and security type
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ISecurity this[string symbol, DataSource source, SecurityType type]
        {
            get
            {
                int idx = getindex(symbol + source.ToString());
                if (idx < 0)
                {
                    var security = GetNewSecurity(type, symbol, source);
                    AddSecurity(security);
                    return security;
                }
                return this[idx];
            }
        }

        /// <summary>
        /// Get the security object based on the index location
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public new ISecurity this[int idx]
        {
            get
            {
                return idx < 0 ? GetNewSecurity(SecurityType.NIL, "UNKOWN", DataSource.Broker) : base[idx];
            }
        }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new security to the security tracker
        /// </summary>
        /// <param name="security"></param>
        public void AddSecurity(ISecurity security)
        {
            //Try and get the current index
            int idx = getindex(security.Name + DefaultSource.ToString());

            //If index does not exist, add it
            if (idx < 0)
                addindex(security.Name + DefaultSource.ToString(), security);
        }

        public void AddSecurity(ISecurity security, DataSource source)
        {
            //Try and get the current index
            int idx = getindex(security.Name + source.ToString());

            //If index does not exist, add it
            if (idx < 0)
                addindex(security.Name + source.ToString(), security);
        }

        #endregion Public Methods

        #region Private Methods

        private ISecurity GetNewSecurity(SecurityType type, string name, DataSource source)
        {
            switch (type)
            {
                case SecurityType.NIL:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Equity:
                    return new Securities.EquitySecurity(name, source);
                    break;

                case SecurityType.Option:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Future:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Forex:
                    return new Securities.ForexSecurity(name, source);
                    break;

                case SecurityType.Index:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Bond:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.CFD:
                    return new Securities.CFDSecurity(name, source);
                    break;

                default:
                    break;
            }

            //TODO: implement other types
            return null;
        }

        #endregion Private Methods
    }
}