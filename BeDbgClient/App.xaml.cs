using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BeDbgClient.Api;

namespace BeDbgClient
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			using var _ = new Decoder(Decoder.Mode.Long64, Decoder.AddressWidth.Width64);
		}

	}
}