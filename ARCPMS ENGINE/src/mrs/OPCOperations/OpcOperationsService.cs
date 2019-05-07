using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ARCPMS_ENGINE.src.mrs.OPCOperations
{
    interface OpcOperationsService : IDisposable
    {
        T ReadTag<T>(string channel,string machine,string tagName );
        bool WriteTag<T>(string channel, string machineName, string tagName, T value);
        //T ReadTagFromCamOpc<T>(string tagName);
        //bool WriteTagToCamOpc<T>( string tagName, T value);
        bool IsMachineHealthy(string command);
    }
}
