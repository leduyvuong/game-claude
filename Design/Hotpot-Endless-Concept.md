# Concept: Quán lẩu chơi vô hạn — nội dung không cạn, độ khó không trần

Bản concept ngày 08/10/2026, viết cho người phát triển tiếp (người dùng + agent). Mục tiêu: trả lời câu hỏi "làm sao chơi mãi không hết concept và luôn có level khó hơn" cho Tiệm lẩu, dựa trên những gì đã có trong code/asset, không đập đi làm lại.

## 0. Hiện trạng và vấn đề

Đã có (xem `FarmHotpotModel.cs`, AGENTS.md):

- Công thức tự phối: 2–6 nguyên liệu × 1–3 phần × 3 phong cách × 3 nền × mặn/cay 0–3 × 3 cách nấu, chấm điểm tất định 0–100.
- 22 nguyên liệu (15 từ farm có sao ★–★★★, 7 từ nhà cung cấp), kho bếp riêng, sổ 24 món, thực đơn bật/tắt.
- 4 tầng, 26 ô trang trí/tầng, 60 mẫu đồ; hạng quán ★→★★→★★★ theo 15/40 nồi, mỗi hạng +10% giá.
- Asset đã bake nhưng **chưa có luật**: 8 cấp bếp (`HotpotKitchenLevelView`), 12 nồi lẩu thành danh (Thái, Tứ Xuyên, cà chua, nấm, hải sản, bò, gà lá é, cá chua cay, dê, cua đồng, Sukiyaki, Shabu-shabu), 24 khay nguyên liệu có `ingredientKey` chưa mua được.

Vấn đề: mọi mục tiêu đều có **trần hữu hạn** — đạt ★★★ + 4 tầng + vài công thức điểm cao là "hết game". Khách không có khẩu vị riêng nên một món 90 điểm bán mãi mãi; không có áp lực nào buộc người chơi đổi cách chơi.

## 1. Nguyên tắc thiết kế (quan trọng nhất)

**Không author từng level.** Level tay-author kiểu nào cũng cạn. Nội dung vô hạn phải đến từ:

```
Nội dung = Nguyên liệu gốc (atoms) × Hệ thống tổ hợp (systems) × Công thức leo thang (scaling)
```

- **Atoms**: nguyên liệu, khẩu vị, modifier, mẫu đồ — mỗi thứ chỉ cần vài chục cái, author một lần.
- **Systems**: khách × món × bếp × mùa kinh doanh tổ hợp chéo nhau → hàng triệu tình huống.
- **Scaling**: các tham số (điểm yêu cầu, mục tiêu doanh thu, độ kiên nhẫn) là **hàm theo hạng/mùa**, không phải bảng cứng → không bao giờ hết "level sau khó hơn".

Ràng buộc giữ nguyên từ kiến trúc hiện tại: mọi thứ **tất định** (hash theo tuần UTC/chỉ số khách/số mùa, như `EnsureQuests` và lễ hội Đom Đóm đang làm), không gọi AI, luật nằm trong Core thuần C# test được bằng csc, state mới **nối cuối schema 1**, online guard như cũ.

Nguyên tắc thứ hai: **mỗi trục khó phải có đáp án nằm ở hệ thống khác của game**. Khách đòi điểm ≥90 → cần nông sản ★★★ → cần độ phì/minigame/ong ngoài farm. Nhờ vậy quán lẩu kéo người chơi quay lại toàn bộ game thay vì thành minigame cô lập.

## 2. Sáu trụ nội dung

### Trụ A — Khách có khẩu vị: động cơ puzzle vô hạn

Hiện tại khách nào cũng như nhau. Thay bằng **khách sinh tất định có hồ sơ khẩu vị**:

