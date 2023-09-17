using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API_Lab_1
{
	public partial class AuthorizationForm : Form
	{
		public AuthorizationForm()
		{
			InitializeComponent();
		}

		// Authorize user
		private void AuthorizationForm_Load(object sender, EventArgs e)
		{
			var endpoint = "https://oauth.vk.com/authorize?";
			var parameters = $"client_id={VkAPI.APPID}" +
				$"&display=page" +
				$"&redirect_uri=https://oauth.vk.com/blank.html" +
				$"&revoke=1" +	// повторное предоставление при каждом запуске
				$"&response_type=token" +
				$"&v=5.131";
			AuthForm.Navigate(endpoint + parameters);

			// Вешаем на делегат новый метод получения токена пользователя
			AuthForm.DocumentCompleted += GetToken;
		}

		private void GetToken(object sender, WebBrowserDocumentCompletedEventArgs e) 
		{
			var accessTokenStartIndex = AuthForm.Url.ToString().IndexOf("access_token=");
			const int access_token_lenght = 13;

			if (accessTokenStartIndex != -1)
			{
				SaveUserToken(accessTokenStartIndex + access_token_lenght);
			}
		}

		private void SaveUserToken(int startPosition)
		{
			string token = AuthForm.Url.ToString().Substring(startPosition);
			File.WriteAllText(VkAPI.USER_INFO_PATH, token);
		}
	}
}
