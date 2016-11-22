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
        public SecurityTracker()
        {

        }

        #endregion Public Constructors


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
                int idx = getindex(symbol);
                if (idx < 0)
                {
                    var security = GetNewSecurity(SecurityType.Forex, symbol);
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
                int idx = getindex(symbol);
                if (idx < 0)
                {
                    var security = GetNewSecurity(type, symbol);
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
                return idx < 0 ? GetNewSecurity(SecurityType.NIL, "UNKOWN") : base[idx];
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
            int idx = getindex(security.Name);

            //If index does not exist, add it
            if (idx < 0)
                addindex(security.Name, security);
        }

        #endregion Public Methods

        #region Private Methods

        private ISecurity GetNewSecurity(SecurityType type, string name)
        {
            switch (type)
            {
                case SecurityType.NIL:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Equity:
                    return new Securities.EquitySecurity(name);
                    break;

                case SecurityType.Option:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Future:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Forex:
                    return new Securities.ForexSecurity(name);
                    break;

                case SecurityType.Index:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.Bond:
                    return new Securities.UnkownSecurity(name);
                    break;

                case SecurityType.CFD:
                    return new Securities.CFDSecurity(name);
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