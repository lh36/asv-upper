using Upper.Controller;
using Upper.Model;
using Upper.Tool;
using Upper.Bean;
using Upper.Manager;
using Upper.View;
using System;
using System.Windows.Forms;

namespace Upper
{
    public partial class  MainView : Form
    {
        public MainView()
        {
            InitializeComponent();
            //模块初始化
            InitModules();
        }

        private void InitModules()
        {
            //串口模块初始化
            SerialPortModel.Instance.Init(serialPort1, timer1);
            //全局管理器初始化
            GlobalManager.Instance.Init(this, new DrawView(PathMap, path_mode, line_Y2, circle_X, circle_Y, circle_R1, tar_Point_X, tar_Point_Y));
            //闭环控制器初始化
            ShipCloseController.Instance.Init(Phi_mode, Boat1_Kp, Boat1_Ki, Boat1_Kd, Boat1_K1,
                                                Boat1_K2, Boat1_DL, Boat2_Kp, Boat2_Ki, Boat2_Kd, Boat2_K1,
                                                Boat2_K2, Boat2_DL, Boat3_Kp, Boat3_Ki, Boat3_Kd, Boat3_K1,
                                                Boat3_K2, Boat3_DL, line_Y1, line_Y2, line_Y3, circle_R1,
                                                circle_R2, circle_R3, circle_X, circle_Y, AutoSpeed, Manualspeedset, path_mode);
        }

        //显示运行参数
        public void ShowShipParam(Ship shipData)
        {
            int id = shipData.ShipID;
            switch(id)
            {
                case 1:
                    Boat1_X.Text = shipData.pos_X.ToString("0.00");
                    Boat1_Y.Text = shipData.pos_Y.ToString("0.00");
                    Boat1_phi.Text = shipData.phi.ToString("0.00");
                    Boat1_Ru.Text = shipData.rud.ToString("0.0");
                    Boat1_speed.Text = shipData.speed.ToString("0.000");
                    Boat1_grade.Text = shipData.gear.ToString();
                    Boat1_time.Text = shipData.Time;
                    break;
                case 2:
                    Boat2_X.Text = shipData.pos_X.ToString("0.00");
                    Boat2_Y.Text = shipData.pos_Y.ToString("0.00");
                    Boat2_phi.Text = shipData.phi.ToString("0.00");
                    Boat2_Ru.Text = shipData.rud.ToString("0.0");
                    Boat2_speed.Text = shipData.speed.ToString("0.000");
                    Boat2_grade.Text = shipData.gear.ToString();
                    Boat2_time.Text = shipData.Time;
                    break;
                case 3:
                    Boat3_X.Text = shipData.pos_X.ToString("0.00");
                    Boat3_Y.Text = shipData.pos_Y.ToString("0.00");
                    Boat3_phi.Text = shipData.phi.ToString("0.00");
                    Boat3_Ru.Text = shipData.rud.ToString("0.0");
                    Boat3_speed.Text = shipData.speed.ToString("0.000");
                    Boat3_grade.Text = shipData.gear.ToString();
                    Boat3_time.Text = shipData.Time;
                    break;
                default:
                    break;
            }
        }

        //船舶数据接收定时器触发
        private void timer1_Tick(object sender, EventArgs e)
        {
            SerialPortModel.Instance.TimerTrigger();
        }

        //串口数据接收回调触发
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            SerialPortModel.Instance.ReceiveData();
        }

        //串口状态按钮点击触发
        private void ComOpen1_Click(object sender, EventArgs e)
        {
            string sCommNum = this.ComPortNum1.Text;
            int iBaudRate = Convert.ToInt32(this.BaudRate1.Text);      //转换方法1

            if (ComOpen1.Text == "打开串口")//打开串口
            {
                Boolean isOK = SerialPortModel.Instance.ChangeSerialPortStatus(sCommNum, iBaudRate);
                if (isOK)
                {
                    ComOpen1.Text = "关闭串口";
                    ComPortNum1.Enabled = false;
                    BaudRate1.Enabled = false;
                }
                else
                {
                    MessageBox.Show("串口打开失败！\r\n");
                }
            }
            else//关闭串口
            {
                Boolean isOK = SerialPortModel.Instance.ChangeSerialPortStatus(sCommNum, iBaudRate);
                if (isOK)
                {
                    ComOpen1.Text = "打开串口";
                    ComPortNum1.Enabled = true;
                    BaudRate1.Enabled = true;
                }
                else
                {
                    MessageBox.Show("串口关闭失败！\r\n");
                }
            }

        }

