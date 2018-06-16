using System;
using System.Windows.Forms;

namespace TVRename
{
    public partial class AutoAddShow : Form
    {
        private readonly TheTvdbCodeFinder codeFinder;
        private readonly string originalHint;

        public AutoAddShow(string hint)
        {
            InitializeComponent();
            ShowItem = new ShowItem();
            codeFinder = new TheTvdbCodeFinder("") {Dock = DockStyle.Fill};
            codeFinder.SetHint(hint);
            codeFinder.SelectionChanged += MTCCF_SelectionChanged;
            pnlCF.SuspendLayout();
            pnlCF.Controls.Add(codeFinder);
            pnlCF.ResumeLayout();
            ActiveControl = codeFinder; // set initial focus to the code entry/show finder control

            cbDirectory.SuspendLayout();
            cbDirectory.Items.Clear();
            cbDirectory.Items.AddRange(TVSettings.Instance.LibraryFolders.ToArray());
            cbDirectory.SelectedIndex = 0;
            cbDirectory.ResumeLayout();

            originalHint = hint;
        }

        private void MTCCF_SelectionChanged(object sender, EventArgs e)
        {
            lblDirectoryName.Text = System.IO.Path.DirectorySeparatorChar + TVSettings.Instance.FilenameFriendly(FileHelper.MakeValidPath(codeFinder.SelectedShow()?.Name ));
        }

        public ShowItem ShowItem { get; }

        private void SetShowItem()
        {
            int code = codeFinder.SelectedCode();

            ShowItem.TVDBCode = code;
            ShowItem.AutoAdd_FolderBase = cbDirectory.Text+lblDirectoryName.Text;
            ShowItem.PadSeasonToTwoDigits = true;
            //Set Default Timezone based on Network
            ShowItem.ShowTimeZone = TimeZone.TimeZoneForNetwork(codeFinder.SelectedShow()?.GetNetwork());
            if (!originalHint.Contains(codeFinder.SelectedShow().Name, StringComparison.OrdinalIgnoreCase))
            {
                ShowItem.AliasNames.Add(originalHint);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (!OkToClose())
            {
                DialogResult = DialogResult.None;
                return;
            }

            SetShowItem();
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool OkToClose()
        {
            if (TheTVDB.Instance.HasSeries(codeFinder.SelectedCode())) return true;

            DialogResult dr = MessageBox.Show("tvdb code unknown, close anyway?", "TVRename Auto Add Show",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            return dr != DialogResult.No;
        }
    }
}