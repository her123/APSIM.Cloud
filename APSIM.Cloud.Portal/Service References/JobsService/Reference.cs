﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace APSIM.Cloud.Portal.JobsService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Job", Namespace="http://schemas.datacontract.org/2004/07/APSIM.Cloud.Service")]
    [System.SerializableAttribute()]
    public partial class Job : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ErrorTextField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private APSIM.Cloud.Portal.JobsService.StatusEnum StatusField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string URLField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ErrorText {
            get {
                return this.ErrorTextField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorTextField, value) != true)) {
                    this.ErrorTextField = value;
                    this.RaisePropertyChanged("ErrorText");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public APSIM.Cloud.Portal.JobsService.StatusEnum Status {
            get {
                return this.StatusField;
            }
            set {
                if ((this.StatusField.Equals(value) != true)) {
                    this.StatusField = value;
                    this.RaisePropertyChanged("Status");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string URL {
            get {
                return this.URLField;
            }
            set {
                if ((object.ReferenceEquals(this.URLField, value) != true)) {
                    this.URLField = value;
                    this.RaisePropertyChanged("URL");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="StatusEnum", Namespace="http://schemas.datacontract.org/2004/07/APSIM.Cloud.Service")]
    public enum StatusEnum : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Queued = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Running = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Completed = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Error = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="JobsService.IJobs")]
    public interface IJobs {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/GetData", ReplyAction="http://tempuri.org/IJobs/GetDataResponse")]
        string GetData(int value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/Add", ReplyAction="http://tempuri.org/IJobs/AddResponse")]
        string Add(APSIM.Cloud.Shared.YieldProphet yieldProphet);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/AddFarm4Prophet", ReplyAction="http://tempuri.org/IJobs/AddFarm4ProphetResponse")]
        string AddFarm4Prophet(APSIM.Cloud.Shared.Farm4Prophet f4p);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/AddAsXML", ReplyAction="http://tempuri.org/IJobs/AddAsXMLResponse")]
        void AddAsXML(string name, string jobXML);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/Delete", ReplyAction="http://tempuri.org/IJobs/DeleteResponse")]
        void Delete(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/Get", ReplyAction="http://tempuri.org/IJobs/GetResponse")]
        APSIM.Cloud.Portal.JobsService.Job Get(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/GetNextToRun", ReplyAction="http://tempuri.org/IJobs/GetNextToRunResponse")]
        APSIM.Cloud.Portal.JobsService.Job GetNextToRun();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/GetMany", ReplyAction="http://tempuri.org/IJobs/GetManyResponse")]
        APSIM.Cloud.Portal.JobsService.Job[] GetMany(int maxNum);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/GetJobXML", ReplyAction="http://tempuri.org/IJobs/GetJobXMLResponse")]
        string GetJobXML(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/AddLogMessage", ReplyAction="http://tempuri.org/IJobs/AddLogMessageResponse")]
        void AddLogMessage(string message, bool isError);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/GetLogMessages", ReplyAction="http://tempuri.org/IJobs/GetLogMessagesResponse")]
        System.Data.DataSet GetLogMessages();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/SetCompleted", ReplyAction="http://tempuri.org/IJobs/SetCompletedResponse")]
        void SetCompleted(string jobName, string errorMessage);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IJobs/ReRun", ReplyAction="http://tempuri.org/IJobs/ReRunResponse")]
        void ReRun(string jobName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IJobsChannel : APSIM.Cloud.Portal.JobsService.IJobs, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class JobsClient : System.ServiceModel.ClientBase<APSIM.Cloud.Portal.JobsService.IJobs>, APSIM.Cloud.Portal.JobsService.IJobs {
        
        public JobsClient() {
        }
        
        public JobsClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public JobsClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JobsClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public JobsClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string GetData(int value) {
            return base.Channel.GetData(value);
        }
        
        public string Add(APSIM.Cloud.Shared.YieldProphet yieldProphet) {
            return base.Channel.Add(yieldProphet);
        }
        
        public string AddFarm4Prophet(APSIM.Cloud.Shared.Farm4Prophet f4p) {
            return base.Channel.AddFarm4Prophet(f4p);
        }
        
        public void AddAsXML(string name, string jobXML) {
            base.Channel.AddAsXML(name, jobXML);
        }
        
        public void Delete(string name) {
            base.Channel.Delete(name);
        }
        
        public APSIM.Cloud.Portal.JobsService.Job Get(string name) {
            return base.Channel.Get(name);
        }
        
        public APSIM.Cloud.Portal.JobsService.Job GetNextToRun() {
            return base.Channel.GetNextToRun();
        }
        
        public APSIM.Cloud.Portal.JobsService.Job[] GetMany(int maxNum) {
            return base.Channel.GetMany(maxNum);
        }
        
        public string GetJobXML(string name) {
            return base.Channel.GetJobXML(name);
        }
        
        public void AddLogMessage(string message, bool isError) {
            base.Channel.AddLogMessage(message, isError);
        }
        
        public System.Data.DataSet GetLogMessages() {
            return base.Channel.GetLogMessages();
        }
        
        public void SetCompleted(string jobName, string errorMessage) {
            base.Channel.SetCompleted(jobName, errorMessage);
        }
        
        public void ReRun(string jobName) {
            base.Channel.ReRun(jobName);
        }
    }
}
