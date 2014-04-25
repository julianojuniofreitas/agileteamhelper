using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Atlassian.Jira;
using System.Security.Cryptography;
using System.Collections.ObjectModel;

namespace JiraTrayApp
{
    public class IssueTemplate: ICustomTypeDescriptor
    {
        private static Dictionary<int, IEnumerable<JiraNamedEntity>> _cachedCustomFields = new Dictionary<int, IEnumerable<JiraNamedEntity>>();
        private static Dictionary<int, IEnumerable<IssueType>> _cachedIssueTypes = new Dictionary<int, IEnumerable<IssueType>>();
        private static Dictionary<int, IEnumerable<IssuePriority>> _cachedIssuePriorities = new Dictionary<int, IEnumerable<IssuePriority>>();
        private static Dictionary<int, IEnumerable<ProjectVersion>> _cachedProjectVersions = new Dictionary<int, IEnumerable<ProjectVersion>>();
        private static Dictionary<int, IEnumerable<ProjectComponent>> _cachedProjectComponents = new Dictionary<int, IEnumerable<ProjectComponent>>();

        private Jira _jira;
        private bool _hideServerProperties;

        public IssueTemplate()
        {
            this.CustomFields = new Collection<CustomFieldTemplate>();
            this.SubTaskId = "5";
        }

        public IssueTemplate(Issue issue)
            : this()
        {
            _jira = issue.Jira;
            _hideServerProperties = true;

            this.Type = issue.Type.Id;
            var jiraIssueType = GetIssueTypes().FirstOrDefault(t => t.Id == issue.Type.Id);
            if (jiraIssueType != null)
            {
                this.Type = jiraIssueType.Name;
            }

            this.Assignee = issue.Assignee;
            this.Summary = issue.Summary;
            this.Priority = issue.Priority.Name;
            this.Environment = issue.Environment;
            this.Description = issue.Description;
            this.ProjectKey = issue.Project;

            this.AffectsVersion = issue.AffectsVersions.Any() ? issue.AffectsVersions.First().Name : null;
            this.FixVersion = issue.FixVersions.Any() ? issue.FixVersions.First().Name : null;
            this.Component = issue.Components.Any() ? issue.Components.First().Name : null;
        }

        public void PopulateIssue(Issue issue)
        {
            if (!String.IsNullOrEmpty(ParentIssueKey))
            {
                issue.Type = SubTaskId;
            }
            else
            {
                issue.Type = this.Type;
                var jiraIssueType = GetIssueTypes().FirstOrDefault(i => i.Name.Equals(this.Type));
                if(jiraIssueType != null)
                {
                    issue.Type = jiraIssueType.Id;
                }
            }

            issue.Summary = Summary;
            issue.Assignee = Assignee;
            issue.Priority = GetIssuePriorities().FirstOrDefault(p => p.Name.Equals(this.Priority)).Id;
            issue.Environment = Environment;
            issue.Description = Description;

            var projectVersions = GetProjectVersions();
            var affectVersion = projectVersions.FirstOrDefault(v => v.Name.Equals(this.AffectsVersion));
            if (affectVersion != null)
            {
                issue.AffectsVersions.Add(affectVersion);
            }

            var fixVersion = projectVersions.FirstOrDefault(v => v.Name.Equals(this.FixVersion));
            if (fixVersion != null)
            {
                issue.FixVersions.Add(fixVersion);
            }

            var component = GetProjectComponents().FirstOrDefault(c => c.Name.Equals(this.Component));
            if (component != null)
            {
                issue.Components.Add(component);
            }

            foreach (var customField in CustomFields)
            {
                issue[customField.Name] = customField.Value;
            }
        }


        [Category("Server")]
        [DisplayName("Url")]
        [Description("Url of the JIRA server.")]
        public string ServerUrl { get; set; }
        [Category("Server")]
        [Description("User name to log in to JIRA server.")]
        public string Username { get; set; }
        [Category("Server")]
        [PasswordPropertyText(true)]
        [Description("Password to log in to JIRA server.")]
        public byte[] Password { get; set; }

        [Browsable(false)]
        public string Name { get; set; }
        [Browsable(false)]
        public int Id { get; set; }
        [Browsable(false)]
        public string Summary { get; set; }
        [Browsable(false)]
        public string Description { get; set; }

