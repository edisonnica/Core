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

using Quantler.Interfaces;
using System;

namespace Quantler.Securities
{
    /// <summary>
    /// Forex Security Implementation
    /// </summary>
    public class ForexSecurity : SecurityImpl
    {
        #region Public Constructors

        public ForexSecurity(string symbol, DataSource source = Interfaces.DataSource.Broker)
        {
            BrokerName = symbol;
            DestEx = "SIM";
            Name = symbol;
            Type = SecurityType.Forex;
            LotSize = 100000;
            PipSize = symbol.EndsWith("JPY") ? 0.01M : 0.0001M;
            OrderStepSize = 1000;
            OrderMinSize = 1000;
            TickSize = PipSize / 10;
            ContractSize = LotSize;

            //For initial pipvalue
            PipValue = 0.00807M;

            //Set source
            DataSource = source.ToString();
        }

        #endregion Public Constructors
    }
}