using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Input;

using IL2CPPToDotNetBridge;
using System.Diagnostics;

using UnityPlayer;
using Windows.ApplicationModel.Activation;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Collections;

using Windows.Storage.Streams;
using System.Windows.Input;
using IVR.UWP.Common;

namespace IVR.UWP.Unity3D
{
    public class Unity3DViewModel : IDisposable, INotifyPropertyChanged
    {
        [DllImport("GameAssembly.dll")]
        static extern void AddActivatedEventArgs(IActivatedEventArgs args);

        public event EventHandler OnGetObjectInfoFrom3D; //3d로부터 수신

        public event EventHandler OnGetHierarchyInfoFrom3D; //3D File Hierarchy 데이터 받기
        public event EventHandler OnGetSelectObjectInfoFrom3D;//3D로부터 선택된 오브젝트ID 수신

        // AI 갱신용 이벤트 추가
        public event EventHandler<string> OnAIPredictionReceived;
        public event EventHandler<string> OnPhysicalLimitReceived;

        public event EventHandler OnToolBarDisplayMenuClicked; //툴바 디스플레이메뉴 클릭
        public event EventHandler OnToolBarObjectMenuClicked; //툴바 오브젝트메뉴 클릭

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                this.RaisePropertyChanged("Text");
            }
        }
        public ICommand ToolCommand { get; set; } //ToolMenu
        //DotNetBridge mDotNetBrige = new DotNetBridge(); //예전브릿지방식(동기화불가, 사용X)
        IIL2CPPBridge unityCom;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var propertyChanged = this.PropertyChanged;
            if (propertyChanged != null)
            {
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        #endregion
        // AI 갱신용 이벤트 트리거
        public void InvokeAIPrediction(string resultData)
        {
            OnAIPredictionReceived?.Invoke(this, resultData);
        }

        public void InvokePhysicalLimit(string warningMsg)
        {
            OnPhysicalLimitReceived?.Invoke(this, warningMsg);
        }
        
        public Unity3DViewModel()
        {
          
            // BridgeBootstrapper.SetDotNetBridge(mDotNetBrige); //예전브릿지방식(동기화불가, 사용X)
            unityCom = BridgeBootstrapper.GetIL2CPPBridge();

            Text = "This is Unity3D Control";
            //비동기 수신 이벤트 처리부 등록
            unityCom.OnUnity3DEvent += UnityCom_OnUnity3DEvent;

            #region "ToolMenu"
            ToolCommand = new RelayParameterCommand(ToolMenuClicked);
            #endregion
        }

        /// <summary>
        /// Tool메뉴 클릭
        /// </summary>
        void ToolMenuClicked(object which)
        {
            switch(which as string)
            {
                case "Solid":
                case "Transparent":
                case "Ground":
                case "ScreenFit":
                    {
                        OnToolBarDisplayMenuClicked?.Invoke(which as string, EventArgs.Empty);
                       // SendToUnity(SendMessage.MenuDisplay, null, which as string);
                    }
                    break;
                //case "Transparent":
                //    {

                //    }
                //    break;
                //case "Ground":
                //    {

                //    }
                //    break;
                //case "FitZoom":
                //    {

                //    }
                //    break;
                case "Select":
                case "Move":
                case "Rotate":
                case "Scale":
                case "PivotMove":
                case "PivotRot":
                case "PivotCenter":
                case "Home":
                    {
                        OnToolBarObjectMenuClicked?.Invoke(which as string, EventArgs.Empty);
                     
                    }
                    break;
                //case "Move":
                //    {

                //    }
                //    break;
                //case "Rotate":
                //    {

                //    }
                //    break;
                //case "Scale":
                //    {

                //    }
                //    break;
                case "Align":
                    {

                    }
                    break;
                //case "PivotMove":
                //    {

                //    }
                //    break;
                //case "PivotRot":
                //    {

                //    }
                //    break;
                //case "PivotCenter":
                //    {

                //    }
                //    break;
                case "Dimension":
                    {

                    }
                    break;

            }
        }
        /// <summary>
        /// Unity3D로부터 이벤트수신처리
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventValue"></param>
        private void UnityCom_OnUnity3DEvent(string eventName, object eventValue)
        {
            if (eventName.Equals("err"))
            {
                if (OnGetObjectInfoFrom3D != null)
                    OnGetObjectInfoFrom3D(eventValue as string, EventArgs.Empty);

                return;
            }
            else if (eventName.Equals("objInfo"))
            {
                Debug.WriteLine(eventName);
                object[] objCol = eventValue as object[];

                StringBuilder sr = new StringBuilder();
                for (int i = 0; i < objCol.Length; i++)
                {
                    byte[] byteInfo = objCol[i] as byte[];

                    int objIdLen = BitConverter.ToInt32(byteInfo, 0);
                    int objNameLen = BitConverter.ToInt32(byteInfo, 4);

                    string objID = Encoding.UTF8.GetString(byteInfo, 8, objIdLen);
                    string objName = Encoding.UTF8.GetString(byteInfo, 8 + objIdLen, objNameLen);
                    int objType = BitConverter.ToInt32(byteInfo, 8 + objIdLen + objNameLen);

                    string objInfoStr = "";
                    objInfoStr += objID.ToString();
                    objInfoStr += ",";
                    objInfoStr += objName.ToString();
                    objInfoStr += ",";
                    objInfoStr += objType.ToString();

                    sr.AppendLine(objInfoStr);
                }

                if (OnGetObjectInfoFrom3D != null)
                    OnGetObjectInfoFrom3D(sr.ToString(), EventArgs.Empty);
            }
            else if (eventName.Equals("SelectObject"))
            {
                Debug.WriteLine($"From Unity SelectedObject is {eventValue.ToString()}");
                if(OnGetSelectObjectInfoFrom3D !=null)
                    OnGetSelectObjectInfoFrom3D(eventValue, EventArgs.Empty);
            }
            else if(eventName.Equals("OpenFile")) // 3DView 로드후 3D File Hierarchy 데이터 받기
            {
                if (OnGetHierarchyInfoFrom3D != null)
                    OnGetHierarchyInfoFrom3D(eventValue, EventArgs.Empty);
            }     
        }


        public void LoadStep(string fileName)
        {
           // unityCom.LoadStep(fileName);
        }

        //private void MDotNetBrige_OnGetObjInfoEvent(object sender, EventArgs e)
        //{
        //    object[] objCol = sender as object[];

        //    StringBuilder sr = new StringBuilder();
        //    for (int i = 0; i < objCol.Length; i++)
        //    {
        //        byte[] byteInfo = objCol[i] as byte[];

        //        int objIdLen = BitConverter.ToInt32(byteInfo, 0);
        //        int objNameLen = BitConverter.ToInt32(byteInfo, 4);

        //        string objID = Encoding.UTF8.GetString(byteInfo, 8, objIdLen);
        //        string objName = Encoding.UTF8.GetString(byteInfo, 8 + objIdLen, objNameLen);
        //        int objType = BitConverter.ToInt32(byteInfo, 8 + objIdLen + objNameLen);

        //        string objInfoStr = "";
        //        objInfoStr += objID.ToString();
        //        objInfoStr += ",";
        //        objInfoStr += objName.ToString();
        //        objInfoStr += ",";
        //        objInfoStr += objType.ToString();

        //        sr.AppendLine(objInfoStr);
        //    }

        //    if (OnGetObjectInfoFrom3D != null)
        //        OnGetObjectInfoFrom3D(sr.ToString(), EventArgs.Empty);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StreamTest(Windows.Storage.Streams.IBuffer buf )
        {
           // unityCom.StreamTest(buf);
        }
        public void GetObjectInfo(string objID)
        {
            #region 사용안함
            // m_il2cppBrige.ObjInfo(name);
            // string val =  m_il2cppBrige.GetObjName(name);
            //  m_il2cppBrige.ObjInfo(objID);
            //IIL2CPPBridge il2cppBridge = BridgeBootstrapper.GetIL2CPPBridge();
            //il2cppBridge.GetObjName(objID);
            // m_il2cppBrige.GetObjName("1");
            #endregion
            //Width Unity 동기
            //string val = unityCom.GetName(objID);
            //Debug.WriteLine("동기적으로 오는 data : " + val);

             //Width Unity 비동기
            // 리턴값은 Unity쪽에서 이벤트로 던질 것, 응용쪽에서 수신하여 처리부 구현 필요.
            //unityCom.ObjInfo(objID);
            
            Debug.WriteLine("비동기적으로 옴 ");
            //Width Unity 비동기
            // 리턴값은 Unity쪽에서 이벤트로 던질 것, 응용쪽에서 수신하여 처리부 구현 필요.
            //unityCom.ObjInfo(objID);
            //Debug.WriteLine("비동기적으로 옴 ");            
        }

        protected virtual void Dispose(bool isdisposable)
        {
            //if (this.ItemsCollection != null)
            //{
            //    this.ItemsCollection.Clear();
            //}
        }

        /// <summary>
        /// 신규노드 추가
        /// </summary>
        /// <param name="objID">신규 objID</param>
        /// <param name="addObjName">신규이름</param>
        /// <param name="parentID">부모id</param>
        /// <param name="dimenstionMode">0:3D,1:2D</param>
        public void AddNode(string objID,string addObjName, string parentID, int dimenstionMode)
        {
            if (unityCom != null)
                unityCom.ReqAddNode(objID, addObjName, parentID, dimenstionMode);
        }

        //public byte[] SaveToBuffer()
        //{
          // return  unityCom.GetDataBuffer() as byte[];
       // }
        public void SendToUnity(SendMessage messageName, object messageType, object contents)
        {
            if (AppCallbacks.Instance.IsInitialized())
            {
              //  IIL2CPPBridge il2cppBridge = BridgeBootstrapper.GetIL2CPPBridge();
                switch (messageName)
                {
                    case SendMessage.UnityZone: //3D영역 구분 신호 전달 
                        unityCom.UnityZone((bool)contents);// contents : false 나감, true 들어옴
                        break;
                    case SendMessage.SelectedNode: //노드의 선택
                        unityCom.SelectedNode((string)contents); //contents : 오브젝트ID
                        break;
                    case SendMessage.ReqSaveInit: //초기상태로 저장 요구
                        unityCom.ReqSaveInit((string)contents); //contents: 오브젝트ID
                        break;
                    case SendMessage.ReqMoveNode: //노드이동요구
                        List<string> cutPastList = contents as List<string>;
                        unityCom.ReqMoveNode(cutPastList[0], cutPastList[1]); //cutPastList[0]:이동할objID, cutPastList[1]:놓여지는곳ID
                        break;
                    case SendMessage.ReqRenameNode: //이름변경 요구
                        List<string> reNametList = contents as List<string>;
                        unityCom.ReqRenameNode(reNametList[0], reNametList[1]);//cutPastList[0]:오브젝트ID, cutPastList[1]:변경할 이름
                        break;
                    case SendMessage.ReqReobjID: //오브젝트아이디 변경 요구
                        List<string> reobjIDtList = contents as List<string>;
                        unityCom.ReqReobjID(reobjIDtList[0], reobjIDtList[1]);//cutPastList[0]:오브젝트ID, cutPastList[1]:변경할오브젝트ID
                        break;
                    //case SendMessage.ReqAddNode: //루트그룹노드 또는 자식그룹노드 추가
                    //    List<string> AddNoodList = contents as List<string>;
                    //    unityCom.ReqAddNode(AddNoodList[0], AddNoodList[1]);//index[0]:parentId, index[1]:type /parentId가 ""은 루트그룹노드, 있으면 자식그룹노드 추가 / type: 1=그룹노드, 2=일반노드, 3=케이블) 
                    //    break;
                    case SendMessage.ReqDeleteNode: //노드삭제 요구
                        unityCom.ReqDeleteNode((string)contents); //contents:오브젝트ID
                        break;
                    case SendMessage.ReqIsShow: //Show And Hide
                        Dictionary<string, bool> isCheckedDic = contents as Dictionary<string, bool>;
                        foreach (KeyValuePair<string, bool> item in isCheckedDic)
                        {
                            unityCom.ReqIsShow(item.Key, item.Value); //Key: objectId, Value: true(show) or false(hide)
                            break; 
                        }
                        break;
                    case SendMessage.MenuDisplay: //디스플레이 메뉴
                        //messageType : Solid, Transparent, Ground, ScreenFit, TopView, BottomView, FrontView, BackView, LeftView, RightView, PerspectiveView
                        //contents : 오브젝트ID or null(Ground).
                        unityCom.MenuDisplay(messageType as string, contents as string);
                        break;
                    case SendMessage.MenuObject: //오브젝트 메뉴
                        //messageType : Home, Select, Move, Rotate, Scale, PivotMove, PivotRotate, PivotCenter
                        unityCom.MenuObject(messageType as string);
                        break;
                    case SendMessage.FileOpen: //파일 열기
                        unityCom.FileOpen(contents); //byte[]로 변환한 파일
                        break;
                    case SendMessage.LinkInfo: //트리노드에서부품링크
                        List<object> linkList = contents as List<object>;
                        string objId = linkList[0] as string; 
                        bool linkYn = Convert.ToBoolean(linkList[1]);
                        //unityCom.ReqLinkInfo(objId, linkYn);
                        break;
                    case SendMessage.AI_WorldModel_Simulate: // AI 월드모델 시뮬레이션 요청
                        List<object> simList = contents as List<object>;
                        unityCom.ReqAIPrediction(simList[0] as string, Convert.ToInt32(simList[1]));
                        break;
                    case SendMessage.AI_Physical_Feedback: // 피지컬 AI 토크 피드백
                        List<object> phyList = contents as List<object>;
                        unityCom.UpdatePhysicalForce(phyList[0] as string, Convert.ToSingle(phyList[1]), Convert.ToSingle(phyList[2]));
                        break;
                    case SendMessage.AI_Spatial_SyncMesh: // 공간지능 매쉬 동기화
                        unityCom.SyncSpatialMesh(contents);
                        break;
                    default:
                        break;
                }
            }
        }
        public object RequestToUnity(SendMessage message, string envetName, object eventValue)
        {
            object getObect = null;   
            switch (message)
            {
                case SendMessage.Req:
                    unityCom.Req(envetName, eventValue);
                    break;
                case SendMessage.Get:
                    getObect = unityCom.Get(envetName, eventValue);
                    break;
                    
            }
            return getObect;
        }

    }

    public enum SendMessage
    {
        UnityZone, SelectedNode, MenuObject,
        ReqSaveInit, ReqMoveNode, ReqRenameNode, ReqReobjID, ReqAddNode,
        ReqDeleteNode, ReqIsRootNode, ReqIsShow,
        Req, MenuDisplay, FileOpen, Get, LinkInfo,
        // AI 통신 추가 (Phase 1)
        AI_WorldModel_Simulate, 
        AI_Agent_AutoOrder, 
        AI_Physical_Feedback,
        AI_Spatial_SyncMesh
    }

    public class DotNetBridge : IDotNetBridge
    {
        public event EventHandler OnGetObjectNameEvent;
        public event EventHandler OnGetObjInfoEvent;
        public event EventHandler OnGetObjEvent;

        public void MyFunction1()
        {
            
        }

        public void MyFunction2(string arg)
        {
          
        }

        public void GetObjName(string objID)
        {
        }

        public void GetObj(object obj)
        {
            
        }

        public void ObjInfo(object[] objCol)
        {
          
        }

        public void GetMessage(object obj)
        {
            
        }

        public void ResSaveInit()
        {
           
        }

        public void ResIsRootNode(bool existMesh)
        {
           
        }

        // AI 응답 인터페이스 구현
        public void ResAIPrediction(string resultData)
        {
            // HUD Panel 1 갱신 (추후 이벤트 발송)
            Unity3DViewModel vm = BridgeBootstrapper.GetDotNetBridge() as Unity3DViewModel;
            if (vm != null) vm.InvokeAIPrediction(resultData);
        }

        public void OnPhysicalLimitReached(string objID, string warningMsg)
        {
            // HUD Panel 4 에러 알림 
            Unity3DViewModel vm = BridgeBootstrapper.GetDotNetBridge() as Unity3DViewModel;
            if (vm != null) vm.InvokePhysicalLimit(warningMsg);
        }
    }
}
