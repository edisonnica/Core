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
using System;
using System.Globalization;
using System.Text;

namespace Quantler.Data.Ticks
{
    /// <summary>
    /// A tick is both the smallest unit of time and the most simple unit of data in Quantler (and
    /// the markets) It is an abstract container for last trade, last trade size, best bid, best
    /// offer, bid and offer sizes.
    /// </summary>
    [Serializable]
    public struct TickImpl : Tick
    {
        #region Internal Fields

        internal ulong _ask;
        internal ulong _bid;
        internal long _datetime;
        internal ulong _trade;

        #endregion Internal Fields

        #region Private Fields

        private string _be;
        private int _bs;
        private int _date;
        private int _depth;
        private string _ex;
        private string _oe;
        private int _os;
        private int _size;
        private string _sym;
        private int _symidx;
        private int _time;

        #endregion Private Fields

        #region Public Constructors

        public TickImpl(string symbol)
        {
            _sym = symbol;
            _be = "";
            _oe = "";
            _ex = "";
            _bs = 0;
            _os = 0;
            _size = 0;
            _depth = 0;
            _date = 0;
            _time = 0;
            _trade = 0;
            _bid = 0;
            _ask = 0;
            _datetime = 0;
            _symidx = 0;
        }

        #endregion Public Constructors

        #region Public Properties

        public decimal Ask { get { return _ask * Const.IPRECV; } set { _ask = (ulong)(value * Const.IPREC); } }

        public string AskSource { get { return _oe; } set { _oe = value; } }
        public int AskSize { get { return _os; } set { _os = value; } }

        public decimal Bid { get { return _bid * Const.IPRECV; } set { _bid = (ulong)(value * Const.IPREC); } }
        public string BidSource { get { return _be; } set { _be = value; } }
        public int BidSize { get { return _bs; } set { _bs = value; } }

        public int Date { get { return _date; } set { _date = value; } }

        public long Datetime { get { return _datetime; } set { _datetime = value; } }

        public int Depth { get { return _depth; } set { _depth = value; } }

        public string Source { get { return _ex; } set { _ex = value; } }

        public bool HasAsk { get { return (Ask != 0) && (_os != 0); } }

        public bool HasBid { get { return (_bid != 0) && (_bs != 0); } }

        public bool HasTick { get { return IsTrade || HasBid || HasAsk; } }

        public bool IsFullQuote { get { return HasBid && HasAsk; } }

        public bool IsIndex { get { return _size < 0; } }

        public bool IsQuote { get { return !IsTrade && (HasBid || HasAsk); } }

        public bool IsTrade { get { return (_trade != 0) && (_size > 0); } }

        public bool IsValid { get { return (_sym != "") && (IsIndex || HasTick); } }
        public int OfferSize { get { return _os; } set { _os = value; } }

        public int Size { get { return _size; } set { _size = value; } }

        public string Symbol { get { return _sym; } set { _sym = value; } }

        public int Symidx { get { return _symidx; } set { _symidx = value; } }

        public DateTime TickDateTime
        {
            get { return Util.ToDateTime(Date, Time); }
        }

        public int Time { get { return _time; } set { _time = value; } }

        public decimal Trade
        {
            get { return _trade * Const.IPRECV; }
            set { _trade = (ulong)(value * Const.IPREC); }
        }

        public int TradeSize { get { return Ts; } set { _size = value; } }

        public int Ts { get { return _size; } }

        #endregion Public Properties

        #region Public Methods

        // normalized to bs/os
        public static TickImpl Copy(Tick c)
        {
            TickImpl k = new TickImpl();
            if (c.Symbol != "") k._sym = c.Symbol;
            k._time = c.Time;
            k._date = c.Date;
            k._datetime = c.Datetime;
            k._size = c.Size;
            k._depth = c.Depth;
            k.Trade = c.Trade;
            k.Bid = c.Bid;
            k.Ask = c.Ask;
            k._bs = c.BidSize;
            k._os = c.AskSize;
            k._be = c.BidSource;
            k._oe = c.AskSource;
            k._ex = c.Source;
            k._symidx = c.Symidx;
            return k;
        }

