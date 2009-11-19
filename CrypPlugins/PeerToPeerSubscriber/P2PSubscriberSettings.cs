﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Control;
using System.Windows;
using Cryptool.Plugins.PeerToPeer;

namespace Cryptool.Plugins.PeerToPeer
{
    public class P2PSubscriberSettings : ISettings
    {
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private bool hasChanges = false;
        private P2PSubscriber p2pSubscriber;

        #region ISettings Members

        public bool HasChanges
        {
            get
            {
                return this.hasChanges;
            }
            set
            {
                this.hasChanges = true;
            }
        }

        #endregion

        public P2PSubscriberSettings (P2PSubscriber p2pSubscriber)
	    {
            this.p2pSubscriber = p2pSubscriber;
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            }
	    }

        private string sTopic = "NewTopic";
        [TaskPane("Topic Name", "Choose a topic name with which this subscriber shall be registered.", null, 0, false, DisplayLevel.Beginner, ControlType.TextBox)]
        public string TopicName
        {
            get { return this.sTopic; }
            set
            {
                if (this.sTopic != value && value != String.Empty && value != null)
                {
                    this.sTopic = value;
                    HasChanges = true;
                    OnPropertyChanged(TopicName);
                }
            }
        }

        /* FOR TESTING ISSUES */
        [TaskPane("Unregister", "Click here to Unregister the publisher from all registered subscribers!", "Control region", 0, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnUnregister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnUnregister");
        }
        [TaskPane("Register", "Click here to Register the publisher pro-active with all formely registered subscribers!", "Control region", 1, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnRegister()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Visible)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Visible)));
            OnPropertyChanged("BtnRegister");
        }

        [TaskPane("Solution found", "TESTING: Emulate solution-found-case!", "Control region", 2, true, DisplayLevel.Beginner, ControlType.Button)]
        public void BtnSolutionFound()
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnSolutionFound", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnUnregister", Visibility.Collapsed)));
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("BtnRegister", Visibility.Visible)));
            OnPropertyChanged("BtnSolutionFound");
        }
        /* FOR TESTING ISSUES */

        private int checkPublishersAvailability = 60;
        [TaskPane("Check Publisher Interval (in sec)","To check liveness or possibly changed publishing peer in intervals","Intervals",0,false,DisplayLevel.Beginner,ControlType.NumericUpDown, ValidationType.RangeInteger,20,int.MaxValue)]
        public int CheckPublishersAvailability 
        {
            get
            {
                // multiplied with thousand because the interval is denoted in milliseconds!
                return this.checkPublishersAvailability;
            }
            set
            {
                if (value != this.checkPublishersAvailability)
                {
                    this.checkPublishersAvailability = value;
                    OnPropertyChanged("CheckPublishersAvailability");
                }
            }
        }

        private int publishersReplyTimespan = 5;
        [TaskPane("Publisher Reply Timespan (in sec)", "When checking publishers availability, ping message is sent. The publisher must answer with a pong message in the timespan!", "Intervals", 0, false, DisplayLevel.Beginner, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 60)]
        public int PublishersReplyTimespan
        {
            get
            {
                // multiplied with thousand because the interval is denoted in milliseconds!
                return this.publishersReplyTimespan;
            }
            set
            {
                if (value != this.publishersReplyTimespan)
                {
                    this.publishersReplyTimespan = value;
                    OnPropertyChanged("PublishersReplyTimespan");
                }
            }
        }
        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
