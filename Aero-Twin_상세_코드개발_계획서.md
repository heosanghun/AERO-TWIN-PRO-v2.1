# Aero-Twin Spatial Copilot - 상세 코드 개발 계획서
**프로젝트명:** Aero-Twin Spatial Copilot (CES 2026 출품작)
**기반 레거시:** UWP(ADMS) + Unity(IVR.UWP.Unity3D) + C++ Bridge(IL2CPPToDotNetBridge)

---

## 1. 아키텍처 업그레이드 전략 (통신 브릿지 및 프로토콜)
기존 시스템은 사용자(Human)의 UI 클릭 이벤트(TreeGrid)를 C++ 브릿지를 통해 Unity로 전달하는 구조입니다. 이를 **AI 기반 자율 생성 커맨드 시스템**으로 확장해야 합니다.

### 1.1. C++ Bridge 확장 (`C:\IVR.Component\IL2CPPToDotNetBridge\Class1.h`)
- **수정 목표:** AI 엔진(World Model, Physical AI)이 계산한 대용량 공간 데이터 및 시뮬레이션 결과를 주고받기 위한 인터페이스 추가.
- **추가 메서드:**
  - `void ReqAIPrediction(Platform::String^ targetID, int simType);` // 시뮬레이션 요청
  - `void ResAIPrediction(Platform::String^ resultData);` // 시뮬레이션 결과(Json/Byte) 수신
  - `void UpdatePhysicalForce(Platform::String^ objID, float currentTorque, float targetTorque);` // 물리(토크) 피드백

### 1.2. Unity 통신 뷰모델 확장 (`IVR.UWP.Unity3D\Unity3DViewModel.cs`)
- **수정 목표:** `SendMessage` Enum 확장을 통해 AI 커맨드 라우팅.
- **코드 구현:**
  ```csharp
  public enum SendMessage {
      // ... 기존 코드 ...
      AI_WorldModel_Simulate, 
      AI_Agent_AutoOrder, 
      AI_Physical_Feedback,
      AI_Spatial_SyncMesh
  }
  ```

---

## 2. 4대 핵심 엔진 실제 코드 구현 계획

### 2.1. Agentic AI & Data Manager (`D:\ADMS\ADMS\TaskDataManager.cs` & `MainViewModelNew.cs`)
- **구현 내용:** ERP 시스템(가상) 및 로봇(AGV) API와 연동하여 자율 판단을 수행하는 Manager 클래스 추가.
- **수정 파일:** `TaskDataManager.cs`
  - `public static void AutoOrderPart(string partID)` 구현.
  - 외부 API(JSON)를 호출하여 부품 재고를 파악하고, UWP UI에 상태(도착 예정 시간 등)를 비동기(`async/await`)로 갱신.
- **명령 제어 (`MainViewModelNew.cs` & `IVRCommand`):**
  - 기존 `TreeAddCommand`와 유사하게 `AIGeneratedNavCommand`를 생성.
  - AI가 최적의 정비 동선을 도출하면, 이 Command를 생성하여 `m_cmdInvoker.ExcuteCommad(addCommands)`로 실행. 사용자가 거부할 경우 `undo()` 호출.

### 2.2. World Model 시뮬레이션 (`D:\ADMS_Unity\Assets\Scripts\WorldModel\`)
- **구현 내용:** Unity 내부에서 물리 법칙(유체/열/간섭)을 연산하고 홀로그램(붉은색)으로 렌더링.
- **신규 스크립트:** `WorldModelSimulator.cs`
  - `AppBridge.cs`의 `Req` 메서드로부터 시뮬레이션 시작 트리거 수신.
  - 유체/열 시뮬레이션을 위해 Unity의 Compute Shader 또는 파티클 시스템을 코드로 동적 제어.
  - `PipeCtrl.cs`, `CableCtrl.cs`의 메쉬 데이터를 기반으로 15분 후의 한계 응력(Stress Limit) 초과 지점(간섭 영역)을 계산하여 붉은색 Material로 `meshRenderer` 변경.

### 2.3. Physical AI 및 인체공학 검증 (`D:\ADMS_Unity\Assets\Scripts\PhysicalAI\`)
- **구현 내용:** K-핵잠수함 설계 검증을 위한 Digital Human 제약 시뮬레이션.
- **신규/수정 스크립트:** `DigitalHumanIK.cs` (기존 `treeHuman` 객체 활용)
  - `ObjectPropertyCtrl.Ins.AddNode` 등으로 부품이 추가될 때, Digital Human의 손끝(End-Effector)에서 부품까지의 거리를 계산.
  - `float distance = Vector3.Distance(humanHand.position, targetPart.position);`
  - 팔 도달 거리(Reachability) 한계치(예: 80cm) 초과 시, 또는 렌치 회전 반경(Torque Space) 내 충돌체(Collider) 감지 시 `AppBridge`를 통해 UWP로 에러 이벤트(`OnPhysicalLimitReached`) 발송.

### 2.4. Spatial Intelligence 및 XR HUD UI (`D:\ADMS\ADMS\MainPageNew.xaml`)
- **구현 내용:** 1인칭 관점(HUD)의 4대 패널 구현. 기존 Syncfusion TreeGrid 기반의 2D UI 레이아웃을 확장.
- **수정 파일:** `MainPageNew.xaml`, `ObjectExplorer.xaml`
  - UnityPlayer 위로 투명한 XAML Overlay 패널(`Grid` Layer) 4개 추가.
  - **Panel 1 (World Model):** 시뮬레이션 결과 텍스트 바인딩 ("15분 후 파열 예측").
  - **Panel 2 (Agentic):** TaskDataManager의 자동 발주 상태(`ObservableCollection`) 바인딩.
  - **Panel 3 (Spatial):** 원격지 렌더링 텍스처를 보여주는 PiP(Picture-in-Picture) `Image` 컨트롤 추가.
  - **Panel 4 (Physical):** 목표 토크(20Nm)와 현재 토크(15Nm)를 보여주는 실시간 프로그레스 바(SfCircularProgressBar 등 활용).

---

## 3. 개발 페이즈 (Action Item)

1. **[Phase 1] 브릿지 및 코어 아키텍처 세팅 (D-8m)**
   - `IL2CPPToDotNetBridge`에 AI 통신 인터페이스 추가 및 빌드 (C++).
   - `Unity3DViewModel`, `AppBridge.cs` 통신 파이프라인 연결.
2. **[Phase 2] UWP HUD 및 Agentic Manager 개발 (D-5m)**
   - `TaskDataManager` 확장 및 UWP 4대 패널 XAML 구현.
   - AI Command 패턴 추가.
3. **[Phase 3] Unity 3D World Model & Physical AI 개발 (D-3m)**
   - `WorldModelSimulator.cs` 유체/간섭 렌더링 로직 개발.
   - `DigitalHumanIK.cs` 인체공학적 제약 연산 및 붉은색 충돌망(Mesh) 렌더링 구현.
4. **[Phase 4] 통합 테스트 및 시연 시나리오 빌드 (D-2m)**
   - K-핵잠수함 CAD 데이터 로딩 -> 자율 검증 모드 실행 -> 간섭/도달 거리 에러 -> AI 자동 보정(설계 변경 제안) 워크플로우 테스트.