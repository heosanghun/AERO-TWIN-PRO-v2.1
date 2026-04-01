using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using IVR.UWP.Common;
using IVR.UWP.Common.TreeView;
using IVR.UWP.Common.ListView;
using IVR.UWP.Common.InfoView;
using IVR.UWP.Common.TaskView;

using IVR.UWP.Unity3D;
using System.Data;
using System.Diagnostics;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.IO;
using Windows.UI.Xaml;
using Syncfusion.UI.Xaml.TreeGrid;

using Windows.UI.Popups;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ADMS.Dialog;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ADMS
{
    class MainViewModelNew : IVR.UWP.Common.ViewModel
    {
        private TreeViewModel treeViewModel;
        public Unity3DViewModel m3DPlayer;
        private ListViewModel listviewModel;
        private InfoViewModel infoviewModel;
        private ScenarioViewModel scenarioViewModel;


        private ITreeModel iTreeModel;
        //MyUserControl1 _treeView;

        MainPageNew _mainView;
        // TreeViewDMS _treeView;
        ObjectExplorer _objectExplorer;
        Unity3DPlayer _unityControl;
        // ListViewProcedure _lstProcedureControl;
        Information _infoView;
        Scenario _scenarioView;

        // TaskView _taskView;

        CommandInvoker m_cmdInvoker = new CommandInvoker();

        string ClickedObjID = null;

        StorageFile _file;
        enum OpenMode
        {
            OPEN,IMPORT,MERGE,NONE
        }

        OpenMode mOpenMode = OpenMode.NONE; //파일오픈모드

        /// <summary>
        /// 타이틀명 
        /// </summary>
        string _titleNm;
        public string TitleNm
        {
            get
            {
                return _titleNm;
            }
            set
            {
                _titleNm = value;
                RaisePropertyChanged("TitleNm");
            }
        }
        /// <summary>
        /// button Cammnd binding
        /// </summary>
        public ICommand BtnCommand { get; set; } //btn

        public Page MainView
        {
            get
            {
                return _mainView;
            }
            set
            {
                _mainView = value as MainPageNew;
                _mainView.OnRedoEvent += _mainView_OnRedoEvent;
                _mainView.OnUndoEvent += _mainView_OnUndoEvent;
                //_mainView.OnOpenFileButtonClicked += _mainView_OnOpenFileButtonClicked;
                //_mainView.OnSaveFileButtonClicked += _mainView_OnSaveFileButtonClicked;
                //_mainView.OnHomeButtonClicked += _mainView_OnHomeButtonClicked;
                //_mainView.OnObjectControlsButtonClicked += _mainView_OnObjectControlsButtonClicked;
                //_mainView.OnDisplayControlButtonClicked += _mainView_OnDisplayControlButtonClicked;
                //_mainView.OnPerspectiveItemClicked += _mainView_OnPerspectiveItemClicked;
                _mainView.OnCreatePrimitiveEvent += _mainView_OnCreatePrimitiveEvent;
            }
        }

        Button _undoBtn;
        public Button UndoBtn
        {
            set { _undoBtn = value; }
        }
        Button _redoBtn;
        public Button RedoBtn
        {
            set { _redoBtn = value; }
        }

        /// <summary>
        /// undo/redo Enable 설정
        /// </summary>
        void VisibleUndoRedo(bool reset)
        {
            if (reset)
            {
                m_cmdInvoker.Clear();
                _undoBtn.IsEnabled = false;
                _redoBtn.IsEnabled = false;
                return;
            }
            _undoBtn.IsEnabled = m_cmdInvoker.ExistUndo;
            _redoBtn.IsEnabled = m_cmdInvoker.ExistRedo;

        }

        public Control ObjectExplorer
        {
            get
            {
                return _objectExplorer;
            }
            set
            {
                _objectExplorer = value as ObjectExplorer;
                _objectExplorer.TreeModifyEvent += _objectExplorer_TreeModifyEvent;
                _objectExplorer.OnSaveEvent += _objectExplorer_OnSaveEvent;
                // _objectExplorer.DataContext = treeViewModel;
                // iTreeModel = treeViewModel as ITreeModel;
                //_treeView.OnButtonClicked += _treeView_OnButtonClicked;
            }
        }


        public Control Unity3DControl
        {
            get
            {
                return _unityControl;
            }
            set
            {
                _unityControl = value as Unity3DPlayer;
                _unityControl.OnInitialize();

                _unityControl.On3DInitialized += _unity3DPlayer_On3DInitialized;
                //_unityControl.OnButtonClicked += _unity3DPlayer_OnButtonClicked;
                _unityControl.OnPointerEntered += _unityControl_OnPointerEntered;
                _unityControl.OnPointerExited += _unityControl_OnPointerExited;

#if SAFETY
                (Unity3DControl as IVR.UWP.Unity3D.Unity3DPlayer).SetSafetyMenu();
#endif
            }
        }

        public Control InfoView
        {
            get
            {
                return _infoView;
            }
            set
            {
                _infoView = value as Information;
                //_infoView.DataContext = infoviewModel;

            }
        }

        public Control Scenario
        {
            get
            {
                return _scenarioView;
            }
            set
            {
                _scenarioView = value as Scenario;
                scenarioViewModel = _scenarioView.DataContext as ScenarioViewModel;
                //scenarioViewModel.OnProcedureInputEvent += ScenarioViewModel_OnProcedureInputEvent;
            }
        }

        /// <summary>
        /// function 링크 창
        /// </summary>
        UserControl _menuContents;
        public UserControl menuContrl
        {
            get;set;
            //get
            //{
            //    return _menuContents;
            //}
            //set
            //{
            //    _menuContents = value;
            //    RaisePropertyChanged("menuContrl");
            //}
        }
        /// <summary>
        /// 트리편집관련
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void _objectExplorer_TreeModifyEvent(object sender, TreeModifyEventArgs args)
        {
            TreeViewModelNew tvm = sender as TreeViewModelNew;
            switch (args.modifyMode)
            {
                case ModifyMode.RootNodeAdd:
                case ModifyMode.ChildNodeAdd:
                    {
                        TreeAddCommand treeAddCommand = new TreeAddCommand(tvm);
                        treeAddCommand.SetAddNodeDtProp = args.ChangeInfoDt;
                        treeAddCommand.SetCustomPlayerProp = m3DPlayer;

                        XePlayerAddCommand xeAddCommand = new XePlayerAddCommand(m3DPlayer);
                        xeAddCommand.SetAddNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] nodeAddCommands = { xeAddCommand, treeAddCommand };
                        LoopMacroCommand addCommands = new LoopMacroCommand(nodeAddCommands);
                        m_cmdInvoker.ExcuteCommad(addCommands);
                    }
                    break;
                case ModifyMode.LevelChange:
                    {
                        TreeLevelChangeCommand levelChangeCommand = new TreeLevelChangeCommand(tvm);
                        levelChangeCommand.SetDropNodeProp = args.ChangeInfoDt;
                        levelChangeCommand.SetCustomPlayerProp = m3DPlayer;

                        XeplayerLevelChangeCommand xeLevelChangeCommand = new XeplayerLevelChangeCommand(m3DPlayer);
                        xeLevelChangeCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] lcCommands = { levelChangeCommand, xeLevelChangeCommand };
                        LoopMacroCommand LevelChangedCommands = new LoopMacroCommand(lcCommands);
                        m_cmdInvoker.ExcuteCommad(LevelChangedCommands);
                    }
                    break;
                case ModifyMode.NodeDelete:
                    {
                        TreeDeleteCommand treeDelCommand = new TreeDeleteCommand(tvm);
                        treeDelCommand.SeNodeDtProp = args.ChangeInfoDt;
                        treeDelCommand.SetCustomPlayerProp = m3DPlayer;

                        XeplayerDeleteCommand xeDelCommand = new XeplayerDeleteCommand(m3DPlayer);
                        xeDelCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] lcCommands = { treeDelCommand, xeDelCommand };
                        LoopMacroCommand treeDelCommands = new LoopMacroCommand(lcCommands);
                        m_cmdInvoker.ExcuteCommad(treeDelCommands);
                    }
                    break;
                case ModifyMode.NameChanged:
                    {
                        TreeNameChangeCommand treenameChangeCommand = new TreeNameChangeCommand(tvm);
                        treenameChangeCommand.SetNodeDtProp = args.ChangeInfoDt;

                        XeplayerChangeNameCommand xeChangeCommand = new XeplayerChangeNameCommand(m3DPlayer);
                        xeChangeCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] lcCommands = { treenameChangeCommand, xeChangeCommand };
                        LoopMacroCommand treenameChangeCommands = new LoopMacroCommand(lcCommands);
                        m_cmdInvoker.ExcuteCommad(treenameChangeCommands);
                    }
                    break;
                case ModifyMode.NodeCheck:
                    {
                        TreeCheckBoxCommand chxCommand = new TreeCheckBoxCommand(tvm);
                        TreeNode node = args.ChangeInfoDt.Rows[0][0] as TreeNode;
                        chxCommand.treeNodeProp = node;
                        chxCommand.VisibleProp = (bool)node.IsChecked;

                        XeplayerShowHideCommand xeShowHideCommand = new XeplayerShowHideCommand(m3DPlayer);
                        xeShowHideCommand.objIdProp = (node.Item as IVRTreeModel).obj_id;
                        xeShowHideCommand.VisibleProp = (bool)node.IsChecked;

                        IVRCommand[] commands = { chxCommand, xeShowHideCommand };
                        LoopMacroCommand visiCommand = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(visiCommand);
                    }
                    break;
                case ModifyMode.InitPosition:
                    {
                        TreeInitPositionCommand treeInitCommand = new TreeInitPositionCommand(tvm);
                        treeInitCommand.SeNodeDtProp = args.ChangeInfoDt;

                        XeplayerInitPositionCommand xeInitCommand = new XeplayerInitPositionCommand(m3DPlayer);
                        xeInitCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] commands = { treeInitCommand, xeInitCommand };
                        LoopMacroCommand treeInitCommands = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(treeInitCommands);
                    }
                    break;
                case ModifyMode.OnlySel:
                    {
                        TreeOnlySelCommand treeOnlySelCommand = new TreeOnlySelCommand(tvm);
                        treeOnlySelCommand.SeNodeDtProp = args.ChangeInfoDt;

                        XeplayerOnlySelCommand xeOnlySelCommand = new XeplayerOnlySelCommand(m3DPlayer);
                        xeOnlySelCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] commands = { treeOnlySelCommand, xeOnlySelCommand };
                        LoopMacroCommand treeOnlySelCommands = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(treeOnlySelCommands);
                    }
                    break;
                case ModifyMode.NodeCopy:
                    {
                        TreeCopyCommand treeNodeCopyCommand = new TreeCopyCommand(tvm);
                        treeNodeCopyCommand.SeNodeDtProp = args.ChangeInfoDt;

                        XeplayerCopyCommand xeNodeCopyCommand = new XeplayerCopyCommand(m3DPlayer);
                        xeNodeCopyCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] commands = { xeNodeCopyCommand, treeNodeCopyCommand };
                        LoopMacroCommand treeNodeCopyCommands = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(treeNodeCopyCommands);
                    }
                    break;
                case ModifyMode.CurveNodeAdd:
                    {
                        TreeCurveAddCommand treeCurveAddCommand = new TreeCurveAddCommand(tvm);
                        treeCurveAddCommand.SeNodeDtProp = args.ChangeInfoDt;

                        XeplayerCurveAddCommand xeCurveAddCommand = new XeplayerCurveAddCommand(m3DPlayer);
                        xeCurveAddCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] commands = { xeCurveAddCommand, treeCurveAddCommand };
                        LoopMacroCommand treeCurveAddCommands = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(treeCurveAddCommands);
                    }
                    break;
                case ModifyMode.PipeNodeAdd:
                    {
                        TreePipeAddCommand treePipeAddCommand = new TreePipeAddCommand(tvm);
                        treePipeAddCommand.SeNodeDtProp = args.ChangeInfoDt;

                        XeplayerPipeAddCommand xePipeAddCommand = new XeplayerPipeAddCommand(m3DPlayer);
                        xePipeAddCommand.SetNodeDtProp = args.ChangeInfoDt;

                        IVRCommand[] commands = { xePipeAddCommand, treePipeAddCommand };
                        LoopMacroCommand treePipeAddCommands = new LoopMacroCommand(commands);
                        m_cmdInvoker.ExcuteCommad(treePipeAddCommands);
                    }
                    break;
            }

            VisibleUndoRedo(false);
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void _objectExplorer_OnSaveEvent(object sender, RoutedEventArgs e)
        {
            IVRTreeModel selTreeModel = _objectExplorer.GetFindLastSelelectedNode();
            if (selTreeModel == null) return;

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("DMS 3D파일", new List<String>() { ".dms3D" });
            savePicker.SuggestedFileName = selTreeModel.Name;
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {

                CachedFileManager.DeferUpdates(file);

                byte[] data = m3DPlayer.SaveNodeToDms3D(selTreeModel.obj_id);

                await FileIO.WriteBytesAsync(file, data);

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    showMsg(file.Name + " 을 성공적으로 저장하였습니다.");
                }
                else
                {
                    showMsg("File" + file.Name + "couldn't be saved.");
                }
            }


        }

        /// <summary> 
        /// nVpm 저장
        /// </summary>
        /// </summary>
        /// <param name="bSaveAs">다른이름으로저장 여부</param>
        public async void SaveNvpm(bool bSaveAs)
        {
            //현재보고있는 절차 Save
            (Scenario as Scenario).UpdateTaskDataMangerProcedure();
            StorageFile file;
            if (_file == null || bSaveAs)
            {
                FileSavePicker savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
#if SAFETY
                savePicker.FileTypeChoices.Add("콘텐츠 작업파일", new List<String>() { ".spm" });
#else
                savePicker.FileTypeChoices.Add("A-DMS 작업파일", new List<String>() { ".nVpm" });
#endif
                savePicker.SuggestedFileName = "NewDoc";
                file = await savePicker.PickSaveFileAsync();
            }
            else file = _file; //open한 파일 저장
             
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                byte[] Data3D = m3DPlayer.SaveNodeToDms3D("ALL");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));

                using (MemoryStream memDataSet = new MemoryStream())
                {
                  
                    IFormatter binFormatter = new BinaryFormatter();
                    DataSet cpDataSet = null;
#if SAFETY
                    cpDataSet = TaskDataManager.TaskDataSet.Copy();
                    ConvertToUnityDataSet(ref cpDataSet);

#else
                    cpDataSet = TaskDataManager.TaskDataSet;
#endif
                    xmlSerializer.Serialize(memDataSet, cpDataSet);

                    List<byte> result = new List<byte>();
                    result.AddRange(BitConverter.GetBytes(TaskDataManager.Ver));

                    int envirInx = 0;
                    switch (AppInstance.Ins.Environment)
                    {
                        case AppInstance.EnvironmentMode.FACTORY:
                            envirInx = 1;
                            break;
                        case AppInstance.EnvironmentMode.OUTSIDE:
                            envirInx = 2;
                            break;
                        default:
                            envirInx = 0;
                            break;
                    }

                    result.AddRange(BitConverter.GetBytes(envirInx)); //가상환경정보 인덱스
                    result.AddRange(BitConverter.GetBytes(memDataSet.Length));
                    result.AddRange(memDataSet.ToArray());
                    result.AddRange(Data3D);

                    await FileIO.WriteBytesAsync(file, result.ToArray());
                }

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    showMsg(file.Name + " 을 성공적으로 저장하였습니다.");

                    AppInstance.Ins.OpenVpmPath = file.Path;
