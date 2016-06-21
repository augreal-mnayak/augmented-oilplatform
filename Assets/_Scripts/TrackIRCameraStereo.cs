using UnityEngine;
using System;
using System.Collections.Generic;
using TrackIRUnity;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using SerializableVector3;
//using System.Diagnostics;
//using System.Reflection;

//[Serializable]
//public class Limit {
//    public float lower, upper;
//}

public class TrackIRCameraStereo : MonoBehaviour {
    public bool useGUI;
    TrackIRUnity.TrackIRClient trackIRclient;
    bool running;
    string status, data;
    public Rect statusRect;
    public Rect dataRect;
    public float positionReductionFactor, rotationReductionFactor;
    public Limit positionXLimits, positionYLimits, positionZLimits, yawLimits, pitchLimits, rollLimits;
    public bool useLimits;
    //public Camera trackIRCamera;
    public GameObject StereoCamera;
    private UdpClient receiver;
    private Vector3Serializer currentRotation;
    private string ipAddr;

   // Create UDP client
   int receiverPort = 20015;

	public TrackIRCameraStereo ()
	{
		Debug.Log ("Constructor TrackIRCameraStereo");

        ipAddr = GetLocalIPAddress();
        Debug.Log("IP Address: " + ipAddr);
        //StartCamera();
    }
		
