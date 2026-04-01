using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Diagnostics;
using IVR.UWP.Common.TreeView;
using Syncfusion.UI.Xaml.TreeGrid;

using Syncfusion.UI.Xaml.Controls.Layout;

using System.Data;
using ADMS.Dialog;
using ADMS.Menu;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.System;

using IVR.UWP.Unity3D;

using ADMS.Safety;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace ADMS
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPageNew : Page
    {
        //TreeViewModelNew mTreeViewModel;
        // CommandInvoker m_cmdInvoker = new CommandInvoker();
        public event EventHandler<KeyRoutedEventArgs> OnUndoEvent;
        public event EventHandler<KeyRoutedEventArgs> OnRedoEvent;

        public event EventHandler OnCreatePrimitiveEvent; //라이브러리 메뉴의 기본도형 생성 이벤트

        Control  _deleteContents; //동적으로 생성된 contents는 수행후 hide되어야 함..
        //  MenuViewModel mMenu;
        public MainPageNew()
        {
            this.InitializeComponent();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyUp;

            // Phase 4: AI Agentic HUD 바인딩
            AgenticManager.AIOrderList.CollectionChanged += AIOrderList_CollectionChanged;

            // Loaded 이벤트에서 Unity3DViewModel 이벤트 구독
            this.Loaded += MainPageNew_Loaded;
        }

        private void MainPageNew_Loaded(object sender, RoutedEventArgs e)
        {
            if(ObjectExplorer != null && ObjectExplorer.UnityPlayer != null)
            {
                ObjectExplorer.UnityPlayer.OnAIPredictionReceived += UnityPlayer_OnAIPredictionReceived;
                ObjectExplorer.UnityPlayer.OnPhysicalLimitReceived += UnityPlayer_OnPhysicalLimitReceived;
            }
        }

        private async void UnityPlayer_OnPhysicalLimitReceived(object sender, string e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtPhysicalWarning.Text = e;
            });
        }

        private async void UnityPlayer_OnAIPredictionReceived(object sender, string e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtWorldModel.Text = e;
            });
        }

        private async void AIOrderList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // HUD Panel 2 업데이트 (Main UI 쓰레드 실행)
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (AgenticManager.AIOrderList.Count > 0)
                {
                    var order = AgenticManager.AIOrderList[0];
                    txtAgenticStatus.Text = $"{order.PartID} {order.Status}";
                    txtAgenticTime.Text = order.EstimatedTime;
                }
            });
        }

            var view = ApplicationView.GetForCurrentView();
            view.TryEnterFullScreenMode();
            //if (view.IsFullScreenMode)
            //{
            //    view.ExitFullScreenMode();
            //}

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            // Set XAML element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

          //  coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
          //  coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
          //  UpdateTitleBarLayout(coreTitleBar);

           

            //this.ObjectExplorer.TreeModifyEvent += ObjExplorer_TreeModifyEvent;
            this.ObjectExplorer.OnPartLinkerEvent += ObjectExplorer_OnPartLinkerEvent;
            this.ObjectExplorer.OnCurveEditorEvent += ObjectExplorer_OnCurveEditorEvent;
            this.ObjectExplorer.OnCurveDockingInfoFrom3dEvent += ObjectExplorer_OnCurveEdifotDockingFrom3dEvent;
            this.ObjectExplorer.OnCurveEditorNodeSelectionEvent += ObjectExplorer_OnCurveEditorNodeSelectionEvent;
            this.ObjectExplorer.OnPipeDockingInfoFrom3dEvent += ObjectExplorer_OnPipeEdifotDockingFrom3dEvent;
            this.ObjectExplorer.OnPipeEditorNodeSelectionEvent += ObjectExplorer_OnPipeEditorNodeSelectionEvent;
            this.ObjectExplorer.OnPipeEditorEvent += ObjectExplorer_OnPipeEditorEvent;
            this.ObjectExplorer.OnSelectedInfoEvent += ObjectExplorer_OnSelectedInfoEvent;
            this.ObjectExplorer.OnRefreshSelectedInfoEvent += ObjectExplorer_OnRefreshSelectedInfoEvent;
            this.ObjectExplorer.OnAlignEditorEvent += ObjectExplorer_OnAlignEditorEvent;
            this.ObjectExplorer.OnAlignReferenceInfoFrom3dEvent += ObjectExplorer_OnAlignReferenceInfoFrom3dEvent;

            //mMenu = this.MenuView.DataContext as MenuViewModel;
            //mMenu.OnMenuClickEvent += MMenu_OnMenuClickEvent;

          //  MenuViewNew.OnMenuClickEvent += MenuViewNew_OnMenuClickEvent;
            //this.PartLinkerCtrl.OnApplyEvent += PartLinkerCtrl_OnApplyEvent;
            //this.PartLinkerCtrl.OnCloseEvent += PartLinkerCtrl_OnCloseEvent;
            this.CurveEditorCtrl.OnCloseEvent += CurveEditorCtrl_OnCloseEvent;
            this.CurveEditorCtrl.OnEditModeEvent += CurveEditorCtrl_OnEditModeEvent;
            this.CurveEditorCtrl.OnValueChangeEvent += CurveEditorCtrl_OnValueChangeEvent;
            this.CurveEditorCtrl.OnColorButtonEvent += CurveEditorCtrl_OnColorButtonEvent;
            this.PipeEditorCtrl.OnCloseEvent += PipeEditorCtrl_OnCloseEvent;
            this.PipeEditorCtrl.OnEditModeEvent += PipeEditorCtrl_OnEditModeEvent;
            this.PipeEditorCtrl.OnValueChangeEvent += PipeEditorCtrl_OnValueChangeEvent;
            this.PipeEditorCtrl.OnColorButtonEvent += PipeEditorCtrl_OnColorButtonEvent;
            this.ColorPaletteCtrl.OnCloseEvent += ColorPaletteCtrl_OnCloseEvent;
            this.infoView.OnTransformEvent += InfoView_OnTransformEvent;
            this.infoView.OnColorChangeEvent += InfoView_OnColorChangeEvent;
            this.infoView.OnSelectedInfoEvent += ObjectExplorer_OnSelectedInfoEvent;
            this.infoView.OnColorPaletteEvent += InfoView_OnColorPaletteEvent;
            this.AlignEditorCtrl.OnApplyEvent += AlignEditorCtrl_OnApplyEvent;
            this.AlignEditorCtrl.OnEditModeEvent += AlignEditorCtrl_OnEditModeEvent;

            //SfDockingManager.SetFloatingWindowRect(PartLinker, new Rect(300, 0, 330, 500));
           // SfDockingManager.SetFloatingWindowRect(ProcedureInput, new Rect(800, 0, 450, 900));

           // SfDockingManager.SetDockState(PartLinker, DockState.Hidden);
           // SfDockingManager.SetDockState(ProcedureInput, DockState.Hidden);
            SfDockingManager.SetDockState(Scenario, DockState.AutoHidden);
            SfDockingManager.SetDockState(InforMation, DockState.Hidden);

            SfDockingManager.SetFloatingWindowRect(CurveEditor, new Rect(300, 0, 330, 350));
            SfDockingManager.SetDockState(CurveEditor, DockState.Hidden);
            SfDockingManager.SetFloatingWindowRect(PipeEditor, new Rect(300, 0, 330, 300));
            SfDockingManager.SetDockState(PipeEditor, DockState.Hidden);
            SfDockingManager.SetFloatingWindowRect(ColorPalette, new Rect(630, 0, 340, 600));
            SfDockingManager.SetDockState(ColorPalette, DockState.Hidden);
            SfDockingManager.SetFloatingWindowRect(AlignEditor, new Rect(630, 0, 410, 230));
            SfDockingManager.SetDockState(AlignEditor, DockState.Hidden);

            SfDockingManager.SetNoHeader(Viewer, true);
           // SfDockingManager.SetNoHeader(this.pnlUnity3d, true);
            (this.DataContext as MainViewModelNew).MainView = this;
            (this.DataContext as MainViewModelNew).UndoBtn = this.btnUndo;
            (this.DataContext as MainViewModelNew).RedoBtn = this.btnRedo;

            (this.DataContext as MainViewModelNew).Unity3DControl = this.pnlUnity3d;
            (this.DataContext as MainViewModelNew).ObjectExplorer = this.ObjectExplorer;
          //  (this.DataContext as MainViewModelNew).InfoView = this.infoView;
            (this.DataContext as MainViewModelNew).Scenario = this.Scenario;

            CurveEditorCtrl.ObjectExplorer = this.ObjectExplorer;
            PipeEditorCtrl.ObjectExplorer = this.ObjectExplorer;
            AlignEditorCtrl.ObjectExplorer = this.ObjectExplorer;

            Scenario.OnProcedureInputEvent += Scenario_OnProcedureInputEvent; //절차입력클릭 이벤트
            Scenario.OnProcedureEditEvent += Scenario_OnProcedureEditEvent;

            //  this._ribbon.Loaded += _ribbon_Loaded;

