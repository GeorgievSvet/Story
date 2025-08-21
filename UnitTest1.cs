using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;

namespace Story
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string StoryId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
           
            string token = GetJwtToken("S_Georgiev", "123456");

            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new {username, password});

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new
            {
                Title = "New Story",
                Description = "Test Story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            //Assert.That(json.GetString(), Is.EqualTo("Successfully created!"));
            StoryId = json.GetProperty("storyId").GetString();

            Assert.That(StoryId, Is.Not.Null.And.Not.Empty, "Story ID should not be empty");

            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Test, Order(2)]
        public void EditStoryTitle_ShouldReturnCorrectMessage()
        {
            var changes = new
            {
               Title = "New Title",
               Description = "Edited Title",
               Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{StoryId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }
        


        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(stories, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{StoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeStoryID = "123";
            var changes = new
            {
               Title = "alabala",
               Description = "bullshit",
               Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeStoryID}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string fakeId = "123";

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