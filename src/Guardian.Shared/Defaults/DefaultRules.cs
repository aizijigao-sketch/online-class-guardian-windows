using Guardian.Shared.Models;

namespace Guardian.Shared.Defaults;

public static class DefaultRules
{
    public static GuardianConfig CreateDefaultConfig()
    {
        var config = new GuardianConfig();

        config.BlockRules.ProcessNames.AddRange([
            "chrome.exe", "msedge.exe", "firefox.exe", "brave.exe", "opera.exe", "vivaldi.exe",
            "iexplore.exe", "360se.exe", "360chrome.exe", "360Chrome.exe", "360se6.exe",
            "360ChromeX.exe", "360Browser.exe", "qqbrowser.exe", "sogouexplorer.exe",
            "WeChat.exe", "Weixin.exe", "QQ.exe", "TIM.exe", "Telegram.exe", "Discord.exe",
            "DingTalk.exe", "Feishu.exe",
            "steam.exe", "steamwebhelper.exe", "EpicGamesLauncher.exe", "Battle.net.exe",
            "RiotClientServices.exe", "WeGame.exe", "UbisoftConnect.exe",
            "cloudmusic.exe", "NeteaseMusic.exe", "QQMusic.exe", "QQMusicService.exe",
            "KuGou.exe", "KuGouMusic.exe", "KwMusic.exe", "KuwoMusic.exe", "SodaMusic.exe",
            "Spotify.exe", "iTunes.exe", "Music.UI.exe", "PotPlayerMini64.exe",
            "bilibili.exe", "BiliBili.exe", "bilibili-livehime.exe", "Livehime.exe",
            "douyin.exe", "Douyin.exe", "douyin_launcher.exe", "TikTok.exe",
            "Kuaishou.exe", "KuaishouLive.exe", "Xiaohongshu.exe", "RED.exe",
            "Thunder.exe", "ThunderStart.exe", "ThunderMini.exe", "ThunderPlatform.exe",
            "XLLiveUD.exe", "XLServicePlatform.exe", "DownloadSDKServer.exe",
            "uTorrent.exe", "qbittorrent.exe", "aria2c.exe", "fdm.exe", "Fdm.exe",
            "NeatDM.exe", "Motrix.exe", "BaiduNetdisk.exe", "百度网盘.exe",
            "baidunetdiskhost.exe", "AliYunDrive.exe", "aDrive.exe", "quark-cloud-drive.exe",
            "quark.exe", "115chrome.exe", "115.exe",
            "WeChatAppEx.exe", "WeChatApp.exe", "YingYongBao.exe", "yyb.exe", "yybcenter.exe",
            "AppMarket.exe", "AppMarketSvc.exe", "TencentAppStore.exe", "MobileGamePC.exe",
            "aow_exe.exe", "TBSWebRenderer.exe",
            "ldplayer.exe", "dnplayer.exe", "ldconsole.exe", "LdVBoxHeadless.exe", "LdVBoxSVC.exe",
            "ldplayerservice.exe", "Nox.exe", "NoxVMHandle.exe", "NoxVMSVC.exe",
            "HD-Player.exe", "BstkSVC.exe", "BlueStacks.exe",
            "MEmu.exe", "MEmuHeadless.exe", "MEmuSVC.exe",
            "MuMuPlayer.exe", "NemuPlayer.exe", "NemuHeadless.exe", "NemuSVC.exe",
            "AndroidEmulator.exe", "AndroidEmulatorEn.exe", "adb.exe",
            "QMEmulatorService.exe", "QMEmulator.exe", "MobileAssistant.exe",
            "GameLoop.exe", "TxGameAssistant.exe",
            "360safe.exe", "360tray.exe", "360sd.exe", "360rp.exe", "360speedld.exe",
            "360SoftMgr.exe", "360SoftMgrLite.exe", "360MobileMgr.exe", "360Game.exe",
            "360huabao.exe", "360TptMon.exe", "SoftMgrLite.exe", "SoftManager.exe",
            "QHActiveDefense.exe", "SeAppService.exe", "sesvc.exe", "sesvr.exe",
            "GameChrome.exe", "GameViewerServer.exe", "GameViewerService.exe"
        ]);

        config.BlockRules.PathKeywords.AddRange([
            "Steam", "Epic Games", @"Tencent\QQBrowser", "YingYongBao", "应用宝", "Tencent App Store",
            "LDPlayer", "leidian", "雷电", "Nox", "BlueStacks", "MEmu", "MuMu", "网易MuMu",
            "KuGou", "酷狗", "Kuwo", "酷我", "QQMusic", "CloudMusic", "网易云音乐",
            "Soda Music", "SodaMusic", "汽水音乐",
            "Douyin", "抖音", "Kuaishou", "快手", "BiliBili", "哔哩", "小红书",
            "360SoftMgr", "360软件管家", "360安全卫士", "360商店", "360Game",
            "360se6", "secoresdk", "GameChrome", "GameViewer",
            "Thunder", "迅雷", "Xunlei", "DownloadSDK", "Motrix", "Free Download Manager",
            "BaiduNetdisk", "百度网盘", "AliYunDrive", "阿里云盘", "Quark", "夸克网盘",
            "115网盘", "GameLoop", "腾讯手游助手", "TxGameAssistant"
        ]);

        config.BlockRules.WindowTitleKeywords.AddRange([
            "应用宝", "抖音", "抖音极速版", "雷电模拟器", "360软件管家", "360安全卫士",
            "迅雷", "酷狗音乐", "酷我音乐", "QQ音乐", "网易云音乐", "汽水音乐",
            "腾讯手游助手", "快手", "小红书", "哔哩哔哩"
        ]);

        config.AllowRules.ProcessNames.AddRange([
            "wemeetapp.exe", "TencentMeeting.exe", "TMeeting.exe", "explorer.exe", "notepad.exe",
            "mspaint.exe", "WINWORD.EXE", "EXCEL.EXE", "POWERPNT.EXE", "ONENOTE.EXE",
            "AcroRd32.exe", "Acrobat.exe", "FoxitPDFReader.exe", "SumatraPDF.exe", "wps.exe",
            "et.exe", "wpp.exe"
        ]);
        config.AllowRules.PathKeywords.AddRange(["Tencent Meeting", "WeMeet", @"Tencent\Meeting"]);
        config.AllowRules.FileExtensions.AddRange([
            ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".txt"
        ]);

        config.SystemProtection.ProtectedProcessNames.AddRange([
            "svchost.exe", "audiodg.exe", "dwm.exe", "sihost.exe", "ctfmon.exe",
            "TextInputHost.exe", "RuntimeBroker.exe", "ApplicationFrameHost.exe",
            "ShellExperienceHost.exe", "SearchHost.exe", "SearchApp.exe"
        ]);

        config.Notification.Messages.AddRange([
            "真正拉开差距的，不是天赋，而是一次次把心收回来的能力。",
            "把眼前这一课听好，就是在给未来的自己铺路。",
            "能控制自己的人，才有资格选择更大的自由。",
            "今天多专注一分钟，明天就少慌张一点。",
            "先把该做的事做好，喜欢的事才会更安心。",
            "优秀不是突然发生的，是每一次认真积累出来的。",
            "你现在守住的注意力，以后会变成你的底气。",
            "别急着逃离课堂，知识会在认真时悄悄长大。",
            "真正厉害的人，懂得在该学习的时候学习。",
            "先完成学习，再享受放松，这叫掌控生活。",
            "能安静听完一节课，也是一种很强的本事。",
            "不要小看这一分钟，它可能正在改变你的习惯。",
            "少一点分心，多一点进步；少一点拖延，多一点从容。",
            "现在的认真，会成为以后不用求人帮忙的能力。",
            "你不是不能玩，只是现在先把更重要的事做好。",
            "把心放回课堂，答案会慢慢变清楚。",
            "越想变强，越要学会和诱惑保持距离。",
            "今天能管住自己，明天就能相信自己。",
            "学习最怕三心二意，进步最喜欢持续专注。",
            "先别放弃这一刻，很多改变都是从这一刻开始的。",
            "不需要一下子很完美，只要现在比刚才更专注。",
            "把这节课坚持下来，你已经赢过了想偷懒的自己。",
            "真正的自由，来自有能力安排好自己的时间。",
            "听懂一个知识点，就多了一点面对未来的力量。",
            "别让短暂的娱乐，偷走长期的进步。",
            "稳住，认真听完这一段，你会感谢现在的自己。",
            "课堂上的每一次专注，都是给自己存下一份本领。",
            "先做难而正确的事，轻松会在后面等你。",
            "你可以慢一点，但不要把注意力交给无关的东西。",
            "现在认真，是为了以后有更多选择。",
            "专注不是天生的，是一次次选择练出来的。",
            "把该学的学会，玩的时候才会真正轻松。",
            "别让自己掉队，跟上老师，也跟上更好的自己。",
            "每一次收心，都是在训练更强大的自己。",
            "今天的自律，不会立刻发光，但一定会慢慢有用。",
            "先把课堂守住，其他事情下课再说。",
            "你认真听课的样子，比偷偷分心更酷。",
            "真正聪明的人，会把精力用在值得的地方。",
            "多坚持一下，困难就会少一点，信心就会多一点。",
            "把这一小段听完，进步就从这里开始。",
            "学习不是为了别人，是为了让自己以后更有办法。",
            "能坚持的人，不一定最快，但一定走得更远。",
            "现在少一点娱乐，以后多一点选择。",
            "请把注意力交给课堂，把成长交给时间。",
            "你的未来，正在被今天每一次认真悄悄塑造。",
            "静下心来，先赢下这一节课。",
            "自律不是束缚，是帮你到达想去的地方。",
            "把眼睛看向老师，把心放在问题上。",
            "别急，认真学会一点，就是很实在的进步。",
            "当你愿意专注，很多事情都会开始变简单。"
        ]);

        return config;
    }
}