#if SAFETY
             ((this.DataContext as MainViewModelNew).menuContrl as SafetyTabViewMenuControl).OnMenuClick += MenuViewNew_OnMenuBtnClick;
            try
            {
                AppInstance.Ins.ConnectionString = "server=192.168.0.13; database=safetydb; uid=vtse; pwd=vtse1234;";
            }
            catch(Exception ex)
            {
                throw ex;
            }
#else
            ((this.DataContext as MainViewModelNew).menuContrl as TabViewMenuControl).OnMenuClick += MenuViewNew_OnMenuBtnClick;
            SfDockingManager.SetHeader(this.Scenario, "정비절차판넬");
#endif
            // this.MenuViewNew.OnMenuClick += MenuViewNew_OnMenuBtnClick;
            this.pnlUnity3d.OnToolPropertyClick += PnlUnity3d_OnToolPropertyClick;

            docking.DockStateChanged += Docking_DockStateChanged;

            ///DB Connection for Safety
            
            
        }

        /// <summary>
        /// 메뉴 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuViewNew_OnMenuBtnClick(object sender, EventArgs e)
        {
            switch((sender as Control).Name)
            {
                case "btnNew":
                    break;
                case "btnOpen":
                    (this.DataContext as MainViewModelNew).OpenNvpm();
                    break;
                case "btnSave":
                case "btnSaveAs":
                    bool bSaveAs = false;
                    if ((sender as Control).Name.Equals("btnSave"))
                         (this.DataContext as MainViewModelNew).SaveNvpm(bSaveAs);
                    else
                    {
                        bSaveAs = true;
                        (this.DataContext as MainViewModelNew).SaveNvpm(bSaveAs);

                    }
                    break;
                case "btnImport": //Import
                    {
                        DataImport importContents = new DataImport();
                        importContents.OnItemClickEvent += ImportContents_OnItemClickEvent;
                        //  _deleteContents.Content = importContents;
                        SfDockingManager.SetHeader(importContents, "Import");

                        SfDockingManager.SetDockState(importContents, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(importContents, new Rect(600, 0, 500, 500));

                        docking.DockItems.Add(importContents);

                        docking.ActivateWindow(importContents);
                         _deleteContents = importContents;
                    }
                    break;
                case "btnMerge":
                    DataImport mergeContents = new DataImport();
                    mergeContents.OnItemClickEvent += MergeContents_OnItemClickEvent;
                    //  _deleteContents.Content = importContents;

                    SfDockingManager.SetHeader(mergeContents, "Merge");
#if SAFETY
                    SfDockingManager.SetHeader(mergeContents, "Import");
#endif
                    SfDockingManager.SetDockState(mergeContents, DockState.Float);
                    SfDockingManager.SetFloatingWindowRect(mergeContents, new Rect(600, 0, 500, 500));

                    docking.DockItems.Add(mergeContents);

                    docking.ActivateWindow(mergeContents);
                    _deleteContents = mergeContents;
                    break;
 				case "btnCube":
                    OnCreatePrimitiveEvent?.Invoke(101, EventArgs.Empty);
                    break;
                case "btnSphere":
                    OnCreatePrimitiveEvent?.Invoke(102, EventArgs.Empty);
                    break;
                case "btnCapsule":
                    OnCreatePrimitiveEvent?.Invoke(103, EventArgs.Empty);
                    break;
                case "btnCylinder":
                    OnCreatePrimitiveEvent?.Invoke(104, EventArgs.Empty);
                    break;
				case "btnExport":
                    {
                        DataExport exportContents = new DataExport();
                        exportContents.OnItemClickEvent += ExportContents_OnItemClickEvent;
                        SfDockingManager.SetHeader(exportContents, "Export");

                        SfDockingManager.SetDockState(exportContents, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(exportContents, new Rect(600, 0, 500, 500));

                        docking.DockItems.Add(exportContents);

                        docking.ActivateWindow(exportContents);
                        _deleteContents = exportContents;
                    }
                    break;
                case "btnToolsLibrary": //치/공구 라이브러리
                    {
                        LibraryView libContents = new LibraryView();
                        libContents.Initialize(LibraryMode.TOOL);

                        libContents.OnItemClickEvent += LibContents_OnItemClickEvent;
                        SfDockingManager.SetHeader(libContents, "치/공구라이브러리");

                        SfDockingManager.SetDockState(libContents, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(libContents, new Rect(300, 0, 1000, 600));

                        docking.DockItems.Add(libContents);

                        docking.ActivateWindow(libContents);
                        _deleteContents = libContents;
                    }
                    break;
                case "btnEnvirLibrary": //가상환경 라이브러리
                    {
                        LibraryView libContents = new LibraryView();
                        libContents.Initialize(LibraryMode.ENVIRONEMT);

                        libContents.OnEnviromentClickEvent += LibContents_OnEnviromentClickEvent;
                      //  libContents.OnItemClickEvent += LibContents_OnItemClickEvent;
                        SfDockingManager.SetHeader(libContents, "가상환경 라이브러리");

                        SfDockingManager.SetDockState(libContents, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(libContents, new Rect(300, 0, 1000, 600));

                        docking.DockItems.Add(libContents);

                        docking.ActivateWindow(libContents);
                        _deleteContents = libContents;
                    }
                    break;
                case "btnHumanLibrary": //휴먼 라이브러리
                    {
                        LibraryView libContents = new LibraryView();
                        libContents.Initialize(LibraryMode.HUMAN);

                        libContents.OnHumanClickEvent += LibContents_OnHumanClickEvent;
                        SfDockingManager.SetHeader(libContents, "휴먼 라이브러리");

                        SfDockingManager.SetDockState(libContents, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(libContents, new Rect(300, 0, 1000, 600));

                        docking.DockItems.Add(libContents);

                        docking.ActivateWindow(libContents);
                        _deleteContents = libContents;
                    }
                    break;
                case "btnGBL":
                    {
                        Safety.GblControl gblView = new Safety.GblControl();

                        gblView.OnApplyEvent += GblView_OnApplyEvent;
                        gblView.OnCloseEvent += GblView_OnCloseEvent;
                        gblView.OnDeleteEvent += GblView_OnDeleteEvent;
                        SfDockingManager.SetHeader(gblView, "GBL링크 창");

                        SfDockingManager.SetDockState(gblView, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(gblView, new Rect(300, 0, 600, 800));

                        docking.DockItems.Add(gblView);
                        docking.ActivateWindow(gblView);
                    }
                    break;
                case "btnHazard":
                    {

                        Safety.HazardLogDlg hazardView = new HazardLogDlg();
                        SfDockingManager.SetHeader(hazardView, "HAZARD LOG 창");

                        SfDockingManager.SetDockState(hazardView, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(hazardView, new Rect(300, 0, 1200, 800));

                        docking.DockItems.Add(hazardView);
                        docking.ActivateWindow(hazardView);
                    }
                    break;
                case "btnContents":
                    {

                        ADMS.Dialog.ContentsDB contentsView = new ContentsDB();
                        contentsView.OnCloseEvent += ContentsView_OnCloseEvent;
                        contentsView.ObjExplore = ObjectExplorer;

                        SfDockingManager.SetHeader(contentsView, "콘텐츠 DB등록 창");

                        SfDockingManager.SetDockState(contentsView, DockState.Float);
                        SfDockingManager.SetFloatingWindowRect(contentsView, new Rect(300, 0, 800, 800));

                        docking.DockItems.Add(contentsView);
                        docking.ActivateWindow(contentsView);
                    }
                    break;
                    

            }
            //_deleteContents = new DataImport();

         
        }

        /// <summary>
        /// 속성메뉴 클릭 이벤트...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PnlUnity3d_OnToolPropertyClick(object sender, EventArgs e)
        {
            if ((bool)(sender as AppBarToggleButton).IsChecked)
            {
                SfDockingManager.SetDockState(InforMation, DockState.Dock);
            }
            else
            {
                SfDockingManager.SetDockState(InforMation, DockState.Hidden);
            }
        }
        
        /// <summary>
        /// Import/Merge...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportContents_OnItemClickEvent(object sender, EventArgs e)
        {
           bool ret =await (this.DataContext as MainViewModelNew).Import(sender as DataImportModel,false);
            if (ret)
            {
                SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
                _deleteContents = null;
            }
               
        }
        private async void MergeContents_OnItemClickEvent(object sender, EventArgs e)
        {
            bool ret = await (this.DataContext as MainViewModelNew).Import(sender as DataImportModel,true);
            if (ret)
            {
                SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
                _deleteContents = null;
            }

        }

        /// <summary>
        /// 라이브러리(치/공구및 환경등등) 선택이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibContents_OnItemClickEvent(object sender, EventArgs e)
        {
            (this.DataContext as MainViewModelNew).LibraryImport(sender as LibraryDataModel);

            SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
            _deleteContents = null;
        }

        /// <summary>
        /// 환경 setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibContents_OnEnviromentClickEvent(object sender, EventArgs e)
        {
            (this.DataContext as MainViewModelNew).SetEnvironmentInx((int)sender);

            SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
            _deleteContents = null;
        }

        /// <summary>
        /// 휴먼 setting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LibContents_OnHumanClickEvent(object sender, EventArgs e)
        {
            (this.DataContext as MainViewModelNew).SetHumanInx((int)sender);

            SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
            _deleteContents = null;
        }

        /// <summary>
        /// Export수행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExportContents_OnItemClickEvent(object sender, EventArgs e)
        {
            (this.DataContext as MainViewModelNew).Export(sender as DataExportModel, false);
            
            SfDockingManager.SetDockState(_deleteContents, DockState.Hidden);
            _deleteContents = null;

        }

        //private void _ribbon_Loaded(object sender, RoutedEventArgs e)
        //{
        //    foreach (Button button in FindVisualChildrenOfType<Button>(_ribbon))
        //    {
        //        if (button.Name == "PART_BackStage")
        //        {
        //            button.Visibility = Visibility.Collapsed;
        //            break;
        //        }
        //    }
        //}

        public static IEnumerable<T> FindVisualChildrenOfType<T>(DependencyObject parent)
        where T : DependencyObject
        {
            List<T> foundChildren = new List<T>();
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T childType = child as T;
                if (childType == null)
                {
                    foreach (var other in FindVisualChildrenOfType<T>(child))
                        yield return other;
                }
                else
                {
                    yield return (T)child;
                }
            }
        }

        private void CoreWindow_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            if (IsCtrlKeyPressed() && args.VirtualKey == VirtualKey.Z)
            {
                OnUndoEvent.Invoke(null, null);
            }

            else if(IsCtrlKeyPressed() && args.VirtualKey == VirtualKey.Y)
            {
                OnRedoEvent.Invoke(null, null);
            }
                //switch (args.VirtualKey)
                //{
                //    case VirtualKey.Z:
                //        {
                //            OnUndoEvent.Invoke(null, null);
                //        }
                //        break;
                //    case VirtualKey.Y:
                //        {
                //            OnRedoEvent.Invoke(null, null);
                //        }
                //        break;
                //}
           // }
        }

        private static bool IsCtrlKeyPressed()
        {
            var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
            return (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
        

        /// <summary>
        ///신규절차입력 창 Open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scenario_OnProcedureInputEvent(object sender, EventArgs e)
        {
            ProcedureInputView procedureInputView = new ProcedureInputView();
            SfDockingManager.SetHeader(procedureInputView, "절차입력 창");

            procedureInputView.ObjExplore = this.ObjectExplorer;
            procedureInputView.UnityPlayer = (this.DataContext as MainViewModelNew).m3DPlayer;

           // procedureInputView.OnProcedureInputEvent -= ProcedureInput_OnProcedureInputEvent;
            procedureInputView.OnProcedureInputEvent += ProcedureInput_OnProcedureInputEvent;

           // procedureInputView.OnCloseEvent -= ProcedureInput_OnCloseEvent;
            procedureInputView.OnCloseEvent += ProcedureInput_OnCloseEvent;

            //var bounds = Window.Current.Bounds;
            //double height = bounds.Height;
            //Debug.WriteLine(height);
            //double width = bounds.Width;
            SfDockingManager.SetDockState(procedureInputView, DockState.Dock);
            SfDockingManager.SetTargetNameInDockedMode(procedureInputView, "Viewer");
            SfDockingManager.SetSideInDockedMode(procedureInputView, Dock.Right);
            SfDockingManager.SetDesiredWidthInDockedMode(procedureInputView, 350);
            // SfDockingManager.SetFloatingWindowRect(procedureInputView, new Rect(width - 500, -100, 450, 900));

            docking.DockItems.Add(procedureInputView);
            docking.ActivateWindow(procedureInputView);

            //procedureInputView.Initialize();
            //ShowNewProcedureInputPanel(); //수동절차입력창 팝업
        }

        /// <summary>
        /// 절차편집창 Open
        /// </summary>
        /// <param name="sender">ProcedureModel</param>
        /// <param name="e"></param>
        private void Scenario_OnProcedureEditEvent(object sender, EventArgs e)
        {
            ProcedureInputView procedureInputView = new ProcedureInputView();
            SfDockingManager.SetHeader(procedureInputView, "절차편집 창");

            procedureInputView.ObjExplore = this.ObjectExplorer;
            procedureInputView.UnityPlayer = (this.DataContext as MainViewModelNew).m3DPlayer;


            procedureInputView.SetProcedureInfo = sender as ProcedureModel;

          //  procedureInputView.OnProcedureEditEvent -= ProcedureInputView_OnProcedureEditEvent;
            procedureInputView.OnProcedureEditEvent += ProcedureInputView_OnProcedureEditEvent;

           // procedureInputView.OnCloseEvent -= ProcedureInput_OnCloseEvent;
            procedureInputView.OnCloseEvent += ProcedureInput_OnCloseEvent;


            SfDockingManager.SetDockState(procedureInputView, DockState.Dock);
            SfDockingManager.SetTargetNameInDockedMode(procedureInputView, "Viewer");
            SfDockingManager.SetSideInDockedMode(procedureInputView, Dock.Right);
            SfDockingManager.SetDesiredWidthInDockedMode(procedureInputView, 350);

            //SfDockingManager.SetDockState(procedureInputView, DockState.Float);
            //SfDockingManager.SetFloatingWindowRect(procedureInputView, new Rect(800, 0, 450, 900));
         

            docking.DockItems.Add(procedureInputView);
            docking.ActivateWindow(procedureInputView);
        }

        /// <summary>
        /// docking창의 위 X표를 눌렀을때 Dispose하기 위한 방법
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Docking_DockStateChanged(object sender, DockStateChangedEventArgs args)
        {
           if(args.NewState == DockState.Hidden)
            {
                if(args.TargetElement.GetType().Equals(typeof(ProcedureInputView)))
                {
                    ProcedureInputView view = args.TargetElement as ProcedureInputView;
                    docking.Children.Remove(view);
                    view.Dispose();
                    view = null;
                }
                else if(args.TargetElement.GetType().Equals(typeof(Safety.GblControl)))
                {
                    Safety.GblControl view = args.TargetElement as Safety.GblControl;
                    docking.Children.Remove(view);
                    view.Dispose();
                    view = null;
                }
            }
        }

        /// <summary>
        /// 편집완료
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcedureInputView_OnProcedureEditEvent(object sender, EventArgs e)
        {
            Scenario.ProcedureEdit(sender as ProcedureModel);
            HideProcedureInputPanel();
        }

#region Title bar 관련
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            //UpdateTitleBarLayout(sender);
            var bar = sender as CoreApplicationViewTitleBar;
            LeftPanel.Margin = new Thickness(0, 0, bar.SystemOverlayLeftInset, 0);
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            //if (sender.IsVisible)
            //{
            //    AppTitleBar.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    AppTitleBar.Visibility = Visibility.Collapsed;
            //}
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            //LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            //RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            //AppTitleBar.Height = coreTitleBar.Height;
        }
#endregion

  
        /// <summary>
        /// 시나리오창 Show/Hide
        /// </summary>
        public void ShowPnlScenarioPanel()
        {
            SfDockingManager.SetDockState(Scenario, DockState.Dock);

            Scenario.ClearAll();
            (Scenario.DataContext as ScenarioViewModel).GetScenario();
        }

        private void Viewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Debug.WriteLine("Viewer Width And Height =" + e.NewSize.Width + "," + e.NewSize.Height);
            this.pnlUnity3d.Width = e.NewSize.Width;
            this.pnlUnity3d.Height = e.NewSize.Height;

        }

        /// <summary>
        /// 부품링크 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PartLinkerCtrl_OnApplyEvent(object sender, RoutedEventArgs e)
        {
            ListViewElement item = sender as ListViewElement;

            /***********************************************
             * TaskDataManager에 저장한다.
             */
            this.ObjectExplorer.SetPartLinkerApply(item);
            //SfDockingManager.SetDockState(PartLinker, DockState.Dock);
            //SfDockingManager.SetDockState(PartLinker, DockState.Hidden);
        }

        /// <summary>
        /// 그냥닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PartLinkerCtrl_OnCloseEvent(object sender, RoutedEventArgs e)
        {
            //SfDockingManager.SetDockState(PartLinker, DockState.Dock);
            //SfDockingManager.SetDockState(PartLinker, DockState.Hidden);

        }

        /// <summary>
        /// 부품링크창 Show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnPartLinkerEvent(object sender, RoutedEventArgs e)
        {

            Safety.GblControl gblView = new Safety.GblControl();

            gblView.OnApplyEvent += GblView_OnApplyEvent;
            gblView.OnCloseEvent += GblView_OnCloseEvent;
            gblView.OnDeleteEvent += GblView_OnDeleteEvent;
            SfDockingManager.SetHeader(gblView, "GBL링크 창");

            SfDockingManager.SetDockState(gblView, DockState.Float);
            SfDockingManager.SetFloatingWindowRect(gblView, new Rect(300, 0, 600, 800));

            docking.DockItems.Add(gblView);
            docking.ActivateWindow(gblView);
        }

      

        /// <summary>
        /// GBL링크 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GblView_OnApplyEvent(object sender, RoutedEventArgs e)
        {
            Safety.GblTreeModel item = sender as Safety.GblTreeModel;
           
            this.ObjectExplorer.SetPartLinkerApplyNew(item);
        }

        /// <summary>
        /// GBL링크창 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GblView_OnCloseEvent(object sender, RoutedEventArgs e)
        {
            foreach (Safety.GblControl view in FindVisualChildrenOfType<Safety.GblControl>(this.docking))
            {
                SfDockingManager.SetDockState(view, DockState.Hidden);
                //tmp = view;
                break;
            }
        }

        /// <summary>
        /// 콘텐츠 링크 창 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsView_OnCloseEvent(object sender, EventArgs e)
        {
            foreach (Dialog.ContentsDB view in FindVisualChildrenOfType<Dialog.ContentsDB>(this.docking))
            {
                SfDockingManager.SetDockState(view, DockState.Hidden);
                //tmp = view;
                break;
            }
        }



        /// <summary>
        /// 링크해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GblView_OnDeleteEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.SetPartLinkerApplyNew(null);
        }

        /// <summary>
        /// 케이블편집 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurveEditorCtrl_OnCloseEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.CurveEditMode, null, sender);
            //SfDockingManager.SetDockState(CurveEditor, DockState.Dock);
            //SfDockingManager.SetDockState(CurveEditor, DockState.Hidden);
        }

        /// <summary>
        /// 케이블편집모드 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurveEditorCtrl_OnEditModeEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.CurveEditMode, null, sender);
        }

        /// <summary>
        /// 케이블속성 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurveEditorCtrl_OnValueChangeEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.CurveValueChange, null, sender);
        }

        /// <summary>
        /// 케이블색상버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurveEditorCtrl_OnColorButtonEvent(object sender, RoutedEventArgs e)
        {
            this.ColorPaletteCtrl.CurrentColor = this.CurveEditorCtrl.CurrentItem.color;
            this.ColorPaletteCtrl.OnApplyEvent += ColorPaletteCtrl_OnCurveEditorApplyEvent;
            Rect rect = SfDockingManager.GetFloatingWindowRect(CurveEditor);
            SfDockingManager.SetFloatingWindowRect(ColorPalette, new Rect(rect.X + rect.Width, rect.Y, 340, 600));
            SfDockingManager.SetDockState(ColorPalette, DockState.Float);
        }

        /// <summary>
        /// 케이블편집창 Show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnCurveEditorEvent(object sender, RoutedEventArgs e)
        {
            this.CurveEditorCtrl.CurrentItem = null;
            this.CurveEditorCtrl.CurrentItem = sender as CurveElement;
            this.CurveEditorCtrl.StartEditMode();
            SfDockingManager.SetDockState(CurveEditor, DockState.Float);
        }

        private void ObjectExplorer_OnCurveEdifotDockingFrom3dEvent(object sender, RoutedEventArgs e)
        {
            this.CurveEditorCtrl.DockingChangeInUI(sender as string);
        }

        /// <summary>
        /// 노드선택으로 케이블 도킹을 시작한 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnCurveEditorNodeSelectionEvent(object sender, EventArgs e)
        {
            this.CurveEditorCtrl.DockingChangeIn3D(sender as string);
        }

        /// <summary>
        /// 배관편집창 Show
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnPipeEditorEvent(object sender, RoutedEventArgs e)
        {
            this.PipeEditorCtrl.CurrentItem = null;
            this.PipeEditorCtrl.CurrentItem = sender as PipeElement;
            SfDockingManager.SetDockState(PipeEditor, DockState.Float);
        }

        private void ObjectExplorer_OnPipeEdifotDockingFrom3dEvent(object sender, RoutedEventArgs e)
        {
            this.PipeEditorCtrl.DockingChangeInUI(sender as string);
        }

        /// <summary>
        /// 노드선택으로 배관 도킹을 시작한 경우
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnPipeEditorNodeSelectionEvent(object sender, EventArgs e)
        {
            this.PipeEditorCtrl.DockingChangeIn3D(sender as string);
        }

        /// <summary>
        /// 배관편집 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeEditorCtrl_OnCloseEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.PipeEditMode, null, sender);
        }

        /// <summary>
        /// 배관편집모드 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeEditorCtrl_OnEditModeEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.PipeEditMode, null, sender);
        }

        /// <summary>
        /// 배관속성 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeEditorCtrl_OnValueChangeEvent(object sender, RoutedEventArgs e)
        {
            this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.PipeValueChange, null, sender);
        }

        /// <summary>
        /// 배관색상버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PipeEditorCtrl_OnColorButtonEvent(object sender, RoutedEventArgs e)
        {
            this.ColorPaletteCtrl.CurrentColor = this.PipeEditorCtrl.CurrentItem.color;
            this.ColorPaletteCtrl.OnApplyEvent += ColorPaletteCtrl_OnPipeEditorApplyEvent;
            Rect rect = SfDockingManager.GetFloatingWindowRect(PipeEditor);
            SfDockingManager.SetFloatingWindowRect(ColorPalette, new Rect(rect.X + rect.Width, rect.Y, 310, 230));
            SfDockingManager.SetDockState(ColorPalette, DockState.Float);
        }

        /// <summary>
        /// 색상선택 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPaletteCtrl_OnCurveEditorApplyEvent(object sender, RoutedEventArgs e)
        {
            byte[] t = sender as byte[];
            this.CurveEditorCtrl.ColorChange(Color.FromArgb(t[0], t[1], t[2], t[3]));
            this.ColorPaletteCtrl.OnApplyEvent -= ColorPaletteCtrl_OnCurveEditorApplyEvent;
            SfDockingManager.SetDockState(ColorPalette, DockState.Dock);
            SfDockingManager.SetDockState(ColorPalette, DockState.Hidden);
        }

        /// <summary>
        /// 색상선택 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPaletteCtrl_OnPipeEditorApplyEvent(object sender, RoutedEventArgs e)
        {
            byte[] t = sender as byte[];
            this.PipeEditorCtrl.ColorChange(Color.FromArgb(t[0], t[1], t[2], t[3]));
            this.ColorPaletteCtrl.OnApplyEvent -= ColorPaletteCtrl_OnPipeEditorApplyEvent;
            SfDockingManager.SetDockState(ColorPalette, DockState.Dock);
            SfDockingManager.SetDockState(ColorPalette, DockState.Hidden);
        }

        /// <summary>
        /// 색상선택 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPaletteCtrl_OnCloseEvent(object sender, RoutedEventArgs e)
        {
            this.ColorPaletteCtrl.OnApplyEvent -= ColorPaletteCtrl_OnCurveEditorApplyEvent;
            this.ColorPaletteCtrl.OnApplyEvent -= ColorPaletteCtrl_OnInfoViewApplyEvent;
            SfDockingManager.SetDockState(ColorPalette, DockState.Dock);
            SfDockingManager.SetDockState(ColorPalette, DockState.Hidden);
        }

        /// <summary>
        /// 속성정보 새로고침 (트리클릭, 월드/로컬 체크박스 클릭)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnSelectedInfoEvent(object sender, EventArgs e)
        {
            string objID = sender as string;
            bool worldPos = infoView.WorldPosition;
            bool worldRot = infoView.WorldRotation;

            object[] condition = new object[3] { objID, worldPos, worldRot };
            object selectedInfo = ObjectExplorer.UnityPlayer.RequestToUnity
                (SendMessage.Get, "SelectedInfo", condition);

            infoView.SelectedID = objID;
            infoView.SelectedInfo = selectedInfo as object[];
        }

        /// <summary>
        /// 속성정보 새로고침 (3D기즈모 드래그)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObjectExplorer_OnRefreshSelectedInfoEvent(object sender, EventArgs e)
        {
            string objID = infoView.SelectedID;
            bool worldPos = infoView.WorldPosition;
            bool worldRot = infoView.WorldRotation;

            object[] condition = new object[3] { objID, worldPos, worldRot };
            object selectedInfo = ObjectExplorer.UnityPlayer.RequestToUnity
                (SendMessage.Get, "SelectedInfo", condition);

            infoView.SelectedInfo = selectedInfo as object[];
        }

        private void InfoView_OnTransformEvent(object sender, EventArgs e)
        {
            ObjectExplorer.UnityPlayer.SendToUnity
                (SendMessage.SetTransform, infoView.SelectedID, sender);
        }

        private void InfoView_OnColorChangeEvent(object sender, EventArgs e)
        {
            ObjectExplorer.UnityPlayer.SendToUnity
                (SendMessage.SetColor, infoView.SelectedID, sender);
        }

        private void InfoView_OnColorPaletteEvent(object sender, EventArgs e)
        {
            byte[] t = sender as byte[];
            this.ColorPaletteCtrl.CurrentColor = Color.FromArgb(t[0], t[1], t[2], t[3]);
            this.ColorPaletteCtrl.OnApplyEvent += ColorPaletteCtrl_OnInfoViewApplyEvent;
            SfDockingManager.SetFloatingWindowRect(ColorPalette, new Rect(300, 0, 340, 600));
            SfDockingManager.SetDockState(ColorPalette, DockState.Float);
        }

        private void ObjectExplorer_OnAlignEditorEvent(object sender, EventArgs e)
        {
            this.AlignEditorCtrl.Target = sender as string;
            this.AlignEditorCtrl.Reference = string.Empty;
            SfDockingManager.SetDockState(AlignEditor, DockState.Float);
        }

        private void ObjectExplorer_OnAlignReferenceInfoFrom3dEvent(object sender, EventArgs e)
        {
            this.AlignEditorCtrl.Reference = sender as string;
        }

        private void AlignEditorCtrl_OnApplyEvent(object sender, EventArgs e)
        {
            if (ObjectExplorer.UnityPlayer != null)
                this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.Align, 0, sender);
        }

        private void AlignEditorCtrl_OnEditModeEvent(object sender, EventArgs e)
        {
            if (ObjectExplorer.UnityPlayer != null)
                this.ObjectExplorer.UnityPlayer.SendToUnity(IVR.UWP.Unity3D.SendMessage.Align, 1, sender);
        }

        /// <summary>
        /// 색상선택 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorPaletteCtrl_OnInfoViewApplyEvent(object sender, RoutedEventArgs e)
        {
            byte[] t = sender as byte[];
            this.infoView.ColorChangedByHere(Color.FromArgb(t[0], t[1], t[2], t[3]));
            this.ColorPaletteCtrl.OnApplyEvent -= ColorPaletteCtrl_OnInfoViewApplyEvent;
            SfDockingManager.SetDockState(ColorPalette, DockState.Dock);
            SfDockingManager.SetDockState(ColorPalette, DockState.Hidden);
        }
        /// <summary>
        /// 메뉴선택
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MMenu_OnMenuClickEvent(object sender, EventArgs e)
        {
            string menuName = sender as string;
            switch (menuName)
            {
                case "3D 구성품":
                    {
                        //Button_Click_1(null, new RoutedEventArgs());
                    }
                    break;
                case "시나리오창":
                    {
                        SfDockingManager.SetDockState(Scenario, DockState.Dock);
                    }
                    break;
                case "속성창":
                    {
                        SfDockingManager.SetDockState(InforMation, DockState.Dock);
                    }
                    break;
            }
            //  throw new NotImplementedException();
        }

       
        /// <summary>
        /// 절차생성및 편집창 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcedureInput_OnCloseEvent(object sender, EventArgs e)
        {
            HideProcedureInputPanel();
        }

        /// <summary>
        /// 수동절차 팝업창 Hide
        /// </summary>
        public void HideProcedureInputPanel()
        {
            foreach (ProcedureInputView view in FindVisualChildrenOfType<ProcedureInputView>(this.docking))
            {
                SfDockingManager.SetDockState(view, DockState.Hidden);
                break;
            }

            //tmp.Dispose();
            //tmp = null;
        }

        /// <summary>
        /// 절차입력 이벤트(from 수동절차창)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcedureInput_OnProcedureInputEvent(object sender, EventArgs e)
        {
            Scenario.ProcedureInput(sender as ProcedureModel);
        }

#region 퀵매뉴
        private void btnQuick_Click(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModelNew).QuickMenuCommand((sender as Control).Name);
        }

#endregion
    }
}

