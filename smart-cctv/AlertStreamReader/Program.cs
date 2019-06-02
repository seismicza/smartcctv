using AlertStreamReader.Entities;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AlertStreamReader
{
    class Program
    {
        static IAmazonS3 client;
        static IConfigurationRoot configuration;
        static void Main(string[] args)
        {
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                
            }
        }

        static void Run()
        {

            Console.WriteLine("Starting up...");

            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            configuration = builder.Build();
            Console.WriteLine("Loading settings...Done!");
            Console.Write("Attaching to DVR...");
            Thread.Sleep(100);
            Console.WriteLine("Done!");
            
            var encoded = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(configuration["AppSettings:DVRUser"].ToString() + ":" + configuration["AppSettings:DVRPass"].ToString()));
            Console.Write("Loading channel list...");
            var client = new RestClient($"{configuration["AppSettings:BaseDVRUrl"].ToString()}/ISAPI/System/Video/inputs");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Basic " + encoded);
            var listResponse = client.Get(request);

            StringReader rdr = new StringReader(listResponse.Content);
            XmlSerializer serializer = new XmlSerializer(typeof(VideoInput));
            var videoInput = (VideoInput)serializer.Deserialize(rdr);
            Console.WriteLine("Done!");
            Console.WriteLine($"{videoInput.VideoInputChannelList.VideoInputChannel.Count(cnt => cnt.resDesc != "NO VIDEO")} cameras online!");




            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create($"{configuration["AppSettings:BaseDVRUrl"].ToString()}/ISAPI/Event/notification/alertStream");
            wr.Method = "GET";
            wr.Accept = "*/*";
            wr.ReadWriteTimeout = System.Threading.Timeout.Infinite;
            wr.KeepAlive = true;
            wr.ProtocolVersion = HttpVersion.Version11;
            //wr.Credentials = new NetworkCredential(configuration["AppSettings:DVRUser"].ToString(), configuration["AppSettings:DVRPass"].ToString()); // will be emncrypted.
           
            wr.Headers.Add("Authorization", "Basic " + encoded);

            using (WebResponse webResponse = wr.GetResponse())
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    string line;
                    string xmlString = "";
                    int[] postCount = new int[] { 3, 30, 50 };
                    var x = new List<string>();
                    Console.WriteLine("==============================================================");
                    Console.WriteLine("                       HAUS IS ON DUTY                        ");
                    Console.WriteLine("                          _\\_(*)_/_                          ");
                    Console.WriteLine("==============================================================");
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        //Console.WriteLine(line);
                        if (string.IsNullOrEmpty(line) || line.Contains("boundary"))
                        {

                            if (xmlString.Length > 0)
                            {

                                // Parse xml feed.
                                StringReader read = new StringReader(xmlString);
                                XmlSerializer ser = new XmlSerializer(typeof(EventNotificationAlert));
                                var eventAlert = (EventNotificationAlert)ser.Deserialize(read);
                                //Console.WriteLine(eventAlert.eventType + videoInput.VideoInputChannelList.VideoInputChannel.FirstOrDefault(item => item.id == eventAlert.channelID)?.name);
                                //Check for motion detection alert. 
                                Console.WriteLine($"{DateTime.Now}:Response received. {eventAlert.channelID} | {eventAlert.eventType} | {eventAlert.eventState} | {eventAlert.activePostCount}");

                                //if(eventAlert.eventType != "videoloss")
                                //    Console.WriteLine(eventAlert.eventType);

                                if (eventAlert.eventType == "VMD" && postCount.Contains(eventAlert.activePostCount) && eventAlert.eventState.ToLower() == "active")
                                {
                                    var channelName = videoInput.VideoInputChannelList.VideoInputChannel.FirstOrDefault(item => item.id == eventAlert.channelID).name;

                                    Console.WriteLine($"{DateTime.Now} : Camera {channelName} : Motion alert triggered.");
                                    var snap = GetSnap(eventAlert.channelID);
                                    Task<bool> upload = UploadToS3Async(channelName, snap);

                                    var result = upload.Result;

                                    Console.WriteLine($"{DateTime.Now} : Uploaded for detection.");
                                    Console.WriteLine();
                                }

                            }
                            xmlString = "";
                        }
                        if (!string.IsNullOrEmpty(line) && (!line.Contains("boundary") && !line.ToLower().Contains("content")))
                            xmlString += line;

                    }
                }
            }
        }

        static byte[] GetSnap(int cameraId)
        {

            var client = new RestClient($"{configuration["AppSettings:BaseDVRUrl"].ToString()}/ISAPI/Streaming/channels/{cameraId}01/picture");

            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept","application/json");
            request.Credentials = new NetworkCredential(configuration["AppSettings:DVRUser"].ToString(), configuration["AppSettings:DVRPass"].ToString());
            //request.ResponseWriter
            byte[] response = client.DownloadData(request);
            return response;
        }

        static async Task<bool> UploadToS3Async(string camName, byte[] snapImage)
        {
            try
            {
                Amazon.AWSConfigsS3.UseSignatureVersion4 = true;
                var credentials = new BasicAWSCredentials(configuration["AppSettings:AwsAccessKeyId"].ToString(), configuration["AppSettings:AwsSecretAccessKey"].ToString());
              
                using (client = new AmazonS3Client(credentials, RegionEndpoint.EUWest1))
                {
                    string keyName = "CAM-"+ camName + "-"+Guid.NewGuid().ToString().Replace(" - ","") + ".jpg";
                    PutObjectRequest request = new PutObjectRequest()
                    {
                        BucketName = configuration["AppSettings:S3BucketName"].ToString(),
                        Key = keyName,
                        //ContentType = "image/jpeg",
                        InputStream = new MemoryStream(snapImage)

                    };
                    request.Metadata.Add("Camera", camName);
                    request.Metadata.Add("DetectionMethods", "Object");

                    PutObjectResponse response = await client.PutObjectAsync(request);

                    Console.WriteLine($"{DateTime.Now} : Upload image completed. RefImage:{keyName}");
                    return true;
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Please check the provided AWS Credentials.");
                    Console.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                    return false;
                }
                else
                {
                    Console.WriteLine("An error occurred with the message '{0}' when writing an object", amazonS3Exception.Message);
                    return false;
                }
            }
        }
    }
}
