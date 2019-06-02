using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;


using Amazon.Rekognition;
using Amazon.Rekognition.Model;

using Amazon.S3;
using Amazon.S3.Model;
using RestSharp;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace smartcctv
{
    public class S3toRekognitionFunction
    {
        /// <summary>
        /// The default minimum confidence used for detecting labels.
        /// </summary>
        public const float DEFAULT_MIN_CONFIDENCE = 70f;

        /// <summary>
        /// The name of the environment variable to set which will override the default minimum confidence level.
        /// </summary>
        public const string MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME = "MinConfidence";

        IAmazonS3 S3Client { get; }

        IAmazonRekognition RekognitionClient { get; }

        float MinConfidence { get; set; } = DEFAULT_MIN_CONFIDENCE;
        string PushBulletToken { get; set; }

        HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

        /// <summary>
        /// Default constructor used by AWS Lambda to construct the function. Credentials and Region information will
        /// be set by the running Lambda environment.
        /// 
        /// This constuctor will also search for the environment variable overriding the default minimum confidence level
        /// for label detection.
        /// </summary>
        public S3toRekognitionFunction()
        {
            this.S3Client = new AmazonS3Client();
            this.RekognitionClient = new AmazonRekognitionClient();

            var environmentMinConfidence = System.Environment.GetEnvironmentVariable(MIN_CONFIDENCE_ENVIRONMENT_VARIABLE_NAME);
            this.PushBulletToken = System.Environment.GetEnvironmentVariable("PushBulletAccessToken");

            if (string.IsNullOrWhiteSpace(PushBulletToken))
                Console.WriteLine("Forgot to add the env variable?");

            if (!string.IsNullOrWhiteSpace(environmentMinConfidence))
            {
                float value;
                if(float.TryParse(environmentMinConfidence, out value))
                {
                    this.MinConfidence = value;
                    Console.WriteLine($"Setting minimum confidence to {this.MinConfidence}");
                }
                else
                {
                    Console.WriteLine($"Failed to parse value {environmentMinConfidence} for minimum confidence. Reverting back to default of {this.MinConfidence}");
                }
            }
            else
            {
                Console.WriteLine($"Using default minimum confidence of {this.MinConfidence}");
            }
        }

        /// <summary>
        /// Constructor used for testing which will pass in the already configured service clients.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="rekognitionClient"></param>
        /// <param name="minConfidence"></param>
        public S3toRekognitionFunction(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient, float minConfidence, string pushBulletAccessToken)
        {
            this.S3Client = s3Client;
            this.RekognitionClient = rekognitionClient;
            this.MinConfidence = minConfidence;
            this.PushBulletToken = pushBulletAccessToken;
        }

        /// <summary>
        /// A function for responding to S3 create events. It will determine if the object is an image and use Amazon Rekognition
        /// to detect labels and add the labels as tags on the S3 object.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task S3toRekognitionFunctionHandler(S3Event input, ILambdaContext context)
        {
            LambdaLogger.Log("Lambda start");
            foreach(var record in input.Records)
            {
                if(!SupportedImageTypes.Contains(Path.GetExtension(record.S3.Object.Key)))
                {
                    Console.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
                    continue;
                }

                LambdaLogger.Log($"Looking for labels in image {record.S3.Bucket.Name}:{record.S3.Object.Key}");

                var detectResponses = await this.RekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
                {
                    MinConfidence = MinConfidence,
                    Image = new Image
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object
                        {
                            Bucket = record.S3.Bucket.Name,
                            Name = record.S3.Object.Key
                        }
                    }
                });


                var numPeopleDetected = detectResponses.Labels.Count(x => x.Name.ToLower() == "person" || x.Name.ToLower() == "human");

                if (numPeopleDetected <= 0)
                {
                    LambdaLogger.Log($"No persons detected for {record.S3.Bucket.Name}:{record.S3.Object.Key}");
                    return;
                }

                LambdaLogger.Log($"Persons detected in image {record.S3.Bucket.Name}:{record.S3.Object.Key}");

                //oh shit, its going down.
                LambdaLogger.Log($"Generating URL for push {record.S3.Bucket.Name}:{record.S3.Object.Key}");
                await this.S3Client.MakeObjectPublicAsync(record.S3.Bucket.Name, record.S3.Object.Key, true);

                var expiryUrlRequest = new GetPreSignedUrlRequest
                {
                    BucketName = record.S3.Bucket.Name,
                    Key = record.S3.Object.Key,
                    Expires = DateTime.Now.AddHours(6)
                };

                LambdaLogger.Log("Requesting presigned URL");
                var url = S3Client.GetPreSignedURL(expiryUrlRequest);

                var message = $"{numPeopleDetected} on property.";

                LambdaLogger.Log($"Sending push {record.S3.Bucket.Name}:{record.S3.Object.Key}");

                var result = FirePushNotification("message", record.S3.Object.Key, url);

                LambdaLogger.Log($"Done!");
                if (!result)
                    LambdaLogger.Log("Ag poes, it broke.");

            }
            return;
        }

        private bool FirePushNotification(string message, string filename, string fileUrl)
        {
            var client = new RestClient("https://api.pushbullet.com/v2/pushes");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Access-Token", this.PushBulletToken);
            request.AddJsonBody(new
            {
                title = "Smart-CCTV : Detection alert",
                body = message,
                type = "file",
                file_name = filename,
                file_url = fileUrl,
                file_type = "image/jpeg"
            });
            IRestResponse response = client.Execute(request);

            return response.StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
