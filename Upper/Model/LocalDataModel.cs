using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upper.Tool;
using Upper.Bean;
using System.IO;

namespace Upper.Model
{
    public class LocalDataModel : Singleton<LocalDataModel>
    {
        private string fileName = "";
        
        public LocalDataModel() { }

        //索引器：文件名查看与设置
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// 存储船舶状态信息
        /// </summary>
        /// <param name="ship">Ship对象</param>
        public void StoreShipData(Ship ship)
        {
            string strAppPath = System.Windows.Forms.Application.StartupPath;//获取相对路径

            using (FileStream fs = new FileStream(strAppPath + "\\db\\" + fileName + ".txt", FileMode.Append))
            {
                //数据保存信息量为：
                //船号，纬度，经度，X坐标(m)，Y坐标，航向角，航迹角，速度，速度等级，时间
                //在速度等级后面增加舵角信息，舵角控制输出量信息和速度控制输出量信息
                //共13个存储量
                string str_data = ship.ShipID.ToString() + "," + ship.Lat.ToString("0.00000000") + "," + ship.Lon.ToString("0.00000000") + ","
                                + ship.pos_X.ToString("0.000") + "," + ship.pos_Y.ToString("0.000") + "," + ship.phi.ToString("0.0") + "," + ship.GPS_Phi.ToString("0.0") + ","
                                + ship.speed.ToString("0.00") + "," + ship.gear.ToString() + "," + ship.rud.ToString("0.0") + ','
                                + ship.CtrlRudOut.ToString() + ',' + ship.CtrlSpeedOut.ToString() + ','
                                + ship.Time.ToString();//将数据转换为字符串

                byte[] data = System.Text.Encoding.Default.GetBytes(str_data);
                byte[] data3 = new byte[2];
                data3[0] = 0x0d; data3[1] = 0x0a;
                //开始写入
                fs.Write(data, 0, data.Length);

                fs.Write(data3, 0, data3.Length);

                //清空缓冲区、关闭流
                fs.Flush();
                fs.Close();
            }
        }

    }
}
