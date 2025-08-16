using System;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using Story.Models;


namespace Story
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            // Взимаме токен за достъп
            string token = GetJwtToken("tatiana1", "tatiana1");

            // Създаваме клиент с токен
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        // Метод за логин и получаване на токен
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();
        }

        [Test, Order(1)]
        public void CreateStorySpoiler()
        {
            var story = new
            {
                Title = "New Story",
                Description = "Story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.Msg, Is.EqualTo("Successfully created!"));
            Assert.That(response.StoryId, Is.Not.Empty);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString();
        }


        [Test, Order(2)]
        public void EditStorySpoiler()
        {
            var editRequest = new StoryDTO
            {
                Title = "Edited Story",
                Description = "This is an updated test story.",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit", Method.Put);
            request.AddQueryParameter("storyId", createdStoryId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));

        }


        [Test, Order(3)]
        public void GetAllStorysSpoiler()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var storys = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(storys, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStorySpoiler()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Msg, Is.EqualTo("Deleted successfully"));
        }

        [Test, Order(5)]
        public void CreateStorySpoilerWithoutRequiredFields()
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
        public void EditNonExistingStorySpoiler()
        {
            string fakeId = "123";
            
            var request = new RestRequest($"/api/Story/Edit", Method.Put);
            request.AddQueryParameter("fakeId", createdStoryId);
            request.AddJsonBody(fakeId);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(editResponse.Msg, Is.EqualTo("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStorySpoiler()
        {
            string fakeId = "123";
            var request = new RestRequest($"/api/Food/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Msg, Is.EqualTo("Unable to delete this story spoiler!"));

        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }

}