using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Upper.Bean;
using Upper.Manager;

namespace Upper.View
{
    public class DrawView
    {
        private PictureBox canvas = null;
        private ComboBox path_mode = null;
        private TextBox line_Y2 = null;
        private TextBox circle_X = null;
        private TextBox circle_Y = null;
        private TextBox circle_R1 = null;
        private TextBox tar_Point_X = null;
        private TextBox tar_Point_Y = null;

        public DrawView(PictureBox canvas, ComboBox path_mode, TextBox line_Y2,
                        TextBox circle_X, TextBox circle_Y, TextBox circle_R1,
                        TextBox tar_Point_X, TextBox tar_Point_Y)
        {
            this.canvas = canvas;
            this.path_mode = path_mode;
            this.line_Y2 = line_Y2;
            this.circle_X = circle_X;
            this.circle_Y = circle_Y;
            this.circle_R1 = circle_R1;
            this.tar_Point_X = tar_Point_X;
            this.tar_Point_Y = tar_Point_Y;
        }
        
        public Point targetPoint;  //目标点
        
        static int halfHeight_mm = 55000;//地图一半长55米

        Point target_pt = new Point();//捕获鼠标按下去的点，以得到目标点
        static List<Point> listPoint_Boat1 = new List<Point>();
        static List<Point> listPoint_Boat2 = new List<Point>();
        static List<Point> listPoint_Boat3 = new List<Point>();

        public void DrawMap()
        {
            while (GlobalManager.flag_draw)
            {
                Graphics g = canvas.CreateGraphics();
                g.Clear(Color.White);
                Pen p = new Pen(Color.Black, 2);//定义了一个蓝色,宽度为2的画笔
                g.DrawLine(p, 0, canvas.Height / 2, canvas.Width, canvas.Height / 2);//在画板上画直线,起始坐标为(10,10),终点坐标为(100,100)
                g.DrawLine(p, canvas.Width / 2, 0, canvas.Width / 2, canvas.Height);
                //地图像素大小
                int Widthmap = canvas.Width / 2;
                int Heightmap = canvas.Height / 2;

                //实际大小
                int Heigh_mm = halfHeight_mm;
                int Width_mm = Heigh_mm / Heightmap * Widthmap;

                //比例尺和反比例尺
                double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
                double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

                int paint_x1 = Widthmap - (int)(GlobalManager.Instance.GetShip(1).Y_mm * paint_scale);//转换为图上的坐标
                int paint_y1 = Heightmap - (int)(GlobalManager.Instance.GetShip(1).X_mm * paint_scale);

                int paint_x2 = Widthmap - (int)(GlobalManager.Instance.GetShip(2).Y_mm * paint_scale);//转换为图上的坐标
                int paint_y2 = Heightmap - (int)(GlobalManager.Instance.GetShip(2).X_mm * paint_scale);

                int paint_x3 = Widthmap - (int)(GlobalManager.Instance.GetShip(3).Y_mm * paint_scale);//转换为图上的坐标
                int paint_y3 = Heightmap - (int)(GlobalManager.Instance.GetShip(3).X_mm * paint_scale);

                #region 画目标直线和圆
                if (GlobalManager.flag_ctrl)
                {
                    if (path_mode.Text == "直线")
                    {
                        int x = Widthmap - (int)(Convert.ToInt32(this.line_Y2.Text) * 1000 * paint_scale);
                        g.DrawLine(new Pen(Color.Blue, 2), x, 0, x, canvas.Height);
                    }
                    else if (path_mode.Text == "圆轨迹")
                    {
                        int x = Convert.ToInt32(this.circle_X.Text);
                        int y = Convert.ToInt32(this.circle_Y.Text);
                        int r = Convert.ToInt32(this.circle_R1.Text);

                        int x1 = Widthmap - (int)((y + r) * 1000 * paint_scale);
                        int y1 = Heightmap - (int)((x + r) * 1000 * paint_scale);

                        g.DrawEllipse(new Pen(Color.Cyan, 2), x1, y1, (int)(r * 1000 * paint_scale) * 2, (int)(r * 1000 * paint_scale) * 2);

                    }
                }
                #endregion
                listPoint_Boat1.Add(new Point(paint_x1, paint_y1));
                listPoint_Boat2.Add(new Point(paint_x2, paint_y2));
                listPoint_Boat3.Add(new Point(paint_x3, paint_y3));
                if (listPoint_Boat1.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Red, 2), listPoint_Boat1.ToArray());
                }
                if (listPoint_Boat2.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Green, 2), listPoint_Boat2.ToArray());
                }
                if (listPoint_Boat3.Count >= 2)
                {
                    g.DrawCurve(new Pen(Color.Gold, 2), listPoint_Boat3.ToArray());
                }
                if (path_mode.Text == "目标点")//绘制目标点
                {
                    g.DrawRectangle(new Pen(Color.Red, 2), target_pt.X - 4, target_pt.Y - 4, 4, 4);
                }
                Thread.Sleep(200);
            }

        }

        public void GetMousePositon(MouseEventArgs e)
        {
            target_pt = new Point(e.X, e.Y);
            //   Graphics g = this.PathMap.CreateGraphics();
            //地图像素大小
            int Widthmap = canvas.Width / 2;
            int Heightmap = canvas.Height / 2;

            //实际大小
            int Heigh_mm = halfHeight_mm;
            int Width_mm = Heigh_mm / Heightmap * Widthmap;

            //比例尺和反比例尺
            double scale = Heigh_mm / Heightmap;//单位像素代表的实际长度，单位：mm
            double paint_scale = 1 / scale;//每毫米在图上画多少像素，单位：像素

            targetPoint.X = (int)((Heightmap - target_pt.Y) * scale);//得到以毫米为单位的目标X值
            targetPoint.Y = (int)((Widthmap - target_pt.X) * scale);//得到以毫米为单位的目标点Y值

            tar_Point_X.Text = (targetPoint.X / 1000).ToString();
            tar_Point_Y.Text = (targetPoint.Y / 1000).ToString();

            GlobalManager.flag_ctrl = true;
        }


        public void ResetView()
        {
            listPoint_Boat1.Clear();
            listPoint_Boat2.Clear();
            listPoint_Boat3.Clear();
        }


    }
}
