using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PupPackScreenTweaker
{
    public partial class MainForm : Form
    {
        private bool useTransparentPupFrames = true;
        private bool authorizeDeleteAnyScreen = true;
        private Color readOnlyCellsColor = Color.FromArgb(255, 230, 230, 230);

        // the ref screens (loaded once from ini file and not modified)
        private PupScreens refScreens;
        // the pup screen
        private PupScreens pupScreens;
        // the selected pup screen
        private PupScreen selectedPupScreen;
        // datasource for the grid
        private BindingList<PupScreen> pupScreenBindingList;
        private BindingSource pupScreensSource;
        // the screens.pup file loaded
        private string screensPupFile;

        public MainForm()
        {
            InitializeComponent();
            // load reference screen definition (ini file)
            loadReferenceScreens();
            pupScreens = new PupScreens();
            TopMost = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            initializeGridList();
            grpScreenProp.Enabled = false;
            Text = PupTools.GetSoftwareName();
        }

        private void initializeGridList()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSize = false;
            dataGridView1.DataSource = null;

            // col 0
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ScreenIndex";
            column.Name = "Num";
            column.Width = 30;
            column.ReadOnly = true;
            column.DefaultCellStyle.BackColor = readOnlyCellsColor;
            column.Frozen = true;
            dataGridView1.Columns.Add(column);

            // col 1
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Description";
            column.Name = "Description";
            column.Width = 110;
            column.Frozen = true;
            dataGridView1.Columns.Add(column);

            // col 2
            DataGridViewComboBoxColumn combocolumn = new DataGridViewComboBoxColumn();
            combocolumn.DataPropertyName = "Active";
            combocolumn.Name = "Active";
            combocolumn.Items.AddRange(PupTools.GetActiveModeList());
            combocolumn.FlatStyle = FlatStyle.Flat;
            combocolumn.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.Columns.Add(combocolumn);

            // col 3
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "CustomPosNOT"; // binding does not work when updated manually
            column.Name = "CustomPos";
            column.Width = 140;
            column.ReadOnly = true;
            column.DefaultCellStyle.BackColor = readOnlyCellsColor;
            dataGridView1.Columns.Add(column);

            // col 4
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Playlist";
            column.Name = "Playlist";
            column.Width = 150;
            column.DefaultCellStyle.BackColor = Color.White;
            column.DefaultCellStyle.Padding = new Padding(0, 0, 20, 0);
            dataGridView1.Columns.Add(column);

            // col 5
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Playfile";
            column.Name = "Playfile";
            column.Width = 100;
            column.DefaultCellStyle.Padding = new Padding(0, 0, 20, 0);
            dataGridView1.Columns.Add(column);

            // col 6
            column = new DataGridViewCheckBoxColumn();
            column.DataPropertyName = "Loopit";
            column.Width = 70;
            column.Name = "Transparent";
            dataGridView1.Columns.Add(column);

            // col 7
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Priority";
            column.Name = "Volume%";
            column.Width = 60;
            dataGridView1.Columns.Add(column);


        }

        /// <summary>
        /// load values from PinUpPlayer.ini (dimensions and locations of ref PuP screens, to be used as references)
        /// </summary>
        private void loadReferenceScreens()
        {
            string iniFile = PupTools.FindPuPIniFile();
            try
            { 
                refScreens = PupTools.GetPupScreenFromIniFile(iniFile, useTransparentPupFrames);
                refScreens.Add(PupScreens.CreateSpecial99Screen()); // add the virtual "99" screen, used for a screen to refer to itself
                cboRefScreen.Items.Add("");
                foreach (PupScreen ps in refScreens) cboRefScreen.Items.Add(ps.ScreenIndex.ToString());
                cboRefScreen.Items.Add(PupScreens.OTHER_SCREENINDEX);
            }
            catch
            {
                MessageBox.Show(this,"Cannot open/read PinUpPlayer.ini... Exiting","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        /// <summary>
        /// load the PuP screens info from a "screens.pup" file
        /// </summary>
        private void loadScreensPupFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "PuP file|*.pup";
            openFileDialog1.Title = "Please pick a screens.pup file to open";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                screensPupFile = openFileDialog1.FileName;
                killAllWindows(); // in case it's not the first time a file is loaded
                dataGridView1.DataSource = null;
                string errors = "";
                pupScreens = PupTools.GetPupScreensFromPupFile(screensPupFile, useTransparentPupFrames, refScreens, ref errors);
                if (pupScreens == null)
                {
                    MessageBox.Show(this, "Not a valid screens.pup file" + Environment.NewLine + errors, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // add the invalid ref screens to the drop down list
                    List<int?> invalidScreens = new List<int?>();
                    foreach (PupScreen screen in pupScreens)
                    {
                        if (screen.InvalidScreenReference != null)
                        {
                            bool found = false;
                            foreach (string item in cboRefScreen.Items) if (item == screen.InvalidScreenReference.ToString()) found = true;
                            if (!found)
                            {
                                invalidScreens.Add(screen.InvalidScreenReference);
                                cboRefScreen.Items.Add(screen.InvalidScreenReference.ToString());
                            }
                        }
                    }
                    if (invalidScreens.Count != 0)
                    {
                        string list = String.Join(", ", invalidScreens.ToArray());
                        MessageBox.Show(this, 
                            "Warning: your screens.pup file contains some definitions using unknown references screens (" + list + ")" 
                            + Environment.NewLine
                            + "These screens are not defined in your PinUpPlayer.ini file",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    pupScreenBindingList = new BindingList<PupScreen>(pupScreens);
                    pupScreensSource = new BindingSource(pupScreenBindingList, null);
                    dataGridView1.DataSource = pupScreensSource;

                    foreach (PupScreen pupScreen in pupScreens)
                    {
                        pupScreen.Window.Visible = pupScreen.Active != "off";
                        pupScreen.PropertyChanged += PupScreenPropertiesChanged;
                        pupScreen.Window.UnauthorizedActivation += UnauthorizedActivationOfPupScreen;
                    }
                    updateAllCustomPosInGrid();

                    enablePropertyControls();

                    if (dataGridView1.Rows.Count > 0)
                    {
                        dataGridView1.Rows[0].Selected = true;
                        selectedPupScreen.Highlight(true);
                        updateScreenPropertiesFields();
                    }

                }
            }
            Focus();
        }

        private void enablePropertyControls()
        {
            btnSavePupScreens.Enabled = true;
            btnAddGrid.Enabled = true;
            btnDelGrid.Enabled = true;
            btnUpGrid.Enabled = true;
            btnDownGrid.Enabled = true;
        }

        /// <summary>
        /// we'll get notified if the window is moved/resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PupScreenPropertiesChanged(object sender, EventArgs e)
        {
            if (selectedPupScreen != null)
            {
                updateScreenPropertiesFields();
            }
        }

        /// <summary>
        /// we'll get notified if the window is moved when it was not supposed to (not selected)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnauthorizedActivationOfPupScreen(object sender, EventArgs e)
        {
            foreach (PupScreen pupScreen in pupScreens) pupScreen.HighlightIfSelected();
        }

        private void btnXm_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangePosByIncrement(-1, 0);
        }

        private void btnXp_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangePosByIncrement(1, 0);
        }

        private void btnYm_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangePosByIncrement(0, -1);
        }

        private void btnYp_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangePosByIncrement(0, 1);
        }

        private void btnWm_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangeSizeByIncrement(-1, 0);
        }

        private void btnWp_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangeSizeByIncrement(1, 0);
        }

        private void btnHm_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangeSizeByIncrement(0, -1);
        }

        private void btnHp_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.ChangeSizeByIncrement(0, 1);
        }

        private void txtX_TextChanged(object sender, EventArgs e)
        {
            validateTxtInteger((TextBox)sender);
            updatedSizeAndPosUI();
        }

        private void validateTxtInteger(TextBox txt)
        {
            Int16 val;
            if (!Int16.TryParse(txt.Text, out val)) txt.Undo();
        }

        private void validateTxtFloat(TextBox txt)
        {
            float val;
            if (!Single.TryParse(txt.Text, out val)) txt.Undo();
        }

        /// <summary>
        /// function to call if a textbox related to physical position or size is modified by the user
        /// </summary>
        private void updatedSizeAndPosUI()
        {
            selectedPupScreen.Window.Width = Convert.ToInt32(txtW.Text);
            selectedPupScreen.Window.Height = Convert.ToInt32(txtH.Text);
            selectedPupScreen.Window.Top = Convert.ToInt32(txtY.Text);
            selectedPupScreen.Window.Left = Convert.ToInt32(txtX.Text);
            selectedPupScreen.CalculateCustomPos();
            selectedPupScreen.Window.ForceRepaint();
            updateScreenPropertiesFields();
        }

        /// <summary>
        /// function to call if a textbox related to relative position or size is modified by the user
        /// </summary>
        private void updatedCustomPosUI()
        {
            if (selectedPupScreen != null)
            {
                if (grpScreenProp.Enabled == false) grpScreenProp.Enabled = true;
                selectedPupScreen.HasCustomPos = cboRefScreen.Text != "";
                selectedPupScreen.InvalidScreenReference = null; // reset "invalid" flag
                if (selectedPupScreen.HasCustomPos)
                {
                    selectedPupScreen.SetRefScreen(Convert.ToInt16(cboRefScreen.Text));
                    if (txtCustX.Text != "") selectedPupScreen.CustPosX = Convert.ToSingle(txtCustX.Text);
                    if (txtCustY.Text != "") selectedPupScreen.CustPosY = Convert.ToSingle(txtCustY.Text);
                    if (txtCustW.Text != "") selectedPupScreen.CustPosW = Convert.ToSingle(txtCustW.Text);
                    if (txtCustH.Text != "") selectedPupScreen.CustPosH = Convert.ToSingle(txtCustH.Text);
                }
                else
                {
                    selectedPupScreen.SetRefScreen(selectedPupScreen.ScreenIndex);
                    txtCustX.Text = "";
                    txtCustY.Text = "";
                    txtCustW.Text = "";
                    txtCustH.Text = "";
                }
                selectedPupScreen.CalculateRealPos();
                lblWarningScreenRef.Visible = selectedPupScreen.InvalidScreenReference != null && selectedPupScreen.HasCustomPos;
                selectedPupScreen.Window.ForceRepaint();
                updateScreenPropertiesFields();
            }
            else
            {
                grpScreenProp.Enabled = false;
            }
        }

        private void txtY_TextChanged(object sender, EventArgs e)
        {
            validateTxtInteger((TextBox)sender);
            updatedSizeAndPosUI();
        }

        private void txtW_TextChanged(object sender, EventArgs e)
        {
            validateTxtInteger((TextBox)sender);
            selectedPupScreen.Window.ChangeWidth(Convert.ToInt16(txtW.Text));
        }

        private void txtH_TextChanged(object sender, EventArgs e)
        {
            validateTxtInteger((TextBox)sender);
            selectedPupScreen.Window.ChangeHeight(Convert.ToInt16(txtH.Text));
        }


        /// <summary>
        /// Update the UI to reflext the properties of the selected pup screen
        /// </summary>
        private void updateScreenPropertiesFields()
        {
            txtX.Text = selectedPupScreen.Window.Left.ToString();
            txtY.Text = selectedPupScreen.Window.Top.ToString();
            txtW.Text = selectedPupScreen.Window.Width.ToString();
            txtH.Text = selectedPupScreen.Window.Height.ToString();
            //this.ckLockAR.Checked = selectedPupScreen.Window.IsAspectRatioLocked();
            pnlGoodAR.BackColor = selectedPupScreen.Window.IsAspectRatioOK() ? Color.Green : Color.Red;
            if (selectedPupScreen.HasCustomPos)
            {
                txtCustX.Text = Math.Round((double)selectedPupScreen.CustPosX, 2).ToString();
                txtCustY.Text = Math.Round((double)selectedPupScreen.CustPosY, 2).ToString();
                txtCustW.Text = Math.Round((double)selectedPupScreen.CustPosW, 2).ToString();
                txtCustH.Text = Math.Round((double)selectedPupScreen.CustPosH, 2).ToString();
            }
            else
            {
                txtCustX.Text = "";
                txtCustY.Text = "";
                txtCustW.Text = "";
                txtCustH.Text = "";
            }
            updateHideButton();

            //dataGridView1.SelectedRows[0].Cells[3].Value = selectedPupScreen.CustomPos;
            updateCustomPosInGrid(dataGridView1.SelectedRows[0].Index);

            cboRefScreen.Text = selectedPupScreen.HasCustomPos ? selectedPupScreen.GetRefScreenIndex().ToString() : "";
        }

        private void updateCustomPosInGrid(int rowIndex)
        {
            dataGridView1.Rows[rowIndex].Cells[3].Value = pupScreens[rowIndex].CustomPos;
        }

        private void updateAllCustomPosInGrid()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows) updateCustomPosInGrid(row.Index);
        }

        private void ckLockAR_CheckedChanged(object sender, EventArgs e)
        {
            updatedAspectRatioUI();
        }

        private void txtAR1_TextChanged(object sender, EventArgs e)
        {
            validateTxtFloat((TextBox)sender);
            updatedAspectRatioUI();
        }

        /// <summary>
        /// function to call if the user changed aspect ratio parameters
        /// </summary>
        private void updatedAspectRatioUI()
        {
            if (txtAR1.Text != "" && txtAR2.Text != "")
            {
                int ar1 = Convert.ToInt16(txtAR1.Text);
                int ar2 = Convert.ToInt16(txtAR2.Text);
                if (ar1 != 0 && ar2 != 0)
                {
                    selectedPupScreen.Window.SetAspectRatio((float)ar1/(float)ar2, ckLockAR.Checked);
                }
                else
                {
                    selectedPupScreen.Window.SetAspectRatio(1, false);
                }                
            }
        }

        private void txtAR2_TextChanged(object sender, EventArgs e)
        {
            validateTxtFloat((TextBox)sender);
            if (Convert.ToInt32(txtAR2.Text) == 0) txtAR2.Undo();
            updatedAspectRatioUI();
        }

        private void btnBringToFront_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.BringToFront();
        }

        private void btnSendToBack_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.BringToFront();
            selectedPupScreen.Window.SendToBack();
        }

        private void btnShowHide_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.Visible = !selectedPupScreen.Window.Visible;
            updateHideButton();
        }

        private void updateHideButton()
        {
            btnShowHide.Text = selectedPupScreen.Window.Visible ? "hide PuP screen" : "show PuP screen";
        }

        private void btnLoadScreensPupFile_Click(object sender, EventArgs e)
        {
            loadScreensPupFile();
        }


        /// <summary>
        /// Load a pic into the selected pup screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadPic_Click(object sender, EventArgs e)
        {
            // browse by default in the playlist folder, if it exists
            string defaultPath = Path.GetDirectoryName(screensPupFile);
            string playlistPath = defaultPath + "\\" + selectedPupScreen.PlayList + "\\";
            if (Directory.Exists(playlistPath)) defaultPath = playlistPath;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = defaultPath;
            openFileDialog1.Filter = "Image/Video Files|*.jpg;*.jpeg;*.png;*.mp4;*.db2s;*.directb2s";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    loadPicToWindow(openFileDialog1.FileName);
                }
                catch(Exception exc)
                {
                    MessageBox.Show(this, "Error while attempting to load picture: " + exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            refreshPicButtonStatus();
        }

        private void loadPicToWindow(string picturePath)
        {
            Bitmap bgPic = null;
            MatDWindow.PictureSource source = MatDWindow.PictureSource.none;
            string defaultPicFileName = "";
            string defaultResName = "";
            string extension = Path.GetExtension(picturePath).ToUpper();
            if (extension == ".MP4")
            {
                Video vid = new Video(picturePath);
                bgPic = vid.GetFrame(vid.Duration / 2);
                source = MatDWindow.PictureSource.video;
                defaultPicFileName = Path.GetFileNameWithoutExtension(picturePath) + "-snapshot.png";
            }
            else if (extension == ".DB2S" || extension == ".DIRECTB2S")
            {
                B2s b2s = new B2s(picturePath);
                if (b2s.IsValid)
                {
                    B2sForm frm = new B2sForm("You picked a DB2S file... Which image would you like to import?",
                                                "Backglass only", true,
                                                "Backglass with speaker grill", b2s.HasGrill(),
                                                "Speaker grill only", b2s.HasGrill());
                    frm.TopMost = true;
                    DialogResult dlg = frm.ShowDialog(this);
                    if (dlg == DialogResult.OK)
                    {
                        bgPic = (Bitmap)b2s.BackGlassImage(false);
                        defaultPicFileName = Path.GetFileNameWithoutExtension(picturePath) + "-backglass.png";
                    }
                    else if (dlg == DialogResult.Retry)
                    {
                        bgPic = (Bitmap)b2s.BackGlassImage(true);
                        defaultPicFileName = Path.GetFileNameWithoutExtension(picturePath) + "-backglass_with_grill.png";
                    }
                    else if (dlg == DialogResult.Ignore)
                    {
                        bgPic = (Bitmap)b2s.GrillImage();
                        defaultPicFileName = Path.GetFileNameWithoutExtension(picturePath) + "-grill.png";
                    }
                    defaultResName = Path.GetFileNameWithoutExtension(picturePath) + ".res";
                    source = MatDWindow.PictureSource.db2s;
                }
            }
            else
            {
                bgPic = new Bitmap(picturePath);
                source = MatDWindow.PictureSource.picture;
                defaultPicFileName = Path.GetFileNameWithoutExtension(picturePath) + ".png";
            }

            if (bgPic != null)
            {
                string extraInfoCaption = "(" + source + " " + bgPic.Width.ToString() + "x" + bgPic.Height.ToString() + ")";
                selectedPupScreen.Window.LoadPicture(VideoTools.CopyAndReleaseImage(bgPic), source, extraInfoCaption, defaultPicFileName, defaultResName);
            }
            else
            {
                throw new Exception("Unrecognized format");
            }

        }

        private void btnGetARfromPic_Click(object sender, EventArgs e)
        {
            if (selectedPupScreen.Window.HasBackgroundPic())
            {
                txtAR1.Text = selectedPupScreen.Window.BackgroundPicWidth().ToString();
                txtAR2.Text = selectedPupScreen.Window.BackgroundPicHeight().ToString();
                updatedAspectRatioUI();
            }
            else
            {
                MessageBox.Show(this,"There is no picture loaded into PuP screen #" + selectedPupScreen.ScreenIndex.ToString(),"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtCustomPos_Validated(object sender, EventArgs e)
        {
            validateTxtFloat((TextBox)sender);
            updatedCustomPosUI();
        }

        private void btnClearPicture_Click(object sender, EventArgs e)
        {
            if (selectedPupScreen.Window.HasBackgroundPic())
            {
                selectedPupScreen.Window.ClearPicture();
            }
            else
            {
                MessageBox.Show(this,"There is no picture loaded into PuP screen #" + selectedPupScreen.ScreenIndex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            refreshPicButtonStatus();
        }

        private void btnSavePupScreens_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(screensPupFile);
            saveFileDialog1.Filter = "PuP file|*.pup";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    string text;
                    text = PupTools.GetScreenPupHeaders() + Environment.NewLine;
                    foreach (PupScreen pupScreen in pupScreens)
                    {
                        text += pupScreen.GetCsv() + Environment.NewLine;
                    }
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, text);
                    MessageBox.Show(this, "File saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void cboRefScreen_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedPupScreen != null)
            {
                // cannot pick ref 99 for screens with index > 10
                if (cboRefScreen.Text == PupScreens.SPECIAL_99_SCREENINDEX.ToString() && selectedPupScreen.ScreenIndex > PupScreens.FIRST_USER_SCREENINDEX - 1)
                {
                    cboRefScreen.Text = selectedPupScreen.HasCustomPos ? selectedPupScreen.GetRefScreenIndex().ToString() : "";
                }
                else if (cboRefScreen.Text == PupScreens.OTHER_SCREENINDEX)
                {
                    int newRef = -1;
                    List<string> existingScreens = new List<string>();
                    foreach (string item in cboRefScreen.Items) existingScreens.Add(item);
                    newRef = GetNewScreenRef(existingScreens, PupScreens.MAX_SCREENINDEX);
                    if (newRef == -1)
                    {
                        cboRefScreen.Text = selectedPupScreen.HasCustomPos ? selectedPupScreen.GetRefScreenIndex().ToString() : "";
                    }
                    else
                    {
                        if (!cboRefScreen.Items.Contains(newRef.ToString())) cboRefScreen.Items.Add(newRef.ToString());
                    }
                    cboRefScreen.Text = newRef.ToString();
                }
                updatedCustomPosUI();
            }
        }

        private void btnResetScreen_Click(object sender, EventArgs e)
        {
            if (!selectedPupScreen.IsNewlyCreated())
            {
                selectedPupScreen.RestoreDefault();
                updateScreenPropertiesFields();
            }
            else
            {
                MessageBox.Show(this, "This screen was created during this session..." + Environment.NewLine + "No settings to restore.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void killAllWindows()
        {
            if (pupScreens != null)
            {
                foreach (PupScreen pupScreen in pupScreens)
                {
                    pupScreen.PropertyChanged -= PupScreenPropertiesChanged;
                    pupScreen.Window.UnauthorizedActivation -= UnauthorizedActivationOfPupScreen;
                    pupScreen.Window.Dispose();
                }
                pupScreens.Clear();
            }
        }

        private void btnRefInfo_Click(object sender, EventArgs e)
        {
            string info = "";
            foreach (PupScreen p in refScreens)
            {
                if (p.ScreenIndex != PupScreens.SPECIAL_99_SCREENINDEX) // we skip the special 99, which is use for a screen to refer to itself and so has no defined dimensions
                {
                    string name = PupScreen.refScreenNames[p.ScreenIndex];
                    info += "Ref Screen #" + p.ScreenIndex + " (" + name + "):" + Environment.NewLine + "{ x=" + p.X + ", y=" + p.Y + ", width=" + p.W + ", height=" + p.H + " }" + Environment.NewLine + Environment.NewLine;
                }
            }
            MessageBox.Show(this,info, "Pup Reference Screens Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUpGrid_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 0)
            {
                int indexA = dataGridView1.SelectedRows[0].Index;
                int indexB = indexA - 1;
                if (indexB >= 0) SwapListviewItem(indexA, indexB);
            }
        }

        private void SwapListviewItem(int indexA, int indexB)
        {
            dataGridView1.DataSource = null;
            PupScreen tmp = pupScreens[indexA];
            pupScreens[indexA] = pupScreens[indexB];
            pupScreens[indexB] = tmp;
            dataGridView1.DataSource = pupScreens;
            updateAllCustomPosInGrid();
            if (!dataGridView1.Rows[indexB].Displayed) dataGridView1.FirstDisplayedScrollingRowIndex = indexB;
            dataGridView1.Rows[indexB].Selected = true;
        }

        private void btnDownGrid_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 0)
            {
                int indexA = dataGridView1.SelectedRows[0].Index;
                int indexB = indexA + 1;
                if (indexB < dataGridView1.Rows.Count) SwapListviewItem(indexA, indexB);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                selectedPupScreen = null;
                foreach (PupScreen pupScreen in pupScreens) pupScreen.Highlight(false); // pupScreen.Window.SetColors(false, useTransparentPupFrames, null, Color.Yellow);
            }
            else
            {
                grpScreenProp.Enabled = true;
                int index = Convert.ToInt16(dataGridView1.SelectedRows[0].Cells[0].Value);
                foreach (PupScreen pupScreen in pupScreens)
                {
                    if (pupScreen.ScreenIndex == index)
                    {
                        selectedPupScreen = pupScreen;
                        pupScreen.Highlight(true);
                    }
                    else
                    {
                        pupScreen.Highlight(false);
                    }
                }
                updateScreenPropertiesFields();
                ckLockAR.Checked = false;
                selectedPupScreen.Window.SetAspectRatio(1, false);
                refreshPicButtonStatus();
                Focus();
            }

        }

        private void refreshPicButtonStatus()
        {
            btnExportRes.Enabled = selectedPupScreen.Window.BgPictureSource == MatDWindow.PictureSource.db2s;
            btnSavePic.Enabled = selectedPupScreen.Window.HasPicture();
            btnClearPicture.Enabled = selectedPupScreen.Window.HasPicture();
        }

        private void btnAddGrid_Click(object sender, EventArgs e)
        {
            int index = pupScreens.GetNextAvailableCustomIndex();
            if (index != -1)
            {
                dataGridView1.DataSource = null;
                PupScreen pupScreen = pupScreens.AddOne(index, useTransparentPupFrames, refScreens);
                pupScreen.PropertyChanged += PupScreenPropertiesChanged;
                pupScreen.Window.UnauthorizedActivation += UnauthorizedActivationOfPupScreen;
                dataGridView1.DataSource = pupScreens;
                updateAllCustomPosInGrid();
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            }
            else
            {
                MessageBox.Show(this, "Maximum number of screens  already reached!" + Environment.NewLine+ "(" + PupScreens.MAX_ALLOWED_SCREENINDEX + " is the max index)", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            enablePropertyControls();
        }

        private void btnDelGrid_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count != 0)
            {
                int screenIndex = selectedPupScreen.ScreenIndex;
                if (screenIndex < PupScreens.FIRST_USER_SCREENINDEX  && !authorizeDeleteAnyScreen)
                {
                    MessageBox.Show(this, "You can only delete screens with index >= " + PupScreens.FIRST_USER_SCREENINDEX, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    if (MessageBox.Show(this, "Are you sure you want to delete Screen #" + screenIndex + "?", "Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        int indexInGrid = dataGridView1.SelectedRows[0].Index;
                        dataGridView1.DataSource = null;
                        pupScreens.RemoveOne(screenIndex);
                        dataGridView1.DataSource = pupScreens;
                        updateAllCustomPosInGrid();
                        if (dataGridView1.Rows.Count <= indexInGrid) indexInGrid--;
                        if (indexInGrid < dataGridView1.Rows.Count && indexInGrid >= 0)
                        {
                            dataGridView1.FirstDisplayedScrollingRowIndex = indexInGrid;
                            dataGridView1.Rows[indexInGrid].Selected = true;
                        }
                        else
                        {
                            grpScreenProp.Enabled = false;
                        }
                    }
                }
            }
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            HelpForm hlp = new HelpForm();
            hlp.TopMost = true;
            hlp.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CloseCancel() == false)
            {
                e.Cancel = true;
            };
        }

        public static bool CloseCancel()
        {
            string message = "Are you sure you want to exit " + PupTools.GetSoftwareName(false) + " ?";
            string caption = "Exit";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return true;
            else
                return false;
        }

        private void btnCenterY_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.CenterY();
        }

        private void btnCenterX_Click(object sender, EventArgs e)
        {
            selectedPupScreen.Window.CenterX();
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            MessageBox.Show("click");
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView grid = sender as DataGridView;
                int pad = grid.Columns[e.ColumnIndex].DefaultCellStyle.Padding.Right;
                if (pad > 5)
                {
                    Rectangle rect = grid.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    rect.X += rect.Width - pad;
                    rect.Y += 2;
                    rect.Width = pad;
                    rect.Height = rect.Height - 6;
                    if (rect.Contains(grid.PointToClient(Control.MousePosition)))
                    {
                        string newContent = "";
                        if (e.ColumnIndex == 4) // "Playlist
                        {
                            newContent = browseForPlaylist();
                            
                        }
                        else if (e.ColumnIndex == 5) // "Playfile
                        {
                            newContent = browseForPlayfile();
                        }
                        if (newContent != "") dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[e.ColumnIndex].Value = newContent;
                    }
                }
            }
            catch
            {
                MessageBox.Show("bad Click");
            }
        }

        private string browseForPlayfile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Please select a file";
            string currentPlaylist;
            try
            { currentPlaylist = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[4].Value.ToString(); }
            catch
            { currentPlaylist = ""; }
            dialog.InitialDirectory = getPlaylistFolder(currentPlaylist);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return Path.GetFileName(dialog.FileName);
            }
            else
            {
                return "";
            }
        }

        private string getPlaylistFolder(string playlist)
        {
            try
            {
                string path1 = Path.Combine(Path.GetDirectoryName(screensPupFile), playlist);
                string path2 = Path.GetDirectoryName(screensPupFile);
                if (Directory.Exists(path1)) return path1;
                else if (Directory.Exists(path2)) return path2;
                else return Directory.GetCurrentDirectory();
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }

        private string browseForPlaylist()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Please select the folder of the Playlist";
            string currentPlaylist;
            try
            { currentPlaylist = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[4].Value.ToString(); }
            catch
            { currentPlaylist = ""; }
            dialog.SelectedPath = getPlaylistFolder(currentPlaylist);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    return new DirectoryInfo(dialog.SelectedPath).Name;
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            int pad = e.CellStyle.Padding.Right;
            if (pad > 5)
            {
                DataGridView grid = sender as DataGridView;
                e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
                Rectangle rect = e.CellBounds;
                rect.X += rect.Width - (pad - 1);
                rect.Y += 2;
                rect.Width = pad - 3;
                rect.Height = rect.Height - 6;
                ButtonState state = (Control.MouseButtons == MouseButtons.Left
                                        && rect.Contains(grid.PointToClient(Control.MousePosition)))
                                ? ButtonState.Pushed : ButtonState.Normal;
                ControlPaint.DrawButton(e.Graphics, rect, state);
                StringFormat formater = new StringFormat();//added
                formater.Alignment = StringAlignment.Center;//added
                e.Graphics.DrawString("...", e.CellStyle.Font, new SolidBrush(e.CellStyle.ForeColor), rect, formater);//added

                e.Handled = true;
            }
        }

        private void btnSavePic_Click(object sender, EventArgs e)
        {
            if (selectedPupScreen.Window.HasPicture())
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = Path.GetDirectoryName(screensPupFile);
                saveFileDialog1.FileName = selectedPupScreen.Window.DefaultPicName;
                saveFileDialog1.Filter = "png file|*.png";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDialog1.FileName != "")
                    {
                        selectedPupScreen.Window.SavePicture(saveFileDialog1.FileName);
                        MessageBox.Show(this, "Picture saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnExportRes_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.InitialDirectory = Path.GetDirectoryName(screensPupFile);
            try
            {
                saveFileDialog1.FileName = selectedPupScreen.Window.DefaultResName;
            }
            catch
            {
                saveFileDialog1.FileName = "";
            }
            saveFileDialog1.Filter = "res file|*.res";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog1.FileName != "")
                {
                    string content = B2sTools.BuildResFile(selectedPupScreen, refScreens);
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, content);
                    MessageBox.Show(this, "RES file saved", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static int GetNewScreenRef(List<string> existingRefScreens, int maxItems)
        {
            ScreenRefInputBox form = new ScreenRefInputBox(existingRefScreens, maxItems);
            form.TopMost = true;
            DialogResult dialogResult = form.ShowDialog();
            return form.SelectedRef;
        }
    }
}


