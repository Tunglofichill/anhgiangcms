# ⚒️ Huyền Thoại Lò Rèn — Legend of the Forge

<p align="center">
  <img src="Assets/_Forge/Art/Textures/KeyVisual_PocketForge.jpg" alt="Key Visual Legend of the Forge" width="800"/>
</p>

<p align="center">
  <b>Trò chơi mô phỏng công xưởng rèn thần binh kết hợp cơ chế Clicker / Idle Tycoon trên nền tảng Unity 6</b><br>
  <i>Phát triển & Kiến trúc bởi <b>Trường Giang</b></i>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Engine-Unity%206%20(6000.5.4f1)-blue?logo=unity" alt="Unity 6"/>
  <img src="https://img.shields.io/badge/Render%20Pipeline-URP-orange" alt="URP"/>
  <img src="https://img.shields.io/badge/.NET-C%23%209.0-purple?logo=csharp" alt="C#"/>
  <img src="https://img.shields.io/badge/Platform-Windows%20x64-0078D6?logo=windows" alt="Windows"/>
  <img src="https://img.shields.io/badge/Multi--AI-Gemini%20|%20Claude%20|%20Grok%20|%20Suno-success" alt="Multi-AI"/>
  <img src="https://img.shields.io/badge/Author-Tr%C6%B0%E1%BB%9Dng%20Giang-gold" alt="Author"/>
</p>

---

## 📖 1. Giới Thiệu Dự Án

**"Huyền Thoại Lò Rèn" (Legend of the Forge)** đưa người chơi vào vai một bậc thầy thợ rèn Dwarven cổ đại sâu trong lòng núi đá ngầm. Bắt đầu từ một chiếc đe thô sơ và lò than hồng, người chơi từng bước chế tác trang bị, thu hoạch quặng thần binh, tự động hóa dây chuyền máy móc và mở rộng quy mô xưởng rèn để vươn tới danh hiệu **Đại Sư Thần Binh**.

Trò chơi được phát triển theo tiêu chuẩn kỹ thuật hiện đại của **Unity 6**, áp dụng mô hình **Human-in-the-Loop** kết hợp Hệ sinh thái **Multi-AI chuyên biệt hóa cao độ** (Gemini 3.8, Claude 4.6, Grok Premium, Suno AI).

---

<p align="center">
  <img src="Assets/_Forge/Art/Textures/Screenshots/Forge_3D_Scene_Capture.png" alt="In-Engine 3D Dwarven Forge Render (Unity 6 URP)" width="880"/>
  <br>
  <i>Hình ảnh thực tế in-engine render thời gian thực trong Unity 6 URP: Native Wavefront OBJ Mesh, PBR Damascus Steel, Carved Stone Hearth, Trilight Lighting & Live Atmospheric Embers.</i>
</p>

---

## ✨ 2. Tính Năng Nổi Bật & Đột Phá Đồ Họa 3D AAA

- 🏛️ **Mô Hình Hóa 3D Thuần Bản Địa (Native Wavefront .OBJ Modeling - `ForgeObjModelGenerator.cs`):**
  - **Đe Rèn Dwarven Anvil (`Anvil_Dwarven.obj`):** Thân đe đúc gang dáng London cong thanh thoát, sừng vuốt nhọn ngang, gót đe vuông, đai sắt ôm thân cây sồi 24 cạnh và thỏi phôi kiếm thần binh rực lửa (`Hot_Ingot`).
  - **Lò Nung Vòm Đá Hoa Cương (`Furnace_Dwarven.obj`):** Bệ đá xếp khối, trụ vòm đá hoa cương chạm khắc, nón hút khói hình chóp bát giác dập đinh tán, ống khói vương miện sắt kèm bễ thổi da, xô nước tôi và củi khô.
  - **Búa Rèn Thợ Cả (`Hammer_Dwarven.obj`):** Cán gỗ sồi dài 0.54m, chuôi bịt đồng chạm khắc, đầu búa bát giác hắc thiết và hai mặt đập tôi thép Damascus sáng bóng.
