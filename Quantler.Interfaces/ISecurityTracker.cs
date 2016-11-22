#region License
/*
Copyright (c) Quantler B.V., All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/
#endregion

namespace Quantler.Interfaces
{
    /// <summary>
    /// Tracks security information
    /// </summary>
    public interface ISecurityTracker
    {
        #region Public Indexers

        /// <summary>
        /// Return a security based on its index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        ISecurity this[int idx] { get; }

        /// <summary>
        /// Returns a security based on its name and type
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ISecurity this[string symbol, SecurityType type] { get; }

        /// <summary>
        /// Return a security based on the symbol name
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        ISecurity this[string symbol] { get; }

        #endregion Public Indexers

        #region Public Methods

        /// <summary>
        /// Add a new security to the security tracker for general use
        /// </summary>
        /// <param name="security"></param>
        void AddSecurity(ISecurity security);

        /// <summary>
        /// Get all current securities in an array
        /// </summary>
        /// <returns></returns>
        ISecurity[] ToArray();

        #endregion Public Methods
    }
}