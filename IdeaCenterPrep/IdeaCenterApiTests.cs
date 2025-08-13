using System;
using System.Net;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using IdeaCenterPrep.Models;

namespace IdeaCenterPrep
{
    [TestFixture]

    public class IdeaCenterApiTests
    {
        
        private RestClient client;
        private static string lastCreatedIdeaId;
        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84/";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIxZjYyMGFjNy1lZDA0LTQ4ZjQtYmRlNS00Y2RhNmE0ZWQ1ZjMiLCJpYXQiOiIwOC8xMy8yMDI1IDA3OjIzOjQ4IiwiVXNlcklkIjoiNzU1YjE4ODktNzRlNS00OGFkLWQyOWItMDhkZGQ0ZTA4YmQ4IiwiRW1haWwiOiJsaWxpdGVzdEBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoiTGlsaXRlc3QiLCJleHAiOjE3NTUwOTE0MjgsImlzcyI6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.J4CKGMCfXS3_LFV6LeqC_2ELN2ZzbB8_RWDsRga2nJk";
        private const string loginEmail = "lilitest@example.com";
        private const string loginPassword = "examprep";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(loginEmail, loginPassword);

            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken),

            };

            this.client = new RestClient(options);

        }
        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Failed to retreive JWT from response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Content: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void Createidea_WithRequiredFields_ShouldReturnSuccess()
        {
            var ideaRequest = new IdeaDTO
            {
                title = "Test idea",
                url = "https://www.artdesign.ph/wp-content/uploads/2024/05/typ130-No-Problems-Just-Meow-Meow-Poster-02.png",
                description = "Test idea description"
            };

            var request = new RestRequest("api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaRequest);

            var response = this.client.Execute(request);

            Console.WriteLine("Status: " + response.StatusCode);
            Console.WriteLine("Response: " + response.Content);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }





        [OneTimeTearDown]

        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}