- Mỗi khách có vector khẩu vị `(ngọt, mặn, chua, cay)` 0–3, cờ `ăn chay`, `ngân sách`, `độ khó tính` — sinh từ hash (seed phiên + số thứ tự khách + hạng quán), không lưu save.
- Độ hài lòng = điểm món − phạt lệch khẩu vị (ví dụ 4 điểm cho mỗi bậc lệch mặn/cay, 15 điểm nếu món có thịt cho khách chay). Khách chỉ trả tiền đầy đủ khi hài lòng ≥ ngưỡng; dưới ngưỡng trả một phần và trừ điểm danh tiếng.
- **Archetype** mở dần theo hạng quán (author ~12 cái, dùng mãi): Sinh viên (ngân sách thấp, dễ tính), Dân văn phòng (nhanh), Gia đình (nhóm 4, khẩu vị trộn), Khách chay, Thực khách sành ăn (đòi điểm cao), Khách du lịch (thích đặc sản vùng miền), VIP (trả ×3 nhưng ngưỡng 90+), **Thanh tra ẩm thực** (xuất hiện khi xét lên hạng — gác cổng)…
- **Yêu cầu tùy chỉnh**: "thêm cay", "không hành", "ít mặn" — áp delta lên công thức đang bán rồi chấm lại điểm; người chơi chọn nhận/từ chối. Một món trên thực đơn giờ thành n biến thể tình huống.
- **Khách đoàn**: một bàn gọi 2–3 nồi khác nhau hoặc một nồi phải thỏa nhiều khẩu vị cùng lúc (lấy min hài lòng) — dùng luôn mẫu nồi song vị/bốn ngăn đã bake làm hình ảnh.

Vì khách là tổ hợp (archetype × vector × yêu cầu × nhóm), **không có hai ca phục vụ giống nhau**, dù atoms chỉ vài chục dòng bảng.

### Trụ B — Sổ tay ẩm thực: sưu tầm và tay nghề

- **12 món thành danh**: mỗi nồi trong bộ prefab 12 nồi có một **pattern** (nền + phong cách + 2–3 nguyên liệu chìa khóa, ví dụ Lẩu cà chua = nền chua cay + cà chua + trứng). Nấu trúng pattern lần đầu → "khám phá đặc sản", món được đặt tên, gắn hình nồi tương ứng, +15% giá vĩnh viễn. 12 phát hiện đầu tiên author tay; sau đó **văn phạm pattern** (tổ hợp nền × nguyên liệu chủ đạo × phong cách) sinh thêm "món nhà làm có tên" tất định — danh sách khám phá dài hàng trăm mục mà không cần author.
- **Tay nghề từng món**: mỗi lần phục vụ cộng XP tay nghề cho công thức; mốc 10/50/200 nồi cho +giá nhẹ, −thời gian nấu. Lưu `timesServed` nối cuối `HotpotRecipe`.
- **Khách chán món** (chống spam một món — cơ chế then chốt): cùng một món phục vụ liên tiếp bị giảm dần hệ số giá với từng archetype (hồi lại theo thời gian). Muốn doanh thu tối đa phải giữ **thực đơn rộng** phủ nhiều khẩu vị — đây là lý do nội tại để người chơi liên tục quay lại bàn chế công thức.
- Mở rộng kho nguyên liệu bằng đồ đã có sẵn trong game: cả 30 nông sản + mật ong + 15 thành phẩm chế biến (viên thả lẩu từ bột, đậu hũ…) + các `ingredientKey` của 24 khay đã bake (nấm kim châm, cải thảo, váng đậu, bò viên…). Từ 22 lên ~50 nguyên liệu mà hầu như không cần model mới.

### Trụ C — Bếp 8 cấp và Giờ cao điểm: trục kỹ năng

Biến 8 cấp bếp từ hình thành luật:

- Cấp bếp quyết định **số nồi nấu song song** (map theo điểm gắn nồi đã có: 1/1/2/4/4/4/4/6), **tốc độ nấu** và mở **cách nấu** nâng cao. Mua/nâng bếp bằng tiền + điều kiện hạng — thang nâng cấp dài gấp đôi hiện tại.
- **Giờ cao điểm** (chế độ chơi chủ động, kiểu Cooking Fever/Overcooked nhẹ): sóng khách dồn trong 90–120 giây, người chơi điều phối nồi — bàn — lên món; combo phục vụ nhanh cộng tip. Sóng `w` có số bàn và nhịp khách tăng dần, **vô hạn sóng**, điểm cao nhất lưu lại. Khó lên tự nhiên vì kiên nhẫn khách giảm theo sóng; trần kỹ năng là của người chơi, không phải của content.
- Giờ cao điểm xuất hiện theo lịch tất định (ăn khớp lễ hội Đom Đóm "khách đến nhanh gấp đôi" đã có) hoặc người chơi chủ động mở "khai trương khuyến mãi".