        [Description("If specified, new issue will be created as a sub-task of the parent.")]
        [Category("Sub-Task")]
        [DisplayName("Parent Issue Key")]
        public string ParentIssueKey { get; set; }

        [Description("The SubTask issue type ID.")]
        [Category("Sub-Task")]
        [DisplayName("Sub-Task Id")]
        public string SubTaskId { get; set; }

        [Description("Hardware or software environment to which the issue relates.")]
        [Category("Issue")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Environment { get; set; }

        [Description("Person to whom the issue is currently assigned.")]
        [Category("Issue")]
        public string Assignee { get; set; }

        [Description("Parent project to which the issue belongs.")]
        [Category("Issue")]
        [DisplayName("Project Key")]
        public string ProjectKey { get; set; }

        [Description("The type of the issue.")]
        [Category("Issue")]
        public string Type { get; set; }

        [Description("Importance of the issue in relation to other issues.")]
        [Category("Issue")]
        public string Priority { get; set; }

        [Description("Project version in which the issue was (or will be) fixed.")]
        [Category("Issue")]
        public string FixVersion { get; set; }

        [Description("Project version for which the issue is (or was) manifesting.")]
        [Category("Issue")]
        public string AffectsVersion { get; set; }

        [Description("Project component(s) to which this issue relates.")]
        [Category("Issue")]
        public string Component { get; set; }

        [Description("Comma separated labels to which this issue relates.")]
        [Category("Issue")]
        public string Labels { get; set; }

        [Description("Custom fields.")]
        [Category("Issue")]
        public virtual Collection<CustomFieldTemplate> CustomFields { get; set; }

        public IEnumerable<JiraNamedEntity> GetIssueTypes()
        {
            if (!_cachedIssueTypes.ContainsKey(this.Id))
            {
                _cachedIssueTypes.Add(this.Id, GetJira().GetIssueTypes(this.ProjectKey));
            }

            return _cachedIssueTypes[this.Id];
        }

        public IEnumerable<ProjectVersion> GetProjectVersions()
        {
            if (!_cachedProjectVersions.ContainsKey(this.Id))
            {
                _cachedProjectVersions.Add(this.Id, GetJira().GetProjectVersions(this.ProjectKey));
            }

            return _cachedProjectVersions[this.Id];
        }

        public IEnumerable<ProjectComponent> GetProjectComponents()
        {
            if (!_cachedProjectComponents.ContainsKey(this.Id))
            {
                _cachedProjectComponents.Add(this.Id, GetJira().GetProjectComponents(this.ProjectKey));
            }

            return _cachedProjectComponents[this.Id];
        }
        
        public IEnumerable<JiraNamedEntity> GetIssuePriorities()
        {
            if (!_cachedIssuePriorities.ContainsKey(this.Id))
            {
                _cachedIssuePriorities.Add(this.Id, GetJira().GetIssuePriorities());
            }

            return _cachedIssuePriorities[this.Id];
        }

        public IEnumerable<JiraNamedEntity> GetCustomFields()
        {
            if(! _cachedCustomFields.ContainsKey(this.Id))
            {
                _cachedCustomFields.Add(this.Id, GetJira().GetCustomFields());
            }
            return _cachedCustomFields[this.Id];
        }

 	    internal Jira GetJira()
        {
            if (_jira == null)
            {
                _jira = new Jira(this.ServerUrl, this.Username, DataProtector.UnProtect(Password));
            }

            return _jira;
        }

        #region ICustomTypeDescriptor
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(this);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            if (_hideServerProperties)
            {
                var descriptorCollection = new PropertyDescriptorCollection(null);

                foreach(var pd in TypeDescriptor.GetProperties(this, true).OfType<PropertyDescriptor>().Where(d =>
                    d.Category != "Server" && d.Category != "Sub-Task" && d.Name != "ProjectKey" && d.Name != "CustomFields"))
                {
                    descriptorCollection.Add(pd);
                }

                return descriptorCollection;
            }
            else 
            { 
                return TypeDescriptor.GetProperties(this, true);
            }
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        #endregion
    }
}