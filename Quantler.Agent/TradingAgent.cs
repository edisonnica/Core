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

using NLog;
using Quantler.Interfaces;
using Quantler.Modules;
using Quantler.Tracker;
using Quantler.Trades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Quantler.Agent
{
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Private Fields

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ILogger _loggerUser = LogManager.GetLogger("User");
        private int _agentid = -1;
        private bool _initialized;
        private string _name = "TestAgent";
        private PortfolioManager _portfolio;
        private Results _results;
        private DateTime _started = DateTime.UtcNow;

        //Storage
        private List<IModule> _modules = new List<IModule>();

        #endregion Private Fields

        #region Public Events

        public event ChartUpdate OnChartUpdate;

        #endregion Public Events

        #region Public Properties

        public int AgentId
        {
            get { return _agentid; }
            set { if (_agentid < 0) _agentid = value; }
        }

        public BarIndexer Bars { get; private set; }

        /// <summary>
        /// Returns the current bar for the specific symbol
        /// </summary>
        public Dictionary<string, Bar> CurrentBar
        {
            get;
            private set;
        }

        /// <summary>
        /// Storage for the decisions made by the modules
        /// </summary>
        public Dictionary<string, List<AgentState>> CurrentState
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current tick for the specific symbol
        /// </summary>
        public Dictionary<string, Tick> CurrentTick
        {
            get;
            private set;
        }

        public Interfaces.IndicatorManager IndicatorManagement { get; private set; }

        /// <summary>
        /// Checks if this agent is in a backtesting state or trading mode
        /// </summary>
        public bool IsBacktesting
        {
            get;
            set;
        }

        /// <summary>
        /// Is true if this agent is either running a backtest or is running a live trading strategy
        /// </summary>
        public bool IsRunning
        {
            get;
            set;
        }

        public string Name
        {
            get { return _name; }
        }

        public PendingOrder[] PendingOrders
        {
            get { return _portfolio.PendingOrders.Where(x => x.AgentId == AgentId && !x.IsCancelled).ToArray(); }
        }

        /// <summary>
        /// Associated portfolio
        /// </summary>
        public IPortfolio Portfolio
        {
            get
            {
                return _portfolio;
            }
        }

        public IPositionTracker Positions { get; private set; }

        public Result Results
        {
            get { return _results; }
        }

        /// <summary>
        /// Default security object associated to this agent
        /// </summary>
        public ISecurity Security
        {
            get { return Stream.Security; }
        }

        public IndicatorFactory StandardIndicators { get; private set; }

        public DateTime StartedDTUTC
        {
            get { return _started; }
        }

        /// <summary>
        /// Return the currently associated statistic modules
        /// </summary>
        public IModule[] Statistics
        {
            get { return Modules.Where(x => x is StatisticModule).ToArray(); }
        }

        /// <summary>
        /// Default DataStream associated to this agent
        /// </summary>
        public DataStream Stream
        {
            get;
            set;
        }

        /// <summary>
        /// Default symbol associated to this agent
        /// </summary>
        public string Symbol
        {
            get { return Stream.Security.Name; }
        }

        /// <summary>
        /// Collection of all modules associated to this agent
        /// </summary>
        public IModule[] Modules
        {
            get
            {
                return _modules.ToArray();
            }
            set
            {
                if (!IsRunning)
                    _modules = value.ToList();
            }
        }

        /// <summary>
        /// Default timeframe of this agent in timespan notation
        /// </summary>
        public TimeSpan TimeFrame
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Add a new modules to the existing modules (not needed when using the dep inj method)
        /// </summary>
        /// <param name="modules"></param>
        public void AddModule(IModule module)
        {
            if (!IsRunning)
                _modules.Add(module);
        }

        public void ChartUpdate(IModule module, string name, ChartType type, decimal value)
        {
            if (OnChartUpdate != null)
                OnChartUpdate(this, module, name, value, type);
        }

        /// <summary>
        /// Close an existing current position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public StatusType ClosePosition(Position pos)
        {
            return SubmitOrder(CreateOrder(pos.Security.Name, Direction.Flat, pos.Quantity));
        }

        public void Deinitialize()
        {
            if (IsRunning)
                throw new Exception("Could not deinitialize agent if it is currently running, stop it from running first.");

            Stream.GotNewBar -= Stream_GotNewBar;
        }

        //Model functions
        public abstract void Entry();

        public abstract void Exit();

        /// <summary>
        /// Initialize this agent, should be runned only once when booting up the agent
        /// </summary>
        public virtual void Initialize()
        {
            LocalLog(LogLevel.Debug, "Initializing agent with id {0}, symbol {1} and timeframe {2}", AgentId, Symbol, TimeFrame);
            //Check for previous initialization
            if (_initialized)
            {
                LocalLog(LogLevel.Warn, "Failed to initialize agent with id {0}, already initialized", AgentId);
                return;
            }

            //Set standard indicators
            IndicatorManagement = new IndicatorManager(this);
            StandardIndicators = new Indicators.StandardIndicators(IndicatorManagement);

            //Subscribe for new data entries
            foreach (var stream in Portfolio.Streams.Values)
                stream.GotNewBar += Stream_GotNewBar;

            //Associate agent
            _modules.ForEach(x => x.Agent = this);

            //init all module
            _modules.ForEach(x => x.Initialize());

            //Set all events
            _modules.ForEach(AddEvent);

            //Set current decisions
            CurrentState = new Dictionary<string, List<AgentState>>();
            foreach (var stream in Portfolio.Streams)
                CurrentState.Add(stream.Key, new List<AgentState>());

            //Set empty objects
            CurrentBar = new Dictionary<string, Bar>();
            CurrentTick = new Dictionary<string, Tick>();
            Bars = new Data.Bars.BarIndexerImpl(_portfolio);
            _results = new Results(0, (PortfolioManager)Portfolio, AgentId);
            Positions = new PositionTracker(_portfolio.Account);

            //Log backfilling information
            IsBackfilling = BackFillingBars > 0;
            LocalLog(LogLevel.Debug, "Backfilling enabled: {0}, bars: {1}, trading time: {2}", IsBackfilling, BackFillingBars, TimeSpan.FromSeconds(BackFillingBars*TimeFrame.TotalSeconds));

            //Set current initialization point
            _initialized = true;
            LocalLog(LogLevel.Debug, "Initializing agent with id {0}, symbol {1} and timeframe {2}. Succeeded!", AgentId, Symbol, TimeFrame);
        }

        public void Log(LogSeverity severity, string message, params object[] args)
        {
            LogEventInfo logEvent = new LogEventInfo(LogLevel.FromString(severity.ToString()), "User", string.Format(message, args));
            logEvent.Properties["AgentID"] = AgentId;
            logEvent.Properties["Occured"] = Security.LastTickEvent;
            _loggerUser.Log(logEvent);
        }

        public abstract void MoneyManagement(PendingOrder order);

        public abstract void RiskManagement(PendingOrder order);

        public void SetName(string name)
        {
            LocalLog(LogLevel.Debug, "Setting agent name (was {0}) to {1}", _name, name);
            _name = name;
        }

        public void SetPortfolio(PortfolioManager portfolio)
        {
            LocalLog(LogLevel.Debug, "Setting agent portfolio to portfolio with id {0}", portfolio.Id);
            if (_portfolio == null)
                _portfolio = portfolio;
            else
                LocalLog(LogLevel.Warn, "Could not set new portfolio (with id {0}) to agent, portfolio was already set to {1}", portfolio.Id, Portfolio.Id);
        }

        public void SetPositionsTracker(IPositionTracker postracker)
        {
            LocalLog(LogLevel.Debug, "Setting position tracker to type of {0}", postracker.GetType().Name);
            Positions = postracker;
        }

        public void Start()
        {
            LocalLog(LogLevel.Info, "Agent start signal received, starting agent and allowing it to process data.");
            IsRunning = true;
            _started = DateTime.UtcNow;
        }

        public void Stop()
        {
            LocalLog(LogLevel.Info, "Agent stop signal received, stopping agent from processing data.");
            IsRunning = false;
        }

        #endregion Public Methods

        #region Private Methods

        private void ClearAgentSate()
        {
            foreach (var dec in CurrentState.Values)
                dec.Clear();
        }

        /// <summary>
        /// Send a log event that is not from the user
        /// </summary>
        /// <param name="lvl"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private void LocalLog(LogLevel lvl, string message, params object[] args)
        {
            _logger.Log(lvl, "[Agent: {0}] " + string.Format(message, args), AgentId);
        }

        private MethodInfo SearchModuleMethod(Type instance, string name, params Type[] parmType)
        {
            return instance.GetMethods()
                                 .Where(x => x.Name == name)
                                 .Where(x => x.GetParameters().Length == parmType.Length)
                                 .FirstOrDefault(x => x.GetParameters()[0].ParameterType == parmType[0]);
        }

        private void Stream_GotNewBar(string symbol, int interval)
        {
            //Set the current bar
            try
            {
                CurrentBar[symbol] = Portfolio.Streams[symbol][interval][-1, interval];
            }
            catch
            {}

            //Execute the bar event if this agent is running
            if (IsRunning)
                OnBar(CurrentBar[symbol]);
        }

        #endregion Private Methods
    }
}