### Trụ D — Hạng quán vô hạn + Mùa kinh doanh: thang không trần

Hai cơ chế thay cho trần ★★★/4 tầng hiện tại:

- **Hạng quán r = 1, 2, 3, … vô hạn.** Lên hạng r cần: `nồi ≥ ceil(15 × 1.5^r)`, `điểm ngon trung bình gần đây ≥ min(95, 55 + 4r)`, qua bài kiểm tra của Thanh tra ẩm thực (một khách có hồ sơ khó nhất hạng đó), và điểm không gian (đồ trang trí/chỗ ngồi — làm 60 mẫu đồ thành chỉ số thay vì thuần hình). Thưởng: ×giá, archetype khách mới, nguyên liệu cung cấp mới, cấp bếp tiếp theo.
- **Mùa kinh doanh N** (reset nhẹ 1–2 tuần/mùa, kiểu season của game dịch vụ): mỗi mùa có mục tiêu doanh thu/sao leo thang `base × 1.18^N` và **2–4 modifier rút tất định từ pool** theo hash số mùa. Pool ~24 modifier author một lần: "Tuần lẩu chay — khách chay ×3", "Bão biển — hải sản đắt gấp đôi", "Đoàn khách đoàn — toàn nhóm 4", "Kiểm định — chỉ được dùng bếp ≤ cấp 4", "Mưa dầm — khách thưa nhưng món cay +20%"… Tổ hợp 3 modifier từ pool 24 = **2.024 mùa khác nhau** trước khi lặp, và kể cả lặp thì mục tiêu đã leo thang nên cách chơi vẫn khác. Đây chính là "các level khó" tự sinh.

### Trụ E — Kinh tế vận hành và nhân viên: áp lực quản lý

- **Chi phí vận hành theo hạng**: tiền nguyên liệu nền, lương nhân viên, "thuế mặt bằng" mỗi chu kỳ — doanh thu tăng theo hạng nhưng chi phí cũng tăng, giữ lợi nhuận biên luôn phải tối ưu.
- **Nhân viên quán** (nối vào hệ Nhân công đã có): phục vụ bàn (tự lên món chậm), phụ bếp (giữ hàng chờ nấu), thu ngân (giảm chán món nhờ gợi ý). Nhân viên có cấp, lương theo cấp — một thang tiến bộ dài nữa; về sau mở **lợi nhuận offline có giới hạn** khi đủ nhân viên (hiện quán đang session-only).
- Giá nhà cung cấp **dao động theo tuần** (hash tuần UTC) và theo modifier mùa → quyết định nhập hàng/trồng gì ngoài farm thành bài toán mỗi tuần.

### Trụ F — Chi nhánh và danh tiếng: prestige dài hạn

Khi hạng đủ cao, mở **chi nhánh** ở bối cảnh khác (bãi biển, phố núi, chợ đêm): giữ sổ công thức + tay nghề, làm lại tầng/trang trí/hạng từ đầu, nhưng phân bố khách và nguyên liệu địa phương khác hẳn (biển = hải sản rẻ + khách du lịch; núi = rau nấm rẻ + khách sành lẩu dê/gà lá é). Mỗi chi nhánh là một lần chơi lại toàn bộ thang với lời giải khác — prestige kiểu roguelite nhưng hợp game nông trại.

## 3. Bảng công thức leo thang mẫu

| Tham số | Công thức | Ý nghĩa |
| --- | --- | --- |
| Nồi cần cho hạng r | `ceil(15 × 1.5^r)` | 15, 23, 34, 51, 76… vô hạn |
| Điểm trung bình cần | `min(95, 55 + 4r)` | Tiệm cận 95 — càng cao càng buộc dùng ★★★ |
| Ngưỡng hài lòng khách | `50 + min(45, 3r)` | Khách khó dần nhưng luôn còn lời giải |
| Mục tiêu mùa N | `base × 1.18^N` | Leo thang mềm, không tường gạch |
| Khoan dung khẩu vị | `max(1, 12 − r)` bậc lệch | Về sau phải khớp gần đúng tuyệt đối |
| Hệ số chán món | `0.85^(số lần liên tiếp)` hồi 10%/phút | Chặn spam một món |
| Kiên nhẫn sóng w (cao điểm) | `×0.97^w` | Vô hạn sóng, trần là kỹ năng người chơi |

