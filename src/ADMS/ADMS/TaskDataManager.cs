using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;

using System.Linq;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;

using ADMS.Dialog;

namespace ADMS
{
    /// <summary>
    /// VPM DataSet 담당 클래스
    /// </summary>
    public enum EditStateMode
    {
        Edited = 0, Closed = 1
    };

    public class TaskDataManager
    {
        private static float ver;

        /// <summary>
        /// VPM 버전정보
        /// </summary>
        public static float Ver
        {
            get { return TaskDataManager.ver; }
            set { TaskDataManager.ver = value; }
        }
       // private static string selectedSer_cd;
        private static DataSet m_TaskDataSet;   //메모리DB 정보
       // private static DataTable task_std_time;
        static EditStateMode m_editState = EditStateMode.Closed;
        private static string m_vpmPath;

        /// <summary>
        /// 실행경로 저장
        /// </summary>
        private static string currentDirectory;

        public static string CurrentDirectory
        {
            get { return TaskDataManager.currentDirectory; }
            set { TaskDataManager.currentDirectory = value; }
        }

        public struct IVRQuternion
        {
            public float x; public float y; public float z; public float w;
        }

        #region "Property"
        //public static DataTable TaskStdTimeDataTable
        //{
        //    get
        //    {
        //        return task_std_time;
        //    }
        //}

        public static DataSet TaskDataSet
        {
            get {
                return m_TaskDataSet;
            }
        }
        public static string VpmPath
        {
            get
            {
                return m_vpmPath;
            }
            set {
                m_vpmPath = value;
            }
        }
    
        // 편집중인지 판단하는 속성
        public static EditStateMode EditState
        {
            get
            {
                if (m_TaskDataSet != null)
                {
                    foreach (DataTable tmpDt in m_TaskDataSet.Tables)
                    {
                        if (tmpDt.GetChanges() != null && tmpDt.GetChanges().Rows.Count > 0)
                        {
                            m_editState = EditStateMode.Edited;
                            break;
                        }
                    }
                    return m_editState;
                }
                else return m_editState;
            }
            set
            {
                m_editState = value;
            }
        }
        #endregion 

        //public static string getStdTimeName(string item_code)
        //{
        //    DataRow[] selRow = task_std_time.Select("ITEM_CODE='" + item_code + "'");
        //    if (selRow.Length > 0)
        //    {
        //        return selRow[0]["ITEM_NM"].ToString();
        //    }
        //    return null;
        //}

        public static void NewMakeDataSet()
        {
            m_TaskDataSet = new DataSet();
            MakeTaskDataTable(m_TaskDataSet);
        }

        public static void OpenDataSetFile(DataSet fileDS)
        {

            //System.Diagnostics.Debug.Assert(ver > 1.0f, "1.0 Version은 지원하지 않습니다. 관리자에게 문의하세요");
            if(m_TaskDataSet != null) m_TaskDataSet.Clear();
            if (ver == 1.0f)
            {
               // MessageBox.Show("1.0 Version은 지원하지 않습니다. 관리자에게 문의하세요");
                return;
                //SetHapticModeModify(ref fileDS);
                //m_TaskDataSet = fileDS;
            }

            m_TaskDataSet = fileDS;
            
            //if (ver < Convert.ToSingle(AppInstance.Ins.VpmVersion) || ver==4.1f)
            //{

            //    MakeCurrentVPMVer();
            //}
        }

        /// <summary>
        /// 신규 테이블 생성
        /// </summary>
        /// <param name="taskDataSet"></param>
        public static void MakeTaskDataTable(DataSet taskDataSet)
        {

            DataTable taskDt = new DataTable("TASK");
            taskDt.Columns.Add("SER_CD", typeof(Int32));   // 시나리오ID
            taskDt.Columns.Add("OBJECT_ID", typeof(String));    // 대상 OBJECT_ID
            taskDt.Columns.Add("SER_NM", typeof(String));  // 시나리오명
            taskDt.Columns.Add("ORDER", typeof(Int32));          // 순서
            //taskDt.Columns.Add("DUR_TIME", typeof(String)); // 수행시간
            //taskDt.Columns.Add("WORK_NUM", typeof(String)); // 소요인원
            //taskDt.Columns.Add("PER_TIME", typeof(String)); // 정비주기
            //taskDt.Columns.Add("DESC_NM", typeof(String));  // 시나리오 설명
            //taskDt.Columns.Add("IMG_DATA", typeof(byte[]));  // 이미지
            //taskDt.Columns.Add("PART_LINK_YN", typeof(Boolean));  // 분해결합 콘텐츠 여부
            taskDataSet.Tables.Add(taskDt);

            //세부시나리오 항목
            DataTable subTaskDt = new DataTable("SUB_TASK");
            subTaskDt.Columns.Add("SER_CD", typeof(Int32));         // 시나리오 ID
            subTaskDt.Columns.Add("SUBSER_CD", typeof(Int32));      // 서브항목ID
            subTaskDt.Columns.Add("SUBSER_NM", typeof(String));     // 서브항목명
            //subTaskDt.Columns.Add("LINK", typeof(Boolean));         // 링크여부
           // subTaskDt.Columns.Add("ORDER", typeof(Int32));          // 순서
            //subTaskDt.Columns.Add("MOV_PATH", typeof(String));      // 동영상 링크 정보, 점검세부항목에서는 점검세부 ID로 사용됨(점검장비,점검대상,점검종류)

            //점검항목 프로그램을 위한 추가 항목
            subTaskDt.Columns.Add("ITEM_TYPE", typeof(Int32));      // 세부항목타입(0: 일반세부항목, 1:점검세부항목-해당점검성적서 출력)
           
            taskDataSet.Tables.Add(subTaskDt);

			//시나리오 링크항목
			DataTable subTaskLinkDt = new DataTable("SUB_TASK_LINK");
			subTaskLinkDt.Columns.Add("SER_CD", typeof(Int32));                      //현재 시나리오 ID
			subTaskLinkDt.Columns.Add("SER_LINK_CD", typeof(Int32));                 //링크대상 시나리오 ID
			subTaskLinkDt.Columns.Add("SUBSER_LINK_CD", typeof(Int32));              //링크대상 서브항목 ID
			subTaskLinkDt.Columns.Add("ORDER", typeof(Int32));                       // 순서
           // taskDataSet.Tables.Add(subTaskLinkDt);

            //점검항목 프로그램을 위한 추가 항목
            subTaskLinkDt.Columns.Add("LINK_ID", typeof(Int32));          //  링크 ID
            subTaskLinkDt.Columns.Add("PARENT_LINK_ID", typeof(Int32));   //  부모ID
            subTaskLinkDt.Columns.Add("PATH_TYPE", typeof(String));      // 항목타입 0:정상패스,1:고장패스,2:케이블고장패스

            taskDataSet.Tables.Add(subTaskLinkDt);

            DataTable task_ProcedureDt = new DataTable("TASK_PROCEDURE");
            task_ProcedureDt.Columns.Add("SER_CD", typeof(Int32));                      //시나리오 ID
            task_ProcedureDt.Columns.Add("SUBSER_CD", typeof(Int32));                   //서브항목 ID
            task_ProcedureDt.Columns.Add("PROCEMODE", typeof(Int32));                  //0:3d,1:2d,2:human            
            task_ProcedureDt.Columns.Add("WORK_ORDER", typeof(Int32));                  //순서
            task_ProcedureDt.Columns.Add("ISSUBPROCE", typeof(Boolean));                //서브절차 여부
            task_ProcedureDt.Columns.Add("OBJECT_NM", typeof(String));                  //MOVING 객체
            task_ProcedureDt.Columns.Add("OBJECT_ID", typeof(String));                  //MOVING 오브젝트 ID
            task_ProcedureDt.Columns.Add("SENTENCE", typeof(String));                   //절차단문
            task_ProcedureDt.Columns.Add("DEST_TRANSFORMATION", typeof(float[]));       //도착 변환 정보
            task_ProcedureDt.Columns.Add("MOV_OBJECT_ID", typeof(String));            //도착참조 OBJECT_ID
            task_ProcedureDt.Columns.Add("ROTATION_INFO", typeof(object[]));            //회전정보
            task_ProcedureDt.Columns.Add("ROT_OBJECT_ID", typeof(String));            //회전참조 OBJECT_ID
            task_ProcedureDt.Columns.Add("ISTRANSLATION", typeof(Boolean));             //Translation:true or ShowHide:false mode
            task_ProcedureDt.Columns.Add("TRANSMODE", typeof(Int32));                      // 진행수행방법 0:move 1: rotate 2: move&rotate
            task_ProcedureDt.Columns.Add("ISSHOW", typeof(Boolean));                    // Show:true or Hide:false
            task_ProcedureDt.Columns.Add("ISLINK", typeof(Boolean));                    //동시실행
            task_ProcedureDt.Columns.Add("TRAINING_TYPE", typeof(Int32));                    //훈련진행타입 0:자동 1:손 2:공구
            task_ProcedureDt.Columns.Add("DUR_TIME", typeof(String));                   // 수행시간
            task_ProcedureDt.Columns.Add("TRAINING_MODE", typeof(Int32));                 //훈련수행방법 0:free, 1:line, 3:rotate,4:self rotate, 5: click count 6: click & Time
            task_ProcedureDt.Columns.Add("CLICK_CNT", typeof(Int32));                   //클릭횟수
            task_ProcedureDt.Columns.Add("TOOL_OBJECT_ID", typeof(String));             //사용공구ID         
            task_ProcedureDt.Columns.Add("SOUNDFILE_DATA", typeof(Byte[]));             //사운드 실 데이터
            task_ProcedureDt.Columns.Add("SOUND_FILE", typeof(String));                  //사운드파일명
            //task_ProcedureDt.Columns.Add("MOVING_MODE", typeof(Int32));                 //2D 절차에서moving 객체 0:커서,1:(+)Probe,2:(+)Probe //Character에서는 idle index로 씀
            //task_ProcedureDt.Columns.Add("PROCE_MODE", typeof(Int32));                  //일반절차 OR 점검절차 0:일반절차,1:점검절차
            //task_ProcedureDt.Columns.Add("MULTITYPE", typeof(Int32));                   //점검절차 정상 멀티미디어타입
            //task_ProcedureDt.Columns.Add("MULTIMEDIA", typeof(String));                 //점검절차 정상 멀티미디어파일
            //task_ProcedureDt.Columns.Add("F_MULTITYPE", typeof(Int32));                 //점검절차 비정상 멀티미디어타입
            //task_ProcedureDt.Columns.Add("F_MULTIMEDIA", typeof(String));               //점검절차 비정상 멀티미디어파일
            //task_ProcedureDt.Columns.Add("ISANIMATION", typeof(Boolean));               // Character모드시 애니메이션 모드인지 자세 모드인지 구분 true:Animation, false:posture
            //task_ProcedureDt.Columns.Add("ANIMATION_MODE", typeof(Int32));              // Character모드시 애니메이션 인덱스
            //task_ProcedureDt.Columns.Add("POSTURE_DATA", typeof(float[,]));           //자세 Data
            //task_ProcedureDt.Columns.Add("POSTURE_DATA", typeof(List<IVRQuternion>)); //자세 Data(다차원 배열은 XMLSerialize가 안됌)
            task_ProcedureDt.Columns.Add("POSTURE_DATA", typeof(byte[]));               //자세 Data 
            task_ProcedureDt.Columns.Add("CURVEPATH", typeof(String));                  //경로 curve
            task_ProcedureDt.Columns.Add("ISBLINK", typeof(Boolean));                   //blink 효과 (made by dscho 180330) . 3D상에서 BLINK효과를 주고 수행토록 처리
            task_ProcedureDt.Columns.Add("IS_SILHOUETTE", typeof(Boolean));                 //실루엣 여부(도착지에 실루엣을 표현할지 여부)

            taskDataSet.Tables.Add(task_ProcedureDt);

            DataTable humanMotionDt = new DataTable("HUMAN_MOTION");
            humanMotionDt.Columns.Add("MOTION_ID", typeof(Int32));                      //모션 ID
            humanMotionDt.Columns.Add("MOTION_NM", typeof(String));                     //모션제목
            humanMotionDt.Columns.Add("WORK_ORDER", typeof(Int32));                     //순서
            humanMotionDt.Columns.Add("POSTURE_DATA", typeof(byte[]));                  //모션제목
            taskDataSet.Tables.Add(humanMotionDt);

            //DataTable partTaskLinkDt = new DataTable("PART_TASK_LINK");                 
            //partTaskLinkDt.Columns.Add("OBJECT_ID", typeof(String));                    //부품ID
            //         partTaskLinkDt.Columns.Add("SER_CD", typeof(Int32));                        //시나리오ID
            //partTaskLinkDt.Columns.Add("LINK_CD", typeof(Int32));                       //부품-시나리오 연결코드
            //partTaskLinkDt.Columns.Add("WORK_ORDER", typeof(Int32));                    //순번
            //taskDataSet.Tables.Add(partTaskLinkDt);

            //DataTable partInfoDt = new DataTable("PART_INFO");
            //partInfoDt.Columns.Add("OBJECT_ID", typeof(String));                        //부품ID
            //partInfoDt.Columns.Add("IMG_DATA", typeof(byte[]));                         // 이미지
            //taskDataSet.Tables.Add(partInfoDt);

            //object_id와 부품과의 관계(부품링크)
            DataTable relationDt = new DataTable("PART_RELATION");
            relationDt.Columns.Add("CAGE", typeof(String));
            relationDt.Columns.Add("PART_NO", typeof(String));
            relationDt.Columns.Add("OBJECT_ID", typeof(String));
            relationDt.Columns.Add("SEQ", typeof(Int32));
            //  relationDt.Columns.Add("EXPAND_SER_CD", typeof(Int32)); //분해 시나리오
            //  relationDt.Columns.Add("COLLAPSE_SER_CD", typeof(Int32)); //결합 시나리오
            taskDataSet.Tables.Add(relationDt);

            //object_id와 부품과의 관계(GBL링크) for SAFETY사업
            DataTable gblDt = new DataTable("GBL_RELATION");
            gblDt.Columns.Add("OBJECT_ID", typeof(String)); //객체 ID
            gblDt.Columns.Add("GBL_ND_ID", typeof(String)); //GBL ID
            gblDt.Columns.Add("GBL_NM", typeof(String));    //GBL 이름
            gblDt.Columns.Add("SEQ", typeof(Int32));
            taskDataSet.Tables.Add(gblDt);


            //object_id와 콘텐츠와의 관계 for SAFETY사업
            DataTable contentsDt = new DataTable("CONTENTS");
            contentsDt.Columns.Add("OBJECT_ID", typeof(String)); //객체 ID
            contentsDt.Columns.Add("SER_CD", typeof(Int32)); //시나리오 ID
            contentsDt.Columns.Add("MODE", typeof(Int32));    //모드= 0:운용, 1:정비, 2:점검, 3:훈련 
            contentsDt.Columns.Add("SEQ", typeof(Int32));
            taskDataSet.Tables.Add(contentsDt);

            m_editState = EditStateMode.Edited;
        }

