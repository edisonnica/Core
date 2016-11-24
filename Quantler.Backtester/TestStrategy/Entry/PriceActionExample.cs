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

using Quantler;
using Quantler.Interfaces;
using Quantler.Modules;

// Price action example entry module
class PriceActionExample : EntryModule
{
    //Amount of bars back to look for changes in the market
    [Parameter(30, 45, 5, "Lookback")]
    public int Period { get; set; }

    public override void OnCalculate()
    {
        //Charting
        UpdateChart("ROI", ChartType.Step, Agent.Results.ROI);
        UpdateChart("DD", ChartType.Line, Agent.Results.MaxDDPortfolio);

        //get bars
        var bars = Agent.Stream[Agent.TimeFrame];

        //Check if we have enough prices
        if (bars.Count < Period)
            NoEntry();

        //Perform checking (Is the close of Bars x period back lower than the current bar? Go Long
        else if (bars[-Period].Close < CurrentBar[Agent.Symbol].Close && !IsLong())
            EnterLong();
        //Is the close of Bars x period back higher than the current bar? Go Short
        else if (bars[-Period].Close > CurrentBar[Agent.Symbol].Close && !IsShort())
            EnterShort();
        else
            NoEntry();
    }

    //Check if we are currently long on our defualt symbol
    private bool IsLong()
    {
        return Agent.Positions[Agent.Symbol].IsLong;
    }

    //Check if we are currently short on our defualt symbol
    private bool IsShort()
    {
        return Agent.Positions[Agent.Symbol].IsShort;
    }
}