        /// <summary>
        /// this constructor creates a new tick by combining two ticks this is to handle tick
        /// updates that only provide bid/ask changes.
        /// </summary>
        /// <param name="a">old tick</param>
        /// <param name="b">new tick or update</param>
        public static Tick Copy(TickImpl a, TickImpl b)
        {
            TickImpl k = new TickImpl();
            if (b.Symbol != a.Symbol) return k; // don't combine different symbols
            if (b.Time < a.Time) return k; // don't process old updates
            k.Time = b.Time;
            k.Date = b.Date;
            k.Datetime = b.Datetime;
            k.Symbol = b.Symbol;
            k.Depth = b.Depth;
            k.Symidx = b.Symidx;
            if (b.IsTrade)
            {
                k.Trade = b.Trade;
                k.Size = b.Size;
                k.Source = b.Source;
                //
                k.Bid = a.Bid;
                k.Ask = a.Ask;
                k.OfferSize = a.OfferSize;
                k.BidSize = a.BidSize;
                k.BidSource = a.BidSource;
                k.AskSource = a.AskSource;
            }
            if (b.HasAsk && b.HasBid)
            {
                k.Bid = b.Bid;
                k.Ask = b.Ask;
                k.BidSize = b.BidSize;
                k.OfferSize = b.OfferSize;
                k.BidSource = b.BidSource;
                k.AskSource = b.AskSource;
                //
                k.Trade = a.Trade;
                k.Size = a.Size;
                k.Source = a.Source;
            }
            else if (b.HasAsk)
            {
                k.Ask = b.Ask;
                k.OfferSize = b.OfferSize;
                k.AskSource = b.AskSource;
                //
                k.Bid = a.Bid;
                k.BidSize = a.BidSize;
                k.BidSource = a.BidSource;
                k.Trade = a.Trade;
                k.Size = a.Size;
                k.Source = a.Source;
            }
            else if (b.HasBid)
            {
                k.Bid = b.Bid;
                k.BidSize = b.BidSize;
                k.BidSource = b.BidSource;
                //
                k.Ask = a.Ask;
                k.OfferSize = a.OfferSize;
                k.AskSource = a.AskSource;
                k.Trade = a.Trade;
                k.Size = a.Size;
                k.Source = a.Source;
            }
            return k;
        }