- 🎨 **Chất Liệu PBR Đỉnh Cao (HD Procedural Textures - `ForgeTextureGenerator.cs`):**
  - 9 bộ PBR maps độ phân giải cao: Thép Damascus xoáy vân lượn sóng (`Tex_Damascus_Steel.png`), sắt đập búa xù xì (`Tex_Hammered_Dark_Iron.png`), đá hoa cương chạm viền (`Tex_Stone_Bricks.png`), than đỏ rực (`Tex_Burning_Embers.png`), vỏ gỗ sồi cổ thụ (`Tex_Oak_Stump.png`), đồng thau đánh bóng (`Tex_Polished_Bronze.png`).
- 💡 **Chiếu Sáng Trilight, Reflection Probe & Lửa Bập Bùng (Cinematic URP Lighting):**
  - **Trilight Ambient & Realtime Reflection Probe:** Khử triệt để hiện tượng kim loại bị đen xì, mang lại ánh phản quang kim loại sâu thẳm chân thực.
  - **Lửa Lò Bập Bùng Perlin Noise (`FurnaceFlickerLight.cs`):** Ngọn lửa lò URP Point Light dao động tần số tự nhiên kèm độ biến thiên nhiệt màu từ vàng cam ấm áp đến đỏ rực.
  - **Hạt Tàn Tro Bay Lơ Lửng (`PS_AmbientEmbers`):** Hàng chục đốm than đỏ và tàn tro phát quang bồng bềnh trong không gian xưởng rèn, tạo chiều sâu thị giác điện ảnh.
- ⚙️ **Hoạt Họa Cơ Khí Tự Động 3D (Procedural Mechanical Rigging - `MachineAnimator3D.cs`):**
  - `G1: Búa Tí Hon` — Nhịp vung đập cơ học răng cưa kèm độ nảy giật đàn hồi.
  - `G2: Cúi Đào Quặng` — Mũi khoan kim cương xoay tròn tốc độ cao kèm rung chấn trục dọc.
  - `G3: Lò Than Tự Đun` — Nồi luyện kim loại đồng dao động bập bùng theo chu kỳ sủi bọt.
  - `G4: Cối Xay Kim` — Bánh đà 8 nan xoay liên tục kết hợp trục piston chuyển động tịnh tiến ngược pha.
  - `G5: Robot Thợ Cả` — Golem tự hành mô phỏng nhịp thở hữu cơ, cánh tay nâng búa và lõi ngực phát sáng dao động.
- 🔨 **Tương Tác Đe Rèn 3D & Búa Rèn Động (`HammerVisual3D.cs`):**
  - Búa rèn tự động vung đập giáng xuống đe `CLANG!` kèm độ nảy đàn hồi (*rebound damping*) và tia lửa phát sáng mỗi lần người chơi nhấp chuột.
- 📐 **Hiệu Ứng Thị Sai Chiều Sâu 3D (`CameraParallax3D.cs`):**
  - Camera URP tự động xoay nghiêng mượt mà theo tọa độ chuột, tôn lên độ sâu không gian 3 chiều và bóng đổ mềm thực tế.
- ⚒️ **Hệ Thống Nâng Cấp Búa Rèn & Lò Nung (Tools & Forge Upgrades):**
  - **Búa Rèn Thần Binh (`UP_HAMMER`):** Gia tăng uy lực mỗi cú đập (*Tap Power*), mở rộng sản lượng quặng khai thác tức thời.
  - **Lò Nung Nhiệt Hạch (`UP_FURNACE`):** Tăng theo % toàn bộ tổng sản lượng công xưởng + Cường hóa ánh sáng Point Light 3D rực lửa theo cấp độ.
  - **Găng Tay Da Rồng (`UP_GLOVES`):** Kích hoạt tỷ lệ Bạo Kích (*Critical Strike* x5 Quặng) hiển thị chữ nổ Ruby rực lửa `💥 CRIT!`.
  - **Dầu Tôi Huyền Bí (`UP_OIL`):** Gia tăng tốc độ khai thác tự động của toàn bộ dàn máy móc (+15% DPS/cấp).