Tính chất chung: hàm mũ cho mục tiêu số lượng (luôn có cái để cày), hàm tiệm cận cho mục tiêu chất lượng (khó mãi nhưng không bao giờ vô nghiệm).

## 4. Vì sao không bao giờ hết nội dung — đếm tổ hợp

- Công thức: ~50 nguyên liệu, chọn 2–6 loại × 1–3 phần × 3×3×4×4×3 tham số nồi → **hàng trăm triệu** công thức phân biệt được về điểm.
- Tình huống phục vụ: 12 archetype × vector khẩu vị 4⁴ × yêu cầu tùy chỉnh × cỡ nhóm → mỗi ca là một bài puzzle mới.
- Level khó: 2.024 tổ hợp modifier mùa × mục tiêu leo thang vô hạn × hạng quán vô hạn.
- Sưu tầm: 12 đặc sản author + hàng trăm món có tên sinh từ pattern + mốc tay nghề từng món.

Toàn bộ chỉ cần author: ~12 archetype, ~24 modifier, ~12 pattern đặc sản, ~30 dòng nguyên liệu mới — một lần, dùng mãi.

## 5. Lộ trình triển khai gợi ý

**Giai đoạn 1 — Luật cho asset đã có (không cần model mới):**
nối ~30 nguyên liệu mới từ farm/chế biến/`ingredientKey`; hạng quán vô hạn thay trần ★★★; khách có khẩu vị + ngưỡng hài lòng; chán món; 8 cấp bếp thành luật nồi song song. Core thuần, test bằng `Tools/test-core.ps1`.

**Giai đoạn 2 — Level khó tự sinh:** mùa kinh doanh + pool modifier; Thanh tra ẩm thực gác lên hạng; yêu cầu tùy chỉnh; khách đoàn; 12 món thành danh + sổ khám phá.

**Giai đoạn 3 — Trục kỹ năng và quản lý:** giờ cao điểm vô hạn sóng; nhân viên quán; chi phí vận hành; giá cung cấp dao động theo tuần.

**Giai đoạn 4 — Dài hạn:** chi nhánh/prestige; lợi nhuận offline có nhân viên; bảng xếp hạng điểm cao điểm (khi online sẵn sàng).

**Ràng buộc kỹ thuật phải giữ:** field mới nối cuối schema 1 (save cũ đọc được, mặc định 0/rỗng); mọi sinh ngẫu nhiên là hash tất định (tuần UTC, số mùa, chỉ số khách) như nhiệm vụ tuần hiện có; khách/đơn vẫn là trạng thái phiên trừ counter tiến trình; luật trong Core độc lập Unity, UI chỉ hiển thị; guard local-only như các hệ hiện tại; không bỏ kiểm tra tiền/kho để một thao tác "trông như" thành công.

## 6. Chống phá game

- Chán món + khẩu vị đa dạng → không tồn tại "một công thức ăn tất cả".
- Chi phí vận hành theo hạng → không treo máy thu tiền vô hạn ở hạng thấp.
- Ngưỡng điểm tiệm cận 95 → nguyên liệu ★★★ (tức gameplay farm tử tế) là đường duy nhất lên hạng cao.
- Thanh tra ẩm thực → không thể lên hạng bằng cày số lượng thuần túy.
- Cap tiền/counter giữ nguyên các guard 10⁹ hiện có trong `FarmHotpotModel`.

## 7. Đối chiếu triển khai — 08/10/2026

Các mục “hiện tại/chưa có” ở đầu tài liệu là baseline trước đợt này. Luật mới nằm trong `HotpotEndlessRules.cs`, `FarmHotpotBusiness.cs`, `HotpotServiceSession.cs`; giao diện ở các partial `FarmHotpot`. Đã triển khai:

| Trụ | Luồng đã nối vào game |
| --- | --- |
| A — Khách | 12 archetype, khẩu vị/ngân sách/ăn chay, nhận hoặc từ chối yêu cầu, nhóm 2–3 dùng nồi chung và lấy hài lòng thấp nhất; giá giảm khi không đạt yêu cầu |
| B — Ẩm thực | 64 nguyên liệu có nguồn thật, 12 pattern đặc sản và hình nồi, +15% giá, phát hiện nhà làm sinh từ pattern, XP món 10/50/200 và chán món riêng archetype; khách ưu tiên món ít chán khi thực đơn đa dạng |
| C — Bếp/kỹ năng | 8 cấp bếp mua bằng tiền/hạng, 1/1/2/4/4/4/4/6 chỗ; hàng nấu và lên món; cao điểm 90 giây/sóng, nhịp khách/kiên nhẫn/mục tiêu leo thang, combo và kỷ lục local |
| D — Hạng/mùa | Lên hạng cần nồi, ≥5 đánh giá gần đây, điểm không gian và thanh tra; mùa 14 ngày hoặc hoàn thành sớm, pool 24 modifier có hiệu lực, mục tiêu ×1,18 |
| E — Vận hành | Phí 5 phút gồm mặt bằng/lương, khoản chưa trả chặn phục vụ; Nhà nhân công mở đội phục vụ/phụ bếp/thu ngân 1–3; giá cung cấp theo tuần/mùa/chi nhánh |
| F — Chi nhánh | Hạng 8 + hoàn thành mùa + tiền; xác nhận xây lại quán, giữ sổ/tay nghề/kho/nhân viên; vòng Vườn nhà/Bãi biển/Phố núi/Chợ đêm có cảnh nền và phân bố khách/giá khác nhau; nhân viên bán khi vắng mặt có giới hạn và dùng kho thật |

Quyết định cụ thể để concept chạy được:

- Hạng 1 cần 15 nồi để lên hạng tiếp, rồi 23/34/51…; dùng số mũ `r-1` để khớp chuỗi mẫu. Không author trần hạng, nhưng giữ giới hạn số 10⁹. Điểm không gian yêu cầu tối đa 312 để luôn có lời giải trong bốn tầng.
- Khách đoàn chọn phương án **một nồi chung, nhiều khẩu vị**, không phải 2–3 giao dịch nồi riêng. Chỗ bếp giữ đến khi lên món; nguyên liệu bị trừ lúc phục vụ, đơn hết kiên nhẫn giải phóng dự trữ.
- 64 nguyên liệu gồm 30 cây, bốn sản phẩm chăn nuôi/ao/ong, 14 thành phẩm ăn được và 16 hàng nhà cung cấp. “15 model thành phẩm” trong baseline có cả phân, thuốc, thức ăn vật nuôi; các món này không biến thành thức ăn. Sáu chuỗi chế biến mới dùng công trình sẵn có; đậu phụ và viên cá phải nhập từ farm.
- Chán món có sàn 20%; tay nghề có ba mốc giúp giảm thời gian nấu và tăng giá bán. Bếp hiệu lực có thể bị giới hạn cấp 4 bởi modifier dù bếp sở hữu cao hơn. Nhân viên quán dùng điều kiện Nhà nhân công, không lấy mất nhân công đang làm ngoài farm.
- Mùa theo UTC, nhưng khách/cao điểm/phí chủ động tạm dừng khi mở bảng quản lý. Bán khi vắng mặt là opt-in, cần đủ ba nhân viên, tối đa 48 nồi/8 giờ và tính phí; không phát tiền chỉ dựa trên số giờ đã thoát.
- Chi nhánh reset bố cục/tầng/bếp/hạng sau xác nhận; không xóa tiền/nông trại/sổ/kho. Mỗi chi nhánh chơi lại bốn tầng; không thêm tầng thứ năm.

**Còn ngoài phạm vi khả dụng:** bảng xếp hạng online chỉ làm khi quán có server authoritative (hiện vẫn local-only theo ràng buộc concept); đo hiệu năng/cảm ứng Android/iOS và cân bằng bằng chơi thử dài hạn. Bộ kiểm tra gồm 116 Core tests, 31 server regression checks, `hotpot-check` và `hotpot-endless-check` trong Unity; không dùng kiểm tra Editor để khẳng định đã chạy trên điện thoại.

## 8. Concept bổ sung: quán có nhịp sống và bốn tầng — 15/10/2026

