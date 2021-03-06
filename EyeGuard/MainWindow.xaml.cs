﻿using EyeGuard.UI;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;//添加引用，必须用到的
using System.Windows.Input;
using System.Windows.Threading;
using static EyeGuard.Model;
using Application = System.Windows.Application;

namespace EyeGuard
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //实例化notifyIOC控件最小化托盘
        private NotifyIcon notifyIcon = null;

        // 最小化系统托盘
        private void initialTray()
        {
            //设置托盘的各个属性
            notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = "眼睛卫士已经启动";//托盘气泡显示内容
            notifyIcon.Text = "珍爱生命，保护眼睛";
            notifyIcon.Visible = true;//托盘按钮是否可见
            //重要提示：此处的图标图片在resouces文件夹。不可删除，否则会死机
            string path = AppDomain.CurrentDomain.BaseDirectory + "Resources/favicon.ico";
            notifyIcon.Icon = new System.Drawing.Icon(path);//托盘中显示的图标
            notifyIcon.ShowBalloonTip(1000);//托盘气泡显示时间

            //鼠标点击事件
            notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseClick);

            //右键菜单--设置面板
            MenuItem SetupPanel = new MenuItem("设置面板");
            SetupPanel.Click += new EventHandler(SetupPanel_Click);

            //右键菜单--清零工作时间
            MenuItem reset_time = new MenuItem("重置工作时间");
            reset_time.Click += new EventHandler(ResetTime_Click);

            //右键菜单--显示&隐藏|桌面插件
            MenuItem DesktopControls = new MenuItem("桌面插件");
            //二级菜单
            MenuItem Display = new MenuItem("显示");
            Display.Tag = "display";
            Display.Click += new EventHandler(WhetherToDisplay_Click);
            MenuItem Hide = new MenuItem("隐藏");
            Hide.Tag = "hide";
            Hide.Click += new EventHandler(WhetherToDisplay_Click);
            DesktopControls.MenuItems.Add(Display);
            DesktopControls.MenuItems.Add(Hide);

            //右键菜单--手动锁定
            MenuItem LockScreen = new MenuItem("锁屏");
            LockScreen.Click += new EventHandler(LockScreen_Click);


            //开机启动项
            MenuItem Whether = new MenuItem("是否开机自启");
            //二级菜单
            MenuItem SelfStarting = new MenuItem("开机自启");
            SelfStarting.Tag = "SelfStarting";
            SelfStarting.Click += new EventHandler(StartupItem_Click);
            MenuItem SelfCancellation = new MenuItem("取消开机自启");
            SelfCancellation.Tag = "SelfCancellation";
            SelfCancellation.Click += new EventHandler(StartupItem_Click);
            Whether.MenuItems.Add(SelfStarting);
            Whether.MenuItems.Add(SelfCancellation);


            //右键菜单--退出菜单项
            MenuItem exit = new MenuItem("退出");
            exit.Click += new EventHandler(exit_Click);


            //关联托盘控件
            MenuItem[] childen = new MenuItem[] {SetupPanel, reset_time,DesktopControls, LockScreen , Whether, exit };

            notifyIcon.ContextMenu = new ContextMenu(childen);
        }


        /// <summary>
        /// 重置工作时间
        /// </summary>
        private void ResetTime_Click(object sender, EventArgs e)
        {
            Count = 0;
        }


        /// <summary>
        /// 启动项管理
        /// </summary>
        private void StartupItem_Click(object sender, EventArgs e)
        {
            //状态
            string state = string.Empty;
            try
            {
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)sender;
                state = mi.Tag.ToString();
            }
            catch
            {
                System.Windows.Forms.MenuItem mi = (System.Windows.Forms.MenuItem)sender;
                state = mi.Tag.ToString();
            }

            //开机启动
            if (state == "SelfStarting")
            {
                Bll.BootUp();
                if (Tips.Function == false)
                {
                    Tips tp = new Tips("已经设置开机自启~");
                    tp.Show();
                }
            }
            else
            {
                Bll.BootUp(false);
                if (Tips.Function == false)
                {
                    Tips tp = new Tips("已经取消了开机自启~");
                    tp.Show();
                }
            }
        }

        /// <summary>
        /// 手动锁定
        /// </summary>
        private void LockScreen_Click(object sender, EventArgs e)
        {
            if (LockScreen.Function == false)
            {
                md.State = (state)1;
                LockScreen ls = new LockScreen();
                ls.md = md;
                ls.Show();
            }
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetupPanel_Click(object sender, EventArgs e)
        {
            if (SetUp.Function == false)
            {
                SetUp sp = new SetUp(md);
                sp.Show();
            }
        }

        /// <summary>
        /// 显示&隐藏桌面插件
        /// </summary>
        private void WhetherToDisplay_Click(object sender, EventArgs e)
        {
            Dal dal = new Dal();
            //状态
            string state = string.Empty;
            try
            {
                System.Windows.Controls.MenuItem mi = (System.Windows.Controls.MenuItem)sender;
                state = mi.Tag.ToString();
            }
            catch 
            {
                try
                {
                    MenuItem mi = (MenuItem)sender;
                    state = mi.Tag.ToString();
                }
                catch 
                {

                }
            }
           
            if (state == "hide")
            {
                this.Visibility = Visibility.Hidden;
                //解决最小化到任务栏可以强行关闭程序的问题。
                this.ShowInTaskbar = false;//使Form不在任务栏上显示
                md.Display = 0;
                dal.SetData(md);
                if (Tips.Function == false)
                {
                    Tips tp = new Tips("桌面插件已经隐藏~");
                    tp.Show();
                }

            }
            else
            {
                this.Visibility = Visibility.Visible;
                //解决最小化到任务栏可以强行关闭程序的问题。
                this.ShowInTaskbar = false;//使Form不在任务栏上显示
                this.Activate();
                md.Display = 1;
                dal.SetData(md);
                if (Tips.Function == false)
                {
                    Tips tp = new Tips("桌面插件已经恢复显示~");
                    tp.Show();
                }


            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        private void exit_Click(object sender, EventArgs e)
        {
            //退出
            Application.Current.Shutdown();
        }


        // 托盘图标鼠标单击事件
        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //鼠标左键
            if (e.Button == MouseButtons.Left)
            {
                if ((int)md.TimerMode == 2)
                {
                    Count = 0;
                    if (Tips.Function == false)
                    {
                        Tips tp = new Tips("加班模式下点击托盘会进行重新计时，您当前的工作时间已被重置~");
                        tp.Show();
                    }
                    return;
                }
                


                if (Tips.Function == false)
                {
                    Tips tp = new Tips("您已经工作了"+(Count/60)+"分钟，"+ (md.Work-(Count / 60)) + "分后进入休息时间");
                    tp.Show();
                }
            }
        }

        /// <summary>
        /// 时钟
        /// </summary>
        private DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            double ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;//WPF
            this.Top = 90;
            this.Left = ScreenWidth-250;
            //隐藏控件
            initialTray();
            md = bll.Initialization();

            DateTime d2 = Convert.ToDateTime(DateTime.Now.ToShortDateString().ToString());
            DateTime d4 = Convert.ToDateTime(string.Format("{0}-{1}-{2}", d2.Year, d2.Month, d2.Day));
            //执行时间为1秒一次
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        /// <summary>
        /// 当前总秒数
        /// </summary>
        private long Count = 0;

        /// <summary>
        /// 空闲时间统计
        /// </summary>
        private long FreeCount = 0;

        /// <summary>
        /// 时钟事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {

            if ((int)md.State == 1)
            {
                Count = 0;
                return;
            }
            
            //正常模式 = 0, 智能计时 = 1, 加班模式 = 2, 游戏模式 = 3
            switch ((int)md.TimerMode)
            {
                //正常模式
                case 0:
                    {
                        Count++;
                        break;
                    }
                //智能计时
                case 1:
                    {
                        //判断是否全屏
                        if(!bll.FullScreen()) //非全屏
                        {
                            //判断系统是否处于空闲时间
                            if (Bll.GetLastInputTime() < 1000)
                            {
                                //如果不处于空闲时间
                                Count++;
                                //清空暂离状态
                                FreeCount = 0;
                            }
                            else
                            {
                                //判断是否处于暂离状态
                                FreeCount++;
                                //如果电脑5分无人进行操作，那么就重新开始计时
                                if (FreeCount >= 300)
                                {
                                    //重新开始计时
                                    Count = 0;
                                }
                            }
                        }
                            break;
                    }
                //加班模式
                case 2:
                    {
                        Count++;
                        break;
                    }
                //游戏模式
                case 3:
                    {
                        //判断是否全屏
                        if (!bll.FullScreen()) //非全屏
                        {
                            //判断系统是否处于空闲时间
                            if (Bll.GetLastInputTime() < 1000)
                            {
                                //如果不处于空闲时间
                                Count++;
                                //清空暂离状态
                                FreeCount = 0;
                            }
                            else
                            {
                                //判断是否处于暂离状态
                                FreeCount++;
                                //如果电脑5分无人进行操作，那么就重新开始计时
                                if (FreeCount >= 300)
                                {
                                    //重新开始计时
                                    Count = 0;
                                }
                            }
                        }
                        else //全屏中
                        {
                            //判断系统是否处于空闲时间
                            if (Bll.GetLastInputTime() < 1000)
                            {
                                //判断工作时间是否已经到达，在游戏模式中，如果检测到全屏，会在即将锁屏的前一秒停止计时
                                if (md.Work * 60 > (Count-1))
                                {
                                    Count++;
                                }

                                //清空暂离状态
                                FreeCount = 0;
                            }
                            else
                            {
                                //判断是否处于暂离状态
                                FreeCount++;
                                //如果电脑5分无人进行操作，那么就重新开始计时
                                if (FreeCount >= 300)
                                {
                                    //重新开始计时
                                    Count = 0;
                                }
                            }
                        }
                        break;
                    }

            }

            if (md.Work * 60 >= Count)
            {
                Time.Content = Bll.GetFormattingTime(Count.ToString());
                md.AlreadyWorked = md.Work / 60;
            }


            //判断是否启用自动关机
            if (md.Shutdown.Time != -1 && md.Shutdown.Branch != -1)
            {
                //分割时间
                string[] time = DateTime.Now.ToLongTimeString().ToString().Split(new string[] { ":" }, StringSplitOptions.None);

                //关机前的提醒，如果是整点，也就是0，你需要做好这个判断

                //是否为0分
                if (md.Shutdown.Branch == 0)
                {
                    //是否为0时
                    if (md.Shutdown.Time == 0)
                    {
                        if (Convert.ToInt32(time[0]) == 23 && Convert.ToInt32(time[1]) == 59 && Convert.ToInt32(time[2]) == 3)
                        {
                            if (Tips.Function == false)
                            {
                                Tips tp = new Tips("当前时间为：" + DateTime.Now.ToLongTimeString().ToString() + "  距离关机还有1分钟，请您注意保存好数据信息~");
                                tp.Show();
                            }
                        }
                    }
                    else
                    {
                        if (Convert.ToInt32(time[0]) == (md.Shutdown.Time - 1) && Convert.ToInt32(time[1]) == 59 && Convert.ToInt32(time[2]) == 3)
                        {
                            if (Tips.Function == false)
                            {
                                Tips tp = new Tips("当前时间为：" + DateTime.Now.ToLongTimeString().ToString() + "  距离关机还有1分钟，请您注意保存好数据信息~");
                                tp.Show();
                            }
                        }
                    }
                }
                else
                {
                    //到达关机时间
                    if (Convert.ToInt32(time[0]) == md.Shutdown.Time && Convert.ToInt32(time[1]) == (md.Shutdown.Branch-1) && Convert.ToInt32(time[2]) == 3)
                    {
                        if (Tips.Function == false)
                        {
                            Tips tp = new Tips("当前时间为：" + DateTime.Now.ToLongTimeString().ToString() + "  距离关机还有1分钟，请您注意保存好数据信息~");
                            tp.Show();
                        }
                    }
                }

                //到达关机时间
                if (Convert.ToInt32(time[0]) == md.Shutdown.Time && Convert.ToInt32(time[1]) == md.Shutdown.Branch && Convert.ToInt32(time[2]) == 3)
                {
                    timer.Stop();
                    Process.Start("shutdown", " -s -t 0");

                }


            }

            //休息前的提醒
            if ((md.Work - 1) * 60 == Count)
            {
                if (Tips.Function == false)
                {
                    Tips tp = new Tips("您已经工作了" + (Count / 60) + "分钟，1分钟后进入休息时间！");
                    tp.Show();
                }
            }

            //到达休息时间
            if (md.Work * 60 == Count)
            {
                md.State = (state)1;
                if (LockScreen.Function == false)
                {
                    LockScreen ls = new LockScreen(md);
                    ls.Show();
                }
            }

        }


        /// <summary>
        /// 实例化类
        /// </summary>
        Bll bll = new Bll();
        Model md = new Model();

        /// <summary>
        /// 窗口初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            md = bll.Initialization();
            if (md.Display == 0)
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 无边框移动
        /// </summary>
        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
