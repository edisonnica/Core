#region License
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
#endregion

using Quantler.Interfaces;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using Quantler.Securities;

namespace Quantler.Broker
{
    /// <summary>
    /// Account information and reference for current states (Simulated account, used for backtesting)
    /// </summary>
    public class SimAccount : IAccount
    {
        #region Private Fields

        private readonly IPositionTracker _currentpositions;
        private readonly string _id;
        private readonly Dictionary<string, Tick> _priceinformation = new Dictionary<string, Tick>();
        private readonly ISecurityTracker _securities;
        private readonly decimal _startingbalance;
        private bool _execute = true;
        private decimal _mutations;
        private bool _notify = true;

        #endregion Private Fields

        #region Public Constructors

        public SimAccount()
            : this("empty", "", 10000, 100)
        {
        }

        public SimAccount(string accountId)
            : this(accountId, "", 10000, 100)
        {
        }

        public SimAccount(string accountId, string description)
            : this(accountId, description, 10000, 100)
        {
        }

        public SimAccount(string accountId, string description, decimal startingbalance, int leverage, DataSource source = DataSource.Broker)
        {
            _id = accountId;
            Desc = description;
            _startingbalance = startingbalance;
            Leverage = leverage;

            _securities = new SecurityTracker(source);
            _currentpositions = new PositionTracker(this);
        }

        #endregion Public Constructors

        #region Public Properties

        public decimal Balance
        {
            get { return _startingbalance + _mutations; }
        }

        public string Client
        {
            get { return "BacktestUser"; }
        }

        public string Company
        {
            get { return "Quantler"; }
        }

        public CurrencyType Currency
        {
            get { return CurrencyType.USD; }
        }

        /// <summary>
        /// Gets or sets the description for this account.
        /// </summary>
        /// <value>The desc.</value>
        public string Desc { get; set; }

        public decimal Equity
        {
            get
            {
                return Balance + _currentpositions.Securities
                    .Where(sym => !_currentpositions[sym].IsFlat)
                    .Sum(sym => Calc.ClosePL(_currentpositions[sym],
                        new TradeImpl(sym.Name, _priceinformation[sym.Name].HasBid ?
                            _priceinformation[sym.Name].Bid :
                            _priceinformation[sym.Name].Trade, int.MaxValue)));
            }
        }

        public bool Execute { get { return _execute; } set { _execute = value; } }

        public decimal FloatingPnL
        {
            get { return Equity - Balance; }
        }

        public decimal FreeMargin
        {
            get { return Math.Abs(Equity - Margin); }
        }

        public string Id { get { return _id; } }

        public bool IsLiveTrading
        {
            get
            {
                return false;
            }
        }

        public bool IsTradingAllowed
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid { get { return !String.IsNullOrWhiteSpace(Id); } }

        /// <summary>
        /// Sim account does not have latency (1ms)
        /// </summary>
        public int Latency { get { return 1; } }

        public int Leverage
        {
            get;
            private set;
        }

        public decimal Margin
        {
            // Sum of the open positions / Leverage
            get
            {
                decimal toreturn = 0;
                foreach (var sym in _currentpositions.Securities)
                {
                    Position pos = _currentpositions[sym];
                    if (pos.IsFlat)
                        continue;

                    string basesymbol = Util.GetPositionValueSymbolCrosses(CurrencyType.USD, pos.Security);
                    decimal price;

                    if (_priceinformation.ContainsKey(basesymbol))
                        price = _priceinformation[basesymbol].HasBid ? _priceinformation[basesymbol].Bid : _priceinformation[basesymbol].Trade;
                    else
                        price = 1;

                    //Margin = (Trade Size / leverage) * account currency exchange rate (if different from the base currency in the pair being traded).
                    if (pos.Security.Type == SecurityType.Forex)
                        toreturn += (pos.UnsignedSize / Leverage) * price;
                    else
                        toreturn += (pos.UnsignedSize * price) * (1 / Leverage);
                }
                return toreturn;
            }
        }

        public decimal MarginLevel
        {
            get { return Margin > 0 ? Equity / Margin * 100 : Equity * 100; }
        }

        public bool Notify { get { return _notify; } set { _notify = value; } }

        public IPositionTracker Positions { get { return _currentpositions; } }

        public ISecurityTracker Securities { get { return _securities; } }

        public string Server
        {
            get { return "Test"; }
        }

        public decimal StopOutLevel
        {
            //TODO: allign with broker model
            get { return 20; }
        }

        #endregion Public Properties

        #region Public Methods

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SimAccount o = (SimAccount)obj;
            return Equals(o);
        }

        public bool Equals(SimAccount a)
        {
            return _id.Equals(a.Id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void OnFill(Trade t)
        {
            Positions.GotFill(t);
            _mutations = Positions.TotalClosedPL;
        }

        public void OnTick(Tick t)
        {
            //Get tick security
            SecurityImpl sec = (SecurityImpl)_securities[t.Symbol];

            //update last tick time
            sec.LastTickEvent = t.TickDateTime;

            if (_priceinformation.ContainsKey(t.Symbol))
            {
                _priceinformation[t.Symbol] = t;

                //Update security info
                UpdateSecurity(t, sec);
            }
            else
                _priceinformation.Add(t.Symbol, t);

        }

        public override string ToString()
        {
            return Id;
        }

        #endregion Public Methods

        #region Private Methods

        private void UpdateSecurity(Tick t, SecurityImpl sec)
        {
            if (sec == null || !t.IsValid)
                return;

            //Set bid ask
            sec.Bid = t.Bid;
            sec.Ask = t.Ask;

            //Set spread information
            if (t.IsFullQuote)
                sec.Spread = (int)((double)(t.Ask - t.Bid) * Math.Pow(10, sec.Digits));

            //Set pip value
            string conversionsymbol = Util.GetPipValueSymbolCrosses(CurrencyType.USD, sec);
            if (_priceinformation.ContainsKey(conversionsymbol) && sec.Type == SecurityType.Forex)
            {
                //get current price based on tick received
                decimal price = _priceinformation[conversionsymbol].IsFullQuote ? _priceinformation[conversionsymbol].Bid : _priceinformation[conversionsymbol].Trade;

                //convert to pip value
                sec.PipValue = sec.PipSize / price * sec.ContractSize;
            }
            else if (sec.Type == SecurityType.CFD)
            {
                //get current price based on tick received
                decimal price = 1;
                if (conversionsymbol != "USDUSD")
                    price = _priceinformation[conversionsymbol].IsFullQuote ? _priceinformation[conversionsymbol].Bid : _priceinformation[conversionsymbol].Trade;

                //convert to pip value (depending on the info that we have of this security)
                if (sec.ContractSize > 1 && sec.TickValue > 0) //CFD has a tickvalue and a contract size
                    sec.PipValue = (1M / sec.TickSize) * sec.TickValue;
                else if (sec.ContractSize > 1 || sec.TickValue == 0) //CFD has no tickvalue
                    sec.PipValue = (1M / sec.TickSize) * price * (sec.ContractSize * sec.TickSize);
                else
                    sec.PipValue = ((1M / sec.TickSize) * sec.TickValue) / price;
            }
            else if (conversionsymbol == "USDUSD")
            {
                sec.PipValue = 10 * (sec.LotSize / 100000M);
            }
        }

        #endregion Private Methods
    }
}