﻿#region License
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

using System;
using Quantler.Interfaces;

namespace Quantler.Securities
{
    /// <summary>
    /// Equity Security Implementation
    /// </summary>
    public class CFDSecurity : SecurityImpl
    {
        #region Public Constructors

        public CFDSecurity(string symbol, DataSource source = Interfaces.DataSource.Broker)
        {
            BrokerName = symbol;
            DestEx = "SIM";
            Name = symbol;
            Type = SecurityType.CFD;
            LotSize = 1;
            PipSize = 0.01M;
            TickSize = 0.01m;
            OrderStepSize = 1;
            OrderMinSize = 1;
            TickSize = PipSize;
            PipValue = PipSize;
            DataSource = source.ToString();
        }

        #endregion Public Constructors
    }
}
