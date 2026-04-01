#pragma once
#include <ppltasks.h>
#include <atomic>
#include <robuffer.h>

using namespace Windows::Storage::Streams;
typedef uint8 byte;

namespace IL2CPPToDotNetBridge
{
    public delegate void Unity3DEventHandler(Platform::String^ eventName, Platform::Object^ eventValue);

    public interface class IDotNetBridge
    {
    public:
       
        void MyFunction1();
        void MyFunction2(Platform::String^ arg);
        void GetObjName(Platform::String^ objID);
        void GetObj(Platform::Object^ obj);
        void ObjInfo(const Platform::Array<Platform::Object^>^ obj);
        void GetMessage(Platform::Object^ obj);
        void ResSaveInit(); //초기상태로저장 응답
        void ResIsRootNode(bool existMesh); //루트노드여부      //  void ObjInfo(const Platform::Array<Platform::Array<Platform::Object^>^>^ obj);

        // AI Engine 응답 인터페이스 (Phase 1)
        void ResAIPrediction(Platform::String^ resultData); // 시뮬레이션 결과 수신
        void OnPhysicalLimitReached(Platform::String^ objID, Platform::String^ warningMsg); // 인체공학적 한계 도달 이벤트

        // 비동기 바이너리 스트림 파이프라인 (Phase 3. 퍼포먼스 이슈 해결)
        void PushSpatialMeshStream(const Platform::Array<byte>^ meshData); // 수만 개의 3D Mesh 비동기 처리용
    };

    public interface class IIL2CPPBridge
    {
    public:
        //기본기능
        void UnityZone(bool inOut); //3D상호작용 On/Off

        //파일기능
        void FileOpen(Platform::Object^ value);

        //메뉴기능
        // type : Solid, Transparent, Ground, TopView~PerspectiveView, ScreenFit
        void MenuDisplay(Platform::String^ type, Platform::String^ objID);
        // type : Home, Select, Move, Rotate, Scale, PivotMove, PivotRotate, PivotCenter
        void MenuObject(Platform::String^ type);

        //노드기능
        // type : Select, SaveInit, Move, ReName, ReID, Add, Delete, CheckRoot, ShowHide
        //void NodeContext(Platform::String^ type, Platform::String^ objID, Platform::String^ value);
        void SelectedNode(Platform::String^ selectedID); //노드 선택
        void SelectedNodes(Platform::Object^ objID); //복수 노드 선택 (string[])
        void ReqSaveInit(Platform::String^ objdID); //초기상태로저장 요청
        void ReqMoveNode(Platform::String^ objID, Platform::String^ parentID); //노드레벨구조변경 요청
        void ReqRenameNode(Platform::String^ objID, Platform::String^ rename); //노드이름변경 요청
        void ReqReobjID(Platform::String^ objID, Platform::String^ reobjID); //오브젝트아이디변경 요청
        void ReqAddNode(Platform::String^ objID, Platform::String^ name, Platform::String^ parentID, int dimensionMode);//노드추가 요청
        void ReqDeleteNode(Platform::String^ objeID);//노드삭제 요청
        void ReqIsRootNode(Platform::String^ objID); //루트노드여부확인(Mesh의 존재여부확인. Mesh없으면 리턴값 false)
        void ReqIsShow(Platform::String^ objID, int mode); //mode : 0=대상보임, 1=대상숨김, 2=대상만보임
        void CopyObject(Platform::String^ originID, Platform::String^ objID, Platform::String^ parentID, Platform::String^ name, int copyMode);//노드복제 요청
        void MoveToContactPoint(Platform::String^ objID);
        void InvertNormal(Platform::String^ objID);

        //케이블편집
        void CurveEditMode(Platform::String^ objID, int mode);//0=종료, 1=추가, 2=삽입, 3=이동, 4=제거, 5=시작점, 6=끝점
        void CurveValueChange(Platform::String^ objID, int type, Platform::Object^ value);//0=시작점, 1=끝점, 2=색상, 3=유연성, 4=크기

        //배관편집
        void PipeEditMode(Platform::String^ objID, int mode);
        void PipeValueChange(Platform::String^ objID, int type, Platform::Object^ value);

        //속성정보
        void SetTransformFromUI(Platform::String^ objID, Platform::Object^ value);//회전값을 Vector3로 보내고, 로컬/월드 구분함
        void SetTransformFromHistory(Platform::String^ objID, Platform::Object^ value, bool pivotMode);//회전값을 쿼터니언으로 보내고, 무조건 로컬로 함
        void SetColor(Platform::String^ objID, Platform::Object^ value);

        //오브젝트 정렬
        void AlignApply(Platform::Object^ value);
        void AlignEditMode(int mode);

        // AI Engine 통신 인터페이스 (Phase 1)
        void ReqAIPrediction(Platform::String^ targetID, int simType); // 시뮬레이션 요청
        void UpdatePhysicalForce(Platform::String^ objID, float currentTorque, float targetTorque); // 물리 피드백
        void SyncSpatialMesh(Platform::Object^ meshData); // 공간 매쉬 동기화 (byte[] 활용)

        //기타기능
        void LoadStep(Platform::String^ filePath);
        void Req(Platform::String^ eventName, Platform::Object^ eventValue);
        Platform::Object^ Get(Platform::String^ eventName, Platform::Object^ eventValue);

        //테스트용(command=명령종류  id1,id2=오브젝트ID  txt1,txt2=기타정보)
        void Test(Platform::String^ command, Platform::String^ id1, Platform::String^ id2, Platform::String^ txt1, Platform::String^ txt2);

       /* void StreamTest(IBuffer^ streamBuffer);
        Platform::Object^ GetDataBuffer();   */
    
    public:
        event Unity3DEventHandler^ OnUnity3DEvent;
      //  Platform::String^ GetText(Platform::String^ objID); //반환값 테스트
    };

   

    public ref class BridgeBootstrapper sealed
    {
    public:
        static IDotNetBridge^ GetDotNetBridge()
        {
            return m_DotNetBridge;
        }

        static void SetDotNetBridge(IDotNetBridge^ dotNetBridge)
        {
            m_DotNetBridge = dotNetBridge;
        }

        static IIL2CPPBridge^ GetIL2CPPBridge()
        {
            return m_IL2CPPBridge;
        }

        static void SetIL2CPPBridge(IIL2CPPBridge^ il2cppBridge)
        {
            m_IL2CPPBridge = il2cppBridge;
        }

    private:
        static IDotNetBridge^ m_DotNetBridge;
        static IIL2CPPBridge^ m_IL2CPPBridge;

        BridgeBootstrapper();
    };

    IDotNetBridge^ BridgeBootstrapper::m_DotNetBridge;
    IIL2CPPBridge^ BridgeBootstrapper::m_IL2CPPBridge;
}
