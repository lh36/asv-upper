using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Upper.Model;
using System.Drawing;
using Upper.Bean;
using Upper.Manager;

namespace Upper.Controller
{
    public class ShipController
    {
        public byte[] command = new byte[6] { 0x00, 0x00, 0x06, 0x00, 0x00, 0xaa };

        public ShipController() { } //默认构造函数
        public ShipController(byte[] initCommand) //带参构造函数
        {
            this.command[0] = initCommand[0];
            this.command[1] = initCommand[1];
        }

        //向串口模块请求发送数据
        public void SendSPCommand()
        {
            SerialPortModel.Instance.SendCommand(command);
        }

        //获取Ship状态参数
        public void GetShipData()
        {
            command[3] = 0x47;
            SendSPCommand();
        }

        #region 开环控制区
        public void Speed_Up()
        {
            command[3] = 0x49;
            SendSPCommand();
        }

        public void Speed_Down()
        {
            command[3] = 0x44;
            SendSPCommand();
        }

        public void Left_Up()
        {
            command[3] = 0x51;
            SendSPCommand();
        }

        public void Left_Down()
        {
            command[3] = 0x4C;
            SendSPCommand();
        }

        public void Right_UP()
        {
            command[3] = 0x52;
            SendSPCommand();
        }

        public void Right_Down()
        {
            command[3] = 0x56;
            SendSPCommand();
        }

        public void Back_Off()
        {
            command[3] = 0x42;
            SendSPCommand();
        }

        public void Stop()
        {
            command[3] = 0x53;
            SendSPCommand();
        }
        #endregion

        //重置
        public void Reset()
        {
            command[3] = 0x53;
            SendSPCommand();
        }

        //遥控
        public void RemoteControl()
        {
            command[3] = 0x59;
            SendSPCommand();
        }

        //自动控制
        public void AutoControl()
        {
            command[3] = 0x5A;
            SendSPCommand();
        }

        #region 闭环控制区-航向控制
        /// <summary>
        /// 跟踪目标点
        /// </summary>
        /// <param name="port"></发送命令端口>
        /// <param name="boat"></当前船状态>
        /// <param name="targetPoint"></目标点>
        public byte Closed_Control_Point(Ship boat, Point targetPoint)
        {
            double current_c = boat.Control_Phi;//实际航向

            double distance = Math.Sqrt((boat.X_mm - targetPoint.X) * (boat.X_mm - targetPoint.X) + (boat.Y_mm - targetPoint.Y) * (boat.Y_mm - targetPoint.Y));//毫米单位的距离

            if (distance <= 800.0d)
            {
              //  Stop_Robot(port);
                GlobalManager.flag_ctrl = false;

                return 0x53;
            }

            else//距离目标点很远 需要控制
            {
                double target_c = Math.Atan2(targetPoint.Y - boat.Y_mm, targetPoint.X - boat.X_mm) / Math.PI * 180;

                double detc = target_c - current_c;	//detc

                if (target_c * current_c < -90 * 90)//处理正负180度附近的偏差值，如期望角和当前角分别是170和-170，则偏差为360-|170|-|-170|=20，而不用170+170=340，   -90*90是阈值
                {
                    detc = 360 - Math.Abs(target_c) - Math.Abs(current_c);
                    if (target_c > 0)
                        detc = -detc;  //若期望角为正，而实际角为负，则此时偏差值要取反
                }
                if (Math.Abs(detc) > 180)
                {
                    if (detc > 0)
                        detc -= 360;
                    else
                        detc += 360;
                }


                int R = (int)(boat.Kp * detc);
                if (R > 32)
                {
                    R = 32;
                }
                else if (R < -32)
                {
                    R = -32;
                }
                R = R + 32;
                return (byte)R;
              //  Send_Command(port);
              //  Get_ShipData(port);//获取最新船状态信息
            }
        }

