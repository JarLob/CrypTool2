﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;
namespace TranspositionAnalyser
{
    class TranspositionAnalyserSettings : ISettings
    {
        #region settings
        private int selected_method = 0;

        internal void UpdateTaskPaneVisibility()
        {
            

            if (TaskPaneAttributeChanged == null)
                return;

            switch (selected_method)
            {
                case 0: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Hidden)));
                        break;
                case 1: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Visible)));
                        break;
                case 2: TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("MaxLength", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("RowColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnRow", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ColumnColumnColumn", Visibility.Hidden)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("KeySize", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Repeatings", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Iterations", Visibility.Visible)));
                        TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("CribSearchKeylength", Visibility.Hidden)));
                        break;
            }
        }
        //[PropertySaveOrder(1)]
        [TaskPane("Analysis_methodCaption", "Analysis_methodTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Analysis_methodList1", "Analysis_methodList2", "Analysis_methodList3" })]
        public int Analysis_method
        {
            get
            {
                return this.selected_method;
            }
            set
            {
                if (value != selected_method)
                {
                    this.selected_method = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Analysis_method");   
                }                
            }

        }

        // FIX: REGEX 
        private int bruteforce_length = 12;
        [PropertySaveOrder(2)]
        [TaskPaneAttribute( "MaxLengthCaption", "MaxLengthTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int MaxLength
        {
            get { return bruteforce_length; }
            set
            {
                if (value != this.bruteforce_length)
                {
                    bruteforce_length = value;
                    OnPropertyChanged("MaxLength");   
                }
            }
        
        }

        private int keysize = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute( "KeySizeCaption", "KeySizeTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int KeySize
        {
            get { return keysize; }
            set
            {
                if (value != this.keysize)
                {
                    keysize = value;
                    OnPropertyChanged("KeySize");   
                }
            }
        }

        private Boolean row_colum_column = true;
        [PropertySaveOrder(4)]
        [ContextMenu( "RowColumnColumnCaption", "RowColumnColumnTooltip", 4, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane( "RowColumnColumnCaption", "RowColumnColumnTooltip", null, 4, false, ControlType.CheckBox, "")]
        public bool RowColumnColumn
        {
            get { return this.row_colum_column; }
            set
            {
                if (value != this.row_colum_column)
                {
                    this.row_colum_column = value;
                    OnPropertyChanged("RowColumnColumn");   
                }
            }
        }

        private Boolean row_colum_row = true;
        [PropertySaveOrder(5)]
        [ContextMenu( "RowColumnRowCaption", "RowColumnRowTooltip", 4, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane( "RowColumnRowCaption", "RowColumnRowTooltip", null, 4, false, ControlType.CheckBox, "")]
        public bool RowColumnRow
        {
            get { return this.row_colum_row; }
            set
            {
                if (value != this.row_colum_row)
                {
                    this.row_colum_row = value;
                    OnPropertyChanged("RowColumnRow");   
                }
            }
        }


        private Boolean column_colum_row = true;
        [PropertySaveOrder(6)]
        [ContextMenu( "ColumnColumnRowCaption", "ColumnColumnRowTooltip", 4, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane( "ColumnColumnRowCaption", "ColumnColumnRowTooltip", null, 4, false, ControlType.CheckBox, "")]
        public bool ColumnColumnRow
        {
            get { return this.column_colum_row; }
            set
            {
                if (value != this.column_colum_row)
                {
                    this.column_colum_row = value;
                    OnPropertyChanged("ColumnColumnRow");   
                }
            }
        }

        private Boolean column_colum_column = true;
        [PropertySaveOrder(7)]
        [ContextMenu( "ColumnColumnColumnCaption", "ColumnColumnColumnTooltip", 4, ContextMenuControlType.CheckBox, null, "Row-Column-Column")]
        [TaskPane( "ColumnColumnColumnCaption", "ColumnColumnColumnTooltip", null, 4, false, ControlType.CheckBox, "")]
        public bool ColumnColumnColumn
        {
            get { return this.column_colum_column; }
            set
            {
                if (value != this.column_colum_column)
                {
                    this.column_colum_column = value;
                    OnPropertyChanged("ColumnColumnColumn");   
                }
            }
        }

        private int repeatings = 10;
        [PropertySaveOrder(8)]
        [TaskPaneAttribute( "RepeatingsCaption", "RepeatingsTooltip", null, 2, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Repeatings
        {
            get { return repeatings; }
            set
            {
                if (value != this.repeatings)
                {
                    repeatings = value;
                    OnPropertyChanged("Repeatings");   
                }
            }
        }

        private int iterations = 5000;
        [PropertySaveOrder(9)]
        [TaskPaneAttribute( "IterationsCaption", "IterationsTooltip", null, 2, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Iterations
        {
            get { return iterations; }
            set
            {
                if (value != this.iterations)
                {
                    iterations = value;
                    OnPropertyChanged("Iterations");   
                }
            }
        }

        private int cribSearchKeylength = 12;
        [PropertySaveOrder(10)]
        [TaskPaneAttribute( "CribSearchKeylengthCaption", "CribSearchKeylengthTooltip", null, 2, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int CribSearchKeylength
        {
            get { return cribSearchKeylength; }
            set
            {
                if (value != this.cribSearchKeylength)
                {
                    cribSearchKeylength = value;
                    OnPropertyChanged("CribSearchKeylength");   
                }
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