**Trạng thái: đề xuất, chưa triển khai.** Đối chiếu code hiện tại: khách được bật/tắt tại ghế qua `SyncTable`; đơn đã phục vụ giữ bàn 12 giây rồi bị gỡ. Có bốn tầng, bàn mua thêm và chuyển tầng, nhưng chưa có khách đi từ cửa đến bàn, hàng chờ, dọn bàn hay chuyên môn hóa từng tầng. Tiền hiện được cộng khi lên món; chưa có giao dịch thu ngân riêng.

### Vòng một lượt khách

Khách xuất hiện ngoài cửa → xếp hàng → được dẫn đến bàn phù hợp → gọi món → chờ nấu → được lên món → ăn và trò chuyện → đứng dậy đi ra → dọn bàn → nhận lượt mới. Nếu hết kiên nhẫn hoặc bị từ chối, khách cũng đi ra cửa. Giữ nhóm khách cùng một đơn, có đủ hình người theo số thành viên.

- Trước hết làm chuyển động cửa–lối đi–ghế và ghế–cửa bằng waypoint; tầng trên đi qua điểm cầu thang. Không cho nhân vật xuyên bàn. Dùng lại model nhân vật, animation và nội thất hiện có.
- Tách vòng đời hình ảnh khỏi đơn kinh tế: khách rời bàn không gọi lại giao dịch thưởng tiền. Bản đầu giữ thời điểm thanh toán khi lên món; biểu tượng cảm ơn khi ra cửa chỉ là phản hồi hình ảnh.
- Hàng chờ dự kiến tối đa ba nhóm; khi đầy hiện “Hết bàn”. Người chơi chọn nhận nhóm phù hợp hoặc tạm ngừng nhận khách. Thời gian chờ và độ dài hàng phải thử cân bằng, không coi là số đã chốt.
- Dọn bàn một chạm, hoặc nhân viên tự làm. Cho người mới thời gian rộng; không cộng nhiều hình phạt khi cùng một đơn bị chậm.
- Đổi tầng vẫn cập nhật tất cả khách. Mở bảng quản lý tiếp tục tạm dừng như hiện tại. Đóng quán kết thúc phiên theo luật hiện có, không giữ khách ma hay nhân đôi tiền khi mở lại.

### Vai trò bốn tầng

| Tầng | Không khí | Quyết định của người chơi |
| --- | --- | --- |
| 1 — Quán xóm | Khách lẻ, bàn nhỏ, vòng phục vụ ngắn | Học gọi món–nấu–lên món–dọn; tạo dòng tiền đều |
| 2 — Sum họp | Gia đình, nhóm bạn, bàn chung | Chọn nồi thỏa nhiều khẩu vị; chuẩn bị nguyên liệu cho nhóm |
| 3 — Đặt tiệc | Sinh nhật, liên hoan, đoàn đặt trước | Chốt thực đơn và giờ nhận đoàn; giữ đủ bàn và năng lực bếp |
| 4 — VIP & sân khấu | Lounge sân thượng, quầy bar, bàn VIP, đèn đổi màu và ca sĩ | Đặt bàn, phối đồ uống với lẩu, thuê nghệ sĩ và vận hành đêm diễn |

Chuyên môn tầng là lựa chọn mềm, không khóa cứng loại khách hay xóa bàn cũ. Các tầng cùng dùng bếp/kho; mở tầng không tự sinh công suất nấu. Phí vận hành bổ sung chỉ xét sau khi mô phỏng lợi nhuận. Chưa chốt giá mở tầng mới, thay điều kiện hiện tại hay thêm tầng thứ năm.

### Tầng 4 — VIP, quầy bar và sân khấu thuê hát

Bổ sung theo yêu cầu ngày 15/10/2026. Đây là thiết kế cho lần triển khai tiếp theo, chưa phải tính năng đang chạy. Tầng 4 vẫn nằm trong giới hạn bốn tầng; phát triển từ sân thượng hiện có thành một lounge nhiệt đới buổi tối, giữ nét quán lẩu Mùa Vàng.

