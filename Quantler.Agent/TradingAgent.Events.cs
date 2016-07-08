using NLog;
using Quantler.Interfaces;
using Quantler.Reflection;
using Quantler.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Quantler.Agent
{
    /// <summary>
    /// Partial class is used for managing trading agents events
    /// </summary>
    public abstract partial class TradingAgent : TradingAgentManager
    {
        #region Protected Fields

        protected InvokeFactory Exec = new InvokeFactory();
        protected List<InvokeLinkVoid<PendingOrder, AgentState>> InvokeMm = new List<InvokeLinkVoid<PendingOrder, AgentState>>();
        protected List<InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>> InvokeRm = new List<InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>>();
        protected List<InvokeLinkVoid> OnCalcEvents = new List<InvokeLinkVoid>();

        #endregion Protected Fields

        #region Private Fields

        private readonly List<Reflection.InvokeLinkVoid<PendingOrder>> _invokeOnOrderUpdate = new List<Reflection.InvokeLinkVoid<PendingOrder>>();
        private List<InvokeLinkVoid<Bar>> _invokeOnBar = new List<InvokeLinkVoid<Bar>>();
        private List<InvokeLinkVoid<Trade, PendingOrder>> _invokeOnFill = new List<InvokeLinkVoid<Trade, PendingOrder>>();
        private List<InvokeLinkVoid<PendingOrder>> _invokeOnOrder = new List<InvokeLinkVoid<PendingOrder>>();
        private List<InvokeLinkVoid<Position>> _invokeOnPosition = new List<InvokeLinkVoid<Position>>();
        private List<InvokeLinkVoid<Tick>> _invokeOnTick = new List<InvokeLinkVoid<Tick>>();

        #endregion Private Fields

        #region Public Methods

        public void AddEvent(object template)
        {
            Type baseType = template.GetType().BaseType;
            DataStream[] streams = new DataStream[0];
            if (baseType == typeof(IndicatorTemplate) || baseType == typeof(Indicators.IndicatorBase))
                streams = ((Indicator)template).DataStreams;

            //Search for OnBars
            var found = SearchTemplateMethod(template.GetType(), "OnBar", typeof(Bar));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Bar), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);

                _invokeOnBar.Add(new InvokeLinkVoid<Bar>()
                {
                    Action = Expression.Lambda<Action<Bar>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(Bar),
                    DataStreams = streams
                });
            }

            //Search for OnOrders
            found = SearchTemplateMethod(template.GetType(), "OnOrder", typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnOrder.Add(new InvokeLinkVoid<PendingOrder>()
                {
                    Action = Expression.Lambda<Action<PendingOrder>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(PendingOrder),
                    DataStreams = streams
                });
            }

            //Search for OnOrderUpdate
            found = SearchTemplateMethod(template.GetType(), "OnOrderUpdate", typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnOrderUpdate.Add(new InvokeLinkVoid<PendingOrder>()
                {
                    Action = Expression.Lambda<Action<PendingOrder>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(PendingOrder),
                    DataStreams = streams
                });
            }

            //Search for OnTicks
            found = SearchTemplateMethod(template.GetType(), "OnTick", typeof(Tick));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Tick), found.GetParameters()[0].Name);
                var call = Expression.Call(referencedTemplate, found, parameter);
                _invokeOnTick.Add(new InvokeLinkVoid<Tick>()
                {
                    Action = Expression.Lambda<Action<Tick>>(call, parameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(Tick),
                    DataStreams = streams
                });
            }

            //Search for OnFills
            found = SearchTemplateMethod(template.GetType(), "OnFill", typeof(Trade), typeof(PendingOrder));
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var parameter = Expression.Parameter(typeof(Trade), found.GetParameters()[0].Name);
                var secondparameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[1].Name);
                var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                _invokeOnFill.Add(new InvokeLinkVoid<Trade, PendingOrder>()
                {
                    Action = Expression.Lambda<Action<Trade, PendingOrder>>(call, parameter, secondparameter).Compile(),
                    BaseType = baseType,
                    ParmType = typeof(Trade),
                    DataStreams = streams
                });
            }

            //Search for OnCalculates
            found = template.GetType().GetMethod("OnCalculate");
            if (found != null)
            {
                var referencedTemplate = Expression.Constant(template);
                var call = Expression.Call(referencedTemplate, found);
                OnCalcEvents.Add(new InvokeLinkVoid()
                {
                    Action = Expression.Lambda<Action>(call).Compile(),
                    BaseType = template.GetType().BaseType,
                    DataStreams = streams
                });
            }

            //Unique events
            if (template is RiskManagementTemplate)
            {
                found = template.GetType().GetMethod("RiskManagement");
                if (found != null)
                {
                    var referencedTemplate = Expression.Constant(template);
                    var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                    var secondparameter = Expression.Parameter(typeof(AgentState), found.GetParameters()[0].Name);
                    var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                    InvokeRm.Add(new InvokeLinkFunc<PendingOrder, AgentState, PendingOrder>()
                    {
                        Action = Expression.Lambda<Func<PendingOrder, AgentState, PendingOrder>>(call, parameter, secondparameter).Compile(),
                        BaseType = template.GetType().BaseType,
                        ParmType = typeof(PendingOrder),
                        ReturnType = typeof(PendingOrder)
                    });
                }
            }
            else if (template is MoneyManagementTemplate)
            {
                found = template.GetType().GetMethod("PositionSize");
                if (found != null)
                {
                    var referencedTemplate = Expression.Constant(template);
                    var parameter = Expression.Parameter(typeof(PendingOrder), found.GetParameters()[0].Name);
                    var secondparameter = Expression.Parameter(typeof(AgentState), found.GetParameters()[0].Name);
                    var call = Expression.Call(referencedTemplate, found, parameter, secondparameter);
                    InvokeMm.Add(new InvokeLinkVoid<PendingOrder, AgentState>()
                    {
                        Action = Expression.Lambda<Action<PendingOrder, AgentState>>(call, parameter, secondparameter).Compile(),
                        BaseType = template.GetType().BaseType,
                        ParmType = typeof(PendingOrder)
                    });
                }
            }

            //Order priorities
            _invokeOnBar = _invokeOnBar.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnFill = _invokeOnFill.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnOrder = _invokeOnOrder.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnPosition = _invokeOnPosition.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
            _invokeOnTick = _invokeOnTick.OrderByDescending(x => x.BaseType.GetInterfaces().Contains(typeof(Indicator))).ToList();
        }

        /// <summary>
        /// Execute the on bar event for each new bar to be processed by the associated templates
        /// </summary>
        /// <param name="bar"></param>
        public void OnBar(Bar bar)
        {
            LocalLog(LogLevel.Trace, "OnBar: Processing bar Symbol: {0}, Interval: {1}", bar.Symbol, bar.CustomInterval);

            //Execute all OnBar events (Indicators first)
            if (string.IsNullOrWhiteSpace(bar.Symbol)) return;
            DataStream stream = Portfolio.Streams[bar.Symbol];

            if (stream == null || stream.Security == null || stream.Security.Name != bar.Symbol)
                LocalLog(LogLevel.Error, "Could not find stream for symbol {0}", bar.Symbol);

            //Execute all OnBar events (Indicators)
            Exec.InvokeAll(_invokeOnBar, bar, stream, typeof(IndicatorTemplate), typeof(Indicators.IndicatorBase));

            //Execute all OnBar events (nonIndicators)
            Exec.InvokeAllExclude(_invokeOnBar, bar, typeof(IndicatorTemplate), typeof(Indicators.IndicatorBase));

            //check for main timeframe
            if (bar.CustomInterval != (int)TimeFrame.TotalSeconds)
            {
                LocalLog(LogLevel.Trace, "OnBar: Bar is discarded for agent calc events, expected interval: {0} but found interval: {1}", TimeFrame.TotalSeconds, bar.CustomInterval);
                return;
            }
            else
                LocalLog(LogLevel.Trace, "OnBar: Bar is processed for agent calc event, found interval: {0}", bar.CustomInterval);

            //Check all entry template logic
            ClearAgentSate();
            Exec.InvokeAll(OnCalcEvents, typeof(EntryTemplate));
            Entry();

            //Check all exit template logic
            ClearAgentSate();
            Exec.InvokeAll(OnCalcEvents, typeof(ExitTemplate));
            Exit();
        }

        public void OnFill(Trade fill, PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnFill: filling order with fill {0} and order {1}", fill.Id, order.OrderId);

            //Process fill for current trading agent
            Positions.GotFill(fill);

            //Execute all OnFill events
            if (_invokeOnFill.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnFill, fill, order);
        }

        public void OnOrder(PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving order event for order with id {0}", order.OrderId);

            //Execute all OnOrder events
            if (_invokeOnOrder.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnOrder, order);
        }

        public void OnOrderUpdate(PendingOrder order)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving order update event for order with id {0}", order.OrderId);

            if (_invokeOnOrderUpdate.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnOrderUpdate, order);
        }

        public void OnPosition(Position pos)
        {
            //Debug logging
            LocalLog(LogLevel.Debug, "OnOrder: receiving position update event for position with symbol {0}", pos.Security.Name);

            //Execute all OnPosition events
            if (_invokeOnPosition.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnPosition, pos);
        }

        /// <summary>
        /// Execute the on tick event for each new tick to be processed by the associated templates
        /// </summary>
        /// <param name="tick"></param>
        public void OnTick(Tick tick)
        {
            //Check tick
            if (string.IsNullOrWhiteSpace(tick.Symbol) || CurrentTick == null)
                return;

            //Set the current Tick
            CurrentTick[tick.Symbol] = tick;

            //Execute all OnBar events (Indicators first)
            if (_invokeOnTick.Count > 0 && IsRunning)
                Exec.InvokeAll(_invokeOnTick, tick, Portfolio.Streams[tick.Symbol]);
        }

        #endregion Public Methods
    }
}