# 차세대 자율형 MRO 저작도구 개발 계획서
**프로젝트명:** Aero-Twin Spatial Copilot (에어로-트윈 스페이셜 코파일럿)
**목표:** CES 2026 Innovation Awards 수상 (공간지능·월드모델 기반 Level 4 자율형 MRO 트윈)
**작성일자:** 2026.03.31

---

## 1. 프로젝트 개요
본 프로젝트는 기존 수동 조작 중심의 MR/VR 저작도구(ADMS, IVR.Component)를 고도화하여, CES 2026에 출품할 **세계 최초 Level 4 자율형 MRO 트윈 플랫폼**을 개발하는 것을 목표로 합니다. AI가 물리법칙을 시뮬레이션하고 스스로 임무를 할당하며, 인간의 물리적 행동을 정밀하게 가이드하는 차세대 플랫폼을 구축합니다.

---

## 2. 레거시 아키텍처 분석 및 업그레이드 전략

### 2.1. As-Is (기존 시스템 분석)
- **아키텍처:** UWP(C#/XAML) UI Layer + Unity3D(C#) Embedded Engine의 하이브리드 구조
- **통신 브릿지:** C++ IL2CPPToDotNetBridge를 통한 .NET과 Unity 간 양방향 메시지 통신 (`SendMessage` 열거형 및 `SendToUnity`/`RequestToUnity` 패턴)
- **데이터 제어:** MVVM 패턴 및 정교한 Command 패턴(Undo/Redo 지원)을 활용한 3D 객체(Spline, Pipe 등) 조작
- **한계점 (Level 2):** 작업자가 직접 매뉴얼을 호출하고 시각적 오버레이에 의존하는 수동적(Prompt-driven) 시스템.

### 2.2. To-Be (업그레이드 방향)
- **자율화 수준 격상 (Level 4):** 에이전트(Agentic AI)가 상황을 인지하고 스스로 해결책을 도출하여 3D 씬을 자율 제어하는 Goal-driven 아키텍처로 진화.
- **AI 엔진 통합 브릿지:** 기존 `IL2CPPToDotNetBridge`를 확장하여, AI 모듈(Vision, LLM, Physics)이 Unity 렌더링 엔진 및 UWP UI와 실시간으로 데이터를 주고받을 수 있는 **AI Data Streaming Layer** 신설.
- **자율 명령 제어 (Agent-Command Pattern):** 기존 사용자의 Undo/Redo를 처리하던 `IVRCommand` 구조에 **AI 생성 커맨드(AI Generated Command)**를 추가하여, AI가 제안/수정한 MRO 동선 및 부품 배치를 사용자가 검토하고 승인/취소할 수 있도록 구현.

---

## 3. 4대 핵심 AI 엔진 개발 계획

### ① 공간지능 (Spatial Intelligence) - 3D 실시간 동기화
- **기능:** 현장 스캔 데이터를 통해 3D Mesh 생성 및 원격지 전문가(아바타) 동기화
- **개발 방안:**
  - Unity 측에 실시간 환경 매핑 및 Mesh 렌더링 파이프라인 최적화
  - UWP 쪽에 실시간 PiP(Picture-in-Picture) UI(`MainPageNew.xaml` 확장) 및 다중 사용자 위치 정보 동기화 모듈 개발

### ② 월드모델 (World Model) - 물리법칙 실시간 시뮬레이션
- **기능:** 중력, 마찰, 유체역학 등을 기반으로 물리적 인과율 시뮬레이션 및 간섭/파손 예측 (예: 15분 후 파열 예측)
- **개발 방안:**
  - 기존 OpenCASCADE 기반 CAD 데이터 로더(`IVRStepLoader`)를 고도화하여 객체의 물리적 속성(재질, 압력 내성 등) 파싱.
  - Unity 내부에 유체/열 시뮬레이션 셰이더 및 충돌 예측 알고리즘(Collision Prediction) 적용하여 홀로그램(붉은색)으로 실시간 시각화.

### ③ 에이전틱 AI (Agentic AI) - 자율 임무 제어 (Core)
- **기능:** 결함 인지, 부품 자동 발주, 최적 정비 동선 생성, AGV 제어 등
- **개발 방안:**
  - UWP 계층에 **Agentic Task Manager** 신설 (기존 `TaskDataManager` 고도화)
  - ERP 시스템 연동 모듈 및 외부 API 통신(무인 운반 로봇 호출 등)을 C# 백그라운드 서비스로 구현.
  - Unity로 자율 브리핑 및 동선(Navigation Path) 표시를 위한 자동화 커맨드 전송.

### ④ 피지컬 AI (Physical AI) - 물리적 행동 정밀 가이드 및 설계 검증
- **기능:** 도구(렌치 등)의 토크/회전 실시간 피드백 및 디지털 휴먼 인체공학적 제약 검증(MRO-in-Design).
- **개발 방안:**
  - Unity 씬 내부의 가상 정비사(Digital Human) 모델의 역운동학(IK) 및 관절 가동 범위 제한 시뮬레이션 로직 구현 (기존 `treeHuman` 객체 제어 로직 활용).
  - 작업자 HUD(UWP UI 오버레이 또는 Unity 3D World Space UI)에 목표/현재 토크, 회전 각도 등을 나타내는 물리 보정 게이지 UI 개발.

---

## 4. CES 2026 핵심 시연 시나리오 구현 방안

### 4.1. Use Case 1: 현장 자율 MRO 대응 (전투기 엔진 결함)
- **UI 구성:** 1인칭 시점 HUD의 4대 패널(물리 정보 렌더링, 자율 임무 패널, 전문가 동기화 미니맵, 물리 행동 가이드)을 UWP와 Unity 렌더링 텍스처를 융합하여 구현.
- **데이터 흐름:** 
  1. 결함 인지 (UWP -> AI 에이전트)
  2. 물리 위험 예측 (월드모델 -> Unity 붉은색 시각화)
  3. 부품 주문 완료 및 로봇 호출 (에이전틱 AI API 호출)
  4. 정비 보정 (피지컬 AI -> Unity 게이지 렌더링).

### 4.2. Use Case 2: Pre-MRO Design Verification (K-핵잠수함 설계 검증)
- **Zero-Defect Design 모드:** 기존 설계 데이터(CAD)를 불러온 후, `MainViewModelNew`에서 "자율 검증 모드" 실행.
- **처리 절차:**
  - 월드모델이 부품 추출 시의 간섭(Collision)을 예측 (`IvrEx.DistancePointLine` 등 기존 기하학 연산 고도화).
  - 피지컬 AI가 팔 도달 거리 제약 파악 (붉은색 간섭 영역 표시).
  - 에이전틱 AI가 배관 위치 이동 및 해치 크기 확장 설계안을 도출하여 화면에 녹색 MRO 동선으로 표시.

---

## 5. 추진 일정 (CES Action Plan 기준)

| 단계 | 기간 (CES 기준) | 주요 개발 목표 | 비고 |
|---|---|---|---|
| **Phase 1** | D-8개월 | - 하이브리드 아키텍처 재설계 및 AI 브릿지 구축<br>- 4대 핵심 AI 엔진 PoC(Proof of Concept) 개발<br>- HUD 4대 패널 UI 디자인 및 프로토타이핑 | **컨셉 시각화** 및 코어 로직 완성 |
| **Phase 2** | D-5개월 | - K-핵잠수함 및 전투기 엔진 시연용 데모 시스템 구축<br>- Agentic AI 백엔드 통신 및 World Model 물리 연산 최적화<br>- CES 혁신상 원서 작성을 위한 고품질 구동 화면 추출 | 실제 작동 시연 및 **스토리텔링** 고도화 |
| **Phase 3** | D-2개월 | - 현장 미니 데모 존(Demo Zone) 구축용 시스템 안정화<br>- 외부 하드웨어(XR 안경, 로봇 API) 연동 통합 테스트<br>- 예외 처리 및 퍼포먼스(프레임 레이트) 최적화 | **안정화** 및 홍보 준비 |

---

## 6. 결론
새로운 **Aero-Twin Spatial Copilot**은 단순한 시각적 AR 오버레이 앱이 아닌, **물리 법칙을 이해하고 인간과 물리적으로 교감하는 자율형 AI 시스템**입니다. 기존 저작도구(ADMS)가 구축해둔 견고한 하이브리드 통신 브릿지와 Undo/Redo 커맨드 아키텍처를 AI 제어용으로 확장함으로써, CES 2026 Innovation Awards 수상을 위한 안정적이고 혁신적인 개발을 단기간에 달성할 수 있습니다.