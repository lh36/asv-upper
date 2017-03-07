using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upper.Bean;
using System.Windows.Forms;
using Upper.Tool;
using Upper.Manager;
using System.Threading;

namespace Upper.Controller
{
    public class ShipCloseController : Singleton<ShipCloseController>
    {
        private ComboBox Phi_mode = null;
        private TextBox Boat1_Kp = null;
        private TextBox Boat1_Ki = null;
        private TextBox Boat1_Kd = null;
        private TextBox Boat1_K1 = null;
        private TextBox Boat1_K2 = null;
        private TextBox Boat1_DL = null;
        private TextBox Boat2_Kp = null;
        private TextBox Boat2_Ki = null;
        private TextBox Boat2_Kd = null;
        private TextBox Boat2_K1 = null;
        private TextBox Boat2_K2 = null;
        private TextBox Boat2_DL = null;
        private TextBox Boat3_Kp = null;
        private TextBox Boat3_Ki = null;
        private TextBox Boat3_Kd = null;
        private TextBox Boat3_K1 = null;
        private TextBox Boat3_K2 = null;
        private TextBox Boat3_DL = null;
        private TextBox line_Y1 = null;
        private TextBox line_Y2 = null;
        private TextBox line_Y3 = null;
        private TextBox circle_R1 = null;
        private TextBox circle_R2 = null;
        private TextBox circle_R3 = null;
        private TextBox circle_X = null;
        private TextBox circle_Y = null;
        private RadioButton AutoSpeed = null;
        private TextBox Manualspeedset = null;
        private ComboBox path_mode = null;

        private Ship boat1 = null;
        private Ship boat2 = null;
        private Ship boat3 = null;
        
        private double targetLine;  //目标线
        public Circle targetCircle; //目标圆


        public ShipCloseController() { }

        //控制器初始化
        public void Init( ComboBox Phi_mode,TextBox Boat1_Kp,TextBox Boat1_Ki,TextBox Boat1_Kd,TextBox Boat1_K1,TextBox Boat1_K2,TextBox Boat1_DL,
                        TextBox Boat2_Kp,TextBox Boat2_Ki,TextBox Boat2_Kd,TextBox Boat2_K1,TextBox Boat2_K2,TextBox Boat2_DL,
                        TextBox Boat3_Kp,TextBox Boat3_Ki,TextBox Boat3_Kd,TextBox Boat3_K1,TextBox Boat3_K2,TextBox Boat3_DL,
                        TextBox line_Y1, TextBox line_Y2, TextBox line_Y3, TextBox circle_R1,TextBox circle_R2,TextBox circle_R3,TextBox circle_X,TextBox circle_Y,
                        RadioButton AutoSpeed, TextBox Manualspeedset, ComboBox path_mode)
        {
            this.Phi_mode = Phi_mode;
            this.Boat1_Kp = Boat1_Kp;
            this.Boat1_Ki = Boat1_Ki;
            this.Boat1_Kd = Boat1_Kd;
            this.Boat1_K1 = Boat1_K1;
            this.Boat1_K2 = Boat1_K2;
            this.Boat1_DL = Boat1_DL;
            this.Boat2_Kp = Boat2_Kp;
            this.Boat2_Ki = Boat2_Ki;
            this.Boat2_Kd = Boat2_Kd;
            this.Boat2_K1 = Boat2_K1;
            this.Boat2_K2 = Boat2_K2;
            this.Boat2_DL = Boat2_DL;
            this.Boat3_Kp = Boat3_Kp;
            this.Boat3_Ki = Boat3_Ki;
            this.Boat3_Kd = Boat3_Kd;
            this.Boat3_K1 = Boat3_K1;
            this.Boat3_K2 = Boat3_K2;
            this.Boat3_DL = Boat3_DL;
            this.line_Y1 = line_Y1;
            this.line_Y2 = line_Y2;
            this.line_Y3 = line_Y3;
            this.circle_R1 = circle_R1;
            this.circle_R2 = circle_R2;
            this.circle_R3 = circle_R3;
            this.circle_X = circle_X;
            this.circle_Y = circle_Y;
            this.AutoSpeed = AutoSpeed;
            this.Manualspeedset = Manualspeedset;
            this.path_mode = path_mode;

            boat1 = GlobalManager.Instance.GetShip(1);
            boat2 = GlobalManager.Instance.GetShip(2);
            boat3 = GlobalManager.Instance.GetShip(3);


        }

        /// <summary>
        /// 闭环控制线程
        /// </summary>
        public void Control_PF()
        {
            while (GlobalManager.flag_ctrl)
            {
                UpdateCtrlPhi();//更新控制方式为航向角或航迹角
                UpdateCtrlPara();//更新PID控制参数和速度控制参数
                UpdateCtrlOutput();//更新航向和速度控制输出
                Thread.Sleep(195);//控制周期
            }
        }

        //更新航向角
        private void UpdateCtrlPhi()
        {
            if (Phi_mode.Text == "航向角")
            {
                GlobalManager.Instance.GetShip(1).Control_Phi = GlobalManager.Instance.GetShip(1).phi;
                GlobalManager.Instance.GetShip(2).Control_Phi = GlobalManager.Instance.GetShip(2).phi;
                GlobalManager.Instance.GetShip(3).Control_Phi = GlobalManager.Instance.GetShip(3).phi;
            }
            else
            {
                GlobalManager.Instance.GetShip(1).Control_Phi = GlobalManager.Instance.GetShip(1).GPS_Phi;
                GlobalManager.Instance.GetShip(2).Control_Phi = GlobalManager.Instance.GetShip(2).GPS_Phi;
                GlobalManager.Instance.GetShip(3).Control_Phi = GlobalManager.Instance.GetShip(3).GPS_Phi;
            }
        }

