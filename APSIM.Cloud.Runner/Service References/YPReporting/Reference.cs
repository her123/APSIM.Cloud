﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace APSIM.Cloud.Runner.YPReporting {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="YPReporting.IReporting")]
    public interface IReporting {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IReporting/StoreReport", ReplyAction="http://tempuri.org/IReporting/StoreReportResponse")]
        void StoreReport(string aReportName, System.Data.DataSet aReportData);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IReportingChannel : APSIM.Cloud.Runner.YPReporting.IReporting, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ReportingClient : System.ServiceModel.ClientBase<APSIM.Cloud.Runner.YPReporting.IReporting>, APSIM.Cloud.Runner.YPReporting.IReporting {
        
        public ReportingClient() {
        }
        
        public ReportingClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ReportingClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReportingClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ReportingClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void StoreReport(string aReportName, System.Data.DataSet aReportData) {
            base.Channel.StoreReport(aReportName, aReportData);
        }
    }
}