        /// <summary>
        /// 跟踪目标直线
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></跟踪目标直线>
        public byte Closed_Control_Line(Ship boat, double line)
        {
            double k = 3.5d;//制导角参数
            double Err_phi = 0.0d;
            double y_d = line;//目标直线，单位为米

            double Ye = boat.pos_Y - y_d;//实际坐标减参考坐标
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            
            Err_phi = Ref_phi - boat.Control_Phi;
           

            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            if (Math.Abs(Ye) < 0.2)
            {
                boat.Err_phi_In += Err_phi;
            }

            int R = (int)(boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In);

            if (R > 32)
            {
                R = 32;
            }
            else if (R < -32)
            {
                R = -32;
            }
            R = R + 32;

            return (byte)R;
           // Send_Command(port);
           // Get_ShipData(port);//获取最新船状态信息
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        /// <param name="line"></param>
        public byte Closed_Control_Circle(Ship boat, Circle circle)
        {
            double Err_phi = 0.0d;
           // double ROBOTphi_r = 0.0d;//相对参考向的航向角或航迹角
            double k = 3.5d;

            
            double Radius = circle.Radius;//目标圆半径，将单位转为毫米
            double Center_X = circle.x;//圆心坐标
            double Center_Y = circle.y;

            double Ye = (Math.Sqrt((boat.pos_X - Center_X) * (boat.pos_X - Center_X) + (boat.pos_Y - Center_Y) * (boat.pos_Y - Center_Y)) - Radius);

            double Robot_xy = Math.Atan2(boat.pos_Y - Center_Y, boat.pos_X - Center_X) / Math.PI * 180;//航行器相对于原点的极坐标点
            double Dir_R = Robot_xy - 90;//圆切线角     得出航行器和制导角的参考0向，即极坐标的x轴，两者角度都是相对该轴的角度值

            if (Dir_R > 180) Dir_R = Dir_R - 360;
            else if (Dir_R < -180) Dir_R = Dir_R + 360;

            double errorRobot_Pos = boat.Control_Phi - Robot_xy;

            #region 获取当前制导角
            double Ref_phi = -Math.Atan(Ye / k) / Math.PI * 180;//制导角（角度制°）
            if ((errorRobot_Pos > 0 && errorRobot_Pos < 180) || (errorRobot_Pos < -180))//根据船向与顺逆边界的关系，选取制导角对称与否
            {
                if (Ref_phi < 0)
                {
                    Ref_phi = -180 - Ref_phi;
                }
                else
                {
                    Ref_phi = 180 - Ref_phi;
                }
            }
            #endregion

            Err_phi = Ref_phi-(boat.Control_Phi - Dir_R);//实际航向减去制导角的偏差
            if (Err_phi > 180)//偏差角大于180°时减去360°得到负值，表示航向左偏于制导角；偏差小于180°时表示航向右偏于制导角。
            {
                Err_phi = Err_phi - 360;
            }
            else if (Err_phi < -180)
            {
                Err_phi = Err_phi + 360;
            }

            if (Math.Abs(Ye) < 0.8)
            {
                boat.Err_phi_In += Err_phi;
            }

            int R = (int)(boat.Kp * Err_phi + boat.Ki * boat.Err_phi_In);

            //   R = (int)(boat.Kp * Err_phi);

            if (R > 32)
            {
                R = 32;
            }
            else if (R < -32)
            {
                R = -32;
            }
            R = R + 32;

            return (byte)R;
          //  Send_Command(port);
          //  Get_ShipData(port);//获取最新船状态信息
        }
        #endregion

        //闭环控制区-速度控制
        public byte Closed_Control_Speed(Ship boat, double leaderShip)
        {
            int U = (int)(boat.K1 * 100 + boat.K2 * (leaderShip - boat.pos_X - boat.dl_err));//设置领航者船2的速度档位10,为了与舵角区分，将命令加上100传输，范围
            if (U > 150)   //将速度档位范围保持在4~16范围内
            {
                U = 150;
            }
            else if (U < 50)
            {
                U = 50;
            }
            return (byte)U;
        }
        

        
    }
}