        /// <summary>
        /// 세부항목별 해당 분해/조립 절차
        /// </summary>
        /// <param name="SER_CD"></param>
        /// <returns></returns>
        public static  ObservableCollection<ProcedureModel> GetProcedure(SubScenarioModel model)
        {
            DataTable tmpDt;
            ObservableCollection<ProcedureModel> procedureList = new ObservableCollection<ProcedureModel>();
            foreach (DataTable taskProcedrue in m_TaskDataSet.Tables)
            {
                if (taskProcedrue.TableName.Equals("TASK_PROCEDURE"))
                {
					//colCnt = 0;
					//foreach (DataColumn col in tmpDt.Columns)
					//{
					//	if (col.ColumnName.Equals("MOTION_DATA"))
					//		break;
					//	colCnt++;
					//}
					//tmpDt.Columns[colCnt].DataType = typeof(object[]); //자세정보라면 float[,]형식으로 바꾼다..

                    DataRow[] drArr = taskProcedrue.Select("SER_CD='" + model.SER_CD + "' AND SUBSER_CD='"+model.SUBSER_CD+"'");
                    if (drArr.Length > 0)
                    {

                        foreach (DataRow dr in drArr)
                        {
                            ProcedureModel newModel = new ProcedureModel();
                            newModel._sc_id = Convert.ToInt32(dr["SER_CD"]);
                            newModel._subScid = Convert.ToInt32(dr["SUBSER_CD"]);
                            newModel._isProceMode = Convert.ToInt32(dr["PROCEMODE"]);
                            newModel._workOrder = Convert.ToInt32(dr["WORK_ORDER"]);
                            newModel._isSubProc = Convert.ToBoolean(dr["ISSUBPROCE"]);
                            newModel.OBJECT_NM = dr["OBJECT_NM"].ToString();
                            newModel._object_id = dr["OBJECT_ID"].ToString();
                            newModel.SENTENCE = dr["SENTENCE"].ToString();
                            newModel._durTime = dr["DUR_TIME"].ToString();
                            newModel._destTransform = dr["DEST_TRANSFORMATION"] as float[];
                            newModel._destRotateInfo = dr["ROTATION_INFO"] as object[];
                            newModel._rotRefObjId = dr["ROT_OBJECT_ID"].ToString();
                            newModel._isTranslation = Convert.ToBoolean(dr["ISTRANSLATION"]);
                            newModel._transMode = Convert.ToInt32(dr["TRANSMODE"]);
                            newModel._isShow = Convert.ToBoolean(dr["ISSHOW"]);
                            newModel.IS_LINK = Convert.ToBoolean(dr["ISLINK"]);
                            newModel.TRAINING_TYPE = Convert.ToInt32(dr["TRAINING_TYPE"]);
                            newModel._trainingMode = Convert.ToInt32(dr["TRAINING_MODE"]);
                            newModel._clickCnt = Convert.ToInt32(dr["CLICK_CNT"]);
                            newModel._toolUseObjId = dr["TOOL_OBJECT_ID"].ToString();

                            if(!string.IsNullOrEmpty(dr["SOUND_FILE"].ToString()))
                            {
                                newModel.IS_SOUND = true;
                                newModel._soundData = dr["SOUNDFILE_DATA"] as byte[];
                                newModel._soundfile = dr["SOUND_FILE"].ToString();

                               
                            }
                            
                            newModel._isBlink = Convert.ToBoolean(dr["ISBLINK"]);

                            if(!newModel._object_id.Equals("CAMERA"))
                            {
                                if (newModel._isTranslation)
                                {
                                    switch (newModel._transMode)
                                    {
                                        case 0:
                                            newModel.TRANSTYPE = "M";
                                            break;
                                        case 1:
                                            newModel.TRANSTYPE = "R";
                                            break;
                                        case 2:
                                            newModel.TRANSTYPE = "A";
                                            break;
                                    }
                                }
                                else
                                {
                                    if (newModel._isShow)
                                        newModel.TRANSTYPE = "S";
                                    else newModel.TRANSTYPE = "H";
                                }
                            }
                            try
                            {
                                newModel._isSilhoutte = Convert.ToBoolean(dr["IS_SILHOUETTE"]);
                            }
                            catch
                            {
                                newModel._isSilhoutte = false;
                            }
                           

                            //   if ((int)dr["PROCE_MODE"] == 4) //캐릭터 모드
                            //   {
                            //       if (dr["ISANIMATION"] != null)
                            //           newRow["ISANIMATION"] = dr["ISANIMATION"];
                            //       if (dr["ANIMATION_MODE"] != null)
                            //           newRow["ANIMATION_MODE"] = dr["ANIMATION_MODE"];

                            //       //if (dr["POSTURE_DATA"] as List<IVRQuternion> != null)
                            //       //{
                            //       //    newRow["POSTURE_DATA"] = ConvertPostureListToFloatArray(dr["POSTURE_DATA"] as List<IVRQuternion>);
                            //       //}
                            //       if (dr["POSTURE_DATA"] as byte[] != null)
                            //       {
                            //           newRow["POSTURE_DATA"] = ConvertPostureByteArrayToFloatArray(dr["POSTURE_DATA"] as byte[]);
                            //       }
                            //       if (dr["CURVEPATH"] != null)
                            //           newRow["CURVEPATH"] = dr["CURVEPATH"];
                            //   }
                            //if (dr["MOTION_DATA"] as byte[] != null)
                            //{
                            //	newRow["MOTION_DATA"] = ConvertByteArrayToObjectArray(dr["MOTION_DATA"] as byte[]);
                            //}
                            procedureList.Add(newModel);
                        }
                    }
                    return procedureList;
                }                
            }
            throw new Exception("TASK_PROCEDURE NOT EXIST!! ");
            //return null;
        }