- 🛒 **Cửa Hàng Công Xưởng 2 Tab Chuyên Biệt (Two-Tab Workshop UI):**
  - **Tab 1: ⚙️ DÂY CHUYỀN MÁY MÓC (G1..G5)** — Mua sắm và nâng cấp các trạm máy tự động hóa từ *Tay Búa Tí Hon* đến *Robot Thợ Cả*.
  - **Tab 2: 🔨 BÚA & LÒ NUNG (Tools & Forge)** — Nâng cấp vũ khí thợ rèn, cường hóa lò nhiệt hạch và kích hoạt bạo kích.
- 🏺 **Đạo Cụ Xưởng Rèn 3D Mở Khóa Theo Cột Mốc (Milestone 3D Props):**
  - Giá để quặng pha lê 3D phát sáng đa diện (`Decor_OreShelf`), Rương kho báu mở nắp lộ các thỏi vàng 3D (`Decor_Chest`), Ống xả khói đồng (`Decor_SmokeStack`) và Bảng hiệu xưởng rèn cổ (`Decor_Sign`).
- 📈 **Đường Cong Kinh Tế Lũy Tiến:** Cân bằng toán học chuẩn xác $\text{Cost}(n) = \text{BaseCost} \times (1.15)^n$ cùng các mốc đột phá (*Milestone Multipliers*) x2, x4 giữ chân người chơi.
- 🏆 **8 Cấp Bậc Danh Hiệu Sử Thi:** Từ *Tập Sự (Apprentice)* đến *Đại Sư Thần Binh (Grandmaster Smith)*.
- 🌐 **Hỗ Trợ Song Ngữ (VI / EN):** Chuyển đổi ngôn ngữ Tiếng Việt và Tiếng Anh thời gian thực thông qua hệ thống Hybrid Typography (Phôi nút kim loại kết hợp Dynamic Text).
- 🎵 **Âm Nhạc Nền Độc Quyền (OST):** Bản nhạc chủ đề *"Dwarven Forge Pulse"* (95 BPM, D-Dorian, nhịp đe búa dân gian Bắc Âu lặp mượt mà).
- 🛡️ **Lưu Trữ An Toàn & Sao Lưu Kép:** Quản lý dữ liệu JSON kèm cơ chế tự động tạo file dự phòng (`.bak`), đảm bảo 100% không mất tiến trình khi mất điện đột ngột.
- 📱 **Hỗ Trợ Đa Khung Hình (Responsive Layout):** Tự động thích ứng cả màn hình ngang (*Landscape 16:9*) và màn hình dọc (*Portrait*).
- 📦 **Đóng Gói 1-Click Standalone Windows:** Tích hợp bộ công cụ Editor `ForgeBuildTools.cs` biên dịch file `.exe` độc lập chỉ với 1 thao tác.

---

## 🤖 3. Hệ Sinh Thái Multi-AI Tích Hợp

Dự án không phụ thuộc vào một công cụ AI duy nhất mà phân bổ theo ma trận chuyên môn hóa:

| Công Cụ AI | Phiên Bản | Phân Hệ Đảm Nhiệm | Sản Phẩm Đầu Ra Thực Tế | Tỷ Lệ Đóng Góp |
| :--- | :---: | :--- | :--- | :---: |
| **Google Gemini** | **3.8 Agent** | **Lead Engineering & Unity Architecture** | Toàn bộ 15 scripts C#, tối ưu Raycast, giải quyết lỗi Canvas Sibling Index, viết tool đóng gói `ForgeBuildTools.cs`. | **45%** |
| **Anthropic Claude** | **4.6 Sonnet** | **Game Design & Lore Master** | Tài liệu GDD, công thức cân bằng kinh tế lũy tiến $1.15^n$, thiết kế 5 trạm máy, 8 danh hiệu thợ rèn, từ điển song ngữ. | **25%** |
| **xAI Grok** | **Premium** | **Concept Art & 2D UI Sprites** | Background lò rèn hắc thiết (`Tex_MainMenu_BG.jpg`), bộ 3 phôi nút kim loại rèn, concept art máy móc và đạo cụ xưởng. | **20%** |
| **Suno AI** | **Free Edition** | **Audio Director & Music Producer** | Bản nhạc nền độc quyền *Dwarven Forge Pulse.mp3* (95 BPM, 44.1kHz, 320 kbps, Seamless Loop). | **10%** |

