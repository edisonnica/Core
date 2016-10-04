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

using Quantler.Interfaces;
using System;

namespace Quantler.Agent
{
    /// <summary>
    /// Class is used for backfilling the trading agent
    /// </summary>
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Public Properties

        public TimeSpan BackFillingPeriod 
        {
            get;
            private set; 
        }

        public int BackFillingBars
        {
            get;
            private set;
        }

        public bool IsBackfilling
        {
            get;
            private set;
        }

        #endregion Public Properties

        #region Public Methods

        public void SetBackFilling(TimeSpan period)
        {
            if (period > BackFillingPeriod)
                BackFillingPeriod = period;
        }

        public void SetBackFilling(int Bars)
        {
            if (Bars > BackFillingBars)
                BackFillingBars = Bars;
        }

        #endregion Public Methods
    }
}