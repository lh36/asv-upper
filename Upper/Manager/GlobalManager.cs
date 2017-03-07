using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Upper.Tool;
using Upper.Bean;
using Upper.Controller;
using Upper.View;
using System.Threading;

namespace Upper.Manager
{
    public class GlobalManager : Singleton<GlobalManager>
    {
        public MainView Main_View = null;
        public DrawView Draw_View = null;
        
        private List<Ship> lsShipList = new List<Ship>();   //Ship对象池

        public static bool flag_ctrl = false;               //控制线程标志
        public static bool flag_draw = false;               //绘画线程标志

        //构造函数
        public GlobalManager() { }

        //绑定主视图和绘画视图，初始化Ship对象池，并绑定控制器
        public void Init(MainView main_view, DrawView draw_view)
        {
            Main_View = main_view;
            Draw_View = draw_view;

            ShipController ship1Controller = new ShipController(Constant.Byte_Ship_1);
            ShipController ship2Controller = new ShipController(Constant.Byte_Ship_2);
            ShipController ship3Controller = new ShipController(Constant.Byte_Ship_3);

            AddShip(new Ship(ship1Controller));
            AddShip(new Ship(ship2Controller));
            AddShip(new Ship(ship3Controller));
        }

        /// <summary>
        /// 添加Ship至对象池
        /// </summary>
        /// <param name="ship">Ship对象</param>
        public void AddShip(Ship ship)
        {
            lsShipList.Add(ship);
        }

        /// <summary>
        /// 根据id获取Ship对象
        /// </summary>
        /// <param name="id">船号</param>
        /// <returns></returns>
        public Ship GetShip(int id)
        {
            return lsShipList[id - 1];
        }

        /// <summary>
        /// 开启绘画线程
        /// </summary>
        public void StartDrawThread()
        {
            Thread threadDraw = new Thread(Draw_View.DrawMap);
            threadDraw.IsBackground = true;
            threadDraw.Start();
        }

        /// <summary>
        /// 开启闭环控制线程
        /// </summary>
        public void StartCtrlThread()
        {
            Thread threadControl = new Thread(ShipCloseController.Instance.Control_PF);
            threadControl.IsBackground = true;
            threadControl.Start();
        }

        /// <summary>
        /// 开启Http传输线程
        /// </summary>
        public void StartHttpThread()
        {
            //TODO:Start the http thread
        }

    }
}
