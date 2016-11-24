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

using Quantler.Interfaces;
using Quantler.Interfaces.Indicators;
using System;

namespace Quantler.Indicators
{
    public class StandardIndicators : IndicatorFactory
    {
        #region Private Fields

        private readonly Interfaces.IndicatorManager _manager;

        #endregion Private Fields

        #region Public Constructors

        public StandardIndicators(Interfaces.IndicatorManager manager)
        {
            _manager = manager;
        }

        #endregion Public Constructors

        #region Public Methods

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            var nmodule = new Aroon(period, barSize, stream, computeLow, computeHigh);
            return _manager.Subscribe<Aroon>(nmodule);
        }

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new Aroon(period, barSize, stream);
            return _manager.Subscribe<Aroon>(nmodule);
        }

        public Interfaces.Indicators.Aroon Aroon(int period, DataStream stream)
        {
            var nmodule = new Aroon(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<Aroon>(nmodule);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> computeLow, Func<Bar, decimal> computeHigh)
        {
            var nmodule = new AroonOscillator(period, barSize, stream, computeLow, computeHigh);
            return _manager.Subscribe<AroonOscillator>(nmodule);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new AroonOscillator(period, barSize, stream);
            return _manager.Subscribe<AroonOscillator>(nmodule);
        }

        public Interfaces.Indicators.AroonOscillator AroonOscillator(int period, DataStream stream)
        {
            var nmodule = new AroonOscillator(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<AroonOscillator>(nmodule);
        }

        public Interfaces.Indicators.AverageTrueRange AverageTrueRange(int period, DataStream stream)
        {
            var nmodule = new AverageTrueRange(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<AverageTrueRange>(nmodule);
        }

        public Interfaces.Indicators.AverageTrueRange AverageTrueRange(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new AverageTrueRange(period, barSize, stream);
            return _manager.Subscribe<AverageTrueRange>(nmodule);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream)
        {
            var nmodule = new BollingerBands(period, sdUp, sdDown, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<BollingerBands>(nmodule);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new BollingerBands(period, sdUp, sdDown, stream, barSize);
            return _manager.Subscribe<BollingerBands>(nmodule);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType)
        {
            var nmodule = new BollingerBands(period, sdUp, sdDown, stream, barSize, maType);
            return _manager.Subscribe<BollingerBands>(nmodule);
        }

        public Interfaces.Indicators.BollingerBands BollingerBands(int period, double sdUp, double sdDown, DataStream stream, TimeSpan barSize, MovingAverageType maType, Func<Bar, decimal> compute)
        {
            var nmodule = new BollingerBands(period, sdUp, sdDown, stream, barSize, maType, compute);
            return _manager.Subscribe<BollingerBands>(nmodule);
        }

        public Interfaces.Indicators.ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream)
        {
            var nmodule = new ChandeMomentumOscillator(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<ChandeMomentumOscillator>(nmodule);
        }

        public Interfaces.Indicators.ChandeMomentumOscillator ChandeMomentumOscillator(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new ChandeMomentumOscillator(period, barSize, stream, comp);
            return _manager.Subscribe<ChandeMomentumOscillator>(nmodule);
        }

        public Interfaces.Indicators.CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream)
        {
            var nmodule = new CommodityChannelIndex(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<CommodityChannelIndex>(nmodule);
        }

        public Interfaces.Indicators.CommodityChannelIndex CommodityChannelIndex(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new CommodityChannelIndex(period, barSize, stream);
            return _manager.Subscribe<CommodityChannelIndex>(nmodule);
        }

        public Interfaces.Indicators.ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream)
        {
            var nmodule = new ExponentialMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<ExponentialMovingAverage>(nmodule);
        }

        public Interfaces.Indicators.ExponentialMovingAverage ExponentialMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new ExponentialMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<ExponentialMovingAverage>(nmodule);
        }

        Interfaces.Indicators.AverageDirectionalIndex IndicatorFactory.AverageDirectionalIndex(int period, DataStream stream)
        {
            var nmodule = new DirectionalIndex(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<DirectionalIndex>(nmodule);
        }

        Interfaces.Indicators.AverageDirectionalIndex IndicatorFactory.AverageDirectionalIndex(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new DirectionalIndex(period, stream, barSize);
            return _manager.Subscribe<DirectionalIndex>(nmodule);
        }

        public Interfaces.Indicators.Momentum Momentum(int period, DataStream stream)
        {
            var nmodule = new Momentum(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<Momentum>(nmodule);
        }

        public Interfaces.Indicators.Momentum Momentum(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new Momentum(period, barSize, stream, comp);
            return _manager.Subscribe<Momentum>(nmodule);
        }

        public Interfaces.Indicators.MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream)
        {
            var nmodule = new MovingAverage(period, maType, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<MovingAverage>(nmodule);
        }

        public Interfaces.Indicators.MovingAverage MovingAverage(int period, MovingAverageType maType, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new MovingAverage(period, barSize, maType, stream, comp);
            return _manager.Subscribe<MovingAverage>(nmodule);
        }

        public Interfaces.Indicators.MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, TimeSpan barSize, DataStream stream, Func<Bar, decimal> comp = null)
        {
            var nmodule = new MovingAverageConDiv(fastPeriod, slowPeriod, signalPeriod, barSize, stream, comp);
            return _manager.Subscribe<MovingAverageConDiv>(nmodule);
        }

        public Interfaces.Indicators.MovingAverageConDiv MovingAverageConDiv(int fastPeriod, int slowPeriod, int signalPeriod, DataStream stream)
        {
            var nmodule = new MovingAverageConDiv(fastPeriod, slowPeriod, signalPeriod, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<MovingAverageConDiv>(nmodule);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream)
        {
            var nmodule = new ParabolicSar(period, accelerator, maximum, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<ParabolicSar>(nmodule);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new ParabolicSar(period, accelerator, maximum, stream, barSize);
            return _manager.Subscribe<ParabolicSar>(nmodule);
        }

        public ParabolicSAR ParabolicSAR(int period, double accelerator, double maximum, DataStream stream, TimeSpan barSize, Func<Bar, decimal> calcHigh, Func<Bar, decimal> calcLow)
        {
            var nmodule = new ParabolicSar(period, accelerator, maximum, stream, barSize, calcHigh, calcLow);
            return _manager.Subscribe<ParabolicSar>(nmodule);
        }

        public Interfaces.Indicators.RateOfChange RateOfChange(int period, DataStream stream)
        {
            var nmodule = new RateOfChange(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<RateOfChange>(nmodule);
        }

        public Interfaces.Indicators.RateOfChange RateOfChange(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new RateOfChange(period, barSize, stream, comp);
            return _manager.Subscribe<RateOfChange>(nmodule);
        }

        public Interfaces.Indicators.RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream)
        {
            var nmodule = new RelativeStrengthIndex(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<RelativeStrengthIndex>(nmodule);
        }

        public Interfaces.Indicators.RelativeStrengthIndex RelativeStrengthIndex(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new RelativeStrengthIndex(period, stream, barSize, comp);
            return _manager.Subscribe<RelativeStrengthIndex>(nmodule);
        }

        public Interfaces.Indicators.SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream)
        {
            var nmodule = new SimpleMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<SimpleMovingAverage>(nmodule);
        }

        public Interfaces.Indicators.SimpleMovingAverage SimpleMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new SimpleMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<SimpleMovingAverage>(nmodule);
        }

        public Interfaces.Indicators.TrueRange TrueRange(DataStream stream)
        {
            var nmodule = new TrueRange(stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<TrueRange>(nmodule);
        }

        public Interfaces.Indicators.TrueRange TrueRange(DataStream stream, TimeSpan barSize)
        {
            var nmodule = new TrueRange(stream, barSize);
            return _manager.Subscribe<TrueRange>(nmodule);
        }

        public Interfaces.Indicators.WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream)
        {
            var nmodule = new WeightedMovingAverage(period, _manager.Agent.TimeFrame, stream);
            return _manager.Subscribe<WeightedMovingAverage>(nmodule);
        }

        public Interfaces.Indicators.WeightedMovingAverage WeightedMovingAverage(int period, DataStream stream, TimeSpan barSize, Func<Bar, decimal> comp = null)
        {
            var nmodule = new WeightedMovingAverage(period, barSize, stream, comp);
            return _manager.Subscribe<WeightedMovingAverage>(nmodule);
        }

        public Interfaces.Indicators.WilliamsR WilliamsR(int period, DataStream stream)
        {
            var nmodule = new WilliamsR(period, stream, _manager.Agent.TimeFrame);
            return _manager.Subscribe<WilliamsR>(nmodule);
        }

        public Interfaces.Indicators.WilliamsR WilliamsR(int period, DataStream stream, TimeSpan barSize)
        {
            var nmodule = new WilliamsR(period, stream, barSize);
            return _manager.Subscribe<WilliamsR>(nmodule);
        }

        #endregion Public Methods
    }
}