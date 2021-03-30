using static System.Console;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace mytraceroute
{
    public class Tracert
    {
        private const int PORT_NUM = 0;
        private const int MAXIMUM_JUMPS = 30;
        private const int PACKET_COUNT = 3;
        private const int RECEIVE_TIMEOUT = 2000;

        public void Start(string host, bool needToViewDns)
        {
            IPHostEntry ipHost;
            try
            {
                ipHost = Dns.GetHostEntry(host);
            }
            catch (SocketException)
            {
                WriteLine("Не удается разрешить системное имя узла " + host);
                return;
            }
            catch (Exception)
            {
                WriteLine("Длина разрешаемого имени узла или IP-адреса превышает 255 символов.");
                return;
            }

            IPEndPoint ipEndPoint = new IPEndPoint(ipHost.AddressList[0], PORT_NUM);
            EndPoint endPoint = ipEndPoint;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, RECEIVE_TIMEOUT);
            bool isEndPointReached = false;
            byte[] data = IcmpTools.GetEchoIcmpPackage();
            byte[] receivedData;
            int ttl = 1;

            WriteLine("Трассировка маршрута к " + host);
            WriteLine("с максимальным количеством прыжков " + MAXIMUM_JUMPS + ":");
            WriteLine();
            for (int i = 0; i < MAXIMUM_JUMPS; i++)
            {
                int errorCount = 0;
                Write("{0, 2}", i + 1);
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl++);
                Stopwatch track = new Stopwatch();
                receivedData = new byte[256];
                for (int j = 0; j < PACKET_COUNT; j++)
                {
                    try
                    {
                        track.Reset();
                        track.Start();
                        socket.SendTo(data, data.Length, SocketFlags.None, ipEndPoint);
                        socket.ReceiveFrom(receivedData, ref endPoint);                        
                        track.Stop();
                        long responseTime = track.ElapsedMilliseconds;
                        isEndPointReached = DestinationReached(receivedData, responseTime);
                    }
                    catch (SocketException)
                    {
                        Write("{0, 10}", "*");
                        errorCount++;
                    }
                }
                if (errorCount == PACKET_COUNT)
                {
                    Write("  Превышен интервал ожидания для запроса.\n");
                }
                else if (needToViewDns)
                {
                    ViewDns(endPoint);
                }
                else
                {
                    string str_endPoint = $"{endPoint}";
                    str_endPoint = str_endPoint.Replace(":0", " ");
                    Write("    {0}\n", str_endPoint);
                }
                if (isEndPointReached)
                {
                    WriteLine("\nТрассировка завершена.");
                    break;
                }
            }
        }
        private bool DestinationReached(byte[] receivedMessage, long responseTime)
        {
            int responceType = IcmpTools.GetIcmpType(receivedMessage);
            if (responceType == 0)
            {
                Write("{0, 10}", responseTime + " мс");
                return true;
            }
            if (responceType == 11)
            {
                Write("{0, 10}", responseTime + " мс");
            }
            return false;
        }
        private void ViewDns(EndPoint endPoint)
        {
            string str_endPoint = $"{endPoint}";
            str_endPoint = str_endPoint.Replace(":0", " ");
            try
            { 
                Write($"  {ExtractDns(endPoint)} [ {str_endPoint}]\n");
            }
            catch (SocketException)
            {
                str_endPoint = str_endPoint.Replace(":0", " ");
                Write($"  {str_endPoint}\n");
            }
        }
        private string ExtractDns(EndPoint endPoint)
        {
            string[] ipHost = endPoint.ToString().Split(':');
            string ipAdress = ipHost[0];
            return Dns.GetHostEntry(IPAddress.Parse(ipAdress)).HostName;
        }
    }
}