**Bố cục:** cửa cầu thang dẫn đến quầy đón khách; quầy bar bên hông; bàn sofa VIP quanh sân khấu nhỏ ở cuối phòng. Giữ lối phục vụ giữa các bàn, không đặt ghế chắn cửa hay mép sân khấu. Lan can nhìn ra biển, cây nhiệt đới, biển hiệu sáng, đèn dây và ánh vàng ở bàn tạo không khí ấm; sân khấu dùng đèn màu, loa, micro và phông riêng. Bản đầu đề xuất bốn bàn VIP và một quầy bar, cần kiểm tra kích thước với slot nội thất hiện tại trước khi chốt.

**Một đêm diễn:** chọn chương trình → xem phí và sức chứa → thuê ca sĩ → nhận đặt bàn → chuẩn bị đồ uống/nguyên liệu → đón khách lên tầng → phục vụ trong lúc biểu diễn → khách vỗ tay, rời quán → tổng kết doanh thu và chi phí. Chưa mở bán vé độc lập ở bản đầu: thu nhập đến từ món ăn và đồ uống thật đã phục vụ, tránh thêm một nguồn tiền tự sinh.

- **Bàn VIP:** đặt theo nhóm, có sở thích món và phong cách nhạc. Khách trả giá cao hơn khi không gian, đồ ăn và phục vụ đạt kỳ vọng; chỉ có biển VIP không tự tăng tiền. Giữ bàn trong thời gian giới hạn, báo rõ bàn trống/đã đặt/đang ăn/chờ dọn. Cần xử lý hủy đặt và hết giờ giữ bàn trước khi thêm cơ chế đặt cọc.
- **Quầy bar:** bắt đầu bằng nước ép trái cây, trà trái cây và mocktail từ nông sản/kho hiện có. Bartender có hàng pha chế riêng; mỗi ly tiêu hao nguyên liệu, cần thời gian và có giá vốn. Một nhóm có thể gọi thêm đồ uống sau khi lên lẩu. Đơn đồ uống có mã riêng, thanh toán đúng một lần, không cộng lại tiền nồi. Khách ngồi bar độc lập để giai đoạn sau.
- **Thuê hát:** ba nhóm nghệ sĩ — ca sĩ địa phương/acoustic, ban nhạc, khách mời nổi tiếng. Xem trước phí trọn ca, thời lượng, thể loại, sức hút và yêu cầu sân khấu rồi mới xác nhận. Nghệ sĩ xuất hiện ở cửa, đi lên sân khấu, biểu diễn, chào và rời đi. Mỗi ca chỉ có một chương trình; không cho thuê trùng hoặc trừ phí lặp. Có thể phát triển hợp đồng nhiều đêm sau khi cân bằng bản đầu.
- **Âm nhạc có tác dụng:** acoustic hợp nhóm trò chuyện, ban nhạc hợp tiệc, chương trình sôi động hợp nhóm thích vui. Nhạc hợp gu tăng hài lòng và khả năng gọi thêm trong giới hạn ngân sách; khách thích yên tĩnh có thể chọn tầng khác. Nghệ sĩ nổi tiếng tăng nhu cầu đến quán nhưng không bỏ qua sức chứa, hàng chờ hay khả năng phục vụ.
- **Đèn nháy và phản ứng:** đèn sân khấu đổi màu theo nhịp, chùm sáng quét chậm, biển bar phát sáng; khách lắc lư nhẹ, vỗ tay và giơ tay gọi phục vụ. Giữ ánh sáng bàn đủ đọc món/UI. Có tùy chọn giảm hiệu ứng nhấp nháy, âm lượng nhạc riêng và tắt tiếng; tắt hiệu ứng không làm giảm thưởng hay chất lượng phục vụ.
- **Nhân viên:** bartender phụ trách ly, phục vụ phụ trách bàn, chủ quán chọn chương trình và điều phối. Sức chứa bar và nhân viên là giới hạn thực tế; không mặc định mở tầng VIP là có đội ngũ miễn phí.

**Nâng cấp theo thứ tự:** mở khu VIP và đặt bàn → quầy bar cơ bản → sân khấu acoustic → bộ đèn/âm thanh → ban nhạc và khách mời. Mỗi nâng cấp phải mở hành vi hoặc lựa chọn cụ thể. Giá, thời lượng ca và hệ số thưởng chưa chốt; thử ca ít khách, đầy bàn và phục vụ chậm trước khi đặt mức chính thức.

