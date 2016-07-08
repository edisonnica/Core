using NLog;
using Quantler.Interfaces;
using Quantler.Templates;
using System.Linq;

namespace Quantler.Agent
{
    /// <summary>
    /// Partial class is used for trading agents orders and trades
    /// </summary>
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Public Methods

        public PendingOrder CreateOrder(ISecurity security, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "")
        {
            return CreateOrder(security.Name, direction, quantity, limitPrice, stopPrice, comment);
        }

        public PendingOrder CreateOrder(string symbol, Direction direction, decimal quantity, decimal limitPrice = 0, decimal stopPrice = 0, string comment = "")
        {
            return _portfolio.OrderFactory.CreateOrder(symbol, direction, quantity, limitPrice, stopPrice, comment, AgentId);
        }

        public void Flatten()
        {
            LocalLog(LogLevel.Info, "Agent flatten signal received, flattening all positions.");
            foreach (var pos in Positions.Where(x => x.UnsignedSize > 0))
            {
                LocalLog(LogLevel.Debug, "Flattening position {0} with quantity {1}", pos.Security.Name, pos.Quantity);
                ClosePosition(pos);
            }
        }

        /// <summary>
        /// Submit a new order to the portfolio
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <returns></returns>
        public StatusType SubmitOrder(PendingOrder pendingorder)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "SubmitOrder: submitting pending order for symbol {0}, direction {1} and type {2}", pendingorder.Order.Security.Name, pendingorder.Order.Direction, pendingorder.Order.Type);

            //Check if order is cancelled, then do not send this order
            if (pendingorder.IsCancelled)
            {
                LocalLog(LogLevel.Warn, "SubmitOrder: pending order for symbol {0} and type {1} was cancelled before submitting: {2}", pendingorder.Order.Security.Name, pendingorder.Order.Type, pendingorder.OrderStatus.ToString());
                return pendingorder.OrderStatus;
            }

            _portfolio.QueueOrder(pendingorder);
            return pendingorder.OrderStatus;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// Submit an order that is going through the regular template cycle of orders
        /// </summary>
        /// <param name="pendingorder"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        protected bool ProcessOrder(PendingOrder pendingorder, AgentState state)
        {
            //Track Orders
            PendingOrder entryOrder = pendingorder;
            PendingOrder rmOrder = null;

            //Check if we are allowed to trade
            var types = Templates.Select(x => x.GetType());
            RiskManagementTemplate rm = (RiskManagementTemplate)Templates.FirstOrDefault(x => x.GetType().BaseType == typeof(RiskManagementTemplate));

            if (rm != null
                && !rm.IsTradingAllowed()
                && (state != AgentState.EntryLong || state != AgentState.EntryShort))
                return false;

            //Check risk management
            if (InvokeRm.Count > 0)
            {
                //Check all Risk Management template logics
                Exec.InvokeAll(InvokeRm, pendingorder, state);

                //On Order Event
                if (InvokeRm[0].Result != null)
                {
                    rmOrder = InvokeRm[0].Result;
                    RiskManagement(rmOrder);
                }
            }

            //Submit our new stop order
            if (rmOrder != null && rmOrder.Order.IsValid)
                SubmitOrder(rmOrder);
            else if (rmOrder != null && !rmOrder.Order.IsValid)
                _logger.Warn("RM: INVALID ORDER {0} - {1}", rmOrder.OrderStatus, rmOrder.Order.Security.Name);

            //Check money management
            if (InvokeMm.Count > 0)
            {
                Exec.InvokeAll(InvokeMm, pendingorder, state);

                MoneyManagement(pendingorder);
            }

            //Submit our new entry order
            if (entryOrder.Order.IsValid)
                SubmitOrder(entryOrder);
            else
                _logger.Warn("MM: INVALID ORDER {0} - {1}", entryOrder.OrderStatus, entryOrder.Order.Security.Name);

            return true;
        }

        #endregion Protected Methods
    }
}