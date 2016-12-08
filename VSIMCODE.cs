using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MathWorks.MATLAB.NET.Arrays;
//using MathWorks.MATLAB.NET.Utility;
using VISSIMLIB;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace OnLineSim
{
    class VissimTools
    {
        static public Vissim vissim = new Vissim();

        static public void InitVissim(string fn, string fn_ini, int sp, int res, int rseed, int warmup)
        {
            //int a = 0;
            vissim.LoadNet(fn, false);
            vissim.LoadLayout(fn_ini);
            vissim.Simulation.set_AttValue("SimPeriod", sp);
            vissim.Simulation.set_AttValue("SimRes", res);
            vissim.Simulation.set_AttValue("RandSeed", rseed);


            // setting for evaluations
            vissim.Evaluation.set_AttValue("NetPerfCollectData", true);
            vissim.Evaluation.set_AttValue("VehTravTmsCollectData", true);
            vissim.Evaluation.set_AttValue("DataCollCollectData", true);

        }

        static public void EndVissim()
        {
            vissim.Exit();
        }

        //static public uint Get_InputVol(int id) { return Convert.ToInt32(vissim.Net.VehicleInputs.get_ItemByKey(id).get_AttValue("Volume")); } // Not OK
        //public static int CheckDetPre(int det) { return Convert.ToInt16(vissim.Net.Detectors.get_ItemByKey(det).get_AttValue("PRESENCE")); } //OK
        //static public int GetDectection(int sc, int det) { return Convert.ToInt16(vissim.Net.Detectors.get_ItemByKey(det).get_AttValue("DETECTION")); } //OK

        //static public double GetDecOcc(int dec) { return (double)vissim.Net.Detectors.get_ItemByKey(dec).get_AttValue("Occup"); } //OK
        //static public int GetDecVeh(int dec) { return (int)vissim.Net.Detectors.get_ItemByKey(dec).get_AttValue("VehNo"); } //OK
        //static public double GetDecSpd(int dec) { return (double)vissim.Net.Detectors.get_ItemByKey(dec).get_AttValue("VehSpeed"); } //OK
        //static public void Set_InputVol(int id, int vol) { vissim.Net.VehicleInputs.get_ItemByKey(id).set_AttValue("Volume(1)", vol); } // OK


        //public static object GetVehiclesFromDet(int det) { return vissim.Net.Detectors.get_ItemByKey(det).get_AttValue("VehNo"); } //OK
        //static public int Get_VehType(int vid) { return (int)vissim.Net.Vehicles.get_ItemByKey(vid).get_AttValue("VehType"); } //OK
        //static public int Get_Lane(int vid) { return (int)vissim.Net.Vehicles.get_ItemByKey(vid).get_AttValue("Lane"); } //OK
        //static public void Set_VehType(int vid, int type) { vissim.Net.Vehicles.get_ItemByKey(vid).set_AttValue("Type", type); }
        //static public double Get_Xcoord(int vid) { return (double)vissim.Net.Vehicles.get_ItemByKey(vid).get_AttValue("Pos"); }
        //static public double Get_DesiredSpeed(int vid) { return (double)vissim.Net.Vehicles.get_ItemByKey(vid).get_AttValue("DesSpeed"); }
        //static public int GetStaVehNum(int staid) { return Convert.ToInt32(vissim.Net.DataCollectionMeasurements.get_ItemByKey(staid).get_AttValue("Vehs(Current,Last,All)")); }
        //static public double GetStaSPD(int staid) { return Convert.ToDouble(vissim.Net.DataCollectionMeasurements.get_ItemByKey(staid).get_AttValue("Speed(Current,Last,All)")); }
        //static public void SetVehSPD(int vid, double spd) { vissim.Net.Vehicles.get_ItemByKey(vid).set_AttValue("DesSpeed", spd); }
        //static public void SetDsrdSPD(int did, int cid, int sid) { vissim.Net.DesSpeedDecisions.get_ItemByKey(did).set_AttValue("DesSpeedDistr(" + cid + ")", sid); }
        //static public double GetRatio(int compID, int vid) { return Convert.ToDouble(vissim.Net.VehicleCompositions.get_ItemByKey(compID).get_AttValue("RelFlow")); }

        //static public void AddNewVehicle(int lane, double xcoord) { vissim.Net.Vehicles.AddVehicleAtLinkPosition(100, 2, lane, xcoord, 0.0, true); }
        //static public void ModifyNodeName(int nid, string msg) { vissim.Net.Nodes.get_ItemByKey(nid).set_AttValue("Name", msg); }
        //static public double GetDelayMeasure(int delid) { return Convert.ToDouble(vissim.Net.DelayMeasurements.get_ItemByKey(delid).get_AttValue("VehDelay(Current,Last,All)")); }
        ////static public double GetNodeDelay(int delid) { return Convert.ToDouble(vissim.Net.Nodes.get_ItemByKey(delid).Movements.Count());}
    }

    class Program
    {
        //public static GetPoisRnd.Class1 PRN = new GetPoisRnd.Class1();

        public static Dictionary<int, Dictionary<int, int>> RTMSDataRaw = new Dictionary<int, Dictionary<int, int>>();
        public static Dictionary<int, string> RTMSFDataFile = new Dictionary<int, string>();
        public static Dictionary<int, Dictionary<int, List<int>>> LinkRTMSLane = new Dictionary<int, Dictionary<int, List<int>>>();
        public static Dictionary<int, Dictionary<int, List<int>>> itsLaneData = new Dictionary<int, Dictionary<int, List<int>>>();

        static byte[] Buffer { get; set; }
        static Socket sck;
        private static List<int> linkArray;

        static void Main(string[] args)
        {
            //MWNumericArray R = new MWNumericArray();
            //GetPoisRnd.Class1 PRN = null;
            //MWArray[] Output = null;

            //PRN = new GetPoisRnd.Class1();
            //Output = PRN.GetPoisRnd(1,2);

            string fn = @"C:\Users\rhj3\Desktop\mihir\Netwotk for Summer Paper (2).inpx";
            string fn_ini = @"C:\Users\rhj3\Desktop\mihir\Netwotk for Summer Paper (1).layx";
            int simresol = 10;
            int simprd = 4500;
            int seed_ = 1101;
            int ui_minute = 15;
            int ui = ui_minute * 60 * simresol;
            int ti = 0;
            int si = 0;
            bool p = true;
            int rcount = 0;
            int DataloadingInterval = 0;

            Dictionary<int, List<int>> LaneDeaprtureInterval = new Dictionary<int, List<int>>();
            List<int> DepartureInterval = new List<int>();
            Dictionary<int, int> LinkLanes = new Dictionary<int, int>();
            Dictionary<int, Dictionary<int, List<int>>> LinkDepartureInterval = new Dictionary<int, Dictionary<int, List<int>>>();
            List<double> RandDec = new List<double>();

            LinkLanes.Add(95, 5);
            LinkLanes.Add(32, 3);
           // LinkLanes.Add(34,3);
            LinkLanes.Add(16, 3);
            LinkLanes.Add(1, 4);
            //LinkLanes.Add(95, 7);
            //LinkLanes.Add(95, 8);
            //LinkLanes.Add(95, 9);
            //LinkLanes.Add(95, 10);
            //LinkLanes.Add(95, 11);

            //LinkLanes.Add(32, 5);
            //LinkLanes.Add(32, 6);
            //LinkLanes.Add(32, 7);
            //LinkLanes.Add(32, 8);

            //LinkLanes.Add(34, 5);
            //LinkLanes.Add(34, 6);
            //LinkLanes.Add(34, 7);
            //LinkLanes.Add(34, 8);

            //LinkLanes.Add(1, 7);
            //LinkLanes.Add(1, 8);
            //LinkLanes.Add(1, 9);
            //LinkLanes.Add(1, 10);
            //LinkLanes.Add(1, 11);

            //LinkLanes.Add(16, 7);
            //LinkLanes.Add(16, 8);
            //LinkLanes.Add(16, 9);
            //LinkLanes.Add(16, 10);
            //LinkLanes.Add(16, 11);



            RTMSFDataFile.Add(95, @"C:\Users\rhj3\Desktop\mihir\RTMS 3 (MP 0.6)"); // Link 95
            RTMSFDataFile.Add(32, @"C:\Users\rhj3\Desktop\mihir\RTMS 1 (MP 27.9)"); // Link 32
            RTMSFDataFile.Add(34, @"C:\Users\rhj3\Desktop\mihir\RTMS 2 (MP 27.1)");

            LinkRTMSLane.Add(95, new Dictionary<int, List<int>>());
            LinkRTMSLane.Add(32, new Dictionary<int, List<int>>());
            LinkRTMSLane.Add(34, new Dictionary<int, List<int>>());

            LinkRTMSLane[95].Add(1, new List<int>());
            LinkRTMSLane[95].Add(2, new List<int>());
            LinkRTMSLane[32].Add(1, new List<int>());
            LinkRTMSLane[32].Add(2, new List<int>());
            LinkRTMSLane[34].Add(1, new List<int>());
            LinkRTMSLane[34].Add(2, new List<int>());
            LinkRTMSLane[34].Add(3, new List<int>());

            // Xa 
            LinkRTMSLane[95][1].Add(7);
            LinkRTMSLane[95][1].Add(8);
            LinkRTMSLane[95][1].Add(9);
            LinkRTMSLane[95][1].Add(10);
            LinkRTMSLane[95][1].Add(11);

            // Ya 
            LinkRTMSLane[34][1].Add(1);

            //Yb
            LinkRTMSLane[34][2].Add(1);
            LinkRTMSLane[34][3].Add(1);
            //LinkRTMSLane[34][4].Add(1);
            //LinkRTMSLane[34][5].Add(1);

            // Za 
            LinkRTMSLane[32][1].Add(1);
            LinkRTMSLane[32][1].Add(2);
            LinkRTMSLane[32][1].Add(3);
            LinkRTMSLane[32][1].Add(4);

            //Client program to get the data from the server
            Console.WriteLine("Client Listening");
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sck.Bind(new IPEndPoint(0, 50000));
            sck.Listen(100);
            Console.WriteLine("Socket Created");

            Socket accepted = sck.Accept();

            Console.WriteLine("Waiting for socket");
            Buffer = new byte[accepted.SendBufferSize];

            int bytesRead = accepted.Receive(Buffer);

            byte[] formatted = new byte[bytesRead];

            for (int i = 0; i < bytesRead; i++)
            {
                formatted[i] = Buffer[i];
            }

            //Split the string into List
            string itsData = Encoding.ASCII.GetString(formatted);
            //Console.Write(itsData+"\r\n");

            char[] DelimiterChar = { ' ' };
            string[] rowData = itsData.Split(DelimiterChar);

            List<string> newRows = new List<string>(rowData); //Stores the data into rows

            //Add Link numbers here
            int[] linkNumber = new int[] { 95, 32, 34, 1, 16 };
            for (int i = 0; i < linkNumber.Length; i++)
            {
                itsLaneData.Add(linkNumber[i], new Dictionary<int, List<int>>());
            }

            //Add data to the itsLaneData structure
            //Link 95
            for (int i = 0; i < newRows.Count; i++)
            {
                char[] delemeter = { ',' };
                string[] rows = rowData[i].Split(delemeter);

                if (rows[1] == "166.149.185.222" && rows[2] == "5")
                {
                    itsLaneData[95].Add(5, new List<int>());
                    //itsLaneData[95][5].Add(int.Parse(rows[3]));
                    itsLaneData[95][5].Add(int.Parse(rows[4]) / 1000);
                    Console.WriteLine(int.Parse(rows[4]) / 1000);
                }
                if (rows[1] == "166.149.185.222" && rows[2] == "7")
                {
                    itsLaneData[95].Add(7, new List<int>());
                    //itsLaneData[95][7].Add(int.Parse(rows[3]));
                    itsLaneData[95][7].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.222" && rows[2] == "8")
                {
                    itsLaneData[95].Add(8, new List<int>());
                    //itsLaneData[95][8].Add(int.Parse(rows[3]));
                    itsLaneData[95][8].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.222" && rows[2] == "9")
                {
                    itsLaneData[95].Add(9, new List<int>());
                    //itsLaneData[95][9].Add(int.Parse(rows[3]));
                    itsLaneData[95][9].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.222" && rows[2] == "10")
                {
                    itsLaneData[95].Add(10, new List<int>());
                    //itsLaneData[95][10].Add(int.Parse(rows[3]));
                    itsLaneData[95][10].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.222" && rows[2] == "11")
                {
                    itsLaneData[95].Add(11, new List<int>());
                    //itsLaneData[95][11].Add(int.Parse(rows[3]));
                    itsLaneData[95][11].Add(int.Parse(rows[3]) / 1000);
                }
            }

            int[] laneNumber = new int[] { 5, 6, 7, 8 };
            for (int i = 0; i < laneNumber.Length; i++)
            {
                itsLaneData[32].Add(laneNumber[i], new List<int>());
            }
            List<int> listArray = new List<int>();
            listArray = itsLaneData.Keys.ToList();

            ////Link 34,32
            //for (int i = 0; i < newRows.Count; i++)
            //{
            //    char[] delemeter = { ',' };
            //    string[] rows = rowData[i].Split(delemeter);
            //    if (rows[1] == "166.249.52.129" && rows[2] == "5")
            //    {
            //        foreach (int link in LinkLanes.Keys)
            //        {
            //            if (itsLaneData.ContainsKey(32))
            //            {
            //                int ln = itsLaneData[link];


            //                itsLaneData[32][5].Add(int.Parse(rows[3]));
            //                itsLaneData[32][5].Add(int.Parse(rows[4]));

            //            }
            //            if (itsLaneData.ContainsKey(34))
            //            {
            //                itsLaneData[34][5].Add(int.Parse(rows[3]));
            //                itsLaneData[34][5].Add(int.Parse(rows[4]));
            //            }
            //        }
            //    }
            //    else if (rows[1] == "166.249.52.129" && rows[2] == "6")
            //    {

            //        itsLaneData[32][6].Add(int.Parse(rows[3]));
            //        itsLaneData[32][6].Add(int.Parse(rows[4]));


            //        itsLaneData[34][6].Add(int.Parse(rows[3]));
            //        itsLaneData[34][6].Add(int.Parse(rows[4]));
            //    }
            //    else if (rows[1] == "166.249.52.129" && rows[2] == "7")
            //    {

            //        itsLaneData[32][7].Add(int.Parse(rows[3]));
            //        itsLaneData[32][7].Add(int.Parse(rows[4]));


            //        itsLaneData[34][7].Add(int.Parse(rows[3]));
            //        itsLaneData[34][7].Add(int.Parse(rows[4]));
            //    }
            //    else if (rows[1] == "166.249.52.129" && rows[2] == "8")
            //    {

            //        itsLaneData[32][8].Add(int.Parse(rows[3]));
            //        itsLaneData[32][8].Add(int.Parse(rows[4]));


            //        itsLaneData[34][8].Add(int.Parse(rows[3]));
            //        itsLaneData[34][8].Add(int.Parse(rows[4]));
            //    }

            //}


            //Link 1 & 16
            for (int i = 0; i < newRows.Count; i++)
            {
                char[] delemeter = { ',' };
                string[] rows = rowData[i].Split(delemeter);
                if (rows[1] == "166.149.185.223" && rows[2] == "7")
                {
                    itsLaneData[1].Add(7, new List<int>());
                    //itsLaneData[1][7].Add(int.Parse(rows[3]));
                    itsLaneData[1][7].Add(int.Parse(rows[4]) / 1000);

                    itsLaneData[16].Add(7, new List<int>());
                    //itsLaneData[16][7].Add(int.Parse(rows[3]));
                    itsLaneData[16][7].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.223" && rows[2] == "8")
                {
                    itsLaneData[1].Add(8, new List<int>());
                    //itsLaneData[1][8].Add(int.Parse(rows[3]));
                    itsLaneData[1][8].Add(int.Parse(rows[4]) / 1000);

                    itsLaneData[16].Add(8, new List<int>());
                    //itsLaneData[16][8].Add(int.Parse(rows[3]));
                    itsLaneData[16][8].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.223" && rows[2] == "9")
                {
                    itsLaneData[1].Add(9, new List<int>());
                    //itsLaneData[1][9].Add(int.Parse(rows[3]));
                    itsLaneData[1][9].Add(int.Parse(rows[4]) / 1000);

                    itsLaneData[16].Add(9, new List<int>());
                    //itsLaneData[16][9].Add(int.Parse(rows[3]));
                    itsLaneData[16][9].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.223" && rows[2] == "10")
                {
                    itsLaneData[1].Add(10, new List<int>());
                    // itsLaneData[1][10].Add(int.Parse(rows[3]));
                    itsLaneData[1][10].Add(int.Parse(rows[4]) / 1000);

                    itsLaneData[16].Add(10, new List<int>());
                    //itsLaneData[16][10].Add(int.Parse(rows[3]));
                    itsLaneData[16][10].Add(int.Parse(rows[4]) / 1000);
                }
                else if (rows[1] == "166.149.185.223" && rows[2] == "11")
                {
                    itsLaneData[1].Add(11, new List<int>());
                    //itsLaneData[1][11].Add(int.Parse(rows[3]));
                    itsLaneData[1][11].Add(int.Parse(rows[4]) / 1000);

                    itsLaneData[16].Add(11, new List<int>());
                    //itsLaneData[16][11].Add(int.Parse(rows[3]));
                    itsLaneData[16][11].Add(int.Parse(rows[4]) / 1000);
                }
            }





            double TrkProb = -0.2;

            foreach (int link in LinkLanes.Keys)
            {
                LinkDepartureInterval.Add(link, new Dictionary<int, List<int>>());
                int ln = LinkLanes[link];
                for (int l = 1; l <= ln; l++)
                {
                    //if (!LaneDeaprtureInterval.ContainsKey(l))
                    //    LaneDeaprtureInterval.Add(l, new List<int>());

                    //DepartureInterval = GetRandom(2, 1000, simresol);
                    //LaneDeaprtureInterval[l] = DepartureInterval;                   
                    //Thread.Sleep(100);

                    for (int i = 0; i < newRows.Count; i++)
                    {
                        char[] delemeter = { ',' };
                        string[] rows = rowData[i].Split(delemeter);
                        Console.WriteLine("Headway Value " + (int.Parse(rows[4]) / 1000));
                        int lanenumber = int.Parse(rows[2]);

                        if (link == 95 && rows[1] == "166.149.185.222" && lanenumber == l)
                        {
                            Console.WriteLine("Link is " + l);
                            LaneDeaprtureInterval[l] = itsLaneData[95][l]; ;
                        }
                        else
                            if (link == 34 && rows[1] == "166.249.52.129" && lanenumber == l)
                            {
                                LaneDeaprtureInterval[l] = itsLaneData[34][l]; ;
                            }
                            else
                                if (link == 32 && rows[1] == "166.249.52.129" && lanenumber == l)
                                {
                                    LaneDeaprtureInterval[l] = itsLaneData[32][l]; ;
                                }
                                else
                                    if (link == 1 && rows[1] == "166.149.185.223" && lanenumber == l)
                                    {
                                        LaneDeaprtureInterval[l] = itsLaneData[1][l]; ;
                                    }
                                    else
                                        if (link == 16 && rows[1] == "166.149.185.223" && lanenumber == l)
                                        {
                                            LaneDeaprtureInterval[l] = itsLaneData[16][l]; ;
                                        }
                    }

                }
                LinkDepartureInterval[link] = LaneDeaprtureInterval;
            }

            RandDec = GetRandomDec(50, 100000);
            rcount = 0;

            VissimTools.InitVissim(fn, fn_ini, simprd, simresol, seed_, 0);

            LoadRTMSData(RTMSFDataFile, DataloadingInterval);

            for (int i = 1; i <= simprd * simresol; i++)
            {
                if (si == simresol)
                {
                    foreach (int link in LinkLanes.Keys)
                    {
                        int ln = LinkLanes[link];
                        for (int l = 1; l <= ln; l++)
                        {
                            //if (LinkDepartureInterval[link][l].Contains(ti))
                            //{
                            if (RandDec[rcount++] > TrkProb)
                            {
                                if (RandDec[rcount++] > GetRouteP(link))
                                    VissimTools.vissim.Net.Vehicles.AddVehicleAtLinkPosition(101, link, l, 5, 80, true);
                                else
                                    VissimTools.vissim.Net.Vehicles.AddVehicleAtLinkPosition(102, link, l, 8, 85, true);
                            }
                            else
                                VissimTools.vissim.Net.Vehicles.AddVehicleAtLinkPosition(201, link, l, 10, 60, true);
                            //}

                        }
                    }
                    si = 0;
                }
                else
                    si++;

                VissimTools.vissim.Simulation.RunSingleStep();

                if (ti == ui)
                {

                    foreach (int link in LinkLanes.Keys)
                    {
                        int ln = LinkLanes[link];
                        for (int l = 1; l <= ln; l++)
                        {
                            //DepartureInterval = GetRandom(2, 1000, simresol);
                            //LaneDeaprtureInterval[l] = DepartureInterval;
                            //Thread.Sleep(100);
                            int j;

                            for (j = 0; j < newRows.Count; j++)
                            {
                                char[] delemeter = { ',' };
                                string[] rows = rowData[j].Split(delemeter);
                                int lanenumber = int.Parse(rows[2]);
                                if (link == 95 && rows[1] == "166.149.185.222" && lanenumber == l)
                                {
                                    LaneDeaprtureInterval[l] = itsLaneData[95][l]; ;
                                }
                                else
                                    if (link == 34 && rows[1] == "166.249.52.129" && lanenumber == l)
                                    {
                                        LaneDeaprtureInterval[l] = itsLaneData[34][l]; ;
                                    }
                                    else
                                        if (link == 32 && rows[1] == "166.249.52.129" && lanenumber == l)
                                        {
                                            LaneDeaprtureInterval[l] = itsLaneData[32][l]; ;
                                        }
                                        else
                                            if (link == 1 && rows[1] == "166.149.185.223" && lanenumber == l)
                                            {
                                                LaneDeaprtureInterval[l] = itsLaneData[1][l]; ;
                                            }
                                            else
                                                if (link == 16 && rows[1] == "166.149.185.223" && lanenumber == l)
                                                {
                                                    LaneDeaprtureInterval[l] = itsLaneData[16][l]; ;
                                                }
                            }
                        }
                        LinkDepartureInterval[link] = LaneDeaprtureInterval;
                    }

                    ti = 0;

                    RandDec = GetRandomDec(50, 4000);
                    rcount = 0;
                }
                else
                    ti++;
            }

            Console.Write("Press a  key to exit");
            Console.Read();
            sck.Close();
            accepted.Close();

        } //Main End

        private static void LoadRTMSData(Dictionary<int, string> RTMSFDataFile, int dli)
        {
            StreamReader sr;
            string line = null;
            string[] token = null;
            string[] data = null;

            foreach (int lk in RTMSFDataFile.Keys)
            {
                sr = new StreamReader(RTMSFDataFile[lk] + dli + ".log");

                for (int i = 0; i < 20; i++) sr.ReadLine();

                while ((line = sr.ReadLine()) != null)
                {
                    foreach (var d in LinkRTMSLane[lk]) { }



                }

            }
        }

        private static double GetRouteP(int lk)
        {
            switch (lk)
            {
                case 95:
                    return 0.0;
                case 34:
                    return 0.0;
                default:
                    return 0.3;
            };
        }

        private static List<int> GetRandom(int lamda, int n, int simresol)
        {
            Random r = new Random();
            List<int> Val = new List<int>();
            int s = 0;
            int r_ = 0;
            int cnt = 0;

            while (cnt < n)
            {
                r_ = r.Next(lamda);
                if (r_ > 0)
                {
                    s = s + r_ * simresol;
                    Val.Add(s);
                    cnt++;
                }
            }

            return Val;
        }

        private static List<double> GetRandomDec(int lamda, int n)
        {
            Random r = new Random();
            List<double> Val = new List<double>();
            int s = 0;
            int r_ = 0;
            int cnt = 0;
            double ss = (double)lamda + (double)lamda;

            while (cnt < n)
            {
                r_ = r.Next(lamda + lamda);
                Val.Add(r_ / ss);
                cnt++;
            }

            return Val;
        }

        private static int poissonRandomNumber(int lambda)
        {
            Random r;
            double L = Math.Exp(-lambda);
            int k = 0;
            double p = 1;
            do
            {
                k = k + 1;
                r = new Random();
                double u = (r.Next(0, 100) / 100.0);
                p = p * u;
            } while (p > L);
            return k - 1;
        }

    }


}