using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValorantAccountSwitcher
{
    class ValorantApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;

        public ValorantApplicationContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                ContextMenu = new ContextMenu(new MenuItem[] {
                    new MenuItem("-"),
                    new MenuItem("Exit", Exit)
                }),
                Text = "Valorant Account Switcher",
                Visible = true
            };
            PopulateAccountList();
        }

        void PopulateAccountList()
        {
            trayIcon.ContextMenu.MenuItems.Clear();
            string currentAccount = AccountManager.GetCurrentAccountName();
            foreach (string account in AccountManager.GetAccountNames())
            {
                var item = new MenuItem(account, ChangeAccount);
                if (account == currentAccount)
                {
                    item.Checked = true;
                }
                trayIcon.ContextMenu.MenuItems.Add(item);
            }
            if (trayIcon.ContextMenu.MenuItems.Count > 0)
            {
                trayIcon.ContextMenu.MenuItems.Add(new MenuItem("-"));
            }
            if (AccountManager.IsCurrentAccountUnknown())
            {
                trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Add current account...", AddCurrentAccount));
            }
            trayIcon.ContextMenu.MenuItems.Add(new MenuItem("Exit", Exit));
        }

        void ChangeAccount(object sender, EventArgs e)
        {
            MenuItem temp = sender as MenuItem;
            string accountName = temp.Text;
            AccountManager.CopyAccountToCurrent(accountName);
            PopulateAccountList();
        }

        void AddCurrentAccount(object sender, EventArgs e)
        {
            string accountName = Microsoft.VisualBasic.Interaction.InputBox("Please enter an account name for this Valorant account",
                       "New Account",
                       "");
            if (accountName.Length == 0) return;

            Regex r = new Regex("^[a-zA-Z0-9#]*$");
            if (!r.IsMatch(accountName))
            {
                Microsoft.VisualBasic.Interaction.MsgBox("Account names must consist of numbers and letters, no spaces or puncuation are allowed", Microsoft.VisualBasic.MsgBoxStyle.Exclamation, "Invalid Account Name");
                return;
            }

            AccountManager.CopyCurrentToAccount(accountName);
            PopulateAccountList();
        }

        void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;

            Application.Exit();
        }
    }
}
