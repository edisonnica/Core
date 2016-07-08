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

        public bool IsBackfilling
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Public Methods

        public void SetBackfilling(TimeSpan period)
        {
            throw new NotImplementedException();
        }

        public void SetBackFilling(int Bars)
        {
            throw new NotImplementedException();
        }

        #endregion Public Methods
    }
}