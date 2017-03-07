using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Upper.Tool;
using Upper.Bean;
using Upper.Manager;
using System.Threading;

namespace Upper.Model
{
    public class SerialPortModel : Singleton<SerialPortModel>
    {
        private SerialPort serialPort = null;
        private System.Windows.Forms.Timer timer = null;

        private Boolean bIsOpen = false;

        //构造函数
        public SerialPortModel(){}

        //初始化函数
        public void Init(SerialPort serialPort, System.Windows.Forms.Timer timer)
        {
            this.serialPort = serialPort;
            this.timer = timer;
        }

        //参数索引器：串口状态查看与设定
        public Boolean IsOpen
        {
            get { return bIsOpen; }
            set
            {
                if(value)
                {
                    serialPort.Open();
                    bIsOpen = true;
                }
                else
                {
                    serialPort.Close();
                    bIsOpen = false;
                }
            }
        }

        /// <summary>
        /// 串口状态变更
        /// </summary>
        /// <param name="portName">串口名</param>
        /// <param name="baudRate">波特率</param>
        /// <returns></returns>
        public Boolean ChangeSerialPortStatus(string portName, int baudRate)
        {
            if (!serialPort.IsOpen)
            {
                try   //try:尝试打开串口，如果错误就返回false
                {
                    serialPort.PortName = portName;
                    serialPort.BaudRate = baudRate;
                    serialPort.Open();
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try   //try:尝试关闭串口，如果错误就返回false
                {
                    timer.Enabled = false;
                    serialPort.Close();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public void SetTimerState(Boolean isOpen)
        {
            timer.Enabled = isOpen;
        }

        //定时器触发函数
        public void TimerTrigger()
        {
            //发送船舶状态参数获取信息
            GlobalManager.Instance.GetShip(1).Controller.GetShipData();
            Thread.Sleep(40);
            GlobalManager.Instance.GetShip(2).Controller.GetShipData();
            Thread.Sleep(40);
            GlobalManager.Instance.GetShip(3).Controller.GetShipData();
        }

        //接收串口数据
        public void ReceiveData()
        {
            byte[] response_data = RecvSeriData.ReceData(serialPort);
            
            if (response_data != null)
            {
                byte ID_Temp = response_data[3];
                Ship ship = GlobalManager.Instance.GetShip(ID_Temp);

                ship.UpdataStatusData(response_data);
                if(GlobalManager.flag_ctrl)
                {
                    LocalDataModel.Instance.StoreShipData(ship);
                }

                Array.Clear(response_data, 0, response_data.Length);

                GlobalManager.Instance.Main_View.ShowShipParam(ship);//有数据更新时才更新显示，否则不更新（即不是每次接收到数据才更新，只有接收到正确的数据才更新）
            }
        }

        //发送数据
        public void SendCommand(byte[] command)
        {
            serialPort.Write(command, 0, 6);
        }










    }
}
