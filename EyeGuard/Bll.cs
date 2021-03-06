﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EyeGuard
{
    class Bll
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns>返回实体类</returns>
        public Model Initialization()
        {
            //实例化类
            Dal dl = new Dal();
            return dl.ReturnData();
        }


        /// <summary>
        /// 写入配置
        /// </summary>
        /// <param name="md"></param>
        public void SetData(Model md)
        {
            //实例化类
            Dal dl = new Dal();
            dl.SetData(md);
        }

        /// <summary>
        /// 获取中英文混排字符串的实际长度(字节数)
        /// </summary>
        /// <param name="str">要获取长度的字符串</param>
        /// <returns>字符串的实际长度值（字节数）</returns>
        public static int GetStringLength(string str)
        {
            if (str.Equals(string.Empty))
                return 0;
            int strlen = 0;
            ASCIIEncoding strData = new ASCIIEncoding();
            //将字符串转换为ASCII编码的字节数字
            byte[] strBytes = strData.GetBytes(str);
            for (int i = 0; i <= strBytes.Length - 1; i++)
            {
                if (strBytes[i] == 63)  //中文都将编码为ASCII编码63,即"?"号
                    strlen++;
                strlen++;
            }
            return strlen;
        }

        /// <summary>
        /// 格式化时间
        /// </summary>
        /// <param name="value">需要被处理的值</param>
        /// <returns>返回格式化的值</returns>
        public static string GetFormattingTime(string value)
        {
            int TotalSeconds = Convert.ToInt32(value);

            int branch = TotalSeconds / 60;
            int second= TotalSeconds % 60;
            return GetFormattingTime(branch) + ":" + GetFormattingTime(second);
        }

       
        /// <summary>
        /// 给单数值加0
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetFormattingTime(int value)
        {
            if (value.ToString().Length == 1)
            {
                return "0" + value;
            }
            return value.ToString();
        }

        #region 获取键盘和鼠标没有操作的时间
        // 创建结构体用于返回捕获时间  
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            // 设置结构体块容量  
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            // 捕获的时间  
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        //获取键盘和鼠标没有操作的时间  
        public static long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            // 捕获时间  
            if (!GetLastInputInfo(ref vLastInputInfo))
                return 0;
            else
                return Environment.TickCount - (long)vLastInputInfo.dwTime;
        }
        #endregion

        #region 检测是否全屏

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //取得前台窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        //取得桌面窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        //取得Shell窗口句柄函数 
        [DllImport("user32.dll")]
        private static extern IntPtr GetShellWindow();
        //取得窗口大小函数 
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowRect(IntPtr hwnd, out RECT rc);

        //桌面窗口句柄 
        private IntPtr desktopHandle; //Window handle for the desktop  
        //Shell窗口句柄 
        private IntPtr shellHandle; //Window handle for the shell  因为桌面窗口和Shell窗口也是全屏，要排除在其他全屏程序之外。 


        /// <summary>
        /// 检测是否全屏
        /// </summary>
        /// <returns>返回true则是全屏</returns>
        public  bool FullScreen()
        {
            //取得桌面和Shell窗口句柄 
            desktopHandle = GetDesktopWindow();
            shellHandle = GetShellWindow();
            RECT appBounds;
            Rectangle screenBounds;
            IntPtr hWnd;
            //取得前台窗口 
            hWnd = GetForegroundWindow();
            if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
            {
                //判断是否桌面或shell        
                if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(shellHandle)))
                {
                    //取得窗口大小 
                    GetWindowRect(hWnd, out appBounds);
                    //判断是否全屏 
                    screenBounds = Screen.FromHandle(hWnd).Bounds;
                    if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width)
                    {
                        return true;

                    }

                }
            }
            return false;

        }
        #endregion


        /// <summary>
        /// 快捷方式所在目录
        /// </summary>
        private static string Iopath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\眼睛卫士.vbs";


        /// <summary>
        /// 创建快捷方式位置
        /// </summary>
        /// <param name="directory">快捷方式存放的位置</param>
        /// <param name="shortcutName">快捷方式名称</param>
        /// <param name="targetPath">程序路径</param>
        /// <param name="description">描述</param>
        /// <param name="iconLocation">程序图标</param>
        /// <returns></returns>
        public static bool Create(string directory, string shortcutName, string targetPath,
    string description = null, string iconLocation = null)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                //添加引用 Com 中搜索 Windows Script Host Object Model
                string shortcutPath = Path.Combine(directory, string.Format("{0}.lnk", shortcutName));
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);//创建快捷方式对象
                shortcut.TargetPath = targetPath;//指定目标路径
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);//设置起始位置
                shortcut.WindowStyle = 1;//设置运行方式，默认为常规窗口
                shortcut.Description = description;//设置备注
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;//设置图标路径
                shortcut.Save();//保存快捷方式

                return true;
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 是否开机启动
        /// </summary>
        /// <param name="Whether"></param>
        public static void BootUp(bool Whether = true)
        {
            try
            {
                var value = Application.ExecutablePath.Replace("/", "\\");
                if (Whether)
                {
                    var currentUser = Registry.CurrentUser;
                    var registryKey = currentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                    registryKey.SetValue("EyeGuard", value);
                    registryKey.Close();
                    currentUser.Close();
                }
                else
                {
                    var currentUser2 = Registry.CurrentUser;
                    var registryKey2 = currentUser2.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                    registryKey2.DeleteValue("EyeGuard", false);
                    registryKey2.Close();
                    currentUser2.Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("您需要管理员权限修改", "提示");
            }


            /*
            if (Whether)
            {
                Create(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "EyeGuard", System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, "眼睛卫士", AppDomain.CurrentDomain.BaseDirectory + "Resources/favicon.ico");
            }
            else
            {
                //检测是否存在
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) + "\\EyeGuard.lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) + "\\EyeGuard.lnk");
                }
            }
            */

            /*
            //删除旧有的vbs开机启动
            if (File.Exists(Iopath))
            {
                File.Delete(Iopath);
            }

            //删除旧有快捷方式的自启
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) + "\\EyeGuard.lnk"))
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) + "\\EyeGuard.lnk");
            }

            
            if (Whether)
            {
                if (!File.Exists(Iopath))
                {
                    string vbs = "CreateObject(\"WScript.Shell\").Run \"" + Application.ExecutablePath + "\""; ;
                    File.WriteAllText(Iopath, vbs, Encoding.GetEncoding(936));
                }
            }
            else
            {
                // 删除该文件
                File.Delete(Iopath);
            }
            */
        }
    }
}
