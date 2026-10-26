using System;
using System.Collections.Generic;

namespace HarvestIsle.Core
{
    public sealed partial class FarmModel
    {
        public readonly List<WorkerAction> LastWorkerActions=new List<WorkerAction>();
        public int CrewCapacity {get{return State.workerHouse*2;}}
        public int AssignedCrew {get{return State.workers.Count+State.recruitment.roster.FindAll(w=>w.assigned).Count;}}
        public bool HasWorkingCrew {get{return State.workers.Count>0||State.recruitment.roster.Exists(w=>w.assigned&&w.jobs>0);}}
        public bool HasUnrevealedWorkers {get{return State.recruitment.reveal.Exists(r=>!r.revealed);}}
        public SpecialistWorker OwnedWorker(string id){return State.recruitment.roster.Find(w=>w.id==id);}
        public static bool ValidRecruitment(RecruitmentState state,int house,int legacyCount,double now)
        {
            if(state==null)return true; // Missing in legacy schema-1 saves; Unity may emit an empty inline object.
            if(state.serial<0||state.serial>1000000000||state.totalDraws<0||state.totalDraws>1000000000||state.serial>state.totalDraws||state.sinceExpert<0||state.sinceExpert>9||state.sinceMaster<0||state.sinceMaster>49||state.sinceExpert>state.sinceMaster||state.sinceMaster>state.totalDraws||state.notes<0||state.notes>1000000)return false;
            if((long)state.totalDraws>5L*state.serial)return false;
            int assigned=legacyCount;var ids=new HashSet<string>();
            if(state.roster!=null)
            {
                if(state.roster.Count>WorkerCatalog.All.Length)return false;
                foreach(var w in state.roster)
                {
                    var spec=w==null?null:WorkerCatalog.Find(w.id);
                    if(spec==null||!ids.Add(w.id)||w.training<0||w.training>2||w.jobs<0||w.jobs>100||w.completed<0||w.completed>1000000000||w.cursor<0||w.cursor>=MaxPlots||!IsFinite(w.nextActionAt)||w.nextActionAt<0||w.nextActionAt>now+24||w.anyCrop&&spec.role!=0)return false;
                    if(w.assigned)assigned++;
                }
            }
            if(assigned>house*2)return false;
            if(state.reveal!=null)
            {
                if(state.reveal.Count!=0&&state.reveal.Count!=1&&state.reveal.Count!=5)return false;
                if(state.reveal.Count>0&&(state.serial==0||state.totalDraws<state.reveal.Count))return false;
                foreach(var r in state.reveal)
                {var spec=r==null?null:WorkerCatalog.Find(r.id);if(spec==null||!ids.Contains(r.id)||r.notes!=(r.duplicate?spec.DuplicateNotes:0))return false;}
            }
            return true;
        }
        public ActionResult HireSpecialist(string id)
        {
            var spec=WorkerCatalog.Find(id);
            if(spec==null)return Fail("Không tìm thấy nhân công.");
            if(State.workerHouse==0)return Fail("Xây Nhà nhân công trước khi tuyển.");
            if(OwnedWorker(id)!=null)return Fail("Nhân công này đã có trong đội ngũ.");
            if(State.coins<spec.HirePrice)return Fail("Chưa đủ xu thuê đích danh.");
            State.coins-=spec.HirePrice;State.recruitment.roster.Add(new SpecialistWorker{id=id});
            return Ok("Đã tuyển "+spec.name+". Mua ca rồi phân công trong Đội ngũ.");
        }
        public ActionResult BuyWorkerShift(string id)
        {
            var worker=OwnedWorker(id);var spec=WorkerCatalog.Find(id);
            if(worker==null)return Fail("Bạn chưa tuyển người này.");
            if(worker.jobs>100-WorkerCatalog.ShiftJobs)return Fail("Tối đa 100 việc trả trước mỗi người.");
            if(State.coins<spec.ShiftPrice)return Fail("Chưa đủ xu trả ca.");
            State.coins-=spec.ShiftPrice;worker.jobs+=WorkerCatalog.ShiftJobs;
            return Ok(spec.name+": thêm 10 việc. Chỉ trừ lượt khi làm thành công.");
        }
        public ActionResult AssignSpecialist(string id,bool assigned)
        {
            var worker=OwnedWorker(id);if(worker==null)return Fail("Chưa tuyển người này.");
            if(worker.assigned==assigned)return Fail("Phân công không thay đổi.");
            if(assigned&&AssignedCrew>=CrewCapacity)return Fail("Đã đủ chỗ làm việc. Cho một người nghỉ hoặc nâng nhà.");
            if(assigned&&worker.jobs==0)return Fail("Mua ca làm việc trước khi phân công.");
            worker.assigned=assigned; // Preserve cooldown across switching to prevent instant-action farming.
            return Ok(assigned?"Đã phân công. Nhân công chỉ làm khi bạn đang chơi ở ruộng.":"Đã cho nghỉ; giữ nguyên ca chưa dùng.");
        }
        public ActionResult SetWorkerAnyCrop(string id,bool value)
        {
            var worker=OwnedWorker(id);if(worker==null||WorkerCatalog.Find(id).role!=0)return Fail("Chỉ thợ thu hoạch có chế độ này.");
            worker.anyCrop=value;return Ok(value?"Ưu tiên cây sở trường; cây khác dùng kỹ năng 0/3.":"Chỉ thu cây đúng chuyên môn.");
        }
        public ActionResult TrainSpecialist(string id)
        {
            var worker=OwnedWorker(id);if(worker==null||worker.training>=2)return Fail("Không thể huấn luyện thêm.");
            int notes=worker.training==0?8:20,cost=worker.training==0?60:180;
            if(State.recruitment.notes<notes||State.coins<cost)return Fail("Chưa đủ tài liệu hoặc xu huấn luyện.");
            State.recruitment.notes-=notes;State.coins-=cost;worker.training++;
            return Ok("Huấn luyện bậc "+worker.training+": giảm 4 giây/việc; bậc 2 thêm 1 kỹ năng thu đúng nghề.");
        }
        public ActionResult RetireLegacyWorker(int index)
        {
            if(index<0||index>=State.workers.Count)return Fail("Nhân công không còn trong đội.");
            State.workers.RemoveAt(index);return Ok("Đã kết thúc hợp đồng cũ; không hoàn tiền cọc.");
        }
        // Random outcomes are committed before any card flips. The UI never rolls on reveal.
        public ActionResult RecruitWorkers(int count,int expectedSerial,Func<int,int> random)
        {
            var state=State.recruitment;
            if(State.workerHouse==0)return Fail("Xây Nhà nhân công trước khi lật thẻ.");
            if(count!=1&&count!=5||random==null||expectedSerial!=state.serial)return Fail("Lượt tuyển không hợp lệ hoặc đã xử lý.");
            if(HasUnrevealedWorkers)return Fail("Lật hết các thẻ đang chờ trước khi tuyển tiếp.");
            if(state.totalDraws>1000000000-count||state.serial>=1000000000||state.notes>1000000-20*count)return Fail("Đã đạt giới hạn sổ tuyển dụng.");
            int price=WorkerCatalog.DrawPrice*count;if(State.coins<price)return Fail("Chưa đủ xu lật thẻ.");
            int expert=state.sinceExpert,master=state.sinceMaster,notes=0;
            var owned=new HashSet<string>();foreach(var w in state.roster)owned.Add(w.id);
            var cards=new List<WorkerReveal>();var hires=new List<SpecialistWorker>();
            for(int i=0;i<count;i++)
            {
                int roll=random(10000);if(roll<0||roll>=10000)return Fail("Nguồn ngẫu nhiên không hợp lệ.");
                int rarity=roll<6000?1:roll<8800?2:roll<9800?3:4;
                if(master>=49)rarity=4;else if(expert>=9&&rarity<3)rarity=3;
                var pool=new List<WorkerSpec>();foreach(var spec in WorkerCatalog.All)if(spec.rarity==rarity)pool.Add(spec);
                int choice=random(pool.Count);if(choice<0||choice>=pool.Count)return Fail("Nguồn ngẫu nhiên không hợp lệ.");
                var chosen=pool[choice];bool duplicate=!owned.Add(chosen.id);
                if(!duplicate)hires.Add(new SpecialistWorker{id=chosen.id});
                int value=duplicate?chosen.DuplicateNotes:0;notes+=value;
                cards.Add(new WorkerReveal{id=chosen.id,duplicate=duplicate,notes=value});
                expert=rarity>=3?0:expert+1;master=rarity==4?0:master+1;
            }
            State.coins-=price;state.serial++;state.totalDraws+=count;state.sinceExpert=expert;state.sinceMaster=master;state.notes+=notes;
            state.roster.AddRange(hires);state.reveal=cards;
            return Ok("Đã tuyển "+count+" thẻ. Chạm từng thẻ để lật.");
        }
        public ActionResult RevealWorker(int serial,int index)
        {
            var state=State.recruitment;
            if(serial!=state.serial||index<0||index>=state.reveal.Count||state.reveal[index].revealed)return Fail("Thẻ đã lật hoặc lượt không còn hợp lệ.");
            state.reveal[index].revealed=true;return Ok("Đã lật thẻ "+WorkerCatalog.Find(state.reveal[index].id).name+".");
        }
        private int RunSpecialists(double now)
        {
            int done=0;
            foreach(var worker in State.recruitment.roster)
            {
                if(!worker.assigned||worker.jobs<=0||now<worker.nextActionAt||worker.completed>=1000000000)continue;
                var spec=WorkerCatalog.Find(worker.id);var action=new WorkerAction{id=worker.id};bool success=false;
                if(spec.role==4)
                {
                    string[] animals={"chicken","cow","duck","pig","bee","fish"};
                    for(int j=0;j<animals.Length;j++)
                    {
                        string id=animals[(worker.cursor+j)%animals.Length];var animal=Animal(id);
                        if(animal.count==0)continue;
                        if(animal.producing&&AnimalRemaining(id,now)<=0)success=CollectAnimal(id,now).success;
                        else if(!animal.producing&&spec.rarity>=3&&(id=="bee"||Count("corn")>0))success=FeedAnimal(id,id!="bee",now).success;
                        if(success){action.animalId=id;worker.cursor=(worker.cursor+j+1)%animals.Length;break;}
                    }
                }
                else
                {
                    int plot=FindSpecialistPlot(worker,spec,now,true);
                    if(plot<0&&spec.role==0&&worker.anyCrop)plot=FindSpecialistPlot(worker,spec,now,false);
                    if(plot>=0)
                    {
                        var p=State.plots[plot];action.plot=plot;
                        if(spec.role<=1)
                        {action.tool=FarmTool.Harvest;success=Harvest(plot,spec.role==1||p.cropId==spec.cropId?spec.Skill(worker.training):0,now).success;}
                        else if(spec.role==2)
                        {
                            if(p.soilWater>=.55&&!IsWaterCrop(p.cropId)){p.soilWater=.05;p.drainProtection=45;action.tool=FarmTool.Drain;}
                            else {p.watered=true;p.soilWater=Math.Max(.25,p.soilWater);State.hasWatered=true;action.tool=FarmTool.Water;}
                            success=true;
                        }
                        else
                        {
                            if(p.pest>0)p.pest=0;
                            else if(p.disease>=1){p.disease=Math.Max(0,p.disease-(spec.rarity>=4?35:20));p.treatmentProtection=30;}
                            else p.soilQuality=Math.Min(2,p.soilQuality+1);
                            action.tool=FarmTool.Treat;success=true;
                        }
                        if(success)worker.cursor=(plot+1)%State.plots.Count;
                    }
                }
                if(success){worker.jobs--;worker.completed++;worker.nextActionAt=now+spec.Interval(worker.training);LastWorkerActions.Add(action);done++;}
            }
            return done;
        }
        private int FindSpecialistPlot(SpecialistWorker worker,WorkerSpec spec,double now,bool specialty)
        {
            for(int j=0;j<State.plots.Count;j++)
            {
                int i=(worker.cursor+j)%State.plots.Count;var p=State.plots[i];
                if(string.IsNullOrEmpty(p.cropId))continue;
                if(spec.role==0&&(!specialty||p.cropId==spec.cropId)&&Progress(i,now)>=1)return i;
                if(spec.role==1&&IsPerennial(p.cropId)&&Progress(i,now)>=1)return i;
                if(spec.role==2&&((spec.rarity>=3&&!IsWaterCrop(p.cropId)&&p.soilWater>=.55)||!p.watered&&Progress(i,now)<1))return i;
                if(spec.role==3&&(p.pest>0||spec.rarity>=3&&p.disease>=1||spec.rarity>=4&&p.soilQuality<2))return i;
            }
            return -1;
        }
    }
}
