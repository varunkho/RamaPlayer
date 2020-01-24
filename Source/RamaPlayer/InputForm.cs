using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RamaPlayer
{
	public partial class InputForm : Form
	{
		private object inputValue;
		private Func<string, object> parser;

		private InputForm()
		{
			InitializeComponent();
		}

		public static (DialogResult result, TValue value) Ask<TValue>(string title, Func<string, TValue> parser = null, string description = null)
		{
			var form = new InputForm();
			form.Text = title;
			form.label1.Text = description ?? form.label1.Text;
			form.parser = s => parser(s);
			form.ShowDialog();
			return (form.DialogResult, form.inputValue == null ? default(TValue) : (TValue)form.inputValue);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(this.inputText.Text))
			{
				this.inputText.Focus();
				return;
			}

			try
			{
				this.inputValue = parser(this.inputText.Text);
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				this.inputText.Focus();
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