#if SAFETY 
                    TitleNm = "콘텐츠 저작도구 " + file.Path;
#else
                    TitleNm = "A-DMS(Advanced Digital Maintenance System) " + file.Path;
#endif
                    mOpenMode = OpenMode.OPEN;
                }
                else
                {
                    showMsg("File" + file.Name + "couldn't be saved.");
                }
            }

            bool reset = true;
            VisibleUndoRedo(reset);
            // m3DPlayer.SaveNodeToDms3D("ALL"); //전제저장
        }


        /// <summary>
        /// VTSE포맷으로 Export
        /// </summary>
        /// <summary>
        /// Phase 4: CES 2026 Level 4 자율 MRO 시나리오 실행
        /// </summary>
        public async void ExecuteLevel4AutonomousMRO(string targetObjID)
        {
            // 1. World Model 시뮬레이션 요청 (유체 파열 예측)
            m3DPlayer.SendToUnity(SendMessage.AI_WorldModel_Simulate, null, new List<object> { targetObjID, 0 });
            
            // 시뮬레이션 대기 (UI 연출용)
            await Task.Delay(3000);

            // 2. Agentic AI 자율 판단 (부품 결함 인지 및 자동 발주)
            await AgenticManager.AutoOrderPart(targetObjID);

            // 3. Physical AI 물리 피드백 시작 (목표 토크 설정)
            m3DPlayer.SendToUnity(SendMessage.AI_Physical_Feedback, null, new List<object> { targetObjID, 15.0f, 20.0f });
        }

        public async void SaveToVTSE()
        {
            //현재보고있는 절차 Save
            (Scenario as Scenario).UpdateTaskDataMangerProcedure();

            StorageFile file;
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.FileTypeChoices.Add("VTSE 콘텐츠파일", new List<String>() { ".vtse" });
            savePicker.SuggestedFileName = "NewDoc";
            file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                CachedFileManager.DeferUpdates(file);

                byte[] Data3D = m3DPlayer.SaveNodeToDms3D("ALL");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));

                using (MemoryStream memDataSet = new MemoryStream())
                {
                    IFormatter binFormatter = new BinaryFormatter();

                    DataSet cpDataSet = TaskDataManager.TaskDataSet.Copy();
                    ConvertToUnityDataSet(ref cpDataSet);

                    xmlSerializer.Serialize(memDataSet, cpDataSet);

                    List<byte> result = new List<byte>();
                    result.AddRange(BitConverter.GetBytes(TaskDataManager.Ver));
                    result.AddRange(BitConverter.GetBytes(memDataSet.Length));
                    result.AddRange(memDataSet.ToArray());
                    result.AddRange(Data3D);

                    await FileIO.WriteBytesAsync(file, result.ToArray());
                }

                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    showMsg(file.Name + " 을 성공적으로 저장하였습니다.");

                    //AppInstance.Ins.OpenVpmPath = file.Path;
                    //TitleNm = "A-DMS(Advanced Digital Maintenance System) " + file.Path;
                    //mOpenMode = OpenMode.OPEN;
                }
                else
                {
                    showMsg("File" + file.Name + "couldn't be saved.");
                }
            }
        }


        /// <summary>
        /// ds.cho(2022.10.26)
        /// DataSet을 Unity에서 불러올수 있도록 .net 3.5 형식의 xml로 바꾸는 함수
        /// System.Single[] 타입 지원안함(4.0이상만 지원),System.Object[]타입 지원안함(4.0이상만 지원)
        /// 일단 byte[]배열로 바꾼후 Unity에서 변환해서 사용해야 함.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="taskDs"></param>
        public void ConvertToUnityDataSet(ref DataSet taskDs)
        {
            DataTable taskDataProcedure = new DataTable();

            foreach (DataTable taskProcedrue in taskDs.Tables)
            {
                if (taskProcedrue.TableName.Equals("TASK_PROCEDURE"))
                {
                    taskDataProcedure = taskProcedrue.Clone();
                    taskDataProcedure.Columns["DEST_TRANSFORMATION"].DataType = typeof(byte[]);
                    taskDataProcedure.Columns["ROTATION_INFO"].DataType = typeof(byte[]);

                    foreach (DataRow dr in taskProcedrue.Rows)
                    {
                        DataRow newRow = taskDataProcedure.NewRow();
                        newRow["SER_CD"] = dr["SER_CD"];
                        newRow["SUBSER_CD"] = dr["SUBSER_CD"];
                        newRow["PROCEMODE"] = dr["PROCEMODE"];  //0:3d,1:2d,2:human    
                        newRow["WORK_ORDER"] = dr["WORK_ORDER"];
                        newRow["ISSUBPROCE"] = dr["ISSUBPROCE"];		//서브절차추가

                        newRow["OBJECT_NM"] = dr["OBJECT_NM"];
                        newRow["OBJECT_ID"] = dr["OBJECT_ID"];
                        newRow["SENTENCE"] = dr["SENTENCE"];

                        if ((dr["DEST_TRANSFORMATION"] as float[]) != null)
                        {
                               newRow["DEST_TRANSFORMATION"] = TaskDataManager.ConvertDestTransformToByteArray(dr["DEST_TRANSFORMATION"] as float[]);
                        }
                        if ((dr["ROTATION_INFO"] as object[]) != null)
                            newRow["ROTATION_INFO"] = TaskDataManager.ConvertRotateInfoToByteArray(dr["ROTATION_INFO"] as object[]);

                        newRow["ROT_OBJECT_ID"] = dr["ROT_OBJECT_ID"]; //회전기준 오브젝트아이디
                        newRow["ISTRANSLATION"] = dr["ISTRANSLATION"];  //Translation:true or ShowHide:false mode

                        newRow["TRANSMODE"] = dr["TRANSMODE"];    // 진행수행방법 0:move 1: rotate 2: move&rotate
                        newRow["ISSHOW"] = dr["ISSHOW"];
                        newRow["ISLINK"] = dr["ISLINK"];
                        newRow["TRAINING_TYPE"] = dr["TRAINING_TYPE"];    //훈련진행타입 0:자동 1:손 2:공구

                        newRow["DUR_TIME"] = dr["DUR_TIME"];
                        newRow["TRAINING_MODE"] = dr["TRAINING_MODE"];   //훈련수행방법 0:free, 1:line, 3:rotate,4:self rotate, 5: click count 6: click & Time
                        newRow["CLICK_CNT"] = dr["CLICK_CNT"];

                        newRow["ISBLINK"] = dr["ISBLINK"];                  //blink 효과 (made by dscho 180330) . 3D상에서 BLINK효과를 주고 수행토록 처리
                        newRow["IS_SILHOUETTE"] = dr["IS_SILHOUETTE"];     //실루엣 여부(도착지에 실루엣을 표현할지 여부)


                        newRow["TOOL_OBJECT_ID"] = dr["TOOL_OBJECT_ID"];   //사용공구ID  

                        newRow["SOUND_FILE"] = dr["SOUND_FILE"];            //사운드파일명
                        newRow["SOUNDFILE_DATA"] = dr["SOUNDFILE_DATA"];   //사운드 실 데이터

                        taskDataProcedure.Rows.Add(newRow);
                    }

                    taskDataProcedure.AcceptChanges();
                    break;
                }
            }

            taskDs.Tables.Remove("TASK_PROCEDURE");
            taskDs.Tables.Add(taskDataProcedure);

            taskDs.AcceptChanges();
        }

        /// <summary>
        /// safety사업에서는 아예 Unity형태의 DataSet구조로 관리하기 위해 처리
        /// </summary>
        /// <param name="taskDs"></param>
        public void ConvertFromUnityDataSet(ref DataSet taskDs)
        {
            DataTable taskDataProcedure = new DataTable();

            foreach (DataTable taskProcedrue in taskDs.Tables)
            {
                if (taskProcedrue.TableName.Equals("TASK_PROCEDURE"))
                {
                    taskDataProcedure = taskProcedrue.Clone();
                    taskDataProcedure.Columns["DEST_TRANSFORMATION"].DataType = typeof(float[]);
                    taskDataProcedure.Columns["ROTATION_INFO"].DataType = typeof(object[]);

                    foreach (DataRow dr in taskProcedrue.Rows)
                    {
                        DataRow newRow = taskDataProcedure.NewRow();
                        newRow["SER_CD"] = dr["SER_CD"];
                        newRow["SUBSER_CD"] = dr["SUBSER_CD"];
                        newRow["PROCEMODE"] = dr["PROCEMODE"];  //0:3d,1:2d,2:human    
                        newRow["WORK_ORDER"] = dr["WORK_ORDER"];
                        newRow["ISSUBPROCE"] = dr["ISSUBPROCE"];		//서브절차추가

                        newRow["OBJECT_NM"] = dr["OBJECT_NM"];
                        newRow["OBJECT_ID"] = dr["OBJECT_ID"];
                        newRow["SENTENCE"] = dr["SENTENCE"];

                        if ((dr["DEST_TRANSFORMATION"] as byte[]) != null)
                        {
                            if(newRow["OBJECT_ID"].Equals("CAMERA"))
                                newRow["DEST_TRANSFORMATION"] = TaskDataManager.ConvertCameraDestTransformByteArrayToFloatArray(dr["DEST_TRANSFORMATION"] as byte[]);
                            else 
                                newRow["DEST_TRANSFORMATION"] = TaskDataManager.ConvertDestTransformByteArrayToFloatArray(dr["DEST_TRANSFORMATION"] as byte[]);
                        }
                        if ((dr["ROTATION_INFO"] as byte[]) != null)
                            newRow["ROTATION_INFO"] = TaskDataManager.ConvertRotateInfoByteArrayToObjectArray(dr["ROTATION_INFO"] as byte[]);

                        newRow["ROT_OBJECT_ID"] = dr["ROT_OBJECT_ID"]; //회전기준 오브젝트아이디
                        newRow["ISTRANSLATION"] = dr["ISTRANSLATION"];  //Translation:true or ShowHide:false mode

                        newRow["TRANSMODE"] = dr["TRANSMODE"];    // 진행수행방법 0:move 1: rotate 2: move&rotate
                        newRow["ISSHOW"] = dr["ISSHOW"];
                        newRow["ISLINK"] = dr["ISLINK"];
                        newRow["TRAINING_TYPE"] = dr["TRAINING_TYPE"];    //훈련진행타입 0:자동 1:손 2:공구

                        newRow["DUR_TIME"] = dr["DUR_TIME"];
                        newRow["TRAINING_MODE"] = dr["TRAINING_MODE"];   //훈련수행방법 0:free, 1:line, 3:rotate,4:self rotate, 5: click count 6: click & Time
                        newRow["CLICK_CNT"] = dr["CLICK_CNT"];

                        newRow["ISBLINK"] = dr["ISBLINK"];                  //blink 효과 (made by dscho 180330) . 3D상에서 BLINK효과를 주고 수행토록 처리
                        newRow["IS_SILHOUETTE"] = dr["IS_SILHOUETTE"];     //실루엣 여부(도착지에 실루엣을 표현할지 여부)


                        newRow["TOOL_OBJECT_ID"] = dr["TOOL_OBJECT_ID"];   //사용공구ID  

                        newRow["SOUND_FILE"] = dr["SOUND_FILE"];            //사운드파일명
                        newRow["SOUNDFILE_DATA"] = dr["SOUNDFILE_DATA"];   //사운드 실 데이터

                        taskDataProcedure.Rows.Add(newRow);
                    }

                    taskDataProcedure.AcceptChanges();
                    break;
                }
            }

            taskDs.Tables.Remove("TASK_PROCEDURE");
            taskDs.Tables.Add(taskDataProcedure);

            taskDs.AcceptChanges();
        }
        public MainViewModelNew()
        {
#region "btn click"
           // BtnCommand = new RelayParameterCommand(BtnClicked);
#endregion

            treeViewModel = new TreeViewModel();
            treeViewModel.OnSaveInitCommandFromTree += TreeViewModel_OnSaveInitCommandFromTree;
            treeViewModel.OnGetMessageFormTree += TreeViewModel_OnGetMessageFormTree;
            treeViewModel.OnPasteNodeCommandFromTree += TreeViewModel_OnPasteNodeCommandFromTree;
            treeViewModel.OnReNameCommandFromTree += TreeViewModel_OnReNameCommandFromTree;
            treeViewModel.OnReObjIDCommandFromTree += TreeViewModel_OnReObjIDCommandFromTree;
            treeViewModel.OnAddRootNodeCommandFromTree += TreeViewModel_OnAddRootNodeCommandFromTree;
            treeViewModel.OnAddChildNodeCommandFromTree += TreeViewModel_OnAddChildNodeCommandFromTree;
            treeViewModel.OnDeleteNodeCommandFromTree += TreeViewModel_OnDeleteNodeCommandFromTree;
            treeViewModel.OnReqIsShowCommandFromTree += TreeViewModel_OnReqIsShowCommandFromTree;
            treeViewModel.OnSelectedItemCommandFromTree += TreeViewModel_OnSelectedItemCommandFromTree;
            treeViewModel.OnLinkYncommandFromTree += TreeViewModel_OnLinkYncommandFromTree;

            listviewModel = new ListViewModel();
            infoviewModel = new InfoViewModel();
            //taskViewModel = new TaskViewModel();

            //text = "요것은 메인페이지 TextBlock";

            ////////신규DataSet 생성//////////
            ///


#if SAFETY
            TitleNm = "콘텐츠 저작도구";
            SafetyTabViewMenuControl menuContrl = new SafetyTabViewMenuControl();
#else
            TitleNm = "A-DMS(Advanced Digital Maintenance System)";
            TabViewMenuControl menuContrl = new TabViewMenuControl();

#endif
            this.menuContrl = menuContrl;

            try
            {
                TaskDataManager.NewMakeDataSet();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           

        }
        /// <summary>
        /// 부품링크여부
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewModel_OnLinkYncommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.LinkInfo, null, sender);
        }


        /// <summary>
        /// 노드선택
        /// </summary>
        /// <param name="sender">SelectedItem.selfId : 선택된노드에 해당하는 오브젝트의 ID</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnSelectedItemCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.SelectedNode, null, sender);
            ClickedObjID = sender as string;
        }

        /// <summary>
        /// 오브젝트Show/Hide
        /// </summary>
        /// <param name="sender">Dictionary<string, bool> isCheckedDic /  Key : objectId, Value : true(show) or false(hide)</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnReqIsShowCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqIsShow, null, sender);
        }

        /// <summary>
        /// 노드삭제
        /// </summary>
        /// <param name="sender">SelectedItem.selfId</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnDeleteNodeCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqDeleteNode, null, sender);


        }

        /// <summary>
        /// 자식노드 추가
        /// </summary>
        /// <param name="sender">List<string> addChildNodeList / index[0] : parentId, index[1] : type</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnAddChildNodeCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqAddNode, null, sender);
        }

        /// <summary>
        /// 루트노드 추가
        /// </summary>
        /// <param name="sender">List<string> addRootNodeList / index[0] : "", index[1] : type</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnAddRootNodeCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqAddNode, null, sender);
        }

        /// <summary>
        /// 오브젝트ID변경
        /// </summary>
        /// <param name="sender">List<string> reNameList / index[0] : 원래ID, index[1] : 변경ID</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnReObjIDCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqReobjID, null, sender);
        }

        /// <summary>
        /// 이름변경
        /// </summary>
        /// <param name="sender">List<string> reNameList / index[0] : 원래이름, index[1] : 변경될이름</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnReNameCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqRenameNode, null, sender);
        }

        /// <summary>
        ///  붙여넣기
        /// </summary>
        /// <param name="sender">List<string> cutPastList / index[0] : 옮길 ID, index[1] : 옮겨질 그룹노드 ID</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnPasteNodeCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqMoveNode, null, sender);
        }

        /// <summary>
        /// TreeView에서 던지는 메세지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewModel_OnGetMessageFormTree(object sender, EventArgs e)
        {
            //  _mainView.showMsg(sender as string);
        }

        /// <summary>
        /// 초기상태로 저장
        /// </summary>
        /// <param name="sender">오브젝트ID</param>
        /// <param name="e"></param>
        private void TreeViewModel_OnSaveInitCommandFromTree(object sender, EventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.ReqSaveInit, null, sender);
        }

        private void _unity3DPlayer_On3DInitialized(object sender, EventArgs e)
        {

            m3DPlayer = new Unity3DViewModel();
            _unityControl.DataContext = m3DPlayer;

            m3DPlayer.OnGetObjectInfoFrom3D += Unity3DViewModel_OnGetObjectInfoFrom3D;
            m3DPlayer.OnGetHierarchyInfoFrom3D += M3DPlayer_OnGetHierarchyInfoFrom3D;
            m3DPlayer.OnGetHierarchyInfoFrom3DNew += M3DPlayer_OnGetHierarchyInfoFrom3DNew;
            m3DPlayer.OnGetSelectObjectInfoFrom3D += M3DPlayer_OnGetSelectObjectInfoFrom3D;
            m3DPlayer.OnToolBarDisplayMenuClicked += M3DPlayer_OnToolBarDisplayMenuClicked;
            m3DPlayer.OnToolBarObjectMenuClicked += M3DPlayer_OnToolBarObjectMenuClicked;

            m3DPlayer.OnGetCurveDockingInfoFrom3D += M3DPlayer_OnGetCurveDockingInfoFrom3D;
            m3DPlayer.OnGetPipeDockingInfoFrom3D += M3DPlayer_OnGetPipeDockingInfoFrom3D;
            m3DPlayer.OnGetRefresSelectedInfoFrom3D += M3DPlayer_OnGetRefresSelectedInfoFrom3D;
            m3DPlayer.OnGetTransformationFrom3D += M3DPlayer_OnGetTransformationFrom3D;
            m3DPlayer.OnGetAlignReferenceInfoFrom3D += M3DPlayer_OnGetAlignReferenceInfoFrom3D;

         //   m3DPlayer.OnToolBarPropertyClicked += M3DPlayer_OnToolBarPropertyClicked; //오브젝트 속성 클릭
            _objectExplorer.UnityPlayer = m3DPlayer; //트리창
            _scenarioView.UnityPlayer = m3DPlayer; //시나리오창
            _scenarioView.ObjExplore = _objectExplorer; //

        }

        private void M3DPlayer_OnGetCurveDockingInfoFrom3D(object sender, EventArgs e)
        {
            _objectExplorer.GetCurveDockingInfoFromObjID(sender);
        }

        private void M3DPlayer_OnGetPipeDockingInfoFrom3D(object sender, EventArgs e)
        {
            _objectExplorer.GetPipeDockingInfoFromObjID(sender);
        }

        /// <summary>
        /// 3D 기즈모 드래그 중 속성정보 새로고침을 요청
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnGetRefresSelectedInfoFrom3D(object sender, EventArgs e)
        {
            _objectExplorer.GetRefresSelectedInfo();
        }

        /// <summary>
        /// 오브잭트의 3D Transformation의 UnDO/REDO를 담당하는 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnGetTransformationFrom3D(object sender, EventArgs e)
        {
            object[] data = sender as object[];
            XePlayerTransformationCommand xeTransformationCommand = new XePlayerTransformationCommand(m3DPlayer, _infoView, data);

            IVRCommand[] commands = { xeTransformationCommand };
            LoopMacroCommand transformationCommands = new LoopMacroCommand(commands);
            m_cmdInvoker.ExcuteCommad(transformationCommands);

            VisibleUndoRedo(false);
        }

        private void M3DPlayer_OnGetAlignReferenceInfoFrom3D(object sender, EventArgs e)
        {
            _objectExplorer.GetAlignReferenceInfoFrom3D(sender);
        }

        /// <summary>
        /// object관련 툴바메뉴 클릭이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnToolBarObjectMenuClicked(object sender, EventArgs e)
        {
            IVRTreeModel selTreeModel = _objectExplorer.GetFindLastSelelectedNode();
            if (selTreeModel != null)
            {
                switch (sender as string)
                {
                    case "Align":
                        {
                            _objectExplorer.ShowAlignEditor(selTreeModel.obj_id);
                        }
                        break;
                    default:
                        {
                            m3DPlayer.SendToUnity(SendMessage.MenuObject, sender, selTreeModel.obj_id);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Display관련 툴바메뉴 클릭이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnToolBarDisplayMenuClicked(object sender, EventArgs e)
        {
            IVRTreeModel selTreeModel = _objectExplorer.GetFindLastSelelectedNode();
            if (selTreeModel != null)
            {
                m3DPlayer.SendToUnity(SendMessage.MenuDisplay, sender, selTreeModel.obj_id);
            }
        }

        /// <summary>
        /// 3DViewer선택 시 트리뷰어 선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnGetSelectObjectInfoFrom3D(object sender, EventArgs e)
        {
            _objectExplorer.GetObjectInfoFromObjID(sender, true);
            //treeViewModel.GetSelectObjectInfoFrom3D(sender);
            ClickedObjID = sender as string;
        }

        /// <summary>
        /// 3D File Hierarchy 데이터 받기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M3DPlayer_OnGetHierarchyInfoFrom3D(object sender, EventArgs e)
        {
            // treeViewModel.GetObjectInfo(sender);
            _objectExplorer.Get3DObjectInfo(sender);
        }
        private void M3DPlayer_OnGetHierarchyInfoFrom3DNew(object sender, EventArgs e)
        {
            if(mOpenMode == OpenMode.OPEN)
            _objectExplorer.Get3DObjectInfoNew(sender,true);
            else
                _objectExplorer.Get3DObjectInfoNew(sender, false);
        }
        /// <summary>
        /// Import창열기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void _mainView_OnOpenFileButtonClicked(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".dms3D");
            openPicker.FileTypeFilter.Add(".fbx");
            openPicker.FileTypeFilter.Add(".step");
            openPicker.FileTypeFilter.Add(".stp");
            openPicker.FileTypeFilter.Add(".STEP");
            //openPicker.FileTypeFilter.Add(".txt");
            try
            {
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    Stream streamTwo = await file.OpenStreamForReadAsync();
                    MemoryStream memoryStream = new MemoryStream();
                    streamTwo.CopyTo(memoryStream);
                    //    byteArray = memoryStream.ToArray();

                    switch (file.FileType.ToLower())
                    {
                        case ".fbx":
                            {
                                m3DPlayer.SendToUnity(SendMessage.FileOpen, null, memoryStream.ToArray());
                            }
                            break;
                        case ".dms3d":
                            {
                                bool bVpm = false;
                                m3DPlayer.OpenFileFromDms3D(memoryStream.ToArray(), false, 1, bVpm);
                            }
                            break;
                        case ".step":
                        case ".stp":
                            {
                                StorageFolder DestiFoloder = ApplicationData.Current.LocalFolder;
                                StorageFile copiedFile = await file.CopyAsync(DestiFoloder, file.Name, NameCollisionOption.ReplaceExisting);
                                m3DPlayer.OpenStepFile(file.Name,false, 10f, 0.25f, 1);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //    _mainView.ClosebackStage(); //BackStage Close
        }

        /// <summary>
        /// 3D Import
        /// </summary>
        /// <param name="model"></param>
        /// <param name="e"></param>
        public async Task<bool> Import(DataImportModel model,bool bMerge)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

            switch(model.FORMAT)
            {
                case "stp":
                    openPicker.FileTypeFilter.Add(".step");
                    openPicker.FileTypeFilter.Add(".stp");
                    openPicker.FileTypeFilter.Add(".STEP");
                    break;
                case "fbx":
                    openPicker.FileTypeFilter.Add(".FBX");
                    openPicker.FileTypeFilter.Add(".fbx");
                    break;
                case "jt":
                    openPicker.FileTypeFilter.Add(".JT");
                    openPicker.FileTypeFilter.Add(".jt");
                    break;
                case "dms3D":
                    openPicker.FileTypeFilter.Add(".dms3D");
                    break;
                case "img":
                    openPicker.FileTypeFilter.Add(".png");
                    openPicker.FileTypeFilter.Add(".jpg");
                    openPicker.FileTypeFilter.Add(".jepg");
                    openPicker.FileTypeFilter.Add(".bmp");
                    break;
            }
            try
            {
                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    switch (file.FileType.ToLower())
                    {
                        case ".fbx":
                        case ".dms3d":
                            {
                                Stream streamTwo = await file.OpenStreamForReadAsync();
                                MemoryStream memoryStream = new MemoryStream();
                                streamTwo.CopyTo(memoryStream);

                                if(file.FileType.ToLower().Equals(".fbx"))
                                {
                                    m3DPlayer.OpenFileFromFbx(memoryStream.ToArray(), bMerge, 1);
                                    //m3DPlayer.SendToUnity(SendMessage.FileOpen, null, memoryStream.ToArray());
                                }                                
                                else
                                {
                                    bool bVpm = false;
                                    if (bMerge)
                                    {
                                        m3DPlayer.OpenFileFromDms3D(memoryStream.ToArray(), bMerge, this._objectExplorer.GetMaxID + 1, bVpm);
                                    }
                                    else
                                    {
                                        m3DPlayer.OpenFileFromDms3D(memoryStream.ToArray(), bMerge, 1, bVpm);
                                        //saveToFile(memoryStream.ToArray(), "test.dat");
                                    }
                                        
                                }                                   
                            }
                            break;
                        case ".step":
                        case ".stp":
                            {
                                StorageFolder DestiFoloder = ApplicationData.Current.LocalFolder;
                                StorageFile copiedFile = await file.CopyAsync(DestiFoloder, file.Name, NameCollisionOption.ReplaceExisting);

                                if (bMerge)
                                {
                                    m3DPlayer.OpenStepFile(file.Name, bMerge, model.deflection, model.angle, this._objectExplorer.GetMaxID + 1);
                                }
                                else 
                                    m3DPlayer.OpenStepFile(file.Name, bMerge, model.deflection, model.angle, 1);
                            }
                            break;
                        case ".jpg": //이미지 플랜
                        case ".jepg":
                        case ".png":
                        case ".bmp":
                            {
                                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                                {
                                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                                    WriteableBitmap image = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                                    image.SetSource(fileStream);

                                    using (Stream stream = image.PixelBuffer.AsStream())
                                    {
                                        MemoryStream memoryStream = new MemoryStream();
                                        stream.CopyTo(memoryStream);

                                        //   bool isDataTest = false;
                                        //if (isDataTest)
                                        //{
                                        //    #region " data를 테스트하기 위한 임시 내용 (이미지 데이터 Serialize)"
                                        //    // Create sample file; replace if exists.
                                        //    Windows.Storage.StorageFolder storageFolder =
                                        //        Windows.Storage.ApplicationData.Current.LocalFolder;
                                        //    Windows.Storage.StorageFile sampleFile =
                                        //        await storageFolder.CreateFileAsync("image.dat",
                                        //            Windows.Storage.CreationCollisionOption.ReplaceExisting);
                                        //    //var bf = new BinaryFormatter();
                                        //    //using (var ms = new MemoryStream())
                                        //    //{
                                        //    //    bf.Serialize(ms, docommand);
                                        //    await FileIO.WriteBytesAsync(sampleFile, memoryStream.ToArray());
                                        //    // }
                                        //    #endregion
                                        //}
                                        //else
                                        //{
                                        if (bMerge)
                                        {
                                            //m3DPlayer.OpenStepFile(file.Name, bMerge, model.deflection, model.angle, this._objectExplorer.GetMaxID + 1);
                                            m3DPlayer.OpenImageFileToPlane(image.PixelWidth, image.PixelHeight, memoryStream.ToArray(), this._objectExplorer.GetMaxID + 1, bMerge);
                                        }
                                        else
                                            m3DPlayer.OpenImageFileToPlane(image.PixelWidth, image.PixelHeight, memoryStream.ToArray(), 1,bMerge);

                                    }
                                }
                            }
                            break;
                    }

                    mOpenMode = OpenMode.IMPORT;
                    AppInstance.Ins.OpenVpmPath = string.Empty;
#if SAFETY
                    TitleNm = "콘텐츠 저작도구 ";
#else
                  TitleNm = "A-DMS(Advanced Digital Maintenance System)";
#endif

                    _file = null;
                    ///////////////만약 기존에 vpm파일을 열었으면 삭제해야 함//////////////
                   DataTable dt =  TaskDataManager.GetGBLRelationDt();
                    foreach (DataRow dr in dt.Select())
                        dr.Delete();
                    /////////////////////////////////////////////////
                    bool reset = true;
                    VisibleUndoRedo(reset);
                    return true;
                }
                else return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                return false;
            }

            //    _mainView.ClosebackStage(); //BackStage Close
        }

        /// <summary>
        /// 치/공구 라이브러리 Import
        /// </summary>
        /// <param name="model"></param>
        async public void LibraryImport(LibraryDataModel model)
        {
            Stream streamTwo = await model._file.OpenStreamForReadAsync();
            MemoryStream memoryStream = new MemoryStream();
            streamTwo.CopyTo(memoryStream);

            m3DPlayer.OpenFileFromDms3D(memoryStream.ToArray(), true, this._objectExplorer.GetMaxID + 1, false);

        }

        public void SetEnvironmentInx(int idx)
        {
            m3DPlayer.SetEnvironmentIndex(idx);
        }

        public void SetHumanInx(int idx)
        {
           // m3DPlayer.SetHumanIndex(idx);
        }

        /// <summary>
        /// Export
        /// </summary>
        /// <param name="model"></param>
        /// <param name="e"></param>
        public void Export(DataExportModel model, bool bMerge)
        {
            switch (model.FORMAT)
            {
                case "vtse":
                    SaveToVTSE();
                    break;
                case "fbx": //FBX or GLTF 3D 포맷
                    {

                    }                   
                    break;
                case "dms3D": //dms 전용 3D포맷
                  
                    break;
                case "img": //이미지 포맷
                 
                    break;
            }
         }
        async void saveToFile(byte[] data,string name)
        {
            Windows.Storage.StorageFolder storageFolder =
                                          Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync(name,
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBytesAsync(sampleFile, data);
        }

        /// <summary>
        /// Vpm파일 열기
        /// </summary>
        public async void OpenNvpm()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
#if SAFETY
            openPicker.FileTypeFilter.Add(".spm");
#else
            openPicker.FileTypeFilter.Add(".nVpm");
#endif
            try
            {
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    Stream streamTwo = await file.OpenStreamForReadAsync();
                    MemoryStream memoryStream = new MemoryStream();
                    streamTwo.CopyTo(memoryStream);

                    TaskDataManager.Ver = BitConverter.ToSingle(memoryStream.ToArray(), 0);   //절차 버전

                    int envirInx =  BitConverter.ToInt32(memoryStream.ToArray(), 4);
                    m3DPlayer.SetEnvironmentIndex(envirInx);

                    switch (envirInx) //환경 라이브러리 인덱스
                    {
                        case 1:
                            AppInstance.Ins.Environment = AppInstance.EnvironmentMode.FACTORY;
                        break;
                        case 2:
                            AppInstance.Ins.Environment = AppInstance.EnvironmentMode.OUTSIDE;
                            break;
                        default:
                            AppInstance.Ins.Environment = AppInstance.EnvironmentMode.NONE;
                            break;
                    }

                    long dataSetLength = BitConverter.ToInt64(memoryStream.ToArray(), 8); //dataset 크기

                    byte[] buffer = new byte[dataSetLength];
                    Array.Copy(memoryStream.ToArray(), 16, buffer, 0, dataSetLength);
//#if SAFETY
//                    Array.Copy(memoryStream.ToArray(), 16, buffer, 0, dataSetLength);
//#else
//                    Array.Copy(memoryStream.ToArray(), 12, buffer, 0, dataSetLength);
//#endif

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(DataSet));
                    MemoryStream dataSetStream = new MemoryStream(buffer);

                    DataSet tmpDataSet = (DataSet)xmlSerializer.Deserialize(dataSetStream);
#if SAFETY
                    ConvertFromUnityDataSet(ref tmpDataSet);
#endif
                    ConvertFromUnityDataSet(ref tmpDataSet);
                    TaskDataManager.OpenDataSetFile(tmpDataSet);

                    dataSetStream.Close();

                    byte[] data3D = new byte[memoryStream.Length - (16 + dataSetLength)];   //3D 데이터
                    Array.Copy(memoryStream.ToArray(), 16 + dataSetLength, data3D, 0, data3D.Length);

//#if SAFETY
//                    byte[] data3D = new byte[memoryStream.Length - (16 + dataSetLength)];   //3D 데이터
//                    Array.Copy(memoryStream.ToArray(), 16 + dataSetLength, data3D, 0, data3D.Length);
//#else
//                    byte[] data3D = new byte[memoryStream.Length - (12 + dataSetLength)];   //3D 데이터
//                    Array.Copy(memoryStream.ToArray(), 12 + dataSetLength, data3D, 0, data3D.Length);
//#endif
                    bool bVpm = true;
                    m3DPlayer.OpenFileFromDms3D(data3D, false, 1, bVpm);

                    ///시나리오창 
                    (MainView as MainPageNew).ShowPnlScenarioPanel();

                    AppInstance.Ins.OpenVpmPath = file.Path ;

#if SAFETY
                    TitleNm = "콘텐츠 저작도구 " + file.Path;
#else
                 TitleNm = "A-DMS(Advanced Digital Maintenance System) " + file.Path;
#endif

                    mOpenMode = OpenMode.OPEN;

                    _file = file;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Display 버튼
        /// </summary>
        /// <param name="sender">Solid, Transparent, Ground, ScreenFit</param>
        /// <param name="e"></param>
        private void _mainView_OnDisplayControlButtonClicked(object sender, TappedRoutedEventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.MenuDisplay, sender, ClickedObjID);

        }
        /// <summary>
        /// Display의 Perspective View
        /// </summary>
        /// <param name="sender">TopView, BottomView, FrontView, BackView, LeftView, RightView, PerspectiveView</param>
        /// <param name="e"></param>
        private void _mainView_OnPerspectiveItemClicked(object sender, TappedRoutedEventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.MenuDisplay, sender, ClickedObjID);
        }
        /// <summary>
        /// Home버튼 초기상태로 세팅
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainView_OnHomeButtonClicked(object sender, TappedRoutedEventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.MenuObject, sender, null);

        }
        /// <summary>
        /// 오브젝트컨트롤
        /// </summary>
        /// <param name="sender">Select, Move, Rotate, Scale, PivotMove, PivotRotate, PivotCenter </param>
        /// <param name="e"></param>
        private void _mainView_OnObjectControlsButtonClicked(object sender, TappedRoutedEventArgs e)
        {
            m3DPlayer.SendToUnity(SendMessage.MenuObject, sender, null);


        }

        /// <summary>
        /// 마우스 3DViewer 영역밖으로 나감
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _unityControl_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Unity 밖으로 나감");
            bool inUnityZone = false;
            if (m3DPlayer != null)
                m3DPlayer.SendToUnity(SendMessage.UnityZone, null, inUnityZone);


        }
        /// <summary>
        /// 마우스 3DViewer 영역안으로 들어옴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _unityControl_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Unity 안으로 들어옴");
            bool inUnityZone = true;
            if (m3DPlayer != null)
                m3DPlayer.SendToUnity(SendMessage.UnityZone, null, inUnityZone);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Unity3DViewModel_OnGetObjectInfoFrom3D(object sender, EventArgs e)
        {
            //   _mainView.showMsg(sender as string);
        }

        /// <summary>
        /// Undo/Redo 관련
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _mainView_OnUndoEvent(object sender, KeyRoutedEventArgs e)
        {
            m_cmdInvoker.ControlZPressed();
            VisibleUndoRedo(false);
        }

        private void _mainView_OnRedoEvent(object sender, KeyRoutedEventArgs e)
        {
            m_cmdInvoker.ControlYPressed();
            VisibleUndoRedo(false);
        }

        /// <summary>
        /// 22.10.18. 기본도형 생성
        /// </summary>
        /// <param name="sender">오브젝트 타입. 101=큐브, 102=구, 103=캡슐, 104=원통</param>
        /// <param name="e"></param>
        private void _mainView_OnCreatePrimitiveEvent(object sender, EventArgs e)
        {
            int type = Convert.ToInt32(sender);
            IVRTreeModel item = _objectExplorer.CreateTreeModel("New Primitive Node", null, null, type, true);
            _objectExplorer.ModifyNode3D(ModifyMode.RootNodeAdd, item, null);
        }

        /// <summary>
        /// messagebox 처리
        /// </summary>
        /// <param name="msg"></param>
        public async void showMsg(string msg)
        {
            //UI code here
            MessageDialog dlg = new MessageDialog(msg);
            dlg.ShowAsync();

        }

        /// <summary>
        /// 버튼 클릭
        /// </summary>
        //void BtnClicked(object parameter)
        //{
        //    switch (parameter as string)
        //    {
        //        case "btnUndo": //Undo
        //            {
        //                m_cmdInvoker.ControlZPressed();
        //                VisibleUndoRedo(false);
        //            }
        //            break;

        //        case "btnRedo": //Redo
        //            {
        //                m_cmdInvoker.ControlYPressed();
        //                VisibleUndoRedo(false);
        //            }
        //            break;
        //        case "btnOpenNvpm": //nVpm열기
        //            {
        //                OpenNvpm();
        //            }
        //            break;
        //        case "btnSaveNvpm": //nvpm 저장
        //            {
        //                SaveNvpm(false);
        //            }
        //            break;

        //    }

           
        //}
        
       
        public void QuickMenuCommand(string param)
        {
            switch (param)
            {
                case "btnUndo": //Undo
                    {
                        m_cmdInvoker.ControlZPressed();
                        VisibleUndoRedo(false);
                    }
                    break;

                case "btnRedo": //Redo
                    {
                        m_cmdInvoker.ControlYPressed();
                        VisibleUndoRedo(false);
                    }
                    break;
                case "btnVpmOpen": //nVpm열기
                    {
                        OpenNvpm();
                    }
                    break;
                case "btnVpmSave": //nvpm 저장
                    {
                        SaveNvpm(false);
                    }
                    break;

            }
        }
    }


}
