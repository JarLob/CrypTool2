﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class CT2Translate : Form
    {
        private ListViewColumnSorter lvwColumnSorter;
        List<string> allResources = new List<string>();
        //Dictionary<string, TranslatedResource> theResources = new Dictionary<string,TranslatedResource>();
        AllResources allres = new AllResources();

        string[] displayedLanguages = { "en", "de" };
        Dictionary<string, string> LongLanguage = new Dictionary<string, string> { {"en","Englisch"}, {"de","Deutsch"} };


        public CT2Translate()
        {
            InitializeComponent();
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            listView1.ListViewItemSorter = lvwColumnSorter;
            listView1.Columns.Clear();
            listView1.Columns.Add(new ColHeader("Resource", 5, HorizontalAlignment.Left, true));
            listView1.Columns.Add(new ColHeader("Key", 100, HorizontalAlignment.Left, true));
            foreach( string lang in displayedLanguages )
                listView1.Columns.Add(new ColHeader(LongLanguage[lang], 160, HorizontalAlignment.Left, true));
            listView1.HideSelection = false;

            fileTree.HideSelection = false;

            ToolTip toolTip1 = new ToolTip();
            toolTip1.ShowAlways = true;

            toolTip1.SetToolTip(prevmissButton, "go to previous missing item");
            toolTip1.SetToolTip(nextmissButton, "go to next missing item");
            toolTip1.SetToolTip(ClearSearchButton, "clear filter");
            toolTip1.SetToolTip(SearchButton, "apply filter");

            //pathTextBox.Text = Properties.Settings.Default.Path;
        }

        private void recursiveDirectoryScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            FolderBrowserDialog objDialog = new FolderBrowserDialog();
            objDialog.Description = "Startpfad wählen";
            objDialog.SelectedPath = Properties.Settings.Default.Path;
            DialogResult objResult = objDialog.ShowDialog(this);

            if (objResult == DialogResult.OK)
            {
                Properties.Settings.Default.Path = objDialog.SelectedPath;
                Properties.Settings.Default.Save();

                allres.ScanDir(objDialog.SelectedPath);

                fileTree.Nodes.Clear();
                fileTree.Nodes.Add(allres.GetTree());

                UpdateList();

                basepathTextBox.Text = allres.basepath;

                toolStripStatusLabel1.Text = allres.Resources.Count + " resources found";
            } 
            
            Cursor.Current = Cursors.Default;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ( listView1.SelectedItems.Count>0 ) {
                fileTextBox.Text = listView1.SelectedItems[0].SubItems[0].Text;
                keyTextBox.Text = listView1.SelectedItems[0].SubItems[1].Text;
                lang1TextBox.Text = listView1.SelectedItems[0].SubItems[displayedLanguages[0]].Text;
                if( displayedLanguages.Count()>1 )
                    lang2TextBox.Text = listView1.SelectedItems[0].SubItems[displayedLanguages[1]].Text;
            }
        }

        private void UpdateItem(ListViewItem item)
        {
            TranslatedKey tk = (TranslatedKey)item.Tag;

            foreach (string lang in displayedLanguages)
            {
                if (tk.Translations.ContainsKey(lang)) {
                    item.SubItems[lang].BackColor = Color.Transparent;
                    item.SubItems[lang].Text = tk.Translations[lang];
                } else {
                    item.SubItems[lang].BackColor = Color.LightSalmon;
                    item.SubItems[lang].Text = "";
                }
            }
        }

        private void listViewAdd(string fname, TranslatedResource dict, string filter = ".*")
        {
            List<ListViewItem> lvi = new List<ListViewItem>();

            try
            {
                foreach (KeyValuePair<string, TranslatedKey> pair in dict.TranslatedKey)
                {
                    RegexOptions options = (searchcaseBox.Checked) ? RegexOptions.None : RegexOptions.IgnoreCase;

                    bool matched = Regex.Match(pair.Key.ToString(), filter, options).Success;

                    foreach (string lang in displayedLanguages)
                        if (pair.Value.Translations.ContainsKey(lang))
                            matched |= Regex.Match(pair.Value.Translations[lang].ToString(), filter, options).Success;

                    if (!matched) continue;
                    
                    // populate listview
                    ListViewItem item = new ListViewItem( new string[] { fname, pair.Key.ToString() } );
                    foreach (string lang in displayedLanguages)
                    {
                        ListViewItem.ListViewSubItem subitem = item.SubItems.Add("");
                        subitem.Name = lang;
                    }

                    item.Tag = pair.Value;  // speichere Zeiger auf TranslatedKey im Item-Tag
                    item.UseItemStyleForSubItems = false;
                    UpdateItem(item);
                    lvi.Add(item);
                }
                listView1.Items.AddRange(lvi.ToArray());    // viel schneller als einzelnes Hinzufügen!
            }
            catch (Exception e)
            {
            }
        }

        private void listViewAdd(string filter = ".*")
        {
            listViewAdd(allres.Resources.Keys.ToArray(), filter);
        }

        private void listViewAdd(string[] paths, string filter = ".*")
        {
            listView1.BeginUpdate();
            foreach (string path in paths)
                listViewAdd(path, allres.Resources[path], filter);
            listView1.EndUpdate();
            //toolStripStatusLabel1.Text = listView1.Items.Count + " items displayed";
            textBox1.Text = String.Format("{0} item{1} displayed{2}", 
                listView1.Items.Count,
                (listView1.Items.Count==1) ? "" : "s",
                (filter==".*" || filter=="") ? "" : " (filtered)"
                );
            textBox2.Text = countEmptyKeys().ToString();
        }

        private int countEmptyKeys()
        {
            int count = 0;

            foreach( ListViewItem item in listView1.Items ) {
                TranslatedKey tk = (TranslatedKey)item.Tag;
                foreach (string lang in displayedLanguages)
                {
                    if (!tk.Translations.ContainsKey(lang))
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        private void nextmissButton_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (listView1.SelectedItems.Count == 0)
                listView1.Items[0].Selected = true;
            var i = listView1.SelectedIndices[0];
            for (int j = 0; j < listView1.Items.Count; j++)
            {
                ListViewItem item = listView1.Items[(i + 1 + j) % listView1.Items.Count];
                foreach (string lang in displayedLanguages)
                    if (!((TranslatedKey)item.Tag).Translations.ContainsKey(lang))
                    {
                        listView1.SelectedItems.Clear();
                        item.Selected = true;
                        item.EnsureVisible();
                        listView1.Select();
                        return;
                    }
            }
        }

        private void prevmissButton_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count == 0) return;
            if (listView1.SelectedItems.Count == 0)
                listView1.Items[0].Selected = true;
            var i = listView1.SelectedIndices[0];
            for (int j = 0; j < listView1.Items.Count; j++)
            {
                ListViewItem item = listView1.Items[(i - 1 - j + listView1.Items.Count) % listView1.Items.Count];
                foreach (string lang in displayedLanguages)
                    if (!((TranslatedKey)item.Tag).Translations.ContainsKey(lang))
                    {
                        listView1.SelectedItems.Clear();
                        item.Selected = true;
                        item.EnsureVisible();
                        listView1.Select();
                        return;
                    }
            }
        }

        private void UpdateList()
        {
            Cursor.Current = Cursors.WaitCursor;

            if( fileTree.Nodes.Count==0 ) return;

            if (fileTree.SelectedNode == null)
                fileTree.SelectedNode = fileTree.Nodes[0];

            string[] files = allres.MatchFiles((string)fileTree.SelectedNode.Tag);

            listView1.Items.Clear();
            listViewAdd(files, filterBox.Text);

            if (listView1.Items.Count > 0)
                listView1.SelectedIndices.Add(0);

            nextmissButton.Enabled = prevmissButton.Enabled = (listView1.Items.Count > 0);

            Cursor.Current = Cursors.Default;
        }

        private void fileTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateList();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //listView1.AutoResizeColumn(e.Column, ColumnHeaderAutoResizeStyle.ColumnContent);
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                lvwColumnSorter.Order = (lvwColumnSorter.Order == SortOrder.Ascending)
                    ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void filterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchButton_Click(sender, e);
        }

        private void ClearSearchButton_Click(object sender, EventArgs e)
        {
            if (filterBox.Text != "")
            {
                filterBox.Text = "";
                SearchButton_Click(sender, e);
            }
        }

        //private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if(fileTree.SelectedNode==null) return;
        //    //if( listView1.SelectedItems.Count==0 ) return;
        //    //string filename = listView1.SelectedItems[0].SubItems[0].Text;
        //    string filename = fileTree.SelectedNode.FullPath;
        //    logBox.Text += "Saving " + filename + "\n";
        //    //SaveResourceFile(listView1.SelectedItems[0].SubItems[0].Text);
        //    string basename = AllResources.getKey(filename);
        //    string culture = AllResources.getCulture(filename);
        //    if( allres.Resources.ContainsKey(basename) )
        //        allres.Resources[basename].SaveAs(culture, "test");
        //}

        private string GetOpenFileName(string title)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Title = "Open Cryptool Merged Resource";            
            dlg.Title = title;
            dlg.Filter = "Merged Resource (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
                //toolStripStatusLabel1.Text = dlg.FileName;
                //Properties.Settings.Default.Save();
            }

            return null;
        }

        private string GetSaveFileName(string title)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            //dlg.Title = "Open Cryptool Merged Resource";            
            dlg.Title = title;
            dlg.Filter = "Merged Resource (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.FileName;
                //toolStripStatusLabel1.Text = dlg.FileName;
                //Properties.Settings.Default.Save();
            }

            return null;
        }

        private void saveMergedResourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";
            string fname = GetSaveFileName("Save Cryptool Merged Resource");

            Cursor.Current = Cursors.WaitCursor;
            if (fname != null) allres.SaveXML(fname);
            Cursor.Current = Cursors.Default;
        }

        private void loadMergedResourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string fname = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\CT2resources.xml";
            string fname = GetOpenFileName("Load Cryptool Merged Resource");
            if (fname == null) return;

            Cursor.Current = Cursors.WaitCursor;

            try
            {
                allres.Clear();
                allres.LoadXML(fname);
                basepathTextBox.Text = allres.basepath;

                fileTree.Nodes.Clear();
                fileTree.Nodes.Add(allres.GetTree());

                UpdateList();
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = fname + " not found!";
            }

            Cursor.Current = Cursors.Default;
        }

        private void saveGermanAsTxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            allres.SaveText(path+"\\CT2german.txt", "de");
        }

        private void lang2TextBox_Leave(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0) return;

            string lang = "de";
            ListViewItem item = listView1.SelectedItems[0];
            TranslatedKey tk = (TranslatedKey)item.Tag;
            if( !tk.Translations.ContainsKey(lang) ) tk.Add(lang, "");
            tk.Translations[lang] = lang2TextBox.Text;
            item.SubItems[lang].Text = lang2TextBox.Text;
            UpdateItem(item);
            textBox2.Text = countEmptyKeys().ToString();
        }

        private void saveToBasepathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allres.Update();
        }

    }
}
