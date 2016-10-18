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

using Quantler.Broker;
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Trades;
using System.Collections.Generic;
using Quantler.Securities;
using System;
using Xunit;
using FluentAssertions;

namespace Quantler.Tests.Common
{
    public class TestAccount
    {
        #region Private Fields

        private const decimal inc = .1m;
        private const decimal p = 1;
        private const int s = 1000;
        private int initialbalance = 10000;
        private int leverage = 100;
        private string sym = "EURUSD";

        #endregion Private Fields

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AccountBalanceUpdateForex()
        {
            //Arrange
            SimAccount testaccount = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage);
            ForexSecurity ts = new ForexSecurity(sym);
            ts.LotSize = 1000;
            ts.ContractSize = ts.LotSize;
            ts.PipSize = 0.0001M;
            ts.TickSize = ts.PipSize / 10;
            testaccount.Securities.AddSecurity(ts);
            TickImpl tick = TickImpl.NewQuote(sym, p, p, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);
            TickImpl secondtick = TickImpl.NewQuote(sym, p + inc, p + inc, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);

            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // 1000 @ $1
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = testaccount, Commission = 2},       // 2000 @ $1.1
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = testaccount, Commission = 1},    // -1000 @ $1.2 (profit = 1000 * (1.2 - 1.0) = 200)
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = testaccount, Commission = 2},    // -2000 @ $1.2 (profit = 2000 * (1.2 - 1.1) = 200)
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = testaccount, Commission = 2},          // -2000 @ $1
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // 1000 @ $1
                // exit (round turn)
                new TradeImpl(sym,p+inc,s) { Security = ts, Account = testaccount, Commission = 1},         // 1000 @ $1 (loss = 1000 * (1.2 - 1.0) = 100)
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1}              // 100 @ 100
            });

            //Act, fill all trades
            testaccount.OnTick(tick);
            testaccount.OnTick(secondtick);
            foreach (var t in fills)
                testaccount.OnFill(t);

            //Assert
            Assert.True(testaccount.Balance == 10300);
            Assert.True(testaccount.Margin == 11);
            Assert.True(testaccount.MarginLevel == (testaccount.Equity / testaccount.Margin) * 100);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void AccountBalanceUpdateCFD()
        {
            //Arrange
            SimAccount testaccount = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage);
            CFDSecurity ts = new CFDSecurity(sym);
            ts.LotSize = 100;
            ts.ContractSize = 50;
            ts.PipSize = 0.1M;
            ts.TickSize = ts.PipSize;
            ts.TickValue = 5;
            ts.Currency = CurrencyType.USD;
            int size = 100;
            testaccount.Securities.AddSecurity(ts);
            TickImpl tick = TickImpl.NewQuote(sym, p, p, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);
            TickImpl secondtick = TickImpl.NewQuote(sym, p + inc, p + inc, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);

            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,size) { Security = ts, Account = testaccount, Commission = 1},             // 1 @ $1
                // increase bet
                new TradeImpl(sym,p+inc,size*2) { Security = ts, Account = testaccount, Commission = 2},       // 2 @ $1.1
                // take some profits
                new TradeImpl(sym,p+inc*2,size*-1) { Security = ts, Account = testaccount, Commission = 1},    // -1 @ $1.2 (profit = 1 * (1.2 - 1.0) * 50 = 100)
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,size*-2) { Security = ts, Account = testaccount, Commission = 2},    // -2 @ $1.2 (profit = 2 * (1.2 - 1.1) * 50 = 200)
                // go short
                new TradeImpl(sym,p,size*-2) { Security = ts, Account = testaccount, Commission = 2},          // -2 @ $1
                // decrease bet
                new TradeImpl(sym,p,size) { Security = ts, Account = testaccount, Commission = 1},             // 1 @ $1
                // exit (round turn)
                new TradeImpl(sym,p+inc,size) { Security = ts, Account = testaccount, Commission = 1},         // 1 @ $1 (loss = 1000 * (1.2 - 1.0) * 50 = 100)
                // do another entry
                new TradeImpl(sym,p,size) { Security = ts, Account = testaccount, Commission = 1}              // 1 @ $1
            });

            //Act, fill all trades
            testaccount.OnTick(tick);
            testaccount.OnTick(secondtick);
            foreach (var t in fills)
                testaccount.OnFill(t);

            //Assert
            Assert.True(testaccount.Balance == 10015);
            Assert.True(testaccount.Margin == 0);
            Assert.True(testaccount.MarginLevel == 1001500);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void Basics()
        {
            SimAccount a = new SimAccount();
            Assert.True(a.IsValid);
            Assert.True(a.Id == "empty");
            const string myid = "quantler";
            a = new SimAccount(myid);
            Assert.True(a.IsValid);
            Assert.True(a.Id == myid);
            a = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage);
            Assert.True(a.Balance == initialbalance);
            Assert.True(a.Leverage == leverage);
            Assert.True(a.Margin == 0);
            Assert.True(a.FreeMargin == (a.Equity - a.Margin));
            Assert.True(a.Equity == a.Balance);
            Assert.True(a.Currency == CurrencyType.USD);
            Assert.True(a.FloatingPnL == 0);
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void LowMarginLevel()
        {
            //Arrange
            SimAccount testaccount = new SimAccount("SIM1", "Menno's Account", initialbalance, leverage);
            ForexSecurity ts = new ForexSecurity(sym);
            ts.LotSize = 1000;
            ts.PipSize = 0.0001M;
            ts.TickSize = ts.PipSize / 10;
            testaccount.Securities.AddSecurity(ts);
            TickImpl tick = TickImpl.NewQuote(sym, p, p, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);
            TickImpl secondtick = TickImpl.NewQuote(sym, p + inc, p + inc, int.MaxValue, int.MaxValue, ts.DestEx, ts.DestEx);

            List<Trade> fills = new List<Trade>(new Trade[] {
                // go long
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // 100 @ $100
                // increase bet
                new TradeImpl(sym,p+inc,s*2) { Security = ts, Account = testaccount, Commission = 2},       // 300 @ $100.066666
                // take some profits
                new TradeImpl(sym,p+inc*2,s*-1) { Security = ts, Account = testaccount, Commission = 1},    // 200 @ 100.0666 (profit = 100 * (100.20 - 100.0666) = 13.34) / maxMIU(= 300*100.06666) = .04% ret
                // go flat (round turn)
                new TradeImpl(sym,p+inc*2,s*-2) { Security = ts, Account = testaccount, Commission = 2},    // 0 @ 0
                // go short
                new TradeImpl(sym,p,s*-2) { Security = ts, Account = testaccount, Commission = 2},          // -200 @ 100
                // decrease bet
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1},             // -100 @100
                // exit (round turn)
                new TradeImpl(sym,p+inc,s*5000) { Security = ts, Account = testaccount, Commission = 1},         // 0 @ 0 (gross profit = -0.10*100 = -$10)
                // do another entry
                new TradeImpl(sym,p,s) { Security = ts, Account = testaccount, Commission = 1}              // 100 @ 100
            });

            //Act, fill all trades
            testaccount.OnTick(tick);
            testaccount.OnTick(secondtick);
            foreach (var t in fills)
                testaccount.OnFill(t);

            //Assert
            Assert.True(testaccount.Balance == 10300);
            Assert.True(testaccount.Margin == 55000);
            //Lower than 20% (Margin Call)
            Assert.True(testaccount.MarginLevel < 20M);
        }

        [Theory]
        [Trait("Quantler.Common", "Quantler")]
        [InlineData("STOXX50", 0.01, 1, 0.01, CurrencyType.EUR, 3017, 0.8952, 1.117068811)] //Normal calculation method
        [InlineData("SBUX", 1, 100, 0.01, CurrencyType.USD, 52.76, 1, 100)] //Using contract size
        [InlineData("NIKKEI", 0, 1000, 0.1, CurrencyType.JPY, 16963.61, 0.0096, 9.6)] //No tickvalue and contract size
        [InlineData("FRENCH40", 0, 1, 0.1, CurrencyType.USD, 4509, 1, 1)] //No tickvalue and no contract size (where 1 lot == 1 usd)
        [InlineData("WHEAT", 5, 50, 0.1, CurrencyType.USD, 420, 1, 50)] //High tick value and a high contract size
        [InlineData("COTTON", 0, 500, 0.01, CurrencyType.USD, 71.40, 1, 500)] //High tick value and a high contract size
        //Test to see all pipvalue calculations
        public void PipValueCalcutionsCFD(string symbol, decimal tickvalue, decimal contractsize, decimal ticksize, CurrencyType basecurrency, decimal tick, decimal basevalue, decimal value)
        {
            //Arrange
            SimAccount naccount = new SimAccount("testing", "test account", 1000, 100, DataSource.Broker);
            TickImpl ntick = new TickImpl(symbol);
            ntick.Bid = tick;
            ntick.Ask = tick;
            ntick.BidSize = int.MaxValue;
            ntick.AskSize = ntick.BidSize;
            TickImpl ntickbase = new TickImpl("USD" + basecurrency.ToString());
            ntickbase.Ask = basevalue;
            ntickbase.Bid = basevalue;
            ntickbase.BidSize = int.MaxValue;
            ntickbase.AskSize = int.MaxValue;

            CFDSecurity nsecurity = new CFDSecurity(symbol, DataSource.Broker);
            nsecurity.TickValue = tickvalue;
            nsecurity.ContractSize = contractsize;
            nsecurity.TickSize = ticksize;
            nsecurity.Currency = basecurrency;

            //add security
            naccount.Securities.AddSecurity(nsecurity);

            //Act
            naccount.OnTick(ntickbase);
            naccount.OnTick(ntick);
            naccount.OnTick(ntick);

            //Assert
            naccount.Securities[symbol].PipValue.Should().BeApproximately(value, .00001M);
        }

        #endregion Public Methods
    }
}