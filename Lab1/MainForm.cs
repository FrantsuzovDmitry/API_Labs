using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API_Lab_1
{
	public partial class MainForm : Form
	{
		private readonly HttpClient client;
		private string token;
		private JObject userInfo;
		private string userID;

		public MainForm()
		{
			client = new HttpClient();
			InitializeComponent();
			DateToInput.Value = DateTime.Now;
            var tip = new System.Windows.Forms.ToolTip();
			// Popup hint
			tip.SetToolTip(TagsInput, "Тэги разделяются запятой");
		}

		private void Button_Authorize_Click(object sender, EventArgs e)
		{
			// full cycle of authorization and getting user token
			AuthorizationForm authForm = new AuthorizationForm();
			authForm.ShowDialog();
			authForm.Visible = false;
			token = File.ReadAllText(VkAPI.USER_INFO_PATH);

			GetUserInfo();
		}

		private void CompleteForm()
		{
			try
			{
				Label_Name.Text = "" + userInfo["first_name"];
				Label_Surname.Text = "" + userInfo["last_name"];
				Label_City.Text = (string)userInfo["home_town"];
				Label_UserID.Text = userID;
			}
			catch (Exception)
			{
				throw new Exception("User data is not exist");
			}
		}

		// Output user data into form
		private async void GetUserInfo()
		{
			var endpoint = Constants.VK_ENDPOINT;
			var method = "account.getProfileInfo?";
			var parameters = $"access_token={token}&v={Constants.VK_API_VERSION}";
			HttpResponseMessage response = await client.GetAsync(endpoint + method + parameters);

			if (!response.IsSuccessStatusCode)
			{
				Label_Result.Text = response.StatusCode.ToString();
				return;  // if error
			}

			var content = await response.Content.ReadAsStringAsync();
			userInfo = JObject.Parse(content);
			userInfo = (JObject)userInfo["response"];
			userID = userInfo["id"].ToString();
			CompleteForm();
		}

		private async void Button_SendMessage_Click(object sender, EventArgs e)
		{
			await GetVacancies();
			SendMessage(userID);
		}

		private async Task GetVacancies()
		{
			var endpoint = Constants.HH_ENDPOINT;
			var method = "/vacancies?";
			var parameters = GetUserInput();

			client.DefaultRequestHeaders.Add("User-Agent", "api-test-agent");
			//ResponseArea.Text = "Типа ответ от ChatGPT";

			HttpResponseMessage response = await client.GetAsync(endpoint + method + parameters);

			if (response.IsSuccessStatusCode)
			{
				await WriteVacanciesToFile(response);
				Process.Start("notepad.exe", Constants.VACANCIES_PATH);
				Label_Result.Text = "Successful";
			}
			else
			{
				Label_Result.Text = response.StatusCode.ToString();
			}
		}

		private string GetUserInput()
		{
			var tags = GetTags();
			var dateFrom = DateFromInput.Value.ToString("yyyy-MM-dd");
			var dateTo = DateToInput.Value.ToString("yyyy-MM-dd");
			var parameters = $"text={tags}" +
				$"&date_from={dateFrom}" +
				$"&date_to={dateTo}" +
				$"&only_with_salary=true" +
				$"&per_page={Constants.NUMBER_OF_VACANCIES}" +
				$"&area=113";

			return parameters;
		}

		private async void SendMessage(string userID)
		{
			var endpoint = Constants.VK_ENDPOINT;
			var method = "messages.send?";
			var parameters = $"user_id={userID}&random_id=0&message=Типа текст от чата гопоты. Жаль, что чат GPT не хочет работать без деняк" +
				$"&access_token={Constants.ACCESS_TOKEN_GROUP}&v={Constants.VK_API_VERSION}";
			var response = await client.GetAsync(endpoint + method + parameters);
		}

		private void Button_Exit_Click(object sender, EventArgs e) => Application.Exit();

		private async Task<string> GetVacancyDescription(JToken ID)
		{
			var response = await client.GetAsync($"{Constants.HH_ENDPOINT}/vacancies/{ID}");
			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();
				JObject vacancy = JObject.Parse(content);
				return $"{vacancy["description"]}";
			}
			else
			{
				return $"ОШИБКА {response.StatusCode}";
			}
		}

		private string GetTags()
		{
			string[] input = TagsInput.Text.Split(',', ';');

			if (input.Length == 0) return string.Empty;

			return string.Join(" AND", input);
		}

		private async Task WriteVacanciesToFile(HttpResponseMessage response)
		{
			string content = await response.Content.ReadAsStringAsync();
			JObject vacancies = JObject.Parse(content);

			File.WriteAllText(Constants.VACANCIES_PATH, "Список вакансий:\n");
			foreach (var item in vacancies["items"])
			{
				string salaryInfo = $"ЗП: {item["salary"]["from"]} - {item["salary"]["to"]} {item["salary"]["currency"]}";
				string requirementInfo = $"Требования: {item["snippet"]["requirement"]}";
				string responsibilityInfo = $"Обязанности: {item["snippet"]["responsibility"]}";
				string description = $"Описание: {await GetVacancyDescription(item["id"])}";
				DeleteTags(ref salaryInfo); DeleteTags(ref requirementInfo);
				DeleteTags(ref responsibilityInfo); DeleteTags(ref description);

				File.AppendAllText(Constants.VACANCIES_PATH,
					$"ID: {item["id"]}, Название: {item["name"]}, Город: {item["area"]["name"]}\t{salaryInfo}" +
					$"\n{requirementInfo}" +
					$"\n{responsibilityInfo}" +
					$"\n{description}" +
					$"\n\n");
			}
		}

		// JSON response contains the tags. This method remove this from every single line
		private void DeleteTags(ref string line)
		{
			line = Regex.Replace(line, "<.*?>", string.Empty);
		}

	}
}