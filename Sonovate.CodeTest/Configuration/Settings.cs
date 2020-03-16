namespace Sonovate.CodeTest.Configuration
{
	public class Settings : ISettings
	{ 
		public string GetSetting(string key)
		{ 
			return Application.Settings[key];
		}
	}
}
