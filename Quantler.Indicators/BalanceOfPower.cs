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
using Quantler.TALib;
using System;
using System.Collections.Generic;

namespace Quantler.Indicators
{
    public class BalanceOfPower : IndicatorBase, Interfaces.Indicators.BalanceOfPower
    {
        #region Private Fields

        private readonly List<double> _close = new List<double>();
        private readonly List<double> _high = new List<double>();
        private readonly List<double> _low = new List<double>();
        private readonly List<double> _open = new List<double>();
        private readonly IndicatorDataSerie _result = new IndicatorDataSerie();
        private readonly TaLib _ta = new TaLib();
        private TimeSpan _timeSpan;

        #endregion Private Fields

        #region Public Constructors

        public BalanceOfPower(DataStream stream, TimeSpan barSize)
        {
            Construct(stream, barSize);
        }

        #endregion Public Constructors

        #region Public Properties

        public new bool IsReady
        {
            get { return (Result.Count > 0 && Result[0] != 0) && !IsBackfilling; }
        }

        public DataSerie Result
        {
            get { return _result; }
        }

        #endregion Public Properties

        #region Public Methods

        public override void OnBar(Bar bar)
        {
            //Check for correct interval
            if (bar.CustomInterval != (int)_timeSpan.TotalSeconds)
                return;

            //Add new values
            _high.Add((double)bar.High);
            _low.Add((double)bar.Low);
            _close.Add((double)bar.Close);
            _open.Add((double)bar.Open);

            //Clean up old values
            Cleanup();

            //Calculate the indicator
            var calced = _ta.Bop(_open.ToArray(), _high.ToArray(), _low.ToArray(), _close.ToArray());

            //Add to current values
            if (calced.IsValid)
                Result[0] = (decimal) calced.CurrentValue;
        }

        #endregion Public Methods

        #region Private Methods

        private void Cleanup()
        {
            if (_close.Count > Period * 3)
            {
                _open.RemoveRange(0, Period);
                _close.RemoveRange(0, Period);
                _high.RemoveRange(0, Period);
                _low.RemoveRange(0, Period);
            }
        }

        private void Construct(DataStream stream, TimeSpan barSize)
        {
            _timeSpan = barSize;
            DataStreams = new[] { stream };
            Period = 100;
        }

        #endregion Private Methods
    }
}