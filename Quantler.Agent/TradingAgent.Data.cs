using NLog;
using Quantler.Interfaces;
using System;

namespace Quantler.Agent
{
    /// <summary>
    /// Partial class is used for managing trading agents data requests
    /// </summary>
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Public Methods

        /// <summary>
        /// Add a new sample to this agent for processing (can only be done if the agent is not already running)
        /// </summary>
        /// <param name="stream"></param>
        public void AddDataStream(DataStream stream)
        {
            _logger.Debug("AddDataStream: Processing request for symbol {0} and timeframe {1}", stream.Security.Name, stream.DefaultInterval);
            if (!IsRunning)
            {
                _portfolio.AddStream(stream);
            }
            else
                _logger.Debug("AddDataStream: Could not add datastream, agent is already running.");
        }

        public void AddDataStream(SecurityType type, string name)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type]));
        }

        public void AddDataStream(SecurityType type, string name, TimeSpan interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], (int)interval.TotalSeconds));
        }

        public void AddDataStream(SecurityType type, string name, int interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], interval));
        }

        public void AddDataStream(SecurityType type, string name, BarInterval interval)
        {
            AddDataStream(new OHLCBarStream(Portfolio.Securities[name, type], (int)interval));
        }

        public void SetDefaultStream(DataStream stream)
        {
            LocalLog(LogLevel.Debug, "Setting new default stream Symbol = {0} current timeframe = {1}", stream.Security.Name, TimeFrame);
            Stream = stream;
            AddDataStream(stream);
        }

        #endregion Public Methods
    }
}