        //串口状态检查
        private Boolean CheckSerialPort()
        {
            if (SerialPortModel.Instance.IsOpen)
            {
                return true;
            }
            else
            {
                MessageBox.Show("请先打开串口！\r\n");
                return false;
            }
        }

        // 开始按钮点击触发
        private void Start_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            if (Start.Text == "开始")
            {
                //TODO
                GlobalManager.flag_draw = true;

                SerialPortModel.Instance.SetTimerState(true);//默认是开环控制，则启动串口状态参数接收定时器
                Start.Text = "停止";

            }
            else if (Start.Text == "停止")
            {
                //TODO
                GlobalManager.flag_draw = false;
                GlobalManager.flag_ctrl = false;//控制线程标志

                SerialPortModel.Instance.SetTimerState(false);
                Start.Text = "开始";
            }
        }
        
        //自主、遥控按钮点击触发
        private void Switch_Click(object sender, EventArgs e)
        {
            if (Switch.Text == "自主")
            {
                GlobalManager.Instance.GetShip(1).Controller.RemoteControl();
                Switch.Text = "遥控";
            }
            else
            {
                GlobalManager.Instance.GetShip(1).Controller.AutoControl();
                Switch.Text = "自主";
            }
        }

        //获取当前可控制的Ship的Id
        private int GetShipControlledId()
        {
            if (asv1.Checked)
            {
                return 1;
            }
            else if (asv2.Checked)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        //前进按钮点击触发
        private void Advance_Click(object sender, EventArgs e)
        {
            //检查串口
            if(!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Speed_Up();
            
        }

        //后退按钮点击触发
        private void Back_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Speed_Down();
        }

        //倒车按钮点击触发
        private void Backoff_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Back_Off();
        }

        //停止按钮点击触发
        private void Stop_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(1).Controller.Stop();
            GlobalManager.Instance.GetShip(2).Controller.Stop();
            GlobalManager.Instance.GetShip(3).Controller.Stop();
        }

        //左上按钮点击触发
        private void leftup_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Left_Up();
        }

        //左下按钮点击触发
        private void leftdown_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Left_Down();
        }

        //右上按钮点击触发
        private void rightup_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Right_UP();
        }

        //右下按钮点击触发
        private void rightdown_Click(object sender, EventArgs e)
        {
            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(GetShipControlledId()).Controller.Right_Down();
        }

        //复位按钮点击触发
        private void Reset_Click(object sender, EventArgs e)
        {
            GlobalManager.Instance.Draw_View.ResetView();

            //检查串口
            if (!CheckSerialPort())
            {
                return;
            }

            GlobalManager.Instance.GetShip(1).Controller.Reset();
            GlobalManager.Instance.GetShip(2).Controller.Reset();
            GlobalManager.Instance.GetShip(3).Controller.Reset();
        }

        //绘画板点击事件触发
        private void PathMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GlobalManager.Instance.Draw_View.GetMousePositon(e);
            }
        }

        //闭环控制开始按钮触发
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "开始跟随")
            {
                LocalDataModel.Instance.FileName = DateTime.Now.ToString("yyyyMMddHHmmss");//设置txt文件名

                timer1.Enabled = false;//首先关闭开环定时器获取当前状态信息的定时器
                GlobalManager.flag_ctrl = true;
                GlobalManager.Instance.StartCtrlThread();

                button1.Text = "停止跟随";

            }
            else
            {
                timer1.Enabled = true;//关闭闭环控制后，重新开启开环获取船位姿状态信息
                GlobalManager.flag_ctrl = false;

                GlobalManager.Instance.GetShip(1).Controller.Stop();
                GlobalManager.Instance.GetShip(2).Controller.Stop();
                GlobalManager.Instance.GetShip(3).Controller.Stop();

                button1.Text = "开始跟随";
            }

        }

        private void boat1_init_Phi_Click(object sender, EventArgs e)
        {
            GlobalManager.Instance.GetShip(1).Phi_buchang = -GlobalManager.Instance.GetShip(1).Init_Phi;
        }

        private void boat2_init_Phi_Click(object sender, EventArgs e)
        {
            GlobalManager.Instance.GetShip(1).Phi_buchang = -GlobalManager.Instance.GetShip(1).Init_Phi;
        }

        private void boat3_init_Phi_Click(object sender, EventArgs e)
        {
            GlobalManager.Instance.GetShip(1).Phi_buchang = -GlobalManager.Instance.GetShip(1).Init_Phi;
        }




































        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        

        
    }
}