	// Use this for initialization
	void Start () {
        trackIRclient = new TrackIRUnity.TrackIRClient();  // Create an instance of the TrackerIR Client to get data from
        status = "";
        data = "";
	}

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Local IP Address Not Found!");
    }

    void StartCamera() {
//		StackTrace trace = new StackTrace ();
//		MethodBase methodBase = trace.GetFrame (1).GetMethod ();
//
//		string typeName = methodBase.DeclaringType.Name;
//		string methodName = methodBase.Name;

//		UnityEngine.Debug.Log ("Calling type: " + typeName);
//		UnityEngine.Debug.Log ("Calling method: " + methodName);
//
        try
        {
			UnityEngine.Debug.Log ("START CAMERA");
            receiver = new UdpClient(receiverPort);

            if (receiver != null)
            {
                UnityEngine.Debug.Log("Init Successful UDP Client");
                string ipAddr = GetLocalIPAddress();
                Debug.Log("IP Address: " + ipAddr);
            }
            else
                UnityEngine.Debug.Log("Init Failure UDP Client");
        }
        catch (Exception ex)
        {
			UnityEngine.Debug.Log ("Error opening socket" + ex.Message);
        }

        // Start async receiving
        if (receiver != null)
        {
			try
			{
				UnityEngine.Debug.Log("Before begin receive");
            	receiver.BeginReceive(DataReceived, receiver);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log ("Error opening socket: " + ex.Message);
			}

            running = true;
        }
    }

    void StopCamera() {
        if (trackIRclient != null && running) {                         // Stop tracking
            status = trackIRclient.TrackIR_Shutdown();
            running = false;
        }
    }

    void OnEnable() {
        StartCamera();
    }

    void OnDisable() {
        //StopCamera();
    }

    void OnApplicationQuit() {                              // Shutdown the camera when we quit the application.
        //StopCamera();
    }

    void OnGUI()
    {
        if (useGUI)
        {                                       // Gui for testing
            if (GUI.Button(new Rect(10, 10, 100, 25), "Init"))
            {
                StartCamera();
            }
            if (GUI.Button(new Rect(10, 45, 100, 25), "Shutdown"))
            {
                StopCamera();
            }
            status = "prevent null ref";
            GUI.TextArea(statusRect, status);
            GUI.TextArea(dataRect, data);
        }
    }

    //// Update is called once per frame
    //// update unity camera position with head tracking data
    //void Update () {
    //    if (running) {
    //        //data = trackIRclient.client_TestTrackIRData();          // Data for debugging output, can be removed if not debugging/testing

    //        TrackIRClient.LPTRACKIRDATA tid = trackIRclient.client_HandleTrackIRData(); // Data for head tracking
    //        Vector3 pos = trackIRCamera.transform.localPosition;                          // Updates main camera, change to whatever
    //        Vector3 rot = trackIRCamera.transform.localRotation.eulerAngles;

    //        Debug.Log("inside update running");
    //        Debug.Log("pos: " + pos);
    //        Debug.Log("rot: " + rot);

    //        if (!useLimits) {
    //            pos.x = -tid.fNPX * positionReductionFactor;
    //            pos.y = tid.fNPY * positionReductionFactor;
    //            pos.z = -tid.fNPZ * positionReductionFactor;

    //            rot.y = -tid.fNPYaw * rotationReductionFactor;
    //            rot.x = tid.fNPPitch * rotationReductionFactor;
    //            rot.z = tid.fNPRoll * rotationReductionFactor;
    //        } else {
    //            pos.x = Mathf.Clamp(-tid.fNPX * -positionReductionFactor, positionXLimits.lower, positionXLimits.upper);
    //            pos.y = Mathf.Clamp(tid.fNPY * positionReductionFactor, positionYLimits.lower, positionYLimits.upper);
    //            pos.z = Mathf.Clamp(-tid.fNPZ * positionReductionFactor, positionZLimits.lower, positionZLimits.upper);

    //            rot.y = Mathf.Clamp(-tid.fNPYaw * rotationReductionFactor, yawLimits.lower, yawLimits.upper);
    //            rot.x = Mathf.Clamp(tid.fNPPitch * rotationReductionFactor, pitchLimits.lower, pitchLimits.upper);
    //            rot.z = Mathf.Clamp(tid.fNPRoll * rotationReductionFactor, rollLimits.lower, rollLimits.upper);
    //        }

    //        if (Camera.main != null)
    //        {
    //            //Debug.Log("Before update camera rotation: " + Camera.main.transform.localRotation);
    //            //Debug.Log("Before update camera position: " + Camera.main.transform.localPosition);

    //            //pos = new Vector3(pos.x, pos.y, pos.z + 1.0f);
    //            //Debug.Log("Pos:" + pos);
    //            pos.z = pos.z - 200;

    //            Camera.main.transform.localRotation = Quaternion.Euler(rot);
    //            Camera.main.transform.localPosition = pos;

    //            //Debug.Log("After update camera rotation: " + Camera.main.transform.localRotation);
    //            //Debug.Log("After update camera position: " + Camera.main.transform.localPosition);
    //        }
    //    }
    //}


    // Update is called once per frame
    // update unity camera position with head tracking data
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (running)
        {
            
            //TrackIRClient.LPTRACKIRDATA tid = trackIRclient.client_HandleTrackIRData(); // Data for head tracking
            Vector3 pos = StereoCamera.transform.localPosition;                          // Updates main camera, change to whatever
            Vector3 rot = currentRotation.V3;

            //Debug.Log("inside update running");
            //Debug.Log("pos: " + pos);
            //Debug.Log("rot: " + rot);

            if (Camera.main != null)
            {


                Camera.main.transform.localRotation = Quaternion.Euler(rot);
                Camera.main.transform.localPosition = pos;

                //Debug.Log("After update camera rotation: " + Camera.main.transform.localRotation);
                //Debug.Log("After update camera position: " + Camera.main.transform.localPosition);
            }
        }
    }

    private void DataReceived(IAsyncResult ar)
    {
		Debug.Log ("Inside Data Received Function");
        Debug.Log("IP Address: " + ipAddr);

        UdpClient c = (UdpClient)ar.AsyncState;
        IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);

        var formatter = new BinaryFormatter();
        var memoryStream = new MemoryStream(receivedBytes);

        object obj = formatter.Deserialize(memoryStream);
        Vector3Serializer headrot = (Vector3Serializer)obj;
        currentRotation = headrot;

        // Restart listening for udp data packages
        c.BeginReceive(DataReceived, ar.AsyncState);
    }
}