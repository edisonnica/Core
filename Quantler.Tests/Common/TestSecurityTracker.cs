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
using Quantler.Securities;
using Quantler.Tracker;
using Xunit;
using FluentAssertions;

namespace Quantler.Tests.Common
{
    public class TestSecurityTracker
    {
        private ISecurityTracker sut;

        public TestSecurityTracker()
        {
            sut = new SecurityTracker();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        public void InitTest_Succeed()
        {
            //Exchange should be set
            sut.Should().NotBeNull();
        }

        [Theory]
        [Trait("Quantler.Common", "Quantler")]
        [InlineData(SecurityType.CFD, "US300")]
        [InlineData(SecurityType.Equity, "SHELL")]
        [InlineData(SecurityType.Forex, "EURUSD")]
        public void RequestEquity_Succeed(SecurityType type, string name)
        {
            //Act
            var security = sut[name, type];

            //Assert
            security.Should().NotBeNull();
            security.Name.Should().Be(name);
            security.Type.Should().Be(type);
        }

        [Theory]
        [Trait("Quantler.Common", "Quantler")]
        [InlineData(SecurityType.Bond, "tbond")]
        [InlineData(SecurityType.Future, "CLV8 FUT GLOBEX")]
        [InlineData(SecurityType.Index, "AEX")]
        [InlineData(SecurityType.NIL, "unkown")]
        [InlineData(SecurityType.Option, "AEX PUT 430")]
        public void RequestEquity_Failed(SecurityType type, string name)
        {
            //Act
            var security = sut[name, type];

            //Assert
            security.Should().NotBeNull();
            security.Name.Should().Be(name);
            security.Type.Should().Be(SecurityType.NIL);
        }

        [Theory]
        [Trait("Quantler.Common", "Quantler")]
        [InlineData(SecurityType.CFD, "US300")]
        [InlineData(SecurityType.Equity, "SHELL")]
        [InlineData(SecurityType.Forex, "EURUSD")]
        [InlineData(SecurityType.CFD, "US300")]
        [InlineData(SecurityType.Equity, "SHELL")]
        [InlineData(SecurityType.Forex, "EURUSD")]
        public void RequestBroker_Fixed_Succeed(SecurityType type, string name)
        {
            //Act
            var security = sut[name, type];

            //Assert
            security.Should().NotBeNull();
            security.Name.Should().Be(name);
            security.Type.Should().Be(type);
        }

        [Theory]
        [Trait("Quantler.Common", "Quantler")]
        [InlineData(SecurityType.CFD, "US300")]
        [InlineData(SecurityType.Equity, "SHELL")]
        [InlineData(SecurityType.Forex, "EURUSD")]
        public void RequestBroker_Default_Succeed(SecurityType type, string name)
        {
            //Arrange
            sut = new SecurityTracker();

            //Act
            var security = sut[name, type];

            //Assert
            security.Should().NotBeNull();
            security.Name.Should().Be(name);
            security.Type.Should().Be(type);
        }
    }
}