using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JC
{
	internal static class Utils
	{
		public static string AssemblyShortName
		{
			get
			{
				if (assemblyShortName == null)
				{
					var assembly = typeof(Utils).Assembly;
					assemblyShortName = assembly.ToString().Split(',')[0];
				}
				return assemblyShortName;
			}
		}
		private static string assemblyShortName;

		public static Uri MakePackUri(string fileName)
		{
			string uriString = "pack://application:,,,/";
			uriString += AssemblyShortName;
			uriString += ";component/";
			uriString += fileName;

			return new Uri(uriString);
		}
	}
}
