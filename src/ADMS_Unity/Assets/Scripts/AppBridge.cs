using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using IVR;
using System.IO;
using System.Linq;

using IVR.NewVPM;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;

#if ENABLE_WINMD_SUPPORT

using IL2CPPToDotNetBridge;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

class IL2CPPBridge : IIL2CPPBridge
{
    public AppBridge app;
    public event Unity3DEventHandler OnUnity3DEvent;

    public void EventFired(string eventName, object eventValue)
    {
        if (UnityEngine.WSA.Application.RunningOnUIThread())
        {
            if (OnUnity3DEvent != null)
            {
                OnUnity3DEvent(eventName, eventValue);
                //Debug.LogError("Main(O), Res(O), Name(" + eventName + ")");
            }
            else
            {
                //Debug.LogError("Main(O), Res(X), Name(" + eventName + ")");
            }
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                if (OnUnity3DEvent != null)
                {
                    OnUnity3DEvent(eventName, eventValue);
                    //Debug.LogError("Main(X), Res(O), Name(" + eventName + ")");
                }
                else
                {
                    //Debug.LogError("Main(X), Res(X), Name(" + eventName + ")");
                }
            }, false);
        }
    }

    public void Req(string eventName, object eventValue)
    {
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            app.Req(eventName, eventValue);
            Debug.LogError("Main(O), Req(O), Name(" + eventName + ")");
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                app.Req(eventName, eventValue);
                Debug.LogError("Main(X), Req(O), Name(" + eventName + ")");
            }, false);
        }
    }

    public object Get(string eventName, object eventValue)
    {
        object result = "";
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            result = app.Get(eventName, eventValue);
            Debug.Log("Main(O), GetObj(O), Name(" + eventName + ")");
            return result;
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = app.Get(eventName, eventValue);
                Debug.Log("Main(X), GetObj(O), Name(" + eventName + ")");
            }, true);
            return result;
        }
    }

    private void Request(Action method)
    {
        if (method == null) return;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            method?.Invoke();
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                method?.Invoke();
            }, false);
        }
    }
    /// <summary>
    /// made by dscho(22.09.07)
    /// 동기적으로 처리해야 하는 함수
    /// </summary>
    /// <param name="method"></param>
    private void RequestSync(Action method)
    {
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            method?.Invoke();
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                method?.Invoke();
            }, true);
        }
    }
    public void UnityZone(bool inOut)
    { Request(delegate { MainManager.Ins.unityZone = inOut; }); }

    public void SelectedNode(string objID)
    { Request(delegate { MainManager.Ins.ReqSelectObject(objID); }); }

    public void SelectedNodes(object objID)
    { }

    public void ReqSaveInit(string objID)
    { Request(delegate { MainManager.Ins.ReqSaveInit(objID); }); }

    public void ReqMoveNode(string objID, string parentID)
    { Request(delegate { MainManager.Ins.ReqMoveNode(objID, parentID); }); }

    public void ReqRenameNode(string objID, string rename)
    { Request(delegate { MainManager.Ins.ReqRenameNode(objID, rename); }); }

    public void ReqReobjID(string objID, string reobjID)
    { Request(delegate { MainManager.Ins.ReqReobjID(objID, reobjID); }); }

    public void ReqAddNode(string objID, string name, string parentID, int dimensionMode)
    { Request(delegate { MainManager.Ins.ReqAddNode(objID, name, parentID, dimensionMode); }); }

    public void ReqDeleteNode(string objID)
    { Request(delegate { MainManager.Ins.ReqDeleteNode(objID); }); }

    public void ReqIsRootNode(string objID)
    { Request(delegate { MainManager.Ins.ReqCheckRootNode(objID); }); }

    public void ReqIsShow(string objID, int mode)
    { Request(delegate { MainManager.Ins.ReqShowHide(objID, mode); }); }

    public void MenuDisplay(string type, string objID)
    { Request(delegate { MainManager.Ins.ReqMenuDisplay(type, objID); }); }

    public void MenuObject(string type)
    { Request(delegate { MainManager.Ins.ReqMenuObject(type); }); }

    public void FileOpen(object value, string objID, bool merge)
    { Request(delegate { MainManager.Ins.ReqOpenFile(value, objID, merge); }); }


    public void LoadStep(string filePath)
    { }

    public void CopyObject(string originID, string objID, string parentID, string name, int copyMode)
    { Request(delegate { MainManager.Ins.ReqCopyObject(originID, objID, parentID, name, copyMode); }); }

    public void CurveEditMode(string objID, int mode)
    { Request(delegate { MainManager.Ins.ReqCurveEditMode(objID, mode); }); }

    public void CurveValueChange(string objID, int type, object value)
    { Request(delegate { MainManager.Ins.ReqCurveValueChange(objID, type, value); }); }

    public void PipeEditMode(string objID, int mode)
    { Request(delegate { MainManager.Ins.ReqPipeEditMode(objID, mode); }); }

    public void PipeValueChange(string objID, int type, object value)
    { Request(delegate { MainManager.Ins.ReqPipeValueChange(objID, type, value); }); }

    public void SetTransformFromUI(string objID, object value)
    { Request(delegate { MainManager.Ins.ReqSetTransformFromUI(objID, value); }); }

    public void SetTransformFromHistory(string objID, object value, bool pivotMode)
    { Request(delegate { MainManager.Ins.ReqSetTransformFromHistory(objID, value, pivotMode); }); }

    public void SetColor(string objID, object value)
    { Request(delegate { MainManager.Ins.ReqSetColor(objID, value); }); }

    public void MoveToContactPoint(string objID)
    { Request(delegate { MainManager.Ins.ReqMoveToContactPoint(objID); }); }

    public void InvertNormal(string objID)
    { Request(delegate { MainManager.Ins.ReqInvertNormal(objID); }); }

    public void AlignApply(object value)
    { Request(delegate { MainManager.Ins.ReqAlignApply(value); }); }

    public void AlignEditMode(int mode)
    { Request(delegate { MainManager.Ins.ReqAlignEditMode(mode); }); }

    public void SetSimulArrayData(object simulationData)
    { Request(delegate { MainManager.Ins.SetSimulArrayData(simulationData); }); }
    
    public void SetSimulPlay(int mode)
    { Request(delegate { MainManager.Ins.SetSimulPlay(mode); }); }
    
    public void DoStop()
    { Request(delegate { MainManager.Ins.DoStop(); }); }
    
    // AI Engine 통신 추가 (Phase 1)
    public void ReqAIPrediction(string targetID, int simType)
    { Request(delegate { WorldModelSimulator.Ins.RunSimulation(targetID, simType); }); }

    public void UpdatePhysicalForce(string objID, float currentTorque, float targetTorque)
    { Request(delegate { DigitalHumanIK.Ins.CheckErgonomicLimits(objID); /* AI 피지컬 피드백 UI 갱신 연동 예정 */ }); }

    public void SyncSpatialMesh(object meshData)
    { Request(delegate { /* 공간지능 매쉬 동기화 연동 예정 */ }); }
    
    public void OpenImageFileToPlane(int width, int heigth, object bytes, int objID, bool merge)
    { Request(delegate { MainManager.Ins.ReqOpenImageFileToPlane(width, heigth, bytes, objID, merge); }); }

    public void Play()
    { 
    }
    public void Pause()
    { 
    }

    /// <summary>
    ///  Dms 전용 3D File로 저장한다.
    ///  made by dscho 22.04.22
    /// </summary>
    /// <param name="ObjID">저장할 대상 ObjectID</param>
    /// <returns></returns>
    public System.Object SaveToDms3D(string ObjID)
    {
        System.Object result = null;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            return  app.SaveToDms3D(ObjID);
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = app.SaveToDms3D(ObjID);
            }, true);
              return result;
        }
    }

    /// <summary>
    /// dms 전용포맷 Import
    /// made by dscho 22.04.22
    /// </summary>
    /// <param name="dataBuffer"></param>
    /// <param name="startInx"></param>
    /// <param name="bVpm">vpm파일여부</param> 수정 22.10.19
    public void OpenDms3DFile(object dataBuffer,bool bMerge,int startInx, bool bVpm)
    {
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
           MainManager.Ins.OpenFileDms3D(dataBuffer as byte[],bMerge,bVpm,startInx);
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                MainManager.Ins.OpenFileDms3D(dataBuffer as byte[],bMerge,bVpm, startInx);
            }, false);

        }
    }
    /// <summary>
    /// made by dscho(220906)
    /// 위치및 방위값을 mode값에 따라 얻는 함수
    /// </summary>
    /// <param name="objID">객체 Object ID</param>
    /// <param name="mode">0:world, 1:from 부모, 2:자기자신(초기위치의)</param>
    /// <returns></returns>
    public System.Object GetPositionEulerAngle(string objID, int mode)
    {
        System.Object result = null;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            return MainManager.Ins.GetPositionEulerAngle(objID,mode) as object;
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = MainManager.Ins.GetPositionEulerAngle(objID, mode) as object;
            }, true);
            return result;
        }
    }

    /// <summary>
    /// made by dscho(220906)
    /// 카메라의 위치및 방위값 Get(World)
    /// </summary>
    /// <returns>float[7]</returns>
    public System.Object GetCameraTransformation()
    {
        System.Object result = null;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            return MainManager.Ins.GetCameraTransformation() as object;
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = MainManager.Ins.GetCameraTransformation() as object;
            }, true);
            return result;
        }
    }
    /// <summary>
    ///  made by dscho(220906)
    ///  구성품의 destInfo정보를 이용하여 위치를 Set하는 함수
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="destInfo"></param>
    public void SetSimulPosnOrient(string objID, System.Object destInfo)
    {
        RequestSync(delegate { MainManager.Ins.SetSimulPosnOrient(objID, destInfo as float[]); });
    }
    /// <summary>
    /// made by dscho(220906)
    /// 구성품의 rotInfo정보를 이용하여 회전을 Set하는 함수
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="refObjID"></param>
    /// <param name="rotInfo"></param>
    public void SetSimulRotation(string objID, string refObjID, System.Object rotInfo)
    {
        RequestSync(delegate { MainManager.Ins.SetSimulRotation(objID, refObjID, rotInfo as object[]); });
    }
    /// <summary>
    /// made by dscho(220907)
    /// 구성품의 Rotate의 angle값및 해당축만 보이게 하기위한 처리
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="refObjID"></param>
    /// <param name="axis"></param>
    public void OnStartRotateMode(string objID, string refObjID, int axis)
    {
        RequestSync(delegate { MainManager.Ins.OnStartRotateMode(objID, refObjID, axis); });
    }
    /// <summary>
    /// Rotate모드 종료(angle 얻는 코루틴함수 죽이기및 모든축 보이게 처리)
    /// </summary>
    public void OnEndRotateMode()
    {
        RequestSync(delegate { MainManager.Ins.OnEndRotateMode(); });
    }
    /// <summary>
    /// Rotate 축 변경시
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="refObjID"></param>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    public void ChanageRotateAxis(string objID, string refObjID, int axis, float angle)
    {
        RequestSync(delegate { MainManager.Ins.ChanageRotateAxis(objID,refObjID,axis,angle); });
    }

    /// <summary>
    /// 절차상의 도착지점의 위치정보를 setting하도록 처리
    /// </summary>
    /// <param name="objID"></param>
    public System.Object GetLocalTransform(string objID)
    {
        System.Object result = null;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            return MainManager.Ins.GetTransformInfo(objID,false,false) as object;
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = MainManager.Ins.GetTransformInfo(objID, false, false) as object;
            }, true);
            return result;
        }
    }
    /// <summary>
    /// made by dscho 22.09.12
    /// 절차의 목적지로 setting하는 함수
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="destT"></param>
    /// <param name="showMode"></param>
    public void SetSceneState(string objID, object destT, int showMode)
    {
        RequestSync(delegate { MainManager.Ins.SetSceneState(objID, destT as float[],showMode); });
    }

    /// <summary>
    /// made by dscho 22.09.13
    /// 현재상태 저장
    /// </summary>
    public void SaveCurrentScene()
    {
        RequestSync(delegate { MainManager.Ins.SaveCurrentScene(); });
    }
    /// <summary>
    /// made by dscho 22.09.13
    /// 저장된 현재상태로 setting
    /// </summary>
    public void SetCurrentScene()
    {
        RequestSync(delegate { MainManager.Ins.SetCurrentScene(); });
    }

    /// <summary>
    /// STEP Loader
    /// made by dscho 22.09.16
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="bMerge"></param>
    /// <param name="deflection"></param>
    /// <param name="angle"></param>
    /// <param name="startInx"></param>
    public void OpenStepFile(string filePath, bool bMerge, float deflection, float angle, int startInx)
    {
        Request(delegate { MainManager.Ins.OpenFileStepFile(filePath,bMerge,deflection,angle,startInx); });
    }

    /// <summary>
    /// Set 순서
    /// made by dscho 22.10.19
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="order">순서</param>
    public void SetAttributeOrder(string objID, int order)
    {
        RequestSync(delegate { MainManager.Ins.SetAttributeOrder(objID,order); });
    }

    /// <summary>
    /// Set 순서
    /// made by dscho 22.12.07
    /// </summary>
    /// <param name="objID"></param>
    /// <param name="order">순서</param>
    public void SetAttributeOrderNew(string parentObjID, object childInfo)
    {
        RequestSync(delegate { MainManager.Ins.SetAttributeOrderNew(parentObjID,childInfo); });
    }

    public void SetEnvironmentIndex(int indx)
    { Request(delegate { MainManager.Ins.SetEnvironmentIndex(indx); }); }

    public void SetHumanIndex(int indx)
    { Request(delegate { MainManager.Ins.SetHumanIndex(indx); }); }

    /// <summary>
    /// made by dscho(22.11.25)
    /// 초기상태(pos,euler,scale) float[9]로 받아오는 함수 
    /// </summary>
    /// <param name="objID">구성품 ID</param>
    /// <returns>float[9]</returns>
    public System.Object GetInitialTransform(string objID)
    {
        System.Object result = null;
        if (UnityEngine.WSA.Application.RunningOnAppThread())
        {
            return ObjectPropertyCtrl.Ins.GetInitialTransformation(objID) as object;
        }
        else
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(() =>
            {
                result = ObjectPropertyCtrl.Ins.GetInitialTransformation(objID) as object;
            }, true);
            return result;
        }

    }
 }
