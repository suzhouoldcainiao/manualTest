using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDemo
{
    public class SocketClient
    {
        public event Action<string> EventReceive;
        public event Action<string> EventconnectState; //1:链接  0：断开
        Socket NewSocket = null;
        byte[] byteBuffer = new byte[10240];
        string beginMsg = string.Empty;


        private void NTime_Tick(object sender, EventArgs e)
        {
            if (NewSocket != null && NewSocket.Connected)
            {
                if (EventconnectState != null)
                {
                    EventconnectState("check");
                }
            }
            else
            {
                if (EventconnectState != null)
                {
                    EventconnectState("ng");
                }
            }
        }

        public void Connect(string ip, int port)
        {
            try
            {
                if (NewSocket != null && NewSocket.Connected)
                {
                    Close(NewSocket);
                }
                NewSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //  NewSocket.Blocking = false;
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
                //绑定  异步链接
                // NewSocket.BeginConnect(ipe, new AsyncCallback(OnConnectRequest), NewSocket);
                NewSocket.Connect(ipe);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"链接异常:{ex}", "链接提示");
            }
        }

        private void OnConnectRequest(IAsyncResult ar)
        {
            //获取链接状态
            Socket _Socket = (Socket)ar.AsyncState;
            try
            {
                //判断链接是否成功
                if (_Socket.Connected)
                {
                    SetReceiveCallback(_Socket);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("链接异常", "链接提示");
            }
        }

        /// <summary>
        /// 回调法法
        /// </summary>
        private void SetReceiveCallback(Socket _Sock)
        {
            AsyncCallback receive = new AsyncCallback(OnReceiveData);
            _Sock.BeginReceive(byteBuffer, 0, byteBuffer.Length, SocketFlags.None, OnReceiveData, _Sock);
        }

        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceiveData(IAsyncResult ar)
        {
            Socket _Sock = (Socket)ar.AsyncState;
            try
            {
                int receiveLengthData = _Sock.EndReceive(ar);
                //大于0接受到报文数据
                if (receiveLengthData > 0)
                {
                    // if (m_ByteBuffer[0]==0)
                    //接受到的数据
                    string receiveMsg = Encoding.UTF8.GetString(byteBuffer, 0, receiveLengthData);
                    //传值
                    if (EventReceive != null)
                    {
                        EventReceive(receiveMsg);
                    }
                    //重复调用
                    SetReceiveCallback(_Sock);


                }
                else
                {
                    //光闭Socket
                    // Close(_Sock);
                }
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        public string Send(string str)
        {
            Thread.Sleep(50);
            NewSocket.ReceiveTimeout = 3000;
            NewSocket.SendTimeout = 3000;
            //把字符转换字节
            byte[] _ByteSend = Encoding.UTF8.GetBytes(str);
            //发送数据         
            try
            {
                NewSocket.Send(_ByteSend, _ByteSend.Length, 0);
                if (str == "CHECK")
                    return "check";
                byte[] vlau = new byte[1024];
                int lengt = NewSocket.Receive(vlau);
                string reasult = Encoding.UTF8.GetString(vlau);
                string reasult1 = Encoding.ASCII.GetString(vlau);
                string reasult2 = Encoding.Unicode.GetString(vlau);
                string reasult3 = Encoding.UTF32.GetString(vlau);
                string reasult4 = Encoding.GetEncoding("GB2312").GetString(vlau).Trim().Replace("\0", "");

                return reasult;
            }
            catch (Exception ex)
            {
                return "";//@@@针对入栈失败，这个地方需要返回一个标识，区分是否发送数据报错
            }

        }

        /// <summary>
        /// 关闭Sockte
        /// </summary>
        /// <param name="sockte"></param>
        public void Close(Socket sockte)
        {
            sockte.Shutdown(SocketShutdown.Both);
            Thread.Sleep(10);
            sockte.Close();
        }
    }
}
