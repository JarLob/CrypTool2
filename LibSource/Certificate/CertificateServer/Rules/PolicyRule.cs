﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.Util.Logging;
using System.Collections;
using CrypTool.CertificateLibrary.Network;
using System.Xml.Serialization;

namespace CrypTool.CertificateServer.Rules
{
    /// <summary>
    /// Represents a single policy rule.
    /// </summary>
    [Serializable]
    public class PolicyRule : IComparable
    {

        public PolicyRule()
            : this(0, "manual", new Filter())
        {
        }

        public PolicyRule(int id, string policy, params Filter[] filters)
        {
            if (policy == null)
            {
                Log.Warn(String.Format("Error in policy rule with ID: {0} | No policy specified!", id));
                this.Id = 0;
                return;
            }
            if (filters == null || filters.Length == 0)
            {
                Log.Warn(String.Format("Error in policy rule with ID: {0} | No filter specified!", id));
                this.Id = 0;
                return;
            }
            if (!policy.Equals("accept") && !policy.Equals("deny") && !policy.Equals("manual"))
            {
                Log.Warn(String.Format("Error in policy rule with ID: {0} | Policy is not valid!", id));
                this.Id = 0;
                return;
            }
            this.Id = id;
            this.Filters = filters;
            this.Policy = policy;
        }

        public bool IsValidPolicyRule()
        {
            if (this.Filters == null)
            {
                return false;
            }
            foreach (Filter filter in this.Filters)
            {
                if (!filter.IsValidFilter())
                {
                    return false;
                }
            }
            return true;
        }

        public string GetPolicy(CertificateRegistration certReg)
        {
            if (certReg == null)
            {
                throw new ArgumentNullException("Certificate Registration can not be null!");
            }
            bool doesMatch = true;
            foreach (Filter filter in this.Filters)
            {
                if (filter.Attribute == "avatar" && !filter.IsMatch(certReg.Avatar))
                {
                    doesMatch = false;
                    break;
                } else if (filter.Attribute == "email" && !filter.IsMatch(certReg.Email))
                {
                    doesMatch = false;
                    break;
                } else if (filter.Attribute == "world" && !filter.IsMatch(certReg.World))
                {
                    doesMatch = false;
                    break;
                }
            }
            return (doesMatch) ? this.Policy : null;
        }

        public int CompareTo(object obj)
        {
            PolicyRule rule = obj as PolicyRule;
            if (rule == null)
            {
                throw new ArgumentException("Can not compare a policy rule to something that is not a rule!");
            }
            return Id.CompareTo(rule.Id);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerator filterEnum = this.Filters.GetEnumerator();
            if (filterEnum.MoveNext())
            {
                Filter firstFilter = filterEnum.Current as Filter;
                sb.Append(String.Format("'{0}' = '{1}'", firstFilter.Attribute, firstFilter.RegEx));

                while (filterEnum.MoveNext())
                {
                    Filter filter = filterEnum.Current as Filter;
                    sb.Append(String.Format(" AND '{0}' = '{1}'", filter.Attribute, filter.RegEx));
                }
                sb.Append(String.Format(" -> '{0}'", this.Policy));
                return sb.ToString();
            }
            else
            {
                return "No filter found";
            }
        }

        #region Properties

        [XmlAttribute]
        public int Id { get; set; }

        public Filter[] Filters { get; set; }

        [XmlAttribute]
        public string Policy
        {
            get { return this.policy; }
            set
            {
                if (value != null)
                {
                    this.policy = value.ToLower();
                }
                else
                {
                    this.policy = null;
                }
            }
        }

        #endregion

        private Filter[] filters;

        private String policy;

    }
}