#endif

namespace IVR
{
    public interface IDispatcher
    {
        void Invoke(Action fn);
    }

    public class Dispatcher : IDispatcher
    {
        private static Dispatcher _instance;

        public static Dispatcher Ins
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Dispatcher();
                }

                return _instance;
            }
        }
        public List<Action> pending = new List<Action>();


        public void Invoke(Action fn)
        {
            lock (pending)
            {
                pending.Add(fn);
            }
        }

        public void InvokePending()
        {
            lock (pending)
            {
                foreach (Action action in pending)
                {
                    action();
                }
                pending.Clear();
            }
        }
    }

    public class AppBridge : MonoBehaviour
    {
        #region Singleton
        protected AppBridge() { }
        private static AppBridge _instance;
        public static AppBridge Ins
        {
            get
            {
                if (_instance == null)
                {
                    //응용단으로부터 이벤트가 할당되어 있으니 새로 생성하지 말 것
                    _instance = GameObject.Find("Invoker").GetComponent<AppBridge>();
                }

                return _instance;
            }
        }
        #endregion

#if ENABLE_WINMD_SUPPORT
        private IL2CPPBridge mILBridge = new IL2CPPBridge();

        private void Awake()
        {
            BridgeBootstrapper.SetIL2CPPBridge(mILBridge);
            mILBridge.app = this;
        }

        private void Start()
        {
            //mILBridge.dm = this.gameObject.GetComponent<DataManager>();
            //IDotNetBridge dotnetBridge = BridgeBootstrapper.GetDotNetBridge();
            //dotnetBridge.MyFunction1();
            //dotnetBridge.MyFunction2("Hello from Unity3D");
        }
#endif

        void Update()
        {
            // Dispatcher.Ins.InvokePending();
            //MainManager mm = MainManager.Ins;
            //float[] info =  mm.GetPositionEulerAngle("5",2);
            //Debug.Log("pos:(" + info[0] + "," + info[1] + "," + info[2] +")");
            //Debug.Log("rot:(" + info[3] + "," + info[4] + "," + info[5] + ")");
            if (Input.GetKey(KeyCode.F2))
            {
                #region DMS3D 파일로드
                //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

                //sw.Start();
                //using (FileStream fs = new FileStream("test.dms3D", FileMode.Open))
                //{
                //    var len = (int)fs.Length;
                //    var bits = new byte[len];
                //    fs.Read(bits, 0, len);
                //    MainManager mm = MainManager.Ins;

                //    mm.OpenFileDms3D(bits, false, false);
                //    using (MemoryStream stream = new MemoryStream(bits))
                //    {
                //        using (BinaryReader br = new BinaryReader(stream))
                //        {
                //            IVRNewVPMSerializer.Open(br, mm.root3D.gameObject);
                //        }
                //    }
                //}
                //if (sw.IsRunning)
                //{
                //    Debug.Log("로드 수행시간은" + sw.Elapsed.ToString());
                //    sw.Stop();
                //}
                //System.Threading.Thread.Sleep(100);
                //MainManager mm = MainManager.Ins;
                //mm.ReqAddNode("1", "test", "0", 2);
                #endregion

                #region "회전"
                //MainManager mm = MainManager.Ins;
                //mm.OnStartRotateMode("5", "5", 0);
                #endregion

                #region "카메라세팅"
                //MainManager mm = MainManager.Ins;
                //float[] destT = new float[7];
                //destT[0] = -0.7648f; destT[1] = 0.694f; destT[2] = -0.1244f;
                //destT[3] = 0.2648f; destT[4] = 0.5299f; destT[5] = -0.1786f; destT[6] = 0.7856f;
                //mm.SetSceneState("CAMERA", destT, 0);
                #endregion

                #region "STEP Loader"
#if DEBUG
                //MainManager mm = MainManager.Ins;
                //mm.OpenFileStepFile("1m.stp", false, 0.8f, 0.5f, 1);
#endif
                #endregion
            }
            if (Input.GetKey(KeyCode.F3))
            {
                #region "fbx load test"
                //using (FileStream fs = new FileStream("test.fbx", FileMode.Open))
                //{
                //    var len = (int)fs.Length;
                //    var bits = new byte[len];
                //    fs.Read(bits, 0, len);

                //    MainManager mm = MainManager.Ins;
                //    mm.ReqOpenFile(bits,"1",false);

                //    System.Threading.Thread.Sleep(100);
                //}
                #endregion

                #region 회전종료
                //MainManager mm = MainManager.Ins;
                //mm.OnEndRotateMode();
                #endregion
                #region "Save Dms3D"
#if DEBUG
                //using (BinaryWriter bw = new BinaryWriter(new FileStream("test.dms3D", FileMode.Create)))
                //{
                //    byte[] data = SaveToDms3D("ALL");
                //    bw.Write(data);
                //}
                    //IVRNewVPMSerializer.Save(bw, MainManager.Ins.root3D.gameObject);
                
#endif
                #endregion

            }
            if (Input.GetKey(KeyCode.F4))
            {
#region "fbx load test"
                //using (FileStream fs = new FileStream("test.fbx", FileMode.Open))
                //{
                //    var len = (int)fs.Length;
                //    var bits = new byte[len];
                //    fs.Read(bits, 0, len);

                //    MainManager mm = MainManager.Ins;
                //    mm.ReqOpenFile(bits);

                //}
#endregion

#region 회전종료
                MainManager mm = MainManager.Ins;
                mm.ChanageRotateAxis("5", "5", 0, -20);
#endregion

            }
        }


        /// <summary>
        /// 메인쓰레드
        /// </summary>
        /// <param name="objID"></param>
        public void ObjInfo(object objID)
        {
            string objidstr = (string)objID;
            Debug.Log(" object id :" + objidstr);
            GameObject findObj = DataManager.Ins.FindObject("1");

            if (findObj == null)
            {
                Debug.Log(" object not found");
#if ENABLE_WINMD_SUPPORT
         mILBridge.EventFired("err", "구성품을 찾을수 없습니다.");
#endif
                return;
            }
            object[] objInfo = new object[3];
            List<byte> byteInfo = new List<byte>();
            Debug.Log("objid:" + objidstr + " find object:" + findObj.name);
            ObjectProperty objP = findObj.GetComponent<ObjectProperty>();
            int objIdLen = objP.objectID.Length;
            byteInfo.AddRange(BitConverter.GetBytes(objIdLen));
            int objNameLen = objP.name.Length;
            byteInfo.AddRange(BitConverter.GetBytes(objNameLen));

            byteInfo.AddRange(Encoding.UTF8.GetBytes(objP.objectID));
            byteInfo.AddRange(Encoding.UTF8.GetBytes(objP.name));
            byteInfo.AddRange(BitConverter.GetBytes(objP.objectType));
            objInfo[0] = byteInfo.ToArray();

            findObj = DataManager.Ins.FindObject("2");
            List<byte> byteInfo2 = new List<byte>();
            Debug.Log("objid:" + objidstr + " find object:" + findObj.name);
            objP = findObj.GetComponent<ObjectProperty>();
            objIdLen = objP.objectID.Length;
            byteInfo2.AddRange(BitConverter.GetBytes(objIdLen));
            objNameLen = objP.name.Length;
            byteInfo2.AddRange(BitConverter.GetBytes(objNameLen));

            byteInfo2.AddRange(Encoding.UTF8.GetBytes(objP.objectID));
            byteInfo2.AddRange(Encoding.UTF8.GetBytes(objP.name));
            byteInfo2.AddRange(BitConverter.GetBytes(objP.objectType));
            objInfo[1] = byteInfo2.ToArray();

            findObj = DataManager.Ins.FindObject("3");
            List<byte> byteInfo3 = new List<byte>();
            Debug.Log("objid:" + objidstr + " find object:" + findObj.name);
            objP = findObj.GetComponent<ObjectProperty>();
            objIdLen = objP.objectID.Length;
            byteInfo3.AddRange(BitConverter.GetBytes(objIdLen));
            objNameLen = objP.name.Length;
            byteInfo3.AddRange(BitConverter.GetBytes(objNameLen));

            byteInfo3.AddRange(Encoding.UTF8.GetBytes(objP.objectID));
            byteInfo3.AddRange(Encoding.UTF8.GetBytes(objP.name));
            byteInfo3.AddRange(BitConverter.GetBytes(objP.objectType));
            objInfo[2] = byteInfo3.ToArray();

            Res("objInfo", objInfo as object);
        }

        public void ResSaveInit()
        {
#if ENABLE_WINMD_SUPPORT
            //IDotNetBridge dotnetBridge = BridgeBootstrapper.GetDotNetBridge();
            //dotnetBridge.ResSaveInit();
#endif
        }

        public void ResIsRootNode(bool existMesh)
        {
#if ENABLE_WINMD_SUPPORT
            //IDotNetBridge dotnetBridge = BridgeBootstrapper.GetDotNetBridge();
            //dotnetBridge.ResIsRootNode(existMesh);
#endif
        }
        
        public void ResAddNode(string objID, string objNm, string type)
        {
#if ENABLE_WINMD_SUPPORT
            //IDotNetBridge dotnetBridge = BridgeBootstrapper.GetDotNetBridge();
            //dotnetBridge.ResAddNode(objID, objNm, type);
#endif
        }

        /// <summary>
        /// 동기 메세지. 응용단으로부터 받고 곧바로 보냄
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Get(string name, object value)
        {
            MainManager mm = MainManager.Ins;

            object result = null;
            switch (name)
            {
                //노드 기능
                case "Node": break;
                case "Transformation": result = mm.GetTransformation(value as string); break;
                case "SelectedInfo":
                    {
                        object[] c = value as object[];
                        string objID = c[0] as string;
                        bool worldPos = Convert.ToBoolean(c[1]);
                        bool worldRot = Convert.ToBoolean(c[2]);
                        result = mm.GetSelectedInfo(objID, worldPos, worldRot);
                    }
                    break;
                //케이블편집 기능
                case "CurveInfo": result = mm.GetCurveInfo(value as string); break;
                //배관편집 기능
                case "PipeInfo": result = mm.GetPipeInfo(value as string); break;

                //메뉴 기능
                case "Menu": break;

                default: Debug.LogError("Unknown Reqest from UI Thread."); break;
            }
            return result;
        }

        /// <summary>
        /// 비동기 메세지. 응용단으로부터 받음
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Req(string name, object value)
        {
            if(value == null)
            Debug.LogError($" Req object value is null");

            MainManager mm = MainManager.Ins;

            switch (name)
            {
                case "UnityZone": mm.unityZone = (bool)value; break;

                //노드 기능
                case "SelectedNode": mm.ReqSelectObject(value as string); break;
                case "ReqSaveInit": mm.ReqSaveInit(value as string); break;
                case "ReqMoveNode": mm.ReqMoveNode(null,null); break;
                case "ReqRenameNode": mm.ReqRenameNode(null, null); break;
                case "ReqReobjID": mm.ReqReobjID(null, null); break;
                case "ReqAddNode": mm.ReqAddNode(null, null, null, 0); break;
                case "ReqDeleteNode": mm.ReqDeleteNode(value as string); break;
                case "ReqIsShow": mm.ReqShowHide(value as Dictionary<string, bool>); break;

                //메뉴 기능
                //case "Solid": mm.ReqSolid(value as string); break;
                //case "Transparent": mm.ReqTransparent(value as string); break;
                //case "Ground": mm.ReqGround(); break;
                //case "TopView": mm.ReqView(0, value as string); break;
                //case "BottomView": mm.ReqView(1, value as string); break;
                //case "FrontView": mm.ReqView(2, value as string); break;
                //case "BackView": mm.ReqView(3, value as string); break;
                //case "LeftView": mm.ReqView(4, value as string); break;
                //case "RightView": mm.ReqView(5, value as string); break;
                //case "PerspectiveView": mm.ReqView(6, value as string); break;
                //case "ScreenFit": mm.ReqFocus(value as string); break;
                //case "Home": mm.ReqHome(); break;
                //case "Select": mm.ReqGizmoType(0); break;
                //case "Move": mm.ReqGizmoType(1); break;
                //case "Rotate": mm.ReqGizmoType(2); break;
                //case "Scale": mm.ReqGizmoType(3); break;
                //case "PivotMove": mm.ReqGizmoType(5); break;
                //case "PivotRotate": mm.ReqGizmoType(6); break;
                //case "PivotCenter": mm.ReqPivotCenter(); break;
                case "SetMove": mm.ReqSetTransform(0, (value as string[]).ToList()); break;
                case "SetRotate": mm.ReqSetTransform(1, (value as string[]).ToList()); break;
                case "SetScale": mm.ReqSetTransform(2, (value as string[]).ToList()); break;

                //파일 기능
                case "OpenFile": mm.ReqOpenFile(value, "1", false); break;
                default: Debug.LogError("Unknown Reqest from UI Thread."); break;
            }
        }

        /// <summary>
        /// 비동기 메세지. 응용단으로 보냄
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        public void Res(string eventName, object eventValue)
        {
#if ENABLE_WINMD_SUPPORT
            mILBridge.EventFired(eventName, eventValue);
#endif
        }

        public void SelectObject(string objID) { Res("SelectObject", objID as object); }
        public void GetTreeNode(byte[] bytes) { Res("GetTreeNode", bytes as object); }


        /// <summary>
        /// DMS 전용 3D파일 Save
        /// </summary>
        /// <param name="objID"></param>
        /// <returns></returns>
        public byte[] SaveToDms3D(string objID)
        {
            using (var stream = new MemoryStream())
            {
                Debug.LogError("여기는 들어오는지...");
                GameObject findObj = null;
                if (objID.Equals("ALL"))
                {
                    findObj = MainManager.Ins.root3D.gameObject;
                }
                else
                {
                    findObj = ObjectPropertyCtrl.Ins.FindObject(objID);
                    Debug.LogError("찾은 오브젝트:" + findObj.name);
                }

                IVRNewVPMSerializer.SaveToBuffer(stream, findObj);
             
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Open Dms3D파일
        /// </summary>
        /// <param name="dataBuffer">파일 buffer</param>
        //public void OpenDms3DFile(byte[] dataBuffer)
        //{
            //using (MemoryStream stream = new MemoryStream(dataBuffer))
            //{
            //    using (BinaryReader br = new BinaryReader(stream))
            //    {
            //        IVRNewVPMSerializer.Open(br, GameObject.Find("3DRoot"));
            //    }
            //}
       // }

    }

    public class myInfo
    {
        public int id;
        public int parent_id;
        public string name;
        public int order;
    }
}

