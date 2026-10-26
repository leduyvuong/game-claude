using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using HarvestIsle.Core;
using UnityEngine;

namespace HarvestIsle
{
    public sealed partial class FarmDevelopment
    {
        private int crewTab,crewPage,crewRarity,confirmRetire=-1;
        private string crewSearch="",confirmHire="";
        private readonly Dictionary<int,float> flippedAt=new Dictionary<int,float>();
        public void ShowWorkerSection(int value){crewTab=Mathf.Clamp(value,0,3);contentPage=crewPage=0;crewSearch="";confirmHire="";}
        private static int RecruitmentRandom(int limit)
        {
            // Rejection sampling avoids modulo bias. No clock seed or reroll on card reveal.
            using(var rng=RandomNumberGenerator.Create())
            {
                byte[] bytes=new byte[4];uint value,ceiling=uint.MaxValue-uint.MaxValue%(uint)limit;
                do{rng.GetBytes(bytes);value=BitConverter.ToUInt32(bytes,0);}while(value>=ceiling);
                return (int)(value%(uint)limit);
            }
        }
        public void DrawWorkerPack(int count)
        {
            int serial=game.Model.State.recruitment.serial;
            Local(()=>game.Model.RecruitWorkers(count,serial,RecruitmentRandom));
            crewTab=2;flippedAt.Clear();
        }
        public void FlipWorkerCard(int index)
        {
            var state=game.Model.State.recruitment;
            if(index<0||index>=state.reveal.Count||state.reveal[index].revealed)return;
            Local(()=>game.Model.RevealWorker(state.serial,index));
            if(state.reveal[index].revealed)flippedAt[index]=Time.unscaledTime;
        }
        private static string RarityColor(int rarity){return new[]{"879B75","4D9C9A","9474AB","CCA34B"}[rarity-1];}
        private string WorkerStats(WorkerSpec spec,int training)
        {
            string ability=spec.role<=1?"Kỹ năng "+spec.Skill(training)+"/3":spec.role==2?(spec.rarity>=3?"Tưới + thoát úng":"Tưới cây đang lớn"):spec.role==3?(spec.rarity==4?"Bắt sâu, trị bệnh, cải đất":spec.rarity==3?"Bắt sâu + giảm 20 bệnh":"Bắt sâu miễn thuốc"):(spec.rarity>=3?"Thu + cho ăn bằng ngô":"Chỉ thu sản phẩm");
            return ability+" · "+spec.Interval(training)+" giây/việc";
        }
        private void DrawWorkersPage()
        {
            var m=game.Model;var s=m.State;var r=s.recruitment;int next=Math.Min(3,s.workerHouse+1);
            Paint(new Rect(16,10,1032,68),"E9EFDF");
            GUI.Label(new Rect(28,15,740,29),"NHÀ NHÂN CÔNG · Cấp "+s.workerHouse+" · "+m.AssignedCrew+"/"+m.CrewCapacity+" chỗ làm · "+r.roster.Count+"/72 thẻ",cardTitle);
            string price=FarmModel.Money(FarmModel.WorkerHouseCost(next))+" + "+FarmModel.WorkerHouseWood(next)+" gỗ + "+FarmModel.WorkerHouseOre(next)+" quặng";
            GUI.Label(new Rect(28,44,740,29),s.workerHouse>=3?"Tối đa 6 chỗ · nghỉ việc vẫn giữ người và số ca đã trả.":"Mỗi cấp thêm 2 chỗ · "+price,small);
            ActionButton(new Rect(810,23,222,42),s.workerHouse>=3?"Nhà đã tối đa":s.workerHouse==0?"Xây nhà":"Nâng nhà",game.Online==null&&s.workerHouse<3,()=>Local(()=>m.UpgradeWorkerHouse()));
            if(m.HasUnrevealedWorkers)crewTab=2;
            string[] labels={"Đội ngũ","Thuê đích danh","Lật thẻ","Hợp đồng cũ"};
            for(int i=0;i<4;i++)
            {
                int selected=i;bool enabled=GUI.enabled;GUI.enabled=!m.HasUnrevealedWorkers||i==2;
                if(SoftButton(new Rect(16+i*224,88,214,37),labels[i],crewTab==i))ShowWorkerSection(selected);
                GUI.enabled=enabled;
            }
            GUI.Label(new Rect(922,91,125,30),r.notes+" tài liệu",small);
            if(game.Online!=null){GUI.Label(new Rect(24,175,1000,65),"Tuyển dụng và lật thẻ hiện dành cho nông trại solo. Ví và đội ngũ online được giữ riêng.",body);return;}
            if(crewTab==2){DrawRecruitmentCards();return;}
            if(crewTab==3){DrawLegacyCrew();return;}
            DrawWorkerList(crewTab==0);
        }
        private void DrawWorkerList(bool ownedOnly)
        {
            var m=game.Model;
            GUI.Label(new Rect(18,135,60,28),"Tìm",small);
            string query=GUI.TextField(new Rect(65,134,288,30),crewSearch,40);
            if(query!=crewSearch){crewSearch=query;crewPage=0;confirmHire="";}
            if(SoftButton(new Rect(368,134,240,30),crewRarity==0?"Độ hiếm: tất cả":WorkerCatalog.Rarities[crewRarity-1])){crewRarity=(crewRarity+1)%5;crewPage=0;}
            GUI.Label(new Rect(624,136,422,29),ownedOnly?"Ca trả trước · chỉ trừ việc thành công":"Thuê đúng người · không phải chờ ngẫu nhiên",small);
            var list=new List<WorkerSpec>();
            foreach(var spec in WorkerCatalog.All)
                if((!ownedOnly||m.OwnedWorker(spec.id)!=null)&&(crewRarity==0||spec.rarity==crewRarity)&&(spec.name+" "+spec.Specialty).IndexOf(crewSearch,StringComparison.CurrentCultureIgnoreCase)>=0)list.Add(spec);
            int perPage=3,pages=Math.Max(1,(list.Count+perPage-1)/perPage);crewPage=Mathf.Clamp(crewPage,0,pages-1);ContentPages=pages;ContentPageForWorkers(pages);
            if(list.Count==0)GUI.Label(new Rect(32,204,960,70),ownedOnly?"Chưa có người phù hợp. Sang Thuê đích danh hoặc Lật thẻ để tuyển, rồi mua ca và phân công.":"Không có nhân công khớp bộ lọc.",body);
            for(int n=0;n<perPage&&crewPage*perPage+n<list.Count;n++)
            {
                var spec=list[crewPage*perPage+n];var w=m.OwnedWorker(spec.id);float x=16+n*349;Card(new Rect(x,174,334,234));Paint(new Rect(x,174,334,6),RarityColor(spec.rarity));
                Icon(new Rect(x+10,187,65,68),"ModelThumbnails/"+spec.portrait);
                GUI.Label(new Rect(x+84,185,241,30),spec.name,cardTitle);
                GUI.Label(new Rect(x+84,214,241,23),WorkerCatalog.Rarities[spec.rarity-1],small);
                GUI.Label(new Rect(x+84,237,241,26),spec.Specialty,small);
                GUI.Label(new Rect(x+10,265,316,25),WorkerStats(spec,w==null?0:w.training),caption);
                if(!ownedOnly)
                {
                    GUI.Label(new Rect(x+10,293,316,50),"Thuê: "+FarmModel.Money(spec.HirePrice)+"\nCa 10 việc: "+FarmModel.Money(spec.ShiftPrice),small);
                    ActionButton(new Rect(x+10,352,314,42),w!=null?"Đã tuyển":confirmHire==spec.id?"Xác nhận thuê · "+FarmModel.Money(spec.HirePrice):"Thuê đích danh",w==null&&m.State.workerHouse>0&&m.State.coins>=spec.HirePrice,()=>{if(confirmHire!=spec.id)confirmHire=spec.id;else {Local(()=>m.HireSpecialist(spec.id));confirmHire="";}});
                }
                else
                {
                    GUI.Label(new Rect(x+10,292,314,26),(w.assigned?(w.jobs==0?"Hết ca":"Đã phân công"):"Đang nghỉ")+" · còn "+w.jobs+" việc · học "+w.training+"/2",caption);
                    ActionButton(new Rect(x+10,321,150,32),w.assigned?"Cho nghỉ":"Phân công",w.assigned||w.jobs>0&&m.AssignedCrew<m.CrewCapacity,()=>Local(()=>m.AssignSpecialist(spec.id,!w.assigned)));
                    ActionButton(new Rect(x+170,321,154,32),"+10 việc · "+spec.ShiftPrice+" xu",w.jobs<=90&&m.State.coins>=spec.ShiftPrice,()=>Local(()=>m.BuyWorkerShift(spec.id)));
                    ActionButton(new Rect(x+10,361,150,34),w.training>=2?"Đã học tối đa":"Học: "+(w.training==0?8:20)+" TL + "+(w.training==0?60:180)+" xu",w.training<2,()=>Local(()=>m.TrainSpecialist(spec.id)));
                    if(spec.role==0&&SoftButton(new Rect(x+170,361,154,34),w.anyCrop?"Thu mọi cây":"Chỉ đúng nghề"))Local(()=>m.SetWorkerAnyCrop(spec.id,!w.anyCrop));
                }
            }
            if(SoftButton(new Rect(16,418,98,32),"← Trước")){crewPage=(crewPage+pages-1)%pages;contentPage=crewPage;}
            GUI.Label(new Rect(128,422,760,29),"Trang "+(crewPage+1)+"/"+pages+" · "+list.Count+" người · Phẩm chất thu hoạch còn tùy đất, bệnh và sâu.",caption);
            if(SoftButton(new Rect(950,418,98,32),"Tiếp →")){crewPage=(crewPage+1)%pages;contentPage=crewPage;}
        }
        // Existing UI review can still traverse the workers tab using its standard pagination API.
        private void ContentPageForWorkers(int pages){contentPage=Mathf.Clamp(contentPage,0,pages-1);}
        private void DrawRecruitmentCards()
        {
            var m=game.Model;var state=m.State.recruitment;
            GUI.Label(new Rect(20,134,1020,28),"Tỷ lệ cơ bản: 60% Phổ thông · 28% Lành nghề · 10% Chuyên gia · 2% Bậc thầy",small);
            GUI.Label(new Rect(20,161,1020,26),"Bảo đảm: còn "+(10-state.sinceExpert)+" lượt ≥ Chuyên gia; "+(50-state.sinceMaster)+" lượt Bậc thầy. Thẻ cùng bậc có cơ hội ngang nhau.",caption);
            var cards=state.reveal;
            if(cards.Count==0)GUI.Label(new Rect(80,232,900,95),"LẬT THẺ TUYỂN NGƯỜI\nMỗi lượt 30 xu kiếm trong game. Người mới vào danh sách, chưa tự đi làm.\nThẻ trùng đổi thành 1 / 3 / 8 / 20 tài liệu theo độ hiếm.",new GUIStyle(body){alignment=TextAnchor.MiddleCenter});
            for(int i=0;i<cards.Count;i++)
            {
                int index=i;var card=cards[i];var spec=WorkerCatalog.Find(card.id);float x=cards.Count==1?430:20+i*207;
                Rect rect=new Rect(x,197,195,174);float elapsed=flippedAt.TryGetValue(i,out float start)?Time.unscaledTime-start:1;
                bool front=card.revealed&&elapsed>=.22f;float width=elapsed<.44f?Mathf.Max(.04f,Mathf.Abs(Mathf.Cos(elapsed/.44f*Mathf.PI))):1;
                var matrix=GUI.matrix;GUIUtility.ScaleAroundPivot(new Vector2(width,1),rect.center);
                Paint(rect,front?"FFF6DF":"49654F");Paint(new Rect(x,197,195,6),front?RarityColor(spec.rarity):"D8B365");
                if(front)
                {
                    Icon(new Rect(x+63,210,68,64),"ModelThumbnails/"+spec.portrait);
                    GUI.Label(new Rect(x+7,278,181,25),spec.name,new GUIStyle(caption){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold});
                    GUI.Label(new Rect(x+7,305,181,24),spec.Specialty,new GUIStyle(caption){alignment=TextAnchor.MiddleCenter});
                    GUI.Label(new Rect(x+7,332,181,36),(card.duplicate?"Trùng · +"+card.notes+" tài liệu":"MỚI · "+WorkerCatalog.Rarities[spec.rarity-1]),new GUIStyle(caption){alignment=TextAnchor.MiddleCenter});
                }
                else
                {
                    var backStyle=new GUIStyle(heading){alignment=TextAnchor.MiddleCenter};Ink(backStyle,"F4E6BE");
                    GUI.Label(new Rect(x+10,228,175,90),"MÙA VÀNG\n?",backStyle);
                    if(!card.revealed&&GUI.Button(rect,GUIContent.none,GUIStyle.none))FlipWorkerCard(index);
                }
                GUI.matrix=matrix;
            }
            bool canDraw=!m.HasUnrevealedWorkers&&m.State.workerHouse>0;
            ActionButton(new Rect(20,384,230,40),"Lật 1 · 30 xu",canDraw&&m.State.coins>=30,()=>DrawWorkerPack(1));
            ActionButton(new Rect(265,384,230,40),"Lật 5 · 150 xu",canDraw&&m.State.coins>=150,()=>DrawWorkerPack(5));
            ActionButton(new Rect(510,384,235,40),"Lật hết thẻ đang chờ",m.HasUnrevealedWorkers,()=>{for(int i=0;i<state.reveal.Count;i++)FlipWorkerCard(i);});
            if(SoftButton(new Rect(760,384,285,40),"Xem đội ngũ →")&&!m.HasUnrevealedWorkers)ShowWorkerSection(0);
            GUI.Label(new Rect(20,430,1025,27),"Kết quả lưu ngay khi tuyển; đóng bảng rồi mở lại giữ nguyên thẻ và tiến độ bảo đảm.",caption);
        }
        private void DrawLegacyCrew()
        {
            var m=game.Model;var workers=m.State.workers;
            GUI.Label(new Rect(20,137,1015,52),"Giữ quyền lợi hợp đồng trước cập nhật: không trừ ca, làm mỗi 4 giây. Trang này chỉ quản lý người cũ; tuyển mới qua Thuê đích danh hoặc Lật thẻ.",small);
            string[] duties={"Tưới cây","Tưới; cấp 2+ thu kỹ năng 1","Thu vật nuôi; cấp 2+ cho ngô"};
            for(int i=0;i<3;i++)
            {
                int kind=i;float x=16+i*349;Card(new Rect(x,200,334,108));GUI.Label(new Rect(x+12,208,310,28),FarmModel.WorkerKinds[i],cardTitle);GUI.Label(new Rect(x+12,237,310,24),duties[i],caption);
                GUI.Label(new Rect(x+12,270,310,32),"Hợp đồng được bảo lưu",caption);
            }
            for(int i=0;i<workers.Count;i++)
            {
                int index=i;float x=16+i*349;GUI.Label(new Rect(x,325,330,46),FarmModel.WorkerKinds[workers[i].kind]+" · cấp "+workers[i].level,small);
                ActionButton(new Rect(x,375,334,40),confirmRetire==i?"Xác nhận nghỉ · không hoàn cọc":"Kết thúc hợp đồng",true,()=>{if(confirmRetire!=index)confirmRetire=index;else{Local(()=>m.RetireLegacyWorker(index));confirmRetire=-1;}});
            }
        }
    }
}
