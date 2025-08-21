using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StoryTests
    {
       
        private RestClient client;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";
        private static string createdStoryId;

        [OneTimeSetUp]
        public void Setup()
        {
                string token = GetJwtToken("vania345", "vania345");
                var options = new RestClientOptions(baseUrl)
                {
                    
                    Authenticator = new JwtAuthenticator(token)
                };
                client = new RestClient(options);
        }
        private string GetJwtToken(string username, string password)
        {
                var loginClient = new RestClient(baseUrl);

                var request = new RestRequest("/api/User/Authentication", Method.Post);
                request.AddJsonBody(new { username, password });

                var response = loginClient.Execute(request);

                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }

        [Order(1)]
        [Test]

        public void CreateStory_WithRequiredFields_ShouldReturnSuccess()
        {
            var story = new 
            {
                Title = "My First Story",
                Description = "This is just a test story",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);
            
            Assert.That(response.StatusCode,Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "Story ID should not be null or empty");
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void EditStory_ShouldReturnOk()
        {
            var updatedStory = new
            {
                Title = "My Updated Story",
                Description = "This is just an updated test story",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }
        [Order(3)]
        [Test]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var storys = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(storys, Is.Not.Empty);
        }
        [Order(4)]
        [Test]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }
        [Order(5)]
        [Test]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "",
                Description = "This is just a test story",
                Url = ""
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                
        }

        [Order(6)]
        [Test]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            
                string fakeId = "123456";
                var storyUpdate = new
                {
                    title = "Does Not Exist",
                    description = "Fake story",
                    url = ""
                };

                var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
                request.AddJsonBody(storyUpdate);
               
                var response = client.Execute(request);
                
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

                Assert.That(response.Content, Does.Contain("No spoilers..."));
        }
        [Order(7)]
        [Test]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string fakeId = "123456";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));

        }




        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}