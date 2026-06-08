using Guardian.Shared.Defaults;
using Guardian.Shared.Models;
using Guardian.Shared.Services;

namespace Guardian.Shared.Tests;

public sealed class RuleMatcherTests
{
    [Fact]
    public void Decide_BlocksChrome()
    {
        var decision = new RuleMatcher().Decide(new ProcessSnapshot(1, "chrome", null, null), DefaultRules.CreateDefaultConfig());

        Assert.Equal(RuleDecisionKind.Block, decision.Kind);
    }

    [Fact]
    public void Decide_AllowsExplorer()
    {
        var decision = new RuleMatcher().Decide(new ProcessSnapshot(1, "explorer.exe", null, null), DefaultRules.CreateDefaultConfig());

        Assert.Equal(RuleDecisionKind.Allow, decision.Kind);
    }

    [Fact]
    public void Decide_AllowsTencentMeetingPath()
    {
        var decision = new RuleMatcher().Decide(
            new ProcessSnapshot(1, "SomeNewMeetingProcess.exe", @"C:\Users\a\AppData\Local\Tencent Meeting\meeting.exe", null),
            DefaultRules.CreateDefaultConfig());

        Assert.Equal(RuleDecisionKind.Allow, decision.Kind);
    }

    [Theory]
    [InlineData("YingYongBao.exe", @"C:\Program Files\Tencent\应用宝\YingYongBao.exe", "")]
    [InlineData("ldplayerservice.exe", @"D:\leidian\LDPlayer\ldplayerservice.exe", "")]
    [InlineData("360SoftMgr.exe", @"C:\Program Files\360\360SoftMgr\360SoftMgr.exe", "")]
    [InlineData("GameChrome.exe", @"C:\Users\a\AppData\Roaming\secoresdk\360se6\Application\GameChrome.exe", "百度一下")]
    [InlineData("SeAppService.exe", @"C:\Users\a\AppData\Roaming\secoresdk\360se6\Application\components\seapp\SeAppService.exe", "")]
    [InlineData("Thunder.exe", @"D:\Program Files\Thunder\Thunder.exe", "")]
    [InlineData("KuGou.exe", @"D:\Program Files\KuGou\KuGou.exe", "")]
    [InlineData("SodaMusic.exe", @"D:\Programs\Soda Music\3.1.2\SodaMusic.exe", "")]
    [InlineData("unknown.exe", @"D:\Apps\抖音极速版\runner.exe", "抖音极速版")]
    public void Decide_BlocksCommonDistractionEntrypoints(string processName, string path, string title)
    {
        var decision = new RuleMatcher().Decide(new ProcessSnapshot(1, processName, path, title), DefaultRules.CreateDefaultConfig());

        Assert.Equal(RuleDecisionKind.Block, decision.Kind);
    }
}
