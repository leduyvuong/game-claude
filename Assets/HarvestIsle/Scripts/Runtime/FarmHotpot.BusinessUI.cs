using System;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmHotpot
    {
        private bool confirmBranch;
        private bool showGuestStats;
        private int discoveryPage;
        private void DrawEndlessTable(Table table)
        {
            int key=table.floor*26+table.slot;var order=Service==null?null:Service.Find(key);
            Panel(new Rect(704,201,480,405));
            Label(720,211,398,34,"Bàn "+(table.slot+1)+(order==null?" · Trống":" · "+order.guest.Name),true);
            if(Button(1129,211,39,33,"×")){selectedTable=-1;return;}
            if(order==null){Label(720,259,435,76,"Sẵn sàng đón khách. Khách tự chọn món trên thực đơn mà kho còn đủ nguyên liệu.");return;}
            Label(720,251,441,30,order.recipe.name+" · Nhóm "+order.party.Count);
            var g=order.guest;
            Label(720,286,441,48,showGuestStats?"Ngọt "+g.sweet+" · Mặn "+g.salt+" · Chua "+g.sour+" · Cay "+g.heat:g.vegetarian?"Khách ăn chay":"Khẩu vị: "+(g.heat>=g.sweet&&g.heat>=g.salt?"thích cay":g.sweet>=g.salt?"thích thanh ngọt":"thích đậm đà"));
            Label(720,336,441,44,showGuestStats?"Ngân sách "+FarmModel.Money(g.budget)+" · Cần ≥"+g.threshold+" điểm":"Yêu cầu: "+HotpotRules.RequestNames[g.request]);
            Label(720,378,441,48,(Service.Satisfaction(order)>=g.threshold?"Hợp khẩu vị":"Cần chú ý khẩu vị")+" · Chờ "+Mathf.CeilToInt((float)order.patience)+"s"+(showGuestStats?" · "+Service.Satisfaction(order)+"/100":""));
            if(!order.answered)
            {
                Label(720,435,441,39,"Yêu cầu: "+HotpotRules.RequestNames[g.request]);
                if(Button(720,481,216,44,"Nhận yêu cầu")){Service.Answer(key,true);message=Service.LastMessage;feedbackUntil=Time.unscaledTime+6;}
                if(Button(946,481,216,44,"Từ chối khách")){Service.Answer(key,false);message=Service.LastMessage;feedbackUntil=Time.unscaledTime+6;SyncServiceTables();}
            }
            else if(order.served)Label(720,449,440,68,"Khách đang dùng bữa. Tay nghề món đã tăng thêm một lượt.");
            else if(!order.cooking)
            {
                Label(720,434,441,33,"Bếp đang dùng "+Service.CookingCount+"/"+game.Model.HotpotBurners+" chỗ");
                if(Button(720,477,442,49,"Bắt đầu nấu · "+Mathf.CeilToInt((float)game.Model.HotpotCookSeconds(order.recipe))+"s"))
                {message=Service.Cook(key)?Service.LastMessage:"Bếp hết chỗ, thiếu nguyên liệu hoặc cách nấu chưa mở.";feedbackUntil=Time.unscaledTime+6;}
            }
            else if(!order.ready)Label(720,449,440,69,"Đang nấu · còn "+Mathf.CeilToInt((float)order.cookRemaining)+"s\nNồi chiếm một chỗ bếp cho đến khi lên món.");
            else if(Button(720,471,442,55,"Lên món · "+FarmModel.Money(Service.Quote(order))))ServeTable(table.floor,table.slot);
            if(Button(720,552,442,36,showGuestStats?"Thu gọn khẩu vị":"Xem chi tiết khẩu vị"))showGuestStats=!showGuestStats;
        }
        private void DrawHotpotBusiness()
        {
            var model=game.Model;var p=model.State.hotpot.progress;
            Label(28,103,1145,38,HotpotRules.BranchNames[p.branch%4]+" · CHI NHÁNH "+(p.branch+1)+" · HẠNG "+model.HotpotRank,true);
            Label(28,151,367,105,"Lên hạng tiếp theo\nNồi: "+p.branchServed+" / "+HotpotRules.RankPots(p.rank)+"\nĐiểm gần đây: "+model.HotpotRecentTaste+" / "+HotpotRules.RankTaste(p.rank));
            Label(28,259,367,58,"Không gian: "+model.HotpotSpace+" / "+model.RequiredHotpotSpace+"\nDanh tiếng: "+p.reputation+"/100");
            if(Button(28,324,363,45,"Mời thanh tra lên hạng"))
            {
                string missing=model.HotpotPromotionMissing();
                if(missing!="")message=missing;
                else
                {
                    var free=tables.Find(t=>!t.occupied);
                    if(free!=null&&Service.Seat(free.floor*26+free.slot,true))
                    {tab=0;ShowFloor(free.floor);selectedTable=free.slot;SyncServiceTables();message="Thanh tra đã đến. Hãy xem khẩu vị và nấu món.";}
                    else message="Cần bàn trống và thực đơn đủ kho; mỗi lần chỉ mời một thanh tra.";
                }
                feedbackUntil=Time.unscaledTime+6;
            }
            Label(28,387,365,57,"Bếp cấp "+model.KitchenLevel+" · hiệu lực "+model.EffectiveKitchenLevel+"\nNấu song song "+model.HotpotBurners+" nồi");
            if(Button(28,450,363,44,model.KitchenLevel>=8?"Bếp đã đạt cấp 8":"Nâng bếp · "+FarmModel.Money(model.KitchenUpgradeCost)))
            {var result=model.UpgradeHotpotKitchen();Perform(result);if(result.success)BuildRoom();}
            Label(28,508,365,60,"Cấp 3 mở đun nhanh. Bếp tốt và tay nghề giúp giảm thời gian nấu.");
            Label(417,151,353,32,"NHÀ NHÂN CÔNG CẤP "+model.State.workerHouse,true);
            string[] roles={"Phục vụ","Phụ bếp","Thu ngân"};int[] levels={p.waiter,p.assistant,p.cashier};
            for(int i=0;i<3;i++)if(Button(417,194+i*49,350,43,roles[i]+" cấp "+levels[i]+" · "+(levels[i]>=3?"Tối đa":FarmModel.Money(150*(levels[i]+1)*(i+1)))))Perform(model.UpgradeHotpotStaff(i));
            Label(417,349,350,77,"Phí mỗi 5 phút mở cửa: "+FarmModel.Money(model.HotpotOperatingCost)+"\nChưa thanh toán: "+FarmModel.Money(p.operatingDebt));
            if(Button(417,429,350,41,"Thanh toán chi phí"))Perform(model.PayHotpotDebt());
            if(Button(417,483,350,44,p.offlineEnabled?"✓ Bán khi vắng mặt":"Bật bán khi vắng mặt"))Perform(model.SetHotpotOffline(!p.offlineEnabled,game.Now));
            Label(417,539,350,39,"Cần đủ 3 nhân viên · tối đa 48 nồi/8h");
            Label(800,151,360,32,"MÙA KINH DOANH "+(p.season+1),true);
            Label(800,192,360,73,"Doanh thu: "+FarmModel.Money(p.seasonRevenue)+" / "+FarmModel.Money(HotpotRules.SeasonTarget(p.season))+"\nNồi ≥85 điểm: "+p.seasonStars+" / "+(10+Math.Min(990,5L*p.season)));
            string modifiers="";foreach(int modifier in HotpotRules.Modifiers(p.season))modifiers+="• "+HotpotRules.ModifierNames[modifier]+"\n";
            Label(800,267,360,110,modifiers);
            if(Button(800,382,360,43,"Nhận thưởng & sang mùa"))Perform(model.ClaimHotpotSeason());
            Label(800,434,360,37,"Tự đổi mùa sau "+Math.Max(0,Math.Ceiling((p.seasonStarted+1209600-game.Now)/86400))+" ngày");
            if(!confirmBranch)
            {if(Button(800,480,360,45,"Mở chi nhánh · "+FarmModel.Money(model.HotpotBranchCost)))confirmBranch=true;}
            else
            {
                Label(800,468,360,50,"Xây lại tầng, đồ trang trí, bếp và hạng. Giữ sổ, tay nghề, kho, nhân viên.");
                if(Button(800,523,235,44,"Xác nhận chi nhánh"))
                {
                    OpenNextBranch();
                    confirmBranch=false;
                }
                if(Button(1043,523,117,44,"Hủy"))confirmBranch=false;
            }
        }
        private void DrawHotpotDiscoveries()
        {
            var p=game.Model.State.hotpot.progress;
            Label(28,103,1135,39,"SỔ TAY ẨM THỰC · "+p.discoveries.Count+" khám phá",true);
            Label(28,145,1135,54,"Đặc sản đúng nền + phong cách + nguyên liệu chủ đạo được +15% giá. Tay nghề 10 / 50 / 200 nồi tăng giá và nấu nhanh hơn.");
            int pages=Math.Max(1,(p.discoveries.Count+11)/12);discoveryPage=Mathf.Clamp(discoveryPage,0,pages-1);
            for(int n=0;n<12;n++)
            {
                int index=discoveryPage*12+n;if(index>=p.discoveries.Count)break;string key=p.discoveries[index];var parts=key.Split(':');
                string title=parts[0]=="special"?HotpotRules.SpecialtyNames[int.Parse(parts[1])]:"Lẩu "+HotpotRules.Find(parts[3]).name+" · "+HotpotRules.Styles[int.Parse(parts[2])];
                float x=28+n%3*384,y=215+n/3*67;Panel(new Rect(x,y,365,59));Label(x+12,y+8,341,43,title);
            }
            if(p.discoveries.Count==0)Label(28,222,1090,100,"Gợi ý: lẩu cà chua = vị Chua cay + nền Chua cay + cà chua + trứng. Lẩu nấm = vị Thanh ngọt + nền Rau củ + nấm + cải ngọt. Nấu thử để khám phá.");
            if(Button(28,531,100,44,"‹"))discoveryPage=(discoveryPage+pages-1)%pages;
            Label(153,539,530,32,"Trang "+(discoveryPage+1)+" / "+pages+" · 12 đặc sản và các món nhà làm");
            if(Button(1060,531,100,44,"›"))discoveryPage=(discoveryPage+1)%pages;
        }
        private void DrawHotpotRush()
        {
            var p=game.Model.State.hotpot.progress;
            Label(35,110,1100,46,"GIỜ CAO ĐIỂM",true);
            Label(35,172,1080,100,"Mỗi sóng kéo dài 90 giây. Khách đến nhanh hơn, ít kiên nhẫn hơn. Chọn bàn → nhận yêu cầu → nấu → lên món trong 8 giây sau khi chín để giữ combo tiền tip.");
            Label(35,283,1080,60,"Kỷ lục: sóng "+p.bestWave+" · "+p.bestRushScore+" điểm. Sóng 1 cần 3 bàn; mục tiêu tăng theo sóng.",true);
            Label(35,358,1070,75,"Đổi sang bảng quản lý tạm dừng sóng và khách. Nhân viên vẫn hỗ trợ khi ở Phòng ăn. Đóng quán sẽ kết thúc lượt cao điểm.");
            if(Button(35,478,534,62,Service.Rush?"Trở lại sóng "+Service.Wave:"Bắt đầu khai trương khuyến mãi"))
            {
                if(Service.Rush||Service.StartRush()){tab=0;nextArrival=.1f;message=Service.LastMessage;}
                else message="Hoàn thành các bàn đang chờ trước khi bắt đầu cao điểm.";
            }
            if(Service.Rush&&Button(593,478,557,62,"Kết thúc và lưu điểm")){Service.EndRush();game.Save();message=Service.LastMessage;}
        }
    }
}
