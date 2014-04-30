﻿/*                              
   Copyright 2010 Nils Kopal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;

namespace WorkspaceManager.Model
{
    /// <summary>
    /// This class wraps a text which can be put to the workspace
    /// </summary>
    [Serializable]
    public class TextModel : VisualElementModel
    {

        internal TextModel()
        {

        }

        private byte[] data = null;

        /// <summary>
        /// Instantiate a new TextModel
        /// </summary>
        /// <param name="data"></param>
        public TextModel(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// The WorkspaceModel of this TextModel
        /// </summary>
        public WorkspaceModel WorkspaceModel { get; set; }

        /// <summary>
        /// Loads the Content of this TextModel to the given RichtTextBox
        /// </summary>
        /// <param name="rtb"></param>
        public void loadRTB(RichTextBox rtb){
            
            if (data == null)
            {
                return;
            }

            try
            {

                var memoryStream = new MemoryStream(data);
                var flowDocument = new FlowDocument();
                var textRange = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
                textRange.Load(memoryStream, System.Windows.DataFormats.XamlPackage);
                rtb.Document = flowDocument;
                memoryStream.Close();
            }
            catch (Exception ex)
            {
                //wtf?
                //not a hard failure if a rtb of a template can not be loaded... we can not do anything here... so
            }
        }

        /// <summary>
        /// Save the Content of the given RichTextBox to this TextModel
        /// </summary>
        /// <param name="rtb"></param>
        public void saveRTB(RichTextBox rtb){

            if (rtb == null || rtb.Document == null || rtb.Document.Blocks.Count == 0)
            {
                return;
            }

            try
            {
                var memoryStream = new MemoryStream();
                var textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                textRange.Save(memoryStream, System.Windows.DataFormats.XamlPackage);

                data = new byte[memoryStream.Length];
                data = memoryStream.ToArray();
                memoryStream.Close();
            }
            catch (Exception)
            {
                //wtf?
                //not a hard failure if a rtb of a template can not be saved... we can not do anything here... so
            }
        }

        public bool IsEnabled { get; set; }
        public Color BackgroundColor = Colors.White;

        public bool HasData()
        {
            return data != null && data.Length != 0;
        }
    }
}
