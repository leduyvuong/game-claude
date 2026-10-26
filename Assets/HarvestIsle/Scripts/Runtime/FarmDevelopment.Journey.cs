using System;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmDevelopment
    {
        private bool confirmJourneyCancel;
        private static string ContractTitle(JourneyContract c)
        {return c.kind==0?"Vụ mùa mới":c.kind==1?"Đơn hàng xưởng":c.kind==2?"Nông sản ba sao":"Bếp làng sum họp";}
        private static string ContractTask(JourneyContract c)
        {
            string name=DevelopmentCatalog.ItemName(c.item);
            if(c.kind==3)return "Phục vụ thêm "+c.target+" nồi qua Tiệm lẩu.";
            return c.kind==1?"Chế và thu thêm "+c.target+" "+name+".\nGiao "+c.quantity+" thành phẩm.":
                "Thu thêm "+c.target+" vụ "+name+(c.kind==2?" đạt ★★★":"")+".\nGiao "+c.quantity+" "+name+(c.kind==2?" ★★★":" (mọi phẩm chất)")+".";
        }
        private void DrawJourneyPage()
        {
            var m=game.Model;var j=m.State.journey;
            if(game.Online!=null){Empty("Hành trình hiện dành cho chơi solo","Trở về nông trại solo để bắt đầu. Tiến trình trên máy chủ chưa hỗ trợ hợp đồng và dự án này.");return;}
            Sections("Hợp đồng", "Tay nghề · 30 cây", "Dự án đảo");
            if(section==1){DrawMasteryPage();return;}
            if(section==2){DrawJourneyProjects();return;}
            Paint(new Rect(16,66,1032,83),"E5EDDB");
            GUI.Label(new Rect(31,76,650,30),"Năm "+m.JourneyYear+" · "+FarmModel.JourneySeasons[j.seasons%4]+" · Danh tiếng "+m.JourneyRank,cardTitle);
            GUI.Label(new Rect(31,114,980,28),m.JourneySeasonProgress+" / 12 hợp đồng · "+j.seasons+" mùa kỷ niệm · Di sản bậc "+j.legacyRank,small);
            GUI.Label(new Rect(800,80,228,30),j.medals+" huy hiệu",cardTitle);
            if(m.JourneySeasonProgress==12)
            {
                Card(new Rect(16,164,1032,230));
                GUI.Label(new Rect(38,183,980,34),"Một mùa nữa đã thành kỷ niệm",cardTitle);
                GUI.Label(new Rect(38,231,980,70),"Tổng kết để nhận 12 huy hiệu và mở chủ đề tiếp theo. Sau bốn mùa, sang năm mới; ruộng, tiền, tay nghề và dự án đều giữ nguyên.",body);
                ActionButton(new Rect(38,324,979,46),"Lưu mùa · nhận 12 huy hiệu",true,()=>Local(()=>m.AdvanceJourneySeason()));return;
            }
            if(j.active!=null)
            {
                var c=j.active;Card(new Rect(16,164,1032,242));
                if(c.kind!=3)Icon(new Rect(32,181,80,80),ItemImage(c.item));
                GUI.Label(new Rect(128,181,885,32),ContractTitle(c)+" · đang thực hiện",cardTitle);
                GUI.Label(new Rect(128,221,885,66),ContractTask(c),body);
                Meter(new Rect(33,300,555,8),(float)m.JourneyFreshProgress(c)/c.target);
                GUI.Label(new Rect(33,319,570,28),"Làm mới: "+m.JourneyFreshProgress(c)+" / "+c.target+(c.quantity>0?" · Kho giao: "+m.JourneyStock(c)+" / "+c.quantity:""),small);
                GUI.Label(new Rect(622,292,410,47),FarmModel.Money(c.reward)+" · "+c.medals+" huy hiệu",cardTitle);
                string blocker=m.JourneyBlocker(c);
                ActionButton(new Rect(621,351,408,39),blocker.Length==0?"Hoàn tất hợp đồng":blocker,blocker.Length==0,()=>Local(()=>m.CompleteJourneyContract(c.serial)));
                if(SoftButton(new Rect(33,358,556,32),confirmJourneyCancel?"Nhấn lại để hủy · bỏ tiến độ hợp đồng":"Đổi hướng · hủy hợp đồng"))
                {if(confirmJourneyCancel){Local(()=>m.CancelJourneyContract(c.serial));confirmJourneyCancel=false;}else confirmJourneyCancel=true;}
            }
            else for(int i=0;i<3;i++)
            {
                int slot=i;var c=m.JourneyOffer(i);Rect r=GridCard(i,164,242);
                GUI.Label(new Rect(r.x+14,r.y+12,309,31),ContractTitle(c),cardTitle);
                GUI.Label(new Rect(r.x+14,r.y+53,309,92),ContractTask(c),small);
                GUI.Label(new Rect(r.x+14,r.y+150,309,39),FarmModel.Money(c.reward)+" · "+c.medals+" huy hiệu",small);
                ActionButton(new Rect(r.x+14,r.y+194,306,35),m.Level<3?"Mở ở cấp 3":"Nhận hợp đồng",m.Level>=3,()=>{confirmJourneyCancel=false;Local(()=>m.AcceptJourneyContract(slot));});
            }
            GUI.Label(new Rect(16,421,1032,39),"Tính hoạt động sau khi nhận. Giao mọi phẩm chất dùng hàng thường trước, rồi ★★, ★★★. Không hết hạn.",caption);
        }
        private void DrawMasteryPage()
        {
            var m=game.Model;int first=PageStart(CropCatalog.All.Length,6);
            for(int n=0;n<6&&first+n<CropCatalog.All.Length;n++)
            {
                var crop=CropCatalog.All[first+n];Rect r=GridCard(n);int rank=m.CropMastery(crop.id),count=m.CropHarvests(crop.id),goal=FarmModel.MasteryGoal(rank);
                Icon(new Rect(r.x+12,r.y+12,65,65),"ModelThumbnails/"+crop.modelName);
                GUI.Label(new Rect(r.x+90,r.y+13,230,30),crop.displayName,cardTitle);
                GUI.Label(new Rect(r.x+90,r.y+49,230,28),"Tay nghề "+rank+" / 5",small);
                GUI.Label(new Rect(r.x+12,r.y+91,309,27),rank==5?count+" vụ · Bậc thầy":count+" / "+goal+" vụ đến bậc tiếp",small);
                Meter(new Rect(r.x+12,r.y+125,309,7),(float)count/goal);
                GUI.Label(new Rect(r.x+12,r.y+140,309,25),"Thưởng hợp đồng cây này +"+rank*2+"%",caption);
            }
            Pager(30,"tay nghề · tính từ bản cập nhật này");
        }
        private void DrawJourneyProjects()
        {
            var m=game.Model;var j=m.State.journey;
            GUI.Label(new Rect(16,73,1016,31),"Dành huy hiệu cho hướng phát triển của bạn · đang có "+j.medals,cardTitle);
            string[] descriptions={"Mỗi cấp giảm 4% thời gian chế biến.\nTối đa giảm 20%.","Mỗi cấp giảm 4% thời gian ươm.\nTối đa giảm 20%.","Mỗi cấp tăng 4% tiền hợp đồng.\nTối đa tăng 20%."};
            string[] images={"Windmill","Nursery","JuicePress"};
            for(int i=0;i<3;i++)
            {
                int kind=i,level=m.JourneyProjectLevel(i);Rect r=GridCard(i,120,282);
                Icon(new Rect(r.x+12,r.y+12,64,64),"DevelopmentThumbnails/"+images[i]);
                GUI.Label(new Rect(r.x+91,r.y+13,230,30),FarmModel.JourneyProjects[i],cardTitle);
                GUI.Label(new Rect(r.x+91,r.y+52,230,27),"Cấp "+level+" / 5",small);
                GUI.Label(new Rect(r.x+14,r.y+93,305,81),descriptions[i],body);
                GUI.Label(new Rect(r.x+14,r.y+184,305,45),level==5?"Đã hoàn thiện · giữ qua mọi mùa":m.JourneyProjectMedals(i)+" huy hiệu · "+FarmModel.Money(m.JourneyProjectCost(i)),small);
                ActionButton(new Rect(r.x+14,r.y+235,305,34),level==5?"Đã hoàn thiện":"Đầu tư dự án",level<5&&j.medals>=m.JourneyProjectMedals(i)&&m.State.coins>=m.JourneyProjectCost(i),()=>Local(()=>m.UpgradeJourneyProject(kind)));
            }
            GUI.Label(new Rect(16,419,610,38),"Di sản bậc "+j.legacyRank+" · ghi danh, không tăng sức mạnh.\n"+m.JourneyLegacyMedals+" huy hiệu + "+FarmModel.Money(m.JourneyLegacyCoins)+" / bậc",caption);
            ActionButton(new Rect(646,419,402,37),j.seasons<4?"Di sản mở sau một năm":"Ghi danh di sản · bậc "+(j.legacyRank+1),j.seasons>=4&&j.medals>=m.JourneyLegacyMedals&&m.State.coins>=m.JourneyLegacyCoins,()=>Local(()=>m.InvestJourneyLegacy()));
        }
    }
}