        public static Tick Deserialize(string msg)
        {
            string[] r = msg.Split(',');
            TickImpl t = new TickImpl();
            decimal d;
            int i;
            t.Symbol = r[(int)TickField.Symbol];
            if (decimal.TryParse(r[(int)TickField.Trade], NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                t.Trade = d;
            if (decimal.TryParse(r[(int)TickField.Bid], NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                t.Bid = d;
            if (decimal.TryParse(r[(int)TickField.Ask], NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                t.Ask = d;
            if (int.TryParse(r[(int)TickField.Tsize], out i))
                t.Size = i;
            if (int.TryParse(r[(int)TickField.Asksize], out i))
                t.OfferSize = i;
            if (int.TryParse(r[(int)TickField.Bidsize], out i))
                t.BidSize = i;
            if (int.TryParse(r[(int)TickField.Time], out i))
                t.Time = i;
            if (int.TryParse(r[(int)TickField.Date], out i))
                t.Date = i;
            if (int.TryParse(r[(int)TickField.Tdepth], out i))
                t.Depth = i;
            t.Source = r[(int)TickField.Tex];
            t.BidSource = r[(int)TickField.Bidex];
            t.AskSource = r[(int)TickField.Askex];
            t.Datetime = t.Date * 1000000 + t.Time;
            return t;
        }

        public static TickImpl NewAsk(string sym, decimal ask, int asksize)
        {
            return NewQuote(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), 0, ask, 0, asksize, "", "");
        }

        public static TickImpl NewAsk(string sym, decimal ask, int asksize, int depth)
        {
            return NewQuote(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), 0, ask, 0, asksize, "", "", depth);
        }

        public static TickImpl NewBid(string sym, decimal bid, int bidsize)
        {
            return NewQuote(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), bid, 0, bidsize, 0, "", "");
        }

        //methods overloaded with depth field
        public static TickImpl NewBid(string sym, decimal bid, int bidsize, int depth)
        {
            return NewQuote(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), bid, 0, bidsize, 0, "", "", depth);
        }

        public static TickImpl NewQuote(string sym, decimal bid, decimal ask, int bidsize, int asksize, string be, string oe)
        {
            return NewQuote(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), bid, ask, bidsize, asksize, be, oe);
        }

        public static TickImpl NewQuote(string sym, int date, int time, decimal bid, decimal ask, int bidsize, int asksize, string be, string oe)
        {
            TickImpl q = new TickImpl(sym)
            {
                Date = date,
                Time = time,
                Bid = bid,
                Ask = ask,
                BidSource = be.Trim(),
                AskSource = oe.Trim(),
                AskSize = asksize,
                BidSize = bidsize,
                Trade = 0,
                Size = 0,
                Depth = 0
            };
            return q;
        }

        public static TickImpl NewQuote(string sym, int date, int time, decimal bid, decimal ask, int bidsize, int asksize, string be, string oe, int depth)
        {
            TickImpl q = new TickImpl(sym)
            {
                Date = date,
                Time = time,
                Bid = bid,
                Ask = ask,
                BidSource = be.Trim(),
                AskSource = oe.Trim(),
                AskSize = asksize,
                BidSize = bidsize,
                Trade = 0,
                Size = 0,
                Depth = depth
            };
            return q;
        }

        public static TickImpl NewTrade(string sym, decimal trade, int size)
        {
            return NewTrade(sym, Util.ToQLDate(DateTime.Now), Util.ToQLTime(DateTime.Now), trade, size, "");
        }

        public static TickImpl NewTrade(string sym, int date, int time, decimal trade, int size, string ex)
        {
            TickImpl t = new TickImpl(sym)
            {
                Date = date,
                Time = time,
                Trade = trade,
                Size = size,
                Source = ex.Trim(),
                Bid = 0
            };
            return t;
        }

        public static string Serialize(Tick t)
        {
            const char d = ',';
            StringBuilder sb = new StringBuilder();
            sb.Append(t.Symbol);
            sb.Append(d);
            sb.Append(t.Date);
            sb.Append(d);
            sb.Append(t.Time);
            sb.Append(d);
            // unused field
            sb.Append(d);
            sb.Append(t.Trade.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(t.Size);
            sb.Append(d);
            sb.Append(t.Source);
            sb.Append(d);
            sb.Append(t.Bid.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(t.Ask.ToString(CultureInfo.InvariantCulture));
            sb.Append(d);
            sb.Append(t.BidSize);
            sb.Append(d);
            sb.Append(t.AskSize);
            sb.Append(d);
            sb.Append(t.BidSource);
            sb.Append(d);
            sb.Append(t.AskSource);
            sb.Append(d);
            sb.Append(t.Depth);
            return sb.ToString();
        }

        public bool AtHigh(decimal high)
        {
            return IsTrade && (Trade >= high);
        }

        public bool AtLow(decimal low)
        {
            return IsTrade && (Trade <= low);
        }

        public void SetQuote(int date, int time, int sec, decimal bid, decimal ask, int bidsize, int asksize, string bidex, string askex)
        {
            Date = date;
            Time = time;
            Bid = bid;
            Ask = ask;
            BidSize = bidsize;
            OfferSize = asksize;
            BidSource = bidex;
            AskSource = askex;
            Trade = 0;
            Size = 0;
            Depth = 0;
        }

        //overload with depth field
        public void SetQuote(int date, int time, int sec, decimal bid, decimal ask, int bidsize, int asksize, string bidex, string askex, int depth)
        {
            Date = date;
            Time = time;
            Bid = bid;
            Ask = ask;
            BidSize = bidsize;
            OfferSize = asksize;
            BidSource = bidex;
            AskSource = askex;
            Trade = 0;
            Size = 0;
            Depth = depth;
        }

        //date, time, sec, Convert.ToDecimal(r[(int)T.PRICE]), isize, r[(int)T.SourceCH]
        public void SetTrade(int date, int time, int sec, decimal price, int size, string sourch)
        {
            Source = sourch;
            Date = date;
            Time = time;
            Trade = price;
            Size = size;
            Bid = 0;
            Ask = 0;
            OfferSize = 0;
            BidSize = 0;
        }

        public override string ToString()
        {
            if (!HasTick) return "";
            if (IsTrade) return Symbol + " " + Size + "@" + Trade + " " + Source;
            return Symbol + " " + Bid + "x" + Ask + " (" + BidSize + "x" + OfferSize + ") " + BidSource + "x" + AskSource;
        }

        #endregion Public Methods
    }
}