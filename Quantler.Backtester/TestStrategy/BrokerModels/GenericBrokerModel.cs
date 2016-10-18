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

using Quantler.Interfaces;
using Quantler.Templates;

/// <summary>
/// Generic trading costs model
/// </summary>
public class GenericBrokerModel : BrokerModelTemplate
{
    #region Public Properties

    public decimal CommPerLot { get; set; }
    public int LatencyInMS { get; set; }
    public decimal SlippageInPips { get; set; }
    public decimal SpreadInPips { get; set; }

    #endregion Public Properties

    #region Public Methods

    public override decimal GetCommission(Order o)
    {
        return CommPerLot * o.Quantity + .06M;
    }

    public override int GetLatencyInMilliseconds(Order o)
    {
        return LatencyInMS;
    }

    public override decimal GetSlippage(Order o)
    {
        return SlippageInPips;
    }

    public override decimal GetSpread(Order o)
    {
        return SpreadInPips;
    }

    public override int MinimumOrderVolume(ISecurity s)
    {
        if (s.Type == SecurityType.Forex)
            return 1000;
        else if (s.Type == SecurityType.CFD)
            switch (s.Name)
            {
                case "JPN225":
                    return 10;
                default:
                    return 1;
            }
        else
            return 1;
    }

    public override int OrderVolumeStepSize(ISecurity s)
    {
        return MinimumOrderVolume(s);
    }

    public override int MaximumOrderVolume(ISecurity s)
    {
        if (s.Type == SecurityType.Forex)
            return 500 * 1000; // 500 microlots
        else if (s.Type == SecurityType.CFD) //Min size for CFD contracts
            switch (s.Name)
            {
                case "US30":
                    return 10000;
                case "SPX500":
                    return 5000;
                case "NAS100":
                    return 10000;
                case "UK100":
                    return 10000;
                case "GER30":
                    return 1000;
                case "ESP35":
                    return 10000;
                case "FRA40":
                    return 10000;
                case "HKG33":
                    return 300;
                case "JPN225":
                    return 1000;
                case "AUS200":
                    return 2000;
                case "EUSTX50":
                    return 10000;
                default:
                    return 10000;
            }
        else
            return 1; //Default return
    }

    #endregion Public Methods
}