using WgWatch.Mikrotik.Model;
using WgWatch.Options;
using WgWatch.Quotas;
using Xunit.Abstractions;
using FluentAssertions;
namespace WgWatch.Tests;

public class QuotaTest
{
    [Theory]
    [InlineData(29,true)]
    [InlineData(30,true)]
    [InlineData(31,false)]
    public void PeriodIsOver(int period, bool expected)
    {
        Quota q = new Quota(new Interface())
        {
            StartDate = DateTime.Now.AddDays(-30),
            Period = period
        };
        q.IsOutsideMonitoringPeriod.Should().Be(expected);
    }
    [Theory]
    [InlineData(1_073_741_824, 0, 1.0)] //1GB
    [InlineData(1_073_741_824, 1_073_741_824, 2.0)] //2GB
    [InlineData(0, 1610612736, 1.5)] //2GB
    public void CalculateUsageInGigabytes(ulong rxbytes, ulong txbytes, double expected)
    {
        Quota q = new Quota(new Interface()
        {
            RxByte = rxbytes,
            TxByte = txbytes,
        });
        q.TrafficUsedGigabytes.Should().Be(expected);
    }
    [Theory]
    [InlineData(ActionOnQuotaExceeded.Auto)]
    [InlineData(ActionOnQuotaExceeded.Shut)]
    public void QuotaAction_Should_Be_DisableInterface(ActionOnQuotaExceeded actionOnQuotaExceeded)
    {
        Quota q = new Quota(new Interface
        {
            RxByte = 16106127360
        })
        {
            StartDate = DateTime.Now.AddDays(-15),
            Period = 30,
            Action = actionOnQuotaExceeded,
            QuotaLimit = 10
        };
        q.EvaluateQuotaAction().Should().Be(ActionToPerform.DisableInterface);
    }
    [Fact]
    public void QuotaAction_Should_Be_EnableInterface()
    {
        Quota q = new Quota(new Interface
        {
            RxByte = 16106127360
        })
        {
            StartDate = DateTime.Now.AddDays(-15),
            Period = 14,
            Action = ActionOnQuotaExceeded.Auto,
            QuotaLimit = 10
        };
        q.EvaluateQuotaAction().Should().Be(ActionToPerform.EnableInterface);
    }
    [Theory]
    [InlineData(true,"")]
    [InlineData(false,"wgwatch-ignore")]
    [InlineData(false,"Test interface WGwatch-ignore")]
    public void QuotaAction_Should_Be_None(bool disabled, string comment)
    {
        Quota q = new Quota(new Interface
        {
            Comment = comment,
            IsDisabled = disabled,
            RxByte = 16106127360
        })
        {
            StartDate = DateTime.Now.AddDays(-15),
            Period = 30,
            Action = ActionOnQuotaExceeded.Auto,
            QuotaLimit = 10
        };
        q.EvaluateQuotaAction().Should().Be(ActionToPerform.None);
    }
    [Fact]
    public void QuotaAction_Should_Be_None_DueTo_Action_Shut()
    {
        Quota q = new Quota(new Interface
        {
            RxByte = 0, //traffic counter was reset
            IsDisabled = true, //still disabled, because quota was exceeded
        })
        {
            StartDate = DateTime.Now.AddDays(-15),
            Period = 10,
            Action = ActionOnQuotaExceeded.Shut,
            QuotaLimit = 10
        };
        q.EvaluateQuotaAction().Should().Be(ActionToPerform.None);
    }
}