**Bảng điều hành tầng 4:** các mục Bàn VIP / Quầy bar / Lịch diễn. Thẻ nghệ sĩ có tên, phong cách, phí, thời lượng, điều kiện và nút Thuê; khi diễn hiện bài/tiết mục hiện tại cùng thời gian còn lại. Tổng kết phân biệt doanh thu lẩu, doanh thu đồ uống, giá vốn, phí nghệ sĩ, chi phí vận hành và lợi nhuận. Gợi ý chuẩn bị kho phải dựa trên số bàn đã nhận.

**Các tình huống phải xử lý:** kho thiếu thì khóa món/ly tương ứng; hết bàn thì xếp hàng có giới hạn; đổi tầng vẫn chạy chương trình; mở quản lý tạm dừng đồng hồ biểu diễn cùng dịch vụ. Trước khi thuê phải ghi rõ đóng quán sớm kết thúc chương trình và không hoàn phí ca đã bắt đầu. Lưu mã ca đã trả phí và kết quả quyết toán để tải lại không thu tiền hay phát thưởng lần hai; không tự chạy biểu diễn offline trong bản đầu.

**Tài nguyên và kiểm thử:** kiểm kê prefab hiện có trước khi dựng bar/sân khấu; không mặc định đã có model ca sĩ hoặc animation hát. Nhạc cần nguồn tự sản xuất hoặc được phép sử dụng. Kiểm tra số bàn/đường đi, thuê và hủy, kho đồ uống, thanh toán lặp, save cũ, đóng/mở quán, đổi tầng, pause, mute và giảm nhấp nháy. Giới hạn số đèn động/hiệu ứng, ưu tiên emission và ánh sáng giả cho thiết bị di động; đo trên máy thật trước khi chốt chất lượng.

### Giao diện ưu tiên nhìn quán

Thanh tầng luôn hiện số bàn đang ăn, số món chín và cảnh báo chờ lâu; chạm cảnh báo chuyển đúng tầng/bàn. Trên đầu khách chỉ giữ biểu tượng trạng thái; chi tiết khẩu vị mở khi chọn. Khi nồi chín, một nút “Lên món” rõ ràng. Cửa có biển mở/đóng nhận khách. Camera có góc tổng quan và góc gần bàn; UI không phủ kín lối đi.

### Lý do quay lại

Khách quen có tên, món ưa thích và mốc ghé thăm mở câu chuyện ngắn; lịch đặt tiệc tạo mục tiêu chuẩn bị từ farm. Mỗi tuần chơi chọn một món chủ lực từ vụ thu để giới thiệu. Giữ hệ tay nghề, mùa, cao điểm và chi nhánh đã có; ưu tiên làm khách và không gian sống động trước khi thêm một thang điểm mới.

### Thứ tự thực hiện và nghiệm thu

1. Khách đi vào/ra trên tầng trệt; đủ số người, không xuyên bàn, không biến mất giữa lối đi. Kiểm tra phục vụ, từ chối, hết kiên nhẫn, đóng/mở quán và đổi tab.
2. Điểm cầu thang và thông báo xuyên tầng; thử cả bốn tầng đồng thời. Không mất đơn khi chuyển camera, không khách ngồi trùng bàn hoặc sai tầng.
3. Hàng chờ và dọn bàn; bổ sung luật Core và kiểm tra dự trữ kho, thời điểm nhận khách, chống thao tác lặp.
4. Chuyên môn tầng, khách quen và đặt tiệc; chơi thử nhiều ca để chốt nhịp, giá và nhân lực.
5. Tầng 4 VIP: làm đặt bàn và bar trước, sau đó thuê ca sĩ, sân khấu và đèn theo nhịp; nghiệm thu một ca hoàn chỉnh có thu–chi, khách vào/ra và đổi tầng mà không mất trạng thái.

Khi triển khai phải giữ save cũ, farm thật và guard solo-only. Chạy Core/server khi thay giao dịch; chạy `hotpot-check`, `hotpot-endless-check`, `play-interface-check`, `farm-check` khi thay UI/runtime, thêm kiểm tra đường đi và đổi tầng. Đo riêng trên Android/iOS trước khi xác nhận hiệu năng cảm ứng.