> 📄 **Xem báo cáo chi tiết minh chứng AI:** Đọc file [TAI_LIEU_MINH_CHUNG_AI_GAME.md](TAI_LIEU_MINH_CHUNG_AI_GAME.md) hoặc [Tai_Lieu_Minh_Chung_AI_Game_Huyen_Thoai_Lo_Ren.docx](Tai_Lieu_Minh_Chung_AI_Game_Huyen_Thoai_Lo_Ren.docx).

---

## 📂 4. Cấu Trúc Thư Mục Dự Án

```
Forge/
├── Assets/
│   └── _Forge/
│       ├── Art/
│       │   ├── Audio/             # Bản nhạc nền Dwarven Forge Pulse.mp3
│       │   └── Textures/          # Background lò rèn, UI Buttons, Icons, Hero Anvil/Furnace
│       ├── Data/                  # ScriptableObject BalanceConfig & 5 Generators (G1-G5)
│       ├── Scenes/
│       │   └── ForgeDesk.unity    # Scene chính hoàn chỉnh của trò chơi
│       ├── Scripts/
│       │   ├── Audio/             # AudioService.cs
│       │   ├── Core/              # GameManager.cs, LocalizationService.cs, SettingsService.cs...
│       │   ├── Economy/           # EconomyService.cs, MilestoneService.cs
│       │   ├── Editor/            # ForgeBuildTools.cs, ForgeSetupTools.cs
│       │   ├── FX/                # CameraPunch.cs, ForgeLightController.cs
│       │   ├── Save/              # SaveService.cs, OfflineService.cs, SaveData.cs
│       │   ├── UI/                # MainMenuView.cs, SettingsView.cs, HudView.cs, ShopView.cs...
│       │   └── World/             # ClickForge.cs, FurnaceVisual.cs, OrientationManager.cs...
│       └── Settings/URP/          # URP Asset, Universal Renderer & Volume Profile
├── Packages/                      # Manifest quản lý package Unity 6
├── ProjectSettings/               # Cấu hình PlayerSettings (Đứng tên Trường Giang)
├── .gitignore                     # Cấu hình loại trừ cache Library/Builds
├── README.md                      # Tài liệu tổng quan dự án
├── TAI_LIEU_MINH_CHUNG_AI_GAME.md # Báo cáo minh chứng AI chi tiết (Markdown)
└── Tai_Lieu_Minh_Chung_AI_Game_Huyen_Thoai_Lo_Ren.docx # Báo cáo minh chứng AI (Word có hình)
```

---

## 🚀 5. Hướng Dẫn Cài Đặt & Chạy Game

### Yêu Cầu Môi Trường:
- **Unity Hub** và **Unity 6 (phiên bản 6000.5.4f1 hoặc Unity 6 LTS bất kỳ)**.
- Hệ điều hành: Windows 10/11 64-bit.

### Các Bước Thực Hiện:
1. **Clone repository về máy:**
   ```bash
   git clone https://github.com/Tunglofichill/anhgiangcms.git
   ```
2. **Mở dự án trong Unity Hub:**
   - Mở Unity Hub ➔ Chọn **Add** (Thêm dự án từ đĩa) ➔ Trỏ vào thư mục vừa clone.
   - Chọn Editor version **Unity 6**.
3. **Chạy game trong Editor:**
   - Trong cửa sổ Project, mở Scene: `Assets/_Forge/Scenes/ForgeDesk.unity`.
   - Bấm nút **Play (Ctrl + P)** để bắt đầu chơi.
4. **Đóng gói ra file chạy Windows (.exe):**
   - Trên thanh menu của Unity Editor, chọn:
     `Forge` ➔ `Build Standalone Windows (x64)`
   - Quá trình biên dịch sẽ tự động diễn ra và xuất file thực thi tại:
     `Builds/Windows/LegendOfTheForge.exe`.

---

## 👤 6. Tác Giả & Bản Quyền

- **Tác giả / Nhà phát triển:** **Trường Giang**
- **Dự án:** Huyền Thoại Lò Rèn *(Legend of the Forge)*
- **Học phần:** Đồ án Game — Giai đoạn 2 (Báo cáo sản phẩm & Minh chứng ứng dụng AI)