        //更新控制参数
        private void UpdateCtrlPara()
        {
            boat1.Kp = double.Parse(Boat1_Kp.Text);//获取舵机控制参数
            boat1.Ki = double.Parse(Boat1_Ki.Text);
            boat1.Kd = double.Parse(Boat1_Kd.Text);
            boat1.K1 = double.Parse(Boat1_K1.Text);//获取螺旋桨控制参数
            boat1.K2 = double.Parse(Boat1_K2.Text);
            boat1.dl_err = double.Parse(Boat1_DL.Text);

            boat2.Kp = double.Parse(Boat2_Kp.Text);//获取舵机控制参数
            boat2.Ki = double.Parse(Boat2_Ki.Text);
            boat2.Kd = double.Parse(Boat2_Kd.Text);
            boat2.K1 = double.Parse(Boat2_K1.Text);//获取螺旋桨控制参数
            boat2.K2 = double.Parse(Boat2_K2.Text);
            boat2.dl_err = double.Parse(Boat2_DL.Text);

            boat3.Kp = double.Parse(Boat3_Kp.Text);//获取舵机控制参数
            boat3.Ki = double.Parse(Boat3_Ki.Text);
            boat3.Kd = double.Parse(Boat3_Kd.Text);
            boat3.K1 = double.Parse(Boat3_K1.Text);//获取螺旋桨控制参数
            boat3.K2 = double.Parse(Boat3_K2.Text);
            boat3.dl_err = double.Parse(Boat3_DL.Text);
        }

        //更新输出
        private void UpdateCtrlOutput()
        {
            targetLine = double.Parse(line_Y1.Text);//1号船目标线和圆
            targetCircle.Radius = double.Parse(circle_R1.Text);
            targetCircle.x = double.Parse(circle_X.Text);
            targetCircle.y = double.Parse(circle_Y.Text);

            boat1.controller.command[3] = Control_fun(boat1.controller, boat1);//1号小船控制
            boat1.controller.command[4] = 110;
            /*if (AutoSpeed.Checked) 
                ship1Control.command[4] = ship1Control.Closed_Control_Speed(boat1, boat2.pos_X);
            else 
                ship1Control.command[4] = (byte)(int.Parse(Manualspeedset.Text)); */

            boat1.CtrlRudOut = boat1.controller.command[3];//舵角控制输出量
            boat1.CtrlSpeedOut = boat1.controller.command[4];//速度控制输出量
            boat1.controller.SendSPCommand();
            boat1.controller.GetShipData();

            targetLine = double.Parse(line_Y2.Text);//2号船目标线和圆
            targetCircle.Radius = double.Parse(circle_R2.Text);
            boat2.controller.command[3] = Control_fun(boat2.controller, boat2);//2号小船控制，2号小船为leader，无需控制速度
                                                                           /* if (AutoSpeed.Checked) 
                                                                                ship2Control.command[4] = ship2Control.Closed_Control_Speed(boat2, boat2.pos_X);
                                                                            else 
                                                                                ship2Control.command[4] = (byte)(int.Parse(Manualspeedset.Text));  */
            boat2.controller.command[4] = 100;
            boat2.CtrlRudOut = boat2.controller.command[3];//舵角控制输出量
            boat2.CtrlSpeedOut = boat2.controller.command[4];//速度控制输出量
            boat2.controller.SendSPCommand();
            boat2.controller.GetShipData();

            targetLine = double.Parse(line_Y3.Text);//2号船目标线和圆
            targetCircle.Radius = double.Parse(circle_R3.Text);
            boat3.controller.command[3] = Control_fun(boat2.controller, boat3);//3号小船控制
            if (AutoSpeed.Checked)
                boat3.controller.command[4] = boat3.controller.Closed_Control_Speed(boat3, boat2.pos_X);
            else
                boat3.controller.command[4] = (byte)(int.Parse(Manualspeedset.Text));
            // ship3Control.command[4] = 110;
            boat3.CtrlRudOut = boat3.controller.command[3];//舵角控制输出量
            boat3.CtrlSpeedOut = boat3.controller.command[4];//速度控制输出量
            boat3.controller.SendSPCommand();
            boat3.controller.GetShipData();
        }

        private byte Control_fun(ShipController shipControl, Ship shipData)
        {
            byte rudder = 0;
            #region 跟踪目标点
            if (path_mode.Text == "目标点")
            {
                rudder = shipControl.Closed_Control_Point(shipData, GlobalManager.Instance.Draw_View.targetPoint);
            }
            #endregion

            #region 跟随直线
            if (path_mode.Text == "直线")
            {
                rudder = shipControl.Closed_Control_Line(shipData, targetLine);
            }
            #endregion

            #region 跟随圆轨迹
            if (path_mode.Text == "圆轨迹")
            {
                rudder = shipControl.Closed_Control_Circle(shipData, targetCircle);
            }
            #endregion

            return rudder;
        }

    }
}
