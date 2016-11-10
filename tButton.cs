using System;
using System.Globalization;
using System.Windows.Controls.Primitives;

namespace OpenKeyboard
{
	public class tButton : ToggleButton
	{
		public KeyboardCommand KBCommand;

		public string Title
		{
			set
			{
				bool flag = value.StartsWith("\\u");
				if (flag)
				{
					this.parseUnicode(value);
				}
				else
				{
					base.Content = value;
				}
			}
		}

		private void parseUnicode(string txt)
		{
			int i = 0;
			string text = "";
			bool flag = txt.Length == 6;
			if (flag)
			{
				base.Content = (char)int.Parse(txt.Substring(2), NumberStyles.HexNumber);
			}
			else
			{
				while (i < txt.Length)
				{
					bool flag2 = txt[i] != '\\';
					if (flag2)
					{
						text += txt[i].ToString();
						i++;
					}
					else
					{
						string s = txt.Substring(i + 2, 4);
						text += ((char)int.Parse(s, NumberStyles.HexNumber)).ToString();
						i += 6;
					}
				}
				base.Content = text;
			}
		}

		protected override void OnToggle()
		{
			base.OnToggle();
			bool flag = base.IsChecked.HasValue && base.IsChecked.Value;
			if (flag)
			{
				vKeyboard.ProcessCommand(this.KBCommand, new bool?(false));
				this.SyncChecked(true);
			}
			else
			{
				vKeyboard.ProcessCommand(this.KBCommand, new bool?(true));
				this.SyncChecked(false);
			}
		}

		private void SyncChecked(bool isChecked)
		{
			foreach (tButton current in vLayout.tButtonList)
			{
				bool flag = current.KBCommand.KBKeys != null && current.KBCommand.KBKeys.Length == 1 && current.KBCommand.KBKeys[0].Substring(1) == this.KBCommand.KBKeys[0].Substring(1);
				if (flag)
				{
					current.IsChecked = new bool?(isChecked);
				}
			}
		}
	}
}
