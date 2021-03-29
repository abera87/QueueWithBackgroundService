using System;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Maersk.Sorting.Api.Test
{
    public class SortControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private HttpClient Client { get; }

        public SortControllerTests(WebApplicationFactory<Startup> fixture)
        {
            this.Client = fixture.CreateClient();
        }
        [Fact]
        public async void Post_EnQueueJob_Should_Status_Pending()
        {
            var data = JsonConvert.SerializeObject(new int[] { 2, 3, 1, 5, 3, 1, -20, 2 });
            var jsonBody = new StringContent(data, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/sort", jsonBody);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var resData = JsonConvert.DeserializeObject<SortJob>(await response.Content.ReadAsStringAsync());
            Assert.Equal(SortJobStatus.Pending, resData.Status);
        }

        [Fact]
        public async void Get_GetJobById_Should_Have_Same_Id()
        {
            //Given
            var data = JsonConvert.SerializeObject(new int[] { 10, 6, -20, 2 });
            var jsonBody = new StringContent(data, Encoding.UTF8, "application/json");
            var response = await Client.PostAsync("/sort", jsonBody);
            var resData = JsonConvert.DeserializeObject<SortJob>(await response.Content.ReadAsStringAsync());

            //When
            var jobResponse = await Client.GetAsync($"/sort/{resData.Id}");
            var jobData = JsonConvert.DeserializeObject<SortJob>(await jobResponse.Content.ReadAsStringAsync());

            //Then
            Assert.Equal(jobData.Id, resData.Id);
        }
    }
}