        #region "시나리오편집"

        /// <summary>
        /// 시나리오 등록
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static void  InsertTask(ScenarioModel args)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK"))
                {
                    DataRow newRow = tmpDt.NewRow();
                  //  selectedSer_cd = maxVal.ToString();
                    newRow["SER_CD"] = args.SER_CD;
                  //  newRow["OBJECT_ID"] = args.Object_id;
                    newRow["SER_NM"] = args.SER_NM;
                    newRow["ORDER"] = Convert.ToInt32(args.ORDER);
                    //newRow["WORK_NUM"] = string.Format("{0:000}", maxVal);
                    //newRow["PER_TIME"] = args.Per_time;
                    //newRow["DESC_NM"] = args.Desc_nm;
                    //newRow["IMG_DATA"] = args.Img_data;
                    //newRow["PART_LINK_YN"] = false;

                    tmpDt.Rows.Add(newRow);
                    tmpDt.AcceptChanges();
                  //  return selectedSer_cd;
                    //}
                }
            }
       //     return null;
        }
        /// <summary>
        /// 선택한 시나리오명 불러오기
        /// </summary>
        /// <param name="ser_cd">시나리오 ID</param>
        /// <returns>시나리오명</returns>
        public static string SearchTaskNm(string ser_cd)
		{
			string taskNm = "";
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("TASK"))
				{
					DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "'");
					if (drArr.Length > 0)
					{
						taskNm = drArr[0]["SER_NM"].ToString();
						return taskNm;
					}
				}
			}

			return taskNm;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ser_cd"></param>
		/// <returns></returns>
		public static DataTable SearchTaskLinkList(string ser_cd)
		{
			DataTable tmpDt = null;
			foreach (DataTable taskProcedrue in m_TaskDataSet.Tables)
			{
				if (taskProcedrue.TableName.Equals("TASK"))
				{
					tmpDt = taskProcedrue.Clone();
					DataRow[] drArr = taskProcedrue.Select("SER_CD <> '" + ser_cd + "'");
					if (drArr.Length > 0)
					{
						foreach (DataRow dr in drArr)
						{
							DataRow newRow = tmpDt.NewRow();
							newRow.ItemArray = dr.ItemArray;
							tmpDt.Rows.Add(newRow);
						}
					}
				}
			}
			return tmpDt;
		}

        ///// <summary>
        ///// 시나리오명 수정
        ///// </summary>
        ///// <param name="oldTaskNm"></param>
        ///// <param name="newTaskNm"></param>
        //public static void ModifyTask(string serCd, string newTaskNm, string newObjectNm)
        //{
        //    foreach (DataTable tmpDt in m_TaskDataSet.Tables)
        //    {
        //        if (tmpDt.TableName.Equals("TASK"))
        //        {
        //            //}
        //            DataRow[] drArr = tmpDt.Select("SER_CD='" + serCd + "'");
        //            foreach (DataRow dr in drArr)
        //            {
        //                dr["SER_NM"] = newTaskNm;
        //                dr["OBJECT_ID"] = newObjectNm;
        //            }
        //            tmpDt.AcceptChanges();
        //        }
        //    }
        //}
        /// <summary>
        /// 시나리오명 수정
        /// </summary>
        /// <param name="oldTaskNm"></param>
        /// <param name="newTaskNm"></param>
        public static void ModifyTask(int serCd, string newTaskNm)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK"))
                {
                    //}
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + serCd + "'");
                    foreach (DataRow dr in drArr)
                    {
                        dr["SER_NM"] = newTaskNm;
                        //dr["OBJECT_ID"] = newObjectNm;
                    }
                    tmpDt.AcceptChanges();
                    break;
                }
            }
        }
        /// <summary>
        /// 시나리오 수정
        /// </summary>
        /// <param name="oldTaskNm"></param>
        /// <param name="newTaskNm"></param>
        public static void ModifyTask(string serCd, bool partLinkYn)
		{
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("TASK"))
				{
					DataRow[] drArr = tmpDt.Select("SER_CD='" + serCd + "'");
					foreach (DataRow dr in drArr)
					{
						dr["PART_LINK_YN"] = partLinkYn;
					}
					tmpDt.AcceptChanges();
				}
			}
		}

        /// <summary>
        /// 시나리오 순서 수정
        /// </summary>
        /// <param name="oldTaskNm"></param>
        /// <param name="newTaskNm"></param>
        public static void ModifyTask(string serCd, string workOrder)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK"))
                {
                    //}
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + serCd + "'");
                    foreach (DataRow dr in drArr)
                    {
                        dr["WORK_NUM"] = string.Format("{0:000}", Int32.Parse(workOrder));
                       // dr["OBJECT_ID"] = newObjectNm;
                    }
                    tmpDt.AcceptChanges();
                }
            }
        }

        /// <summary>
        /// 시나리오 삭제
        /// </summary>
        /// <param name="taskNm"></param>
        public static void DeleteTask(int ser_cd)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                //먼저 삭제할 세부항목을 부모로 갖는 항목은 부모를 '0'으로 setting한다.
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    foreach (DataRow delDr in tmpDt.Select("SER_LINK_CD='" + ser_cd + "'"))
                    {
                        foreach (DataRow modDr in tmpDt.Select("SER_CD='" + delDr["SER_CD"].ToString() + "'and PARENT_LINK_ID='" + delDr["LINK_ID"].ToString() + "'"))
                        {
                            modDr["PARENT_LINK_ID"] = 0;
                        }
                    }

                    tmpDt.AcceptChanges();
                    break;
                }
            }
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
              
                if (tmpDt.TableName.Equals("TASK") || tmpDt.TableName.Equals("SUB_TASK") ||
                    tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();
                       
                    }
                }
                else if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_LINK_CD='" + ser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();

                    }
                }
            }
             //   if (tmpDt.TableName.Equals("TASK_TOOLUSING") || tmpDt.TableName.Equals("TASK_PROCEDURE"))
        }

        /// <summary>
        /// 시나리오 정보
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static DataTable GetScenario()
        {
            // DataTable tmpDt;
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK"))
                {
                    return tmpDt;
                }
            }
            throw new Exception("TASK NOT EXIST!! ");
            //return null;
        }

        #endregion

        #region "세부시나리오 관련"
        /// <summary>
        /// lstsubMaint item의 값으로 Insert한다.....
        ///   const int INX_SUBTASK_SUBSER_CD = 0;
        /// </summary>
        /// <param name="item"></param>
        public static void InsertSubTask(SubScenarioModel item)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK"))
                {
                    DataRow newRow = tmpDt.NewRow();

                    newRow["SER_CD"] = item.SER_CD;
                    newRow["SUBSER_CD"] = item.SUBSER_CD;
                    newRow["SUBSER_NM"] = item.SUBSER_NM;
                    newRow["ITEM_TYPE"] = item.ITEM_TYPE;
                    // newRow["LINK"] = false;
                    // newRow["ORDER"] = item.SubItems[3].Text;
                    //newRow["MOV_PATH"] = item.SubItems[4].Text;
                    //if (item.SubItems[5].Text.Equals("일반"))
                    //    newRow["ITEM_TYPE"] = 0;
                    //else newRow["ITEM_TYPE"] = 1;
                    tmpDt.Rows.Add(newRow);

                    tmpDt.AcceptChanges();
                    break;
                }

            }
        }

        /// <summary>
        /// subTaskLink Table을 리턴한다.
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static DataTable SearchSubTaskLinkList(string ser_cd)
        {
            DataTable subTaskDt = null;
            DataTable subTaskLinkDt = null;
            DataTable returnDt = null;

            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK"))
                {
                    subTaskDt = tmpDt;
                    break;
                }
            }

            if (subTaskDt.Rows.Count > 0)
            {
                returnDt = subTaskDt.Clone();
            }
            else
            {
                return null;
            }

            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    subTaskLinkDt = tmpDt.Clone();
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            DataRow newRow = subTaskLinkDt.NewRow();
                            newRow.ItemArray = dr.ItemArray;
                            subTaskLinkDt.Rows.Add(newRow);
                        }
                    }
                }
            }

            if (subTaskLinkDt.Rows.Count > 0)
            {
                for (int x = 0; x < subTaskLinkDt.Rows.Count; x++)
                {
                    DataRow[] drArr = subTaskDt.Select("SER_CD = '" + subTaskLinkDt.Rows[x]["SER_LINK_CD"] + "' AND SUBSER_CD ='" + subTaskLinkDt.Rows[x]["SUBSER_LINK_CD"] + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            DataRow newRow = returnDt.NewRow();
                            newRow["SER_CD"] = dr["SER_CD"];
                            newRow["SUBSER_CD"] = dr["SUBSER_CD"];
                            newRow["SUBSER_NM"] = dr["SUBSER_NM"];
                            newRow["LINK"] = dr["LINK"];
                            newRow["ORDER"] = Int32.Parse(subTaskLinkDt.Rows[x]["ORDER"].ToString());
                            newRow["MOV_PATH"] = dr["MOV_PATH"];
                            newRow["ITEM_TYPE"] = dr["ITEM_TYPE"];
                            returnDt.Rows.Add(newRow);
                        }
                    }
                }
            }
            else
            {

                DataRow[] drArr = subTaskDt.Select("SER_CD = '" + ser_cd + "'");
                if (drArr.Length > 0)
                {
                    foreach (DataRow dr in drArr)
                    {
                        DataRow newRow = returnDt.NewRow();
                        newRow.ItemArray = dr.ItemArray;
                        returnDt.Rows.Add(newRow);
                    }
                }
            }

            return returnDt;
        }
        /// <summary>
        /// 정비업무명 수정
        /// </summary>
        /// <param name="oldTaskNm"></param>
        /// <param name="newTaskNm"></param>
        public static void ModifySubTask(int mode,int ser_cd, int subser_cd, string value)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK"))
                {
                   DataRow [] dr = tmpDt.Select("SER_CD='"+ser_cd+"' AND SUBSER_CD='"+subser_cd+"'");
                    if(dr.Length == 0) break;
                    switch (mode)
                    { 
                        case 0:// 명칭수정
                            dr[0]["SUBSER_NM"] = value;
                            break;
                        case 1: //병렬
                            if(string.IsNullOrEmpty(value))
                            dr[0]["LINK"] = false;
                            else dr[0]["LINK"] = true;
                            break;
                        case 2: //순서
                            dr[0]["ORDER"] = Convert.ToInt32(value);
                            break;
                        case 3: //동영상 패스
                            dr[0]["MOV_PATH"] = value;
                            break;
                        case 100://점검
                            string[] strCol = value.Split(',');
                            string val = "";
                            for (int i = 0; i < 3; i++)
                                val += strCol[i] + ",";
                            val = val.Substring(0, val.Length - 1);
                            dr[0]["SUBSER_NM"] = val;

                            val = "";
                             for (int i = 3; i <6; i++)
                                val += strCol[i] + ",";
                             val = val.Substring(0, val.Length - 1);
                             dr[0]["MOV_PATH"] = val;
                            break;

                    
                    }
                  
                    tmpDt.AcceptChanges();
                    break;
                }

            }

        }

        /// <summary>
        /// 정비업무명 수정
        /// </summary>
        /// <param name="oldTaskNm"></param>
        /// <param name="newTaskNm"></param>
        public static void ModifySubTaskLink(int mode, int ser_cd, int link_id, int value)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    DataRow[] dr = tmpDt.Select("SER_CD='" + ser_cd + "' AND LINK_ID='" + link_id + "'");
                    if (dr.Length == 0) return;
                    switch (mode)
                    {
                        case 8:// 부모 ID
                            if (!value.Equals("0"))
                                dr[0]["PARENT_LINK_ID"] = value;
                            else
                                dr[0]["PARENT_LINK_ID"] = "0";
                            break;
                        //case 9: //경로
                        //    if (value.Equals("정상"))
                        //        dr[0]["PATH_TYPE"] = 0;
                        //    else if (value.Equals("비정상"))
                        //        dr[0]["PATH_TYPE"] = 1;
                        //    else
                        //        dr[0]["PATH_TYPE"] = 2;
                        //    break;


                    }

                    tmpDt.AcceptChanges();
                    break;
                }

            }
        }
        /// <summary>
        /// 세부항목 삭제
        /// </summary>
        /// <param name="taskNm"></param>
        public static void DeleteSubTask(int ser_cd, int subser_cd)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUBTASK") || tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "' and SUBSER_CD='"+subser_cd+"'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();
                    }
                }
                else if (tmpDt.TableName.Equals("SUB_TASK_LINK")) //해당 본항목을 다른시나리오에 링크된 세부항목을 삭제한다. 
                {
                    DataRow[] linkDrCol = tmpDt.Select("SER_LINK_CD='" + ser_cd + "' and SUBSER_LINK_CD='" + subser_cd + "'");

                    foreach (DataRow linkDr in linkDrCol)
                    {
                        DataRow[] convertDrCol =  tmpDt.Select("SER_CD='" + linkDr["SER_CD"].ToString() + "' and PARENT_LINK_ID='" + linkDr["LINK_ID"].ToString() + "'");
                        foreach (DataRow convertDr in convertDrCol)
                        {
                            convertDr["PARENT_LINK_ID"] = 0; //부모가 삭제되었기때문에 부모링크를 0으로 SETTING!!!
                        }
                    }
                    DataRow[] drArr = tmpDt.Select("SER_LINK_CD='" + ser_cd + "' and SUBSER_LINK_CD='" + subser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();
                    }
                }
            }
        }

		/// <summary>
		/// 세부시나리오링크 테이블 업데이트
		/// </summary>
		public static void UpdateSubTaskLink(int ser_cd, DataTable subTaskLinkDt)
		{
			//시나리오 링크항목
			DataTable subTaskLinktempDt = new DataTable("SUB_TASK_LINK");
			subTaskLinktempDt.Columns.Add("SER_CD", typeof(Int32));                      //현재 시나리오 ID
			subTaskLinktempDt.Columns.Add("SER_LINK_CD", typeof(Int32));                 //링크대상 시나리오 ID
			subTaskLinktempDt.Columns.Add("SUBSER_LINK_CD", typeof(Int32));              //링크대상 서브항목 ID
			subTaskLinktempDt.Columns.Add("ORDER", typeof(Int32));                       // 순서

			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
				{
					DataRow[] drArr = tmpDt.Select("SER_CD=" + ser_cd);
					if (drArr.Length > 0)
					{
						foreach (DataRow dr in drArr)
						{
							dr.Delete();
						}
						tmpDt.AcceptChanges();
					}

					for (int x = 0; x < subTaskLinkDt.Rows.Count; x++)
					{
						DataRow newRow = tmpDt.NewRow();
						newRow["SER_CD"] = ser_cd;
						newRow["SER_LINK_CD"] = subTaskLinkDt.Rows[x]["SER_LINK_CD"];
						newRow["SUBSER_LINK_CD"] = subTaskLinkDt.Rows[x]["SUBSER_LINK_CD"];
						newRow["ORDER"] = subTaskLinkDt.Rows[x]["ORDER"];
                        newRow["LINK_ID"] = subTaskLinkDt.Rows[x]["LINK_ID"];
                        newRow["PARENT_LINK_ID"] = subTaskLinkDt.Rows[x]["PARENT_LINK_ID"];
                        newRow["PATH_TYPE"] = subTaskLinkDt.Rows[x]["PATH_TYPE"];
						tmpDt.Rows.Add(newRow);
						
					}
					tmpDt.AcceptChanges();
					break;
				}
			}
		}

        /// <summary>
        /// 절차 삭제
        /// </summary>
        /// <param name="taskNm"></param>
        public static void DeleteTaskProcedure(ProcedureModel model)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if ( tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + model._sc_id + "' and SUBSER_CD='" + model._subScid + "' and WORK_ORDER="+model._workOrder);
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();
                    }
                }
            }
            //   if (tmpDt.TableName.Equals("TASK_TOOLUSING") || tmpDt.TableName.Equals("TASK_PROCEDURE"))
        }

        /// <summary>
        /// Get 세부항목
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static DataTable GetSubTask(int ser_cd)
        {
            DataTable subTaskDt = null;
            if (m_TaskDataSet == null) return subTaskDt;

            foreach (DataTable task in m_TaskDataSet.Tables)
            {
                if (task.TableName.Equals("SUB_TASK"))
                {
                    subTaskDt = task.Clone();
                    DataRow[] drSel = task.Select("SER_CD='" + ser_cd + "'");
                    if (drSel.Length == 0) return subTaskDt;

                    foreach (DataRow dr in drSel)
                    {
                        DataRow newR = subTaskDt.NewRow();
                        newR.ItemArray = dr.ItemArray;
                        subTaskDt.Rows.Add(newR);
                    }
                    return subTaskDt;
                }
               // else throw new Exception("TASK NOT EXIST!! ");
            }
            return subTaskDt;

        }

        /// <summary>
        /// Get 세부항목 MaxSubSer_CD
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static int GetSubTaskMaxID(string ser_cd)
        {

            foreach (DataTable task in m_TaskDataSet.Tables)
            {
                if (task.TableName.Equals("SUB_TASK"))
                {
                    DataRow[] drSel = task.Select("SER_CD='" + ser_cd + "'");
                    if (drSel.Length == 0) return 0;

                    int maxID = 0;
                    foreach (DataRow dr in drSel)
                    {
                      if(Convert.ToInt16(dr["SUBSER_CD"].ToString()) > maxID)
                          maxID = Convert.ToInt16(dr["SUBSER_CD"].ToString());
                    }
                    return maxID;
                }
                // else throw new Exception("TASK NOT EXIST!! ");
            }
            return 0;

        }


        /// <summary>
        /// Get 세부항목 MaxSubSer_CD
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static int GetSubTaskLinkMaxID(string ser_cd)
        {
            foreach (DataTable task in m_TaskDataSet.Tables)
            {
                if (task.TableName.Equals("SUB_TASK_LINK"))
                {
                    DataRow[] drSel = task.Select("SER_CD='" + ser_cd + "'");
                    if (drSel.Length == 0) return 0;

                    int maxID = 0;
                    foreach (DataRow dr in drSel)
                    {
                        if (Convert.ToInt16(dr["LINK_ID"].ToString()) > maxID)
                            maxID = Convert.ToInt16(dr["LINK_ID"].ToString());
                    }
                    return maxID;
                }
                // else throw new Exception("TASK NOT EXIST!! ");
            }
            return 0;

        }

        /// <summary>
        /// SubTask 리턴....
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSubTask()
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK"))
                {
                    return tmpDt;
                }
                //break;
            }
            return null;
        }
        /// <summary>
        /// SubTask 리턴....
        /// </summary>
        /// <returns></returns>
        public static DataTable GetSubTaskLink()
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    return tmpDt;
                }
                //break;
            }
            return null;
        }

        /// <summary>
        /// 해당 ser_cd를 갖는 subTaskLink테이블
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <returns></returns>
        public static DataTable GetSubTaskLink(int ser_cd)
        {
            //DataTable subTaskDt = null;
            DataTable subTaskLinkDt = null;
            //DataTable returnDt = null;

            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    subTaskLinkDt = tmpDt.Clone();
                    DataRow[] drArr = tmpDt.Select("SER_CD=" + ser_cd );
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            DataRow newRow = subTaskLinkDt.NewRow();
                            newRow.ItemArray = dr.ItemArray;
                            subTaskLinkDt.Rows.Add(newRow);
                        }
                    }
                }
            }
            return subTaskLinkDt;
        }

        /// <summary>
        /// 자식
        /// </summary>
        /// <param name="link_id"></param>
        /// <returns></returns>
        public static DataTable GetChildSubTaskLink(int link_id)
        {
            DataTable returnDt = null;
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    returnDt = tmpDt.Clone();
                   foreach(DataRow dr in tmpDt.Select("PARENT_LINK_ID="+link_id))
                    {
                      DataRow newR =   returnDt.NewRow();
                        newR.ItemArray = dr.ItemArray;

                        returnDt.Rows.Add(newR);
                    }
                    break;
                }
                //break;
            }

            return returnDt;
        }
       #endregion

        #region "세부시나리오별 절차 편집"


        /// <summary>
        /// TaskProcesure 리턴....
        /// </summary>
        /// <returns></returns>
        /// 수정요망
        public static DataTable GetTaskProcedure()
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataTable newDataTable = tmpDt.Clone();
           //         newDataTable.Columns["POSTURE_DATA"].DataType = typeof(float[,]);

                    foreach (DataRow r in tmpDt.Rows)
                    {
                        DataRow newRow = newDataTable.NewRow();
                        newRow["SER_CD"] = r["SER_CD"];
                        newRow["SUBSER_CD"] = r["SUBSER_CD"];
                        newRow["PROCEMODE"] = r["PROCEMODE"];
                        newRow["WORK_ORDER"] = r["WORK_ORDER"];
                        newRow["ISSUBPROCE"] = r["ISSUBPROCE"];		//서브절차추가

                        newRow["OBJECT_NM"] = r["OBJECT_NM"];
                        newRow["OBJECT_ID"] = r["OBJECT_ID"];
                        newRow["SENTENCE"] = r["SENTENCE"];

                        newRow["DEST_TRANSFORMATION"] = r["DEST_TRANSFORMATION"];
                        newRow["ROTATION_INFO"] = r["ROTATION_INFO"];
                        newRow["ISTRANSLATION"] = r["ISTRANSLATION"];

                        newRow["TRANSMODE"] = r["TRANSMODE"];
                        newRow["ISSHOW"] = r["ISSHOW"];
                        newRow["ISLINK"] = r["ISLINK"];
                        newRow["TRAINING_TYPE"] = r["TRAINING_TYPE"];

                        newRow["DUR_TIME"] = r["DUR_TIME"];
                        newRow["TRAINING_MODE"] = r["TRAINING_MODE"];
                        newRow["CLICK_CNT"] = r["CLICK_CNT"];

                        newRow["ISBLINK"] = r["ISBLINK"];       //
                        newRow["IS_SILHOUETTE"] = r["IS_SILHOUETTE"];

                        
                        newRow["TOOL_OBJECT_ID"] = r["TOOL_OBJECT_ID"];

                        newRow["SOUND_FILE"] = r["SOUND_FILE"];
                        newRow["SOUNDFILE_DATA"] = r["SOUNDFILE_DATA"];
                       
      //                  newRow["MOVING_MODE"] = r["MOVING_MODE"];
      //                  newRow["PROCE_MODE"] = r["PROCE_MODE"];
      //                  newRow["MULTITYPE"] = r["MULTITYPE"];
      //                  newRow["MULTIMEDIA"] = r["MULTIMEDIA"];
      //                  newRow["F_MULTITYPE"] = r["F_MULTITYPE"];
      //                  newRow["F_MULTIMEDIA"] = r["F_MULTIMEDIA"];

      //                  if ((int)r["PROCE_MODE"] == 4) //캐릭터 모드
      //                  {
      //                      newRow["ISANIMATION"] = r["ISANIMATION"];
      //                      newRow["ANIMATION_MODE"] = r["ANIMATION_MODE"];
      //                      newRow["CURVEPATH"] = r["CURVEPATH"];
      //                      //if (r["POSTURE_DATA"] as List<IVRQuternion> != null)
      //                      //{
      //                      //    newRow["POSTURE_DATA"] = ConvertPostureListToFloatArray((r["POSTURE_DATA"] as List<IVRQuternion>));
      //                      //}
      //                      if (r["POSTURE_DATA"] as byte[] != null)
      //                      {
      //                          newRow["POSTURE_DATA"] = ConvertPostureByteArrayToFloatArray(r["POSTURE_DATA"] as byte[]);
      //                      }
      //                  }
						
						//newRow["TOOL_OBJECT_ID"] = r["TOOL_OBJECT_ID"];
						//newRow["HAPTIC_TYPE"] = r["HAPTIC_TYPE"];
						//newRow["CLICK_CNT"] = r["CLICK_CNT"];
					//	newRow["IS_SILHOUETTE"] = r["IS_SILHOUETTE"];
                        newDataTable.Rows.Add(newRow);
                      
                    }
                    newDataTable.AcceptChanges();

                    return newDataTable;
                }
               // break;
            }
            return null;
        }

        /// <summary>
        /// 신규저장
        /// </summary>
        /// <param name="dtDisassem"></param>
        public static void InsertTaskProcedure(ProcedureModel model)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow newR = tmpDt.NewRow();
                    newR["SER_CD"] = model._sc_id;
                    newR["SUBSER_CD"] = model._subScid;
                    newR["PROCEMODE"] = model._isProceMode;
                    newR["WORK_ORDER"] = model._workOrder;
                    newR["ISSUBPROCE"] = model._isSubProc;
                    newR["OBJECT_NM"] = model.OBJECT_NM;
                    newR["OBJECT_ID"] = model._object_id;
                    newR["SENTENCE"] = model.SENTENCE;
                    newR["DEST_TRANSFORMATION"] = model._destTransform;
                    newR["ROTATION_INFO"] = model._destRotateInfo;
                    newR["ISTRANSLATION"] = model._isTranslation;
                    newR["TRANSMODE"] = model._transMode;
                    newR["ISSHOW"] = model._isShow;
                    newR["ISLINK"] = model.IS_LINK;
                    newR["TRAINING_TYPE"] = model.TRAINING_TYPE;
                    newR["DUR_TIME"] = model._durTime;
                    newR["TRAINING_MODE"] = model._trainingMode;
                    newR["CLICK_CNT"] = model._clickCnt;
                    newR["ISBLINK"] = model._isShow;
                    newR["TOOL_OBJECT_ID"] = model._toolUseObjId;
                    newR["SOUNDFILE_DATA"] = model._soundData;
                    newR["SOUND_FILE"] = model._soundfile;
                    tmpDt.Rows.Add(newR);

                    tmpDt.AcceptChanges();
                    break;
                }
            }
        }

        /// <summary>
        /// 신규저장
        /// </summary>
        /// <param name="dtDisassem"></param>
        public static void InsertTaskProcedure(DataTable dtDisassem)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    if (dtDisassem != null)
                    {
                        if (dtDisassem.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtDisassem.Rows)
                            {
                                DataRow newRow = tmpDt.NewRow();
                                newRow.ItemArray = r.ItemArray;
                                tmpDt.Rows.Add(newRow);
                            }
                            tmpDt.AcceptChanges();
                        }
                    }
                    break;
                }
            }
        }


       

        /// <summary>
        /// 하나의 SubTask에 대해서 절차 저장
        /// </summary>
        /// <param name="taskDt">변경된 정비절차 ListView에 대한 Table</param>
        /// <param name="SER_CD">정비절차명</param>
        /// /// <param name="SER_CD">정비절차명</param>
        public static void UpdateTaskProcedure(DataTable dtDisassem, string ser_cd, string subser_cd)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "' and SUBSER_CD='" + subser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                            dr.Delete();
                        tmpDt.AcceptChanges();
                    }

                    if (dtDisassem != null)
                    {
                        // DisAssm 업데이트
                        if (dtDisassem.Rows.Count > 0)
                        {
                            foreach (DataRow r in dtDisassem.Rows)
                            {
                                DataRow newRow = tmpDt.NewRow();
                                newRow["SER_CD"]                =  r["SER_CD"];             
                                newRow["SUBSER_CD"]             =  r["SUBSER_CD"];          
                                newRow["WORK_ORDER"]            =  r["WORK_ORDER"];         
                                newRow["OBJECT_NM"]             =  r["OBJECT_NM"];          
                                newRow["OBJECT_ID"]             =  r["OBJECT_ID"];          
                                newRow["SENTENCE"]              =  r["SENTENCE"];           
                                newRow["DEST_TRANSFORMATION"]   =  r["DEST_TRANSFORMATION"];
                                newRow["ROTATION_INFO"]         =  r["ROTATION_INFO"] ;     
                                newRow["ISTRANSLATION"]         =  r["ISTRANSLATION"]  ;    
                                newRow["ISMOVE"]                =  r["ISMOVE"];         
                                newRow["ISSHOW"]                =  r["ISSHOW"];        
                                newRow["ISRINK"]                =  r["ISRINK"];       
                                newRow["ISAUTO"]                =  r["ISAUTO"];         
                                newRow["DUR_TIME"]              =  r["DUR_TIME"];      
                                newRow["HAPTIC_MODE"]           =  r["HAPTIC_MODE"];   
                                newRow["SOUNDFILE"]             =  r["SOUNDFILE"];  
                                newRow["SOUNDFILE_DATA"]        =  r["SOUNDFILE_DATA"]   ;
                                newRow["IS3D"]                  =  r["IS3D"];
                                newRow["MOVING_MODE"]           =  r["MOVING_MODE"]      ;  
                                newRow["PROCE_MODE"]            =  r["PROCE_MODE"]      ;   
                                newRow["MULTITYPE"]             =  r["MULTITYPE"]     ;     
                                newRow["MULTIMEDIA"]            =  r["MULTIMEDIA"]     ;    
                                newRow["F_MULTITYPE"]           =  r["F_MULTITYPE"]     ;   
                                newRow["F_MULTIMEDIA"]          =  r["F_MULTIMEDIA"]    ;

                                if((int)r["PROCE_MODE"]==4) //캐릭터 모드
                                {
                                    newRow["ISANIMATION"] = r["ISANIMATION"];
                                    newRow["ANIMATION_MODE"] = r["ANIMATION_MODE"];
                                    newRow["CURVEPATH"] = r["CURVEPATH"];
                                    //if (r["POSTURE_DATA"] as float[,] != null)
                                    //{
                                    //    newRow["POSTURE_DATA"] = ConvertPostureToList((r["POSTURE_DATA"] as float[,]));
                                    //}
                                    if (r["POSTURE_DATA"] as float[,] != null)
                                    {
                                        newRow["POSTURE_DATA"] = ConvertPostureToByteArray(r["POSTURE_DATA"] as float[,]);
                                    }
                                }
                                newRow["ISBLINK"] = r["ISBLINK"];
								if ((int)r["PROCE_MODE"] == 4) //캐릭터 모드
								{
									if (r["MOTION_DATA"] as object[] != null)
									{
										newRow["MOTION_DATA"] = ConvertObjectArrayTobyteArray(r["MOTION_DATA"] as object[]);
									}
								}
								newRow["ISSUBPROCE"] = r["ISSUBPROCE"];
								newRow["TOOL_OBJECT_ID"] = r["TOOL_OBJECT_ID"];
								newRow["HAPTIC_TYPE"] = r["HAPTIC_TYPE"];
								newRow["CLICK_CNT"] = r["CLICK_CNT"];
								newRow["IS_SILHOUETTE"] = r["IS_SILHOUETTE"];
                                tmpDt.Rows.Add(newRow);
                            }
                            tmpDt.AcceptChanges();
                        }

                    }
                    break;
                    // 해당 정비절차 삭제

                }
                //break;
            }
        }


        /// <summary>
        /// 하나의 SubTask에 대해서 절차 저장
        /// </summary>
        /// <param name="taskDt">변경된 정비절차 ListView에 대한 Table</param>
        /// <param name="SER_CD">정비절차명</param>
        /// /// <param name="SER_CD">정비절차명</param>
        public static void UpdateProcedure(ObservableCollection<ProcedureModel> procedureList, int ser_cd, int subser_cd)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("TASK_PROCEDURE"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "' and SUBSER_CD='" + subser_cd + "'");
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                            dr.Delete();
                        tmpDt.AcceptChanges();
                    }

                    // DisAssm 업데이트
                    if (procedureList.Count > 0)
                    {
                        foreach (ProcedureModel model in procedureList)
                        {
                            DataRow newR = tmpDt.NewRow();
                            newR["SER_CD"] = model._sc_id;
                            newR["SUBSER_CD"] = model._subScid;
                            newR["PROCEMODE"] = model._isProceMode;
                            newR["WORK_ORDER"] = model._workOrder;
                            newR["ISSUBPROCE"] = model._isSubProc;
                            newR["OBJECT_NM"] = model.OBJECT_NM;
                            newR["OBJECT_ID"] = model._object_id;
                            newR["SENTENCE"] = model.SENTENCE;
                            newR["DEST_TRANSFORMATION"] = model._destTransform;
                            newR["ROTATION_INFO"] = model._destRotateInfo;
                            newR["ROT_OBJECT_ID"] = model._rotRefObjId;
                            newR["ISTRANSLATION"] = model._isTranslation;
                            newR["TRANSMODE"] = model._transMode;
                            newR["ISSHOW"] = model._isShow;
                            newR["ISLINK"] = model.IS_LINK;
                            newR["TRAINING_TYPE"] = model.TRAINING_TYPE;
                            newR["DUR_TIME"] = model._durTime;
                            newR["TRAINING_MODE"] = model._trainingMode;
                            newR["CLICK_CNT"] = model._clickCnt;
                            newR["TOOL_OBJECT_ID"] = model._toolUseObjId;
                            newR["SOUNDFILE_DATA"] = model._soundData;
                            newR["SOUND_FILE"] = model._soundfile;
                            newR["ISBLINK"] = model._isBlink;
                            newR["IS_SILHOUETTE"] = model._isSilhoutte;
                            tmpDt.Rows.Add(newR);
                        }
                        tmpDt.AcceptChanges();
                    }

                    break;
                    // 해당 정비절차 삭제

                }
                //break;
            }
        }

       
        public static int GetMotionId()
		{
			int maxVal = 0;
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("HUMAN_MOTION"))
				{
					foreach (DataRow row in tmpDt.Rows)
					{
						int itemVal = Int32.Parse(row["MOTION_ID"].ToString());
						if (itemVal > maxVal) maxVal = itemVal;
					}
					maxVal++;
				}
			}
			return maxVal;
		}

		public static void UpdateHumnaData(int motionId, int workOrder, float[,] postureDt, string motionTitle)
		{
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("HUMAN_MOTION"))
				{
					DataRow newRow = tmpDt.NewRow();
					newRow["MOTION_ID"] = motionId;
					newRow["MOTION_NM"] = motionTitle;
					newRow["WORK_ORDER"] = workOrder;
					newRow["POSTURE_DATA"] = ConvertPostureToByteArray(postureDt);
					tmpDt.Rows.Add(newRow);
					tmpDt.AcceptChanges();
				}
			}
		}

		public static DataTable GetHumanMotionIDList()
		{
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("HUMAN_MOTION"))
				{
					DataTable newDataTable = tmpDt.Clone();

					DataRow[] drArr = tmpDt.Select("WORK_ORDER = '0'");

					foreach (DataRow r in drArr)
					{
						DataRow newRow = newDataTable.NewRow();
						newRow["MOTION_ID"] = r["MOTION_ID"];
						newRow["MOTION_NM"] = r["MOTION_NM"];

						newDataTable.Rows.Add(newRow);
					}
					newDataTable.AcceptChanges();

					return newDataTable;
				}
			}

			return null;
		}

		public static DataTable FindHumanMotionData(string motionId)
		{
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("HUMAN_MOTION"))
				{
					DataTable newDataTable = tmpDt.Clone();

					DataRow[] drArr = tmpDt.Select("MOTION_ID = '" + motionId + "'");

					foreach (DataRow r in drArr)
					{
						DataRow newRow = newDataTable.NewRow();
						newRow["WORK_ORDER"] = r["WORK_ORDER"];
						newRow["POSTURE_DATA"] = r["POSTURE_DATA"];

						newDataTable.Rows.Add(newRow);
					}
					return newDataTable;
				}
			}
			return null;
		}

		/// <summary>
		/// Byte[] 값을 float[,]로 변환하여 object에 저장
		/// </summary>
		/// <param name="motionId"></param>
		/// <returns></returns>
		public static object[] ConvertByteArrayToObjectArray(string motionId)
		{
			List<object> result = new List<object>();
			foreach (DataTable tmpDt in m_TaskDataSet.Tables)
			{
				if (tmpDt.TableName.Equals("HUMAN_MOTION"))
				{
					DataRow[] drArr = tmpDt.Select("MOTION_ID = '" + motionId + "'");

					foreach (DataRow dr in drArr)
					{
						result.Add(ConvertPostureByteArrayToFloatArray(dr["POSTURE_DATA"] as byte[]));
					}
				}
			}
			return result.ToArray();
		}

		public static object[] ConvertByteArrayToObjectArray(byte[] byteArray)
		{
			byte[] sizeInfo = new byte[4];
			for (int x = 0; x < sizeInfo.Length; x++)
			{
				sizeInfo[x] = byteArray[x];
			}

			int sizeIntInfo = BitConverter.ToInt32(sizeInfo, 0);

			List<object> result = new List<object>();

			int byteLength = sizeInfo.Length;
			int objectLength = (byteArray.Length - 4) / sizeIntInfo;
			for (int x = 0; x < objectLength; x++)
			{
				byte[] tempMotionByte = new byte[sizeIntInfo];
				Array.Copy(byteArray, byteLength, tempMotionByte, 0, sizeIntInfo);
				byteLength += sizeIntInfo;

				MemoryStream memStream = new MemoryStream();
				BinaryFormatter binForm = new BinaryFormatter();
				memStream.Write(tempMotionByte, 0, tempMotionByte.Length);
				memStream.Seek(0, SeekOrigin.Begin);
				Object obj = (Object)binForm.Deserialize(memStream);

				result.Add(obj);
			}

			return result.ToArray();
		}

		public static byte[] ConvertObjectArrayTobyteArray(object[] objectArray)
		{
			if (objectArray == null)
				return null;

			List<byte[]> byteDt = new List<byte[]>();

			int byteLength =0;

			for (int x = 0; x < objectArray.Length; x++)
			{
				BinaryFormatter bf = new BinaryFormatter();
				MemoryStream ms = new MemoryStream();
				bf.Serialize(ms, objectArray[x]);
				byteDt.Add(ms.ToArray());
				byteLength += byteDt[0].Length;
			}
			byte[] sizeInfo = BitConverter.GetBytes(byteDt[0].Length);
			byteLength += sizeInfo.Length;
			byte[] resultByteDt = new byte[byteLength];
			Array.Copy(sizeInfo, 0, resultByteDt, 0, sizeInfo.Length);

			byteLength = sizeInfo.Length;

			for (int x = 0; x < byteDt.Count; x++)
			{
				Array.Copy(byteDt[x], 0, resultByteDt, byteLength, byteDt[x].Length);

				byteLength += byteDt[x].Length;
			}


			//int postureDtLength = objectArray.Length;
			//byte[] sizeInfo = BitConverter.GetBytes(postureDtLength);
			return resultByteDt;
		}

        /// <summary>
        /// float[,]을 받아서 Byte[] 형태로 Convert하는 함수
        /// </summary>
        /// <param name="postureData"></param>
        /// <returns></returns>
        public static byte[] ConvertPostureToByteArray(float[,] postureData)
        {
            List<byte> result = new List<byte>();

         //   List<IVRQuternion> tmpPosData = new List<IVRQuternion>();
            int row = postureData.GetUpperBound(0);
            int col = postureData.GetUpperBound(1);

            for (int i = 0; i <= row; i++)
            {
                //IVRQuternion quternion;
                for (int j = 0; j < 4; j++)
                {
                    result.AddRange(BitConverter.GetBytes(postureData[i, j]));
                }
                //quternion.x = postureData[i, 0];
                //quternion.y = postureData[i, 1];
                //quternion.z = postureData[i, 2];
                //quternion.w = postureData[i, 3];

              //  tmpPosData.Add(quternion);
            }

            return result.ToArray();
           // return tmpPosData;
        }

        /// <summary>
        /// float[](dest_transform, 6개 float 배열(pos.x,pos.y,pos.z,euler.x,euler.y,euler.z)을 받아서 Byte[] 형태로 Convert하는 함수(Unity에서 쓸수 있도록 처리)
        /// </summary>
        /// <param name="postureData"></param>
        /// <returns></returns>
        public static byte[] ConvertDestTransformToByteArray(float[] destTransform)
        {
            List<byte> result = new List<byte>();

            //   List<IVRQuternion> tmpPosData = new List<IVRQuternion>();
            int row = destTransform.GetUpperBound(0);
            for (int i = 0; i <= row; i++)
            {
                result.AddRange(BitConverter.GetBytes(destTransform[i]));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Byte[]을 받아서 float[6]형태로 Convert하는 함수 6개 float 배열(pos.x,pos.y,pos.z,euler.x,euler.y,euler.z)
        /// </summary>
        /// <param name="tmpList"></param>
        /// <returns></returns>
        public static float[] ConvertDestTransformByteArrayToFloatArray(byte[] destTransformByte)
        {
            float[] tmpFloatArr = new float[9];
            int j = 0;
            for (int i = 0; i < 9; i++)
            {
                tmpFloatArr[i] = BitConverter.ToSingle(destTransformByte, j);
                j = j + 4;
            }
            return tmpFloatArr;
        }
        /// <summary>
        /// Byte[]을 받아서 float[7]형태로 Convert하는 함수 7개 float 배열(pos.x,pos.y,pos.z,quat.x,quat.y,quat.z,quat.w)
        /// </summary>
        /// <param name="tmpList"></param>
        /// <returns></returns>
        public static float[] ConvertCameraDestTransformByteArrayToFloatArray(byte[] destTransformByte)
        {
            float[] tmpFloatArr = new float[7];
            int j = 0;
            for (int i = 0; i < 7; i++)
            {
                tmpFloatArr[i] = BitConverter.ToSingle(destTransformByte, j);
                j = j + 4;
            }
            return tmpFloatArr;
        }
        /// <summary>
        /// object[](회전정보, String|Angle)을 받아서 Byte[] 형태로 Convert하는 함수(Unity에서 쓸수 있도록 처리)
        /// </summary>
        /// <param name="postureData"></param>
        /// <returns></returns>
        public static byte[] ConvertRotateInfoToByteArray(object[] rotateInfo)
        {
            List<byte> result = new List<byte>();
            string tmpAxis = rotateInfo[0] as string;
            result.AddRange(Encoding.Unicode.GetBytes(tmpAxis));
            float angle = Convert.ToSingle(rotateInfo[1]);
            result.AddRange(BitConverter.GetBytes(angle));
            return result.ToArray();
        }

        /// <summary>
        /// Byte[]을 받아서 object[](회전정보, String|Angle)의 형태로 Convert하는 함수
        /// </summary>
        /// <param name="tmpList"></param>
        /// <returns></returns>
        public static object[] ConvertRotateInfoByteArrayToObjectArray(byte[] RotateInfoByte)
        {
            object[] objectInfo = new object[2];
            objectInfo[0] =  Encoding.Unicode.GetString(RotateInfoByte,0, 2); //축string 2byte
            objectInfo[1] =  BitConverter.ToSingle(RotateInfoByte, 2);
           
            return objectInfo;
        }

        /// <summary>
        /// Byte[]을 받아서 float[,]형태로 Convert하는 함수
        /// </summary>
        /// <param name="tmpList"></param>
        /// <returns></returns>
        public static float[,] ConvertPostureByteArrayToFloatArray(byte[] postureArray)
        {
            int row =  postureArray.Length / 16; //(x,y,z,w)

            float[,] tmpFloatArr = new float[row, 4];
            int j = 0;
            for (int i = 0; i < row; i++)
            {
                tmpFloatArr[i, 0] = BitConverter.ToSingle(postureArray,j);
                tmpFloatArr[i, 1] = BitConverter.ToSingle(postureArray, j + 4);
                tmpFloatArr[i, 2] = BitConverter.ToSingle(postureArray, j + 8);
                tmpFloatArr[i, 3] = BitConverter.ToSingle(postureArray, j + 12);
                j = j + 16;
            }
            return tmpFloatArr;
        }

        /// <summary>
        /// List을 받아서 float[,]형태로 Convert하는 함수
        /// </summary>
        /// <param name="tmpList"></param>
        /// <returns></returns>
        public static float[,] ConvertPostureListToFloatArray(List<IVRQuternion> tmpList)
        { 
            float[,] tmpFloatArr = new float[tmpList.Count,4];

            int rowCnt = 0;
            foreach (IVRQuternion quter in tmpList)
            {
                tmpFloatArr[rowCnt, 0] = quter.x;
                tmpFloatArr[rowCnt, 1] = quter.y;
                tmpFloatArr[rowCnt, 2] = quter.z;
                tmpFloatArr[rowCnt, 3] = quter.w;
                rowCnt++;
            }

            return tmpFloatArr;
        }


        /// <summary>
        /// 세부항목 순서 변경(ORDER)
        /// </summary>
        /// <param name="ser_cd"></param>
        /// <param name="ser_link_cd"></param>
        /// <param name="subser_link_cd"></param>
        /// <param name="order"></param>
        public static void ReNumberingSubTaskLink(string ser_cd,string ser_link_cd,string subser_link_cd, int order)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("SUB_TASK_LINK"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + ser_cd + "' and SER_LINK_CD='" + ser_link_cd + "' and SUBSER_LINK_CD='" + subser_link_cd + "'");
                    if (drArr.Length > 0)
                    { 
                        foreach(DataRow dr in drArr)
                        {
                            dr["ORDER"] = order;
                        }
                    }
                    tmpDt.AcceptChanges();
                }
            }
        }
        #endregion 

        #region PART_RELATION
        /// <summary>
        /// PART_RELATION 데이터 불러오기
        /// </summary>
        /// <returns></returns>
        public static DataTable GetPartRelationDt()
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("PART_RELATION"))
                {
                    return tmpDt;
                }
            }
            return null;
        }
        /// <summary>
        /// GBL_RELATION 데이터 불러오기
        /// </summary>
        /// <returns></returns>
        public static DataTable GetGBLRelationDt()
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("GBL_RELATION"))
                {
                    return tmpDt;
                }
            }
            return null;
        }
        public static void UpdatePartRelationInfo(string part_id, string object_id, int seq)
        {
            DataTable dtPartUsing = null;
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("PART_RELATION"))
                {
                    dtPartUsing = tmpDt;
                    break;
                }
            }
            //return;

            if (dtPartUsing == null) return;
            if (String.IsNullOrEmpty(part_id)) //파트정보가 없을경우는 해제 된것으로 간주
            {
                DataRow[] drCol = dtPartUsing.Select("PART_ID='" + part_id + "' AND OBJECT_ID = '" + object_id + "'");
                foreach (DataRow dr in drCol)
                    dr.Delete();

            }
            else
            { // 신규나 수정으로 간주

                DataRow[] drCol = dtPartUsing.Select("PART_ID='" + part_id + "' AND OBJECT_ID = '" + object_id + "'");

                if (drCol.Length > 0) //수정
                {
                    //foreach (DataRow dr in drCol)
                    //{
                    //    dr["PART_NM"] = part_nm;
                    //    dr["PARENT_ID"] = parent_id;
                    //    dr["PART_CD"] = part_cd;
                    //    dr["SPEC"] = spec;
                    //    dr["SEQ"] = seq;

                    //}
                }
                else //신규
                {
                    DataRow newR = dtPartUsing.NewRow();
                    newR["PART_ID"] = part_id;
                    newR["OBJECT_ID"] = object_id;
                    newR["SEQ"] = seq;
                    dtPartUsing.Rows.Add(newR);
                }
            }
            dtPartUsing.AcceptChanges();
        }

        public static void DeletePartRelationInfo(string part_id, string object_id, int seq)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("PART_RELATION"))
                {
                    int cnt = 0;

                    DataRow[] selDrCol = tmpDt.Select("PART_ID='" + part_id + "' and OBJECT_ID = '" + object_id + "' and SEQ = " + seq + "");
                    foreach (DataRow dr in selDrCol)
                    {
                        dr.Delete();
                    }

                    tmpDt.AcceptChanges();
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// 콘텐츠
        /// </summary>
        /// <returns></returns>
        public static DataTable GetContents()
        {
            // DataTable tmpDt;
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("CONTENTS"))
                {
                    return tmpDt;
                }
            }
            throw new Exception("TASK NOT EXIST!! ");
            //return null;
        }

        /// <summary>
        /// 콘텐츠 DB 입력
        /// </summary>
        /// <param name="newModel"></param>
        public static void InsertContents(ContentsModel newModel)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("CONTENTS"))
                {
                    DataRow newRow = tmpDt.NewRow();
                    newRow["SER_CD"] = newModel.ser_cd;
                    newRow["OBJECT_ID"] = newModel.object_id;
                    newRow["MODE"] = newModel.intMode;//모드= 0:운용, 1:정비, 2:점검, 3:훈련 
                    newRow["SEQ"] = Convert.ToInt32(newModel.SEQ);
                    tmpDt.Rows.Add(newRow);

                    tmpDt.AcceptChanges();
                    break;
                }
            }
        }

        /// <summary>
        /// 콘텐츠 DB 입력
        /// </summary>
        /// <param name="newModel"></param>
        public static void DeleteContents(ContentsModel newModel)
        {
            foreach (DataTable tmpDt in m_TaskDataSet.Tables)
            {
                if (tmpDt.TableName.Equals("CONTENTS"))
                {
                    DataRow[] drArr = tmpDt.Select("SER_CD='" + newModel.ser_cd + "' AND OBJECT_ID='"+newModel.object_id+"' AND MODE="+newModel.intMode);
                    if (drArr.Length > 0)
                    {
                        foreach (DataRow dr in drArr)
                        {
                            dr.Delete();
                        }
                        tmpDt.AcceptChanges();

                    }
                    break;
                }
            }
        }
        /// <summary>
        /// Inner Join
        /// </summary>     
        public static DataTable InnerJoin(DataTable First, DataTable Second, DataColumn[] FJC, DataColumn[] SJC)
        {
            //Create Empty Table
            DataTable table = new DataTable("Join");
            // Use a DataSet to leverage DataRelation
            using (DataSet ds = new DataSet())
            {
                //Add Copy of Tables
                ds.Tables.AddRange(new DataTable[] { First.Copy(), Second.Copy() });
                //Identify Joining Columns from First
                DataColumn[] parentcolumns = new DataColumn[FJC.Length];
                for (int i = 0; i < parentcolumns.Length; i++)
                {
                    parentcolumns[i] = ds.Tables[0].Columns[FJC[i].ColumnName];
                }
                //Identify Joining Columns from Second
                DataColumn[] childcolumns = new DataColumn[SJC.Length];
                for (int i = 0; i < childcolumns.Length; i++)
                {
                    childcolumns[i] = ds.Tables[1].Columns[SJC[i].ColumnName];
                }
                //Create DataRelation
                DataRelation r = new DataRelation(string.Empty, parentcolumns, childcolumns, false);
                ds.Relations.Add(r);

                //Create Columns for JOIN table
                for (int i = 0; i < First.Columns.Count; i++)
                {
                    table.Columns.Add(First.Columns[i].ColumnName, First.Columns[i].DataType);
                }
                for (int i = 0; i < Second.Columns.Count; i++)
                {
                    //Beware Duplicates
                    if (!table.Columns.Contains(Second.Columns[i].ColumnName))
                        table.Columns.Add(Second.Columns[i].ColumnName, Second.Columns[i].DataType);
                    else
                        table.Columns.Add(Second.Columns[i].ColumnName + "_Second", Second.Columns[i].DataType);
                }

                DataTable notExistTableInfo =  ds.Tables[0].Clone();

                //Loop through First table
                table.BeginLoadData();
                foreach (DataRow firstrow in ds.Tables[0].Rows)
                {
                    //Get "joined" rows
                    DataRow[] childrows = firstrow.GetChildRows(r);
                    if (childrows != null && childrows.Length > 0)
                    {
                        object[] parentarray = firstrow.ItemArray;
                        foreach (DataRow secondrow in childrows)
                        {
                            object[] secondarray = secondrow.ItemArray;
                            object[] joinarray = new object[parentarray.Length + secondarray.Length];
                            Array.Copy(parentarray, 0, joinarray, 0, parentarray.Length);
                            Array.Copy(secondarray, 0, joinarray, parentarray.Length, secondarray.Length);
                            table.LoadDataRow(joinarray, true);
                        }
                    }
                    else // not exist Delete from First
                    {
                        DataRow nR = notExistTableInfo.NewRow();
                        nR.ItemArray = firstrow.ItemArray;
                        notExistTableInfo.Rows.Add(nR);
                    }
                }
                table.EndLoadData();

                foreach (DataRow dR in notExistTableInfo.Rows)
                {
                    foreach (DataRow firstRow in ds.Tables[0].Rows)
                    {
                        if (dR.Equals(firstRow))
                        {
                            firstRow.Delete();
                            break;
                        }
                    }
                }
            }
            return table;
        }

       
        /// <summary>
        /// 이전 상태정보 object의 위치정보를 담는 메서드
        /// </summary>
        public static DataTable GetPrevSceneState(string ser_cd, string subTaskId, int subser_order, int procedure_order)
        {
            DataTable tblResetList = new DataTable();
   
            return tblResetList;
        }

     
        /// <summary>
        /// 자신의 부모 세부항목들의 위치정보 object의 위치정보를 담는 메서드
        /// </summary>
        public static DataTable GetHierarchySubTaskObjectData(string ser_cd, string parent_id, string ser_link_cd, string subser_link_cd)
        {
            DataTable subItemDt = new DataTable();
            subItemDt.Columns.Add("SER_CD", typeof(int));
            subItemDt.Columns.Add("LINK_ID", typeof(int));
            subItemDt.Columns.Add("SER_LINK_CD", typeof(int));
            subItemDt.Columns.Add("SUBSER_LINK_CD", typeof(int));
            subItemDt.Columns.Add("ORDER", typeof(int));
            DataTable subTaskdt = TaskDataManager.GetSubTaskLink();
            int order = Int32.MaxValue;

            GetRecursiveSubDt(subTaskdt, ser_cd, parent_id, ref subItemDt, ref order);

            DataTable tblResetList = new DataTable();

            tblResetList.Columns.Add("ID", typeof(int));            
            tblResetList.Columns.Add("OBJECT_ID", typeof(System.String));
            tblResetList.Columns.Add("DEST_TRANSFORMATION", typeof(float[]));
            tblResetList.Columns.Add("ISSHOW", typeof(Boolean));
            tblResetList.Columns.Add("ISTRANSLATION", typeof(Boolean));
          //  tblResetList.Columns.Add("TRANSMODE", typeof(Int32));
          //  tblResetList.Columns.Add("ROTATION_INFO", typeof(object[]));
            tblResetList.Columns.Add("PROCEMODE", typeof(Int32));
            //tblResetList.Columns.Add("MOVING_MODE", typeof(Int32));
            //tblResetList.Columns.Add("PROCE_MODE", typeof(Int32));
            //tblResetList.Columns.Add("ISANIMATION", typeof(Boolean));
            //tblResetList.Columns.Add("ANIMATION_MODE", typeof(Int32));
            //tblResetList.Columns.Add("CURVEPATH", typeof(String));
            //tblResetList.Columns.Add("POSTURE_DATA", typeof(float[,]));

            
            DataTable taskProcedureDt = GetTaskProcedure();

            //if (bSubTaskClick)
            //{
                #region "이전세부항목"
                //  DataRow[] selDr = subTaskdt.Select("SER_CD='" + ser_cd + "' AND ORDER < " + order);
                DataRow[] selDr = subItemDt.Select("", "ORDER");
                foreach (DataRow subDr in selDr)
                {
                    DataRow[] procDr = taskProcedureDt.Select("SER_CD='" + subDr["SER_LINK_CD"].ToString() + "' AND SUBSER_CD='"
                                                                + subDr["SUBSER_LINK_CD"].ToString() + "'", "WORK_ORDER");
                    foreach (DataRow proceDr in procDr)
                    {
                        if (!proceDr["PROCEMODE"].ToString().Equals("0")) continue; //점검절차면 SKIP

                        DataRow[] selMoveRow = tblResetList.Select("OBJECT_ID='" + proceDr["OBJECT_ID"].ToString() + "'");

                        DataRow newR = tblResetList.NewRow();
                        newR["ID"] = (Convert.ToInt32(ser_cd) * 1000 * 1000) + (Convert.ToInt32(proceDr["SUBSER_CD"].ToString()) * 1000) + Convert.ToInt32(proceDr["WORK_ORDER"]);
                        newR["OBJECT_ID"] = proceDr["OBJECT_ID"].ToString();
                        if (proceDr["PROCEMODE"].ToString().Equals("0"))
                            newR["DEST_TRANSFORMATION"] = proceDr["DEST_TRANSFORMATION"];
                        newR["ISSHOW"] = proceDr["ISSHOW"];
                        newR["ISTRANSLATION"] = proceDr["ISTRANSLATION"];
                        newR["PROCEMODE"] = (int)proceDr["PROCEMODE"];
                        tblResetList.Rows.Add(newR);

                    }

                }
                #endregion

                return tblResetList;
            //}

            //DataRow[] currProcDr = taskProcedureDt.Select("SER_CD='" + ser_link_cd + "' AND SUBSER_CD='"
            //                                               + subser_link_cd + "' AND WORK_ORDER <= " + procedure_order, "WORK_ORDER");

            //#region "현재 세부항목 해당 절차전까지"
            //foreach (DataRow dr in currProcDr)
            //{

            //    DataRow[] selMoveRow = tblResetList.Select("OBJECT_ID='" + dr["OBJECT_ID"].ToString() + "'");

            //    DataRow newR = tblResetList.NewRow();
            //  //  newR["SER_CD"] = ser_cd;
            //   // newR["SUBSER_CD"] = dr["SUBSER_CD"].ToString();
            //   // newR["WORK_ORDER"] = dr["WORK_ORDER"];
            //    newR["ID"] = (Convert.ToInt32(ser_cd) * 1000 * 1000) + (Convert.ToInt32(dr["SUBSER_CD"].ToString()) * 1000) + Convert.ToInt32(dr["WORK_ORDER"]);
            //    newR["OBJECT_ID"] = dr["OBJECT_ID"].ToString();
            //    if (Convert.ToInt32(dr["PROCEMODE"])==0)
            //        newR["DEST_TRANSFORMATION"] = dr["DEST_TRANSFORMATION"];
            //    newR["ISSHOW"] = dr["ISSHOW"];
            //    newR["ISTRANSLATION"] = dr["ISTRANSLATION"];
            //    newR["TRANSMODE"] = Convert.ToInt32(dr["TRANSMODE"]);
            //   // newR["ISAUTO"] = dr["ISAUTO"];
            //    newR["ROTATION_INFO"] = dr["ROTATION_INFO"];
            //    newR["PROCEMODE"] = (int)dr["PROCEMODE"];
            //    //newR["MOVING_MODE"] = (int)dr["MOVING_MODE"];
            //    //newR["PROCE_MODE"] = (int)dr["PROCE_MODE"];

            //    tblResetList.Rows.Add(newR);
            //}
            //#endregion

            //return tblResetList;
        }



        /// <summary>
        /// 부모노드항목을 찾는다.
        /// </summary>
        /// <param name="subDt"></param>
        /// <param name="ser_cd"></param>
        /// <param name="parent_id"></param>
        /// <param name="subHistoryDt"></param>
        /// <param name="order"></param>
        static void GetRecursiveSubDt(DataTable subLinkDt, string ser_cd, string parent_id, ref DataTable subHistoryDt, ref int order)
        {
            DataRow[] selDr = subLinkDt.Select("SER_CD='" + ser_cd + "' AND LINK_ID='" + parent_id + "'");
            if (selDr.Length > 0)
            {
                DataRow newR = subHistoryDt.NewRow();
                newR[0] = selDr[0]["SER_CD"].ToString();
                newR[1] = selDr[0]["LINK_ID"].ToString();                
                newR[2] = selDr[0]["SER_LINK_CD"].ToString();
                newR[3] = selDr[0]["SUBSER_LINK_CD"].ToString();
                newR[4] = order;
                subHistoryDt.Rows.Add(newR);

                order--;

                if (string.IsNullOrEmpty(selDr[0]["PARENT_LINK_ID"].ToString()))
                {
                    throw new Exception("부모항목이 없습니다. error입니다.");
                    // return;
                }
                if (selDr[0]["PARENT_LINK_ID"].ToString().Equals("0"))
                    return;
                else
                {
                    GetRecursiveSubDt(subLinkDt, ser_cd, selDr[0]["PARENT_LINK_ID"].ToString(), ref subHistoryDt, ref order);
                }
            }
        }


    }

    // --- Agentic AI ?? ?? (Phase 2) ---
    public class AIOrderInfo
    {
        public string PartID { get; set; }
        public string Status { get; set; }
        public string EstimatedTime { get; set; }
    }

    public static class AgenticManager
    {
        // HUD Panel 2?? ????? ??
        public static ObservableCollection<AIOrderInfo> AIOrderList { get; set; } = new ObservableCollection<AIOrderInfo>();

        public static async Task AutoOrderPart(string partID)
        {
            var newOrder = new AIOrderInfo { PartID = partID, Status = "?????", EstimatedTime = "???..." };
            
            // UI ??? ??? ?? CoreDispatcher? ????? ??? ?? (UWP)
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                AIOrderList.Add(newOrder);
            });

            // ??? API ?? ??
            await Task.Delay(2000);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                newOrder.Status = "AGV ???";
                newOrder.EstimatedTime = "2? ? ??";
                // ?? ????? PropertyChanged ??? ?? ?? ??? ??? ??
            });
        }